using Microsoft.Data.Sqlite;
using TSMQServer.Core;
using TSMQServer.Db;
using TSMQServer.Db.Sqlite;
using TSMQServer.Db.Sqlite.Entities;
using TSMQServer.Db.Sqlite.Repositories;

namespace TSMQServer.Integration.Tests;

public sealed class PasswordStorageTests
{
    [Fact]
    public void HashUsesRandomSaltAndRejectsInvalidCredentials()
    {
        var first = PasswordHasher.Hash("密码🔒");
        var second = PasswordHasher.Hash("密码🔒");
        Assert.NotEqual(first, second);
        Assert.True(PasswordHasher.IsHash(first));
        Assert.True(PasswordHasher.Verify("密码🔒", first));
        Assert.False(PasswordHasher.Verify("错误密码", first));
        Assert.False(PasswordHasher.Verify(first, first));
    }

    [Theory]
    [InlineData("")]
    [InlineData("plaintext")]
    [InlineData("$tsmq$pbkdf2-sha256$v1$600000$invalid$invalid")]
    [InlineData("$tsmq$pbkdf2-sha256$v1$600000$!!!!!!!!!!!!!!!!!!!!!!!!$!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")]
    [InlineData("$tsmq$pbkdf2-sha256$v1$999999999$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    public void RejectsPlaintextAndMalformedHashes(string value)
    {
        Assert.False(PasswordHasher.IsHash(value));
        Assert.False(PasswordHasher.Verify(value, value));
    }

    [Fact]
    public async Task StoresHashesInvalidatesLegacyPasswordsAndSupportsReset()
    {
        var path = Path.Combine(Path.GetTempPath(), "tsmq-password-" + Guid.NewGuid().ToString("N") + ".db");
        try
        {
            var initializer = new SqliteDbInitializer(path);
            await initializer.InitializeAsync();
            using var db = initializer.CreateOrmClient();
            var users = new UserRepository(db);
            var sessions = new LoginSessionRepository(db);
            var manager = new UserManagerFromDB(users, sessions);
            var mqttUsers = new MqttAuthUserRepository(db);
            var mqtt = new MqttConnectAuthFromDB(mqttUsers);

            Assert.True(await manager.ValidatePasswordAsync("admin", "admin123"));
            await manager.RegisterAsync(new UserInfo { Account = "new-user", Password = "密码🔒" });
            var user = (await users.GetByAccountAsync("new-user"))!;
            Assert.NotEqual("密码🔒", user.Password);
            Assert.True(await manager.ValidatePasswordAsync(user.Account, "密码🔒"));
            Assert.False(await manager.ValidatePasswordAsync(user.Account, "wrong"));
            Assert.True(string.IsNullOrEmpty((await manager.GetUserByIdAsync(user.Id))!.Password));
            Assert.True(string.IsNullOrEmpty((await manager.TryGetRegisteredInfoAsync(user.Account)).userInfo!.Password));
            await manager.LoginAsync("session", new UserInfo { Account = user.Account });
            Assert.True(string.IsNullOrEmpty((await manager.TryGetLoginedInfoAsync("session")).userInfo!.Password));
            Assert.True(await manager.ChangePasswordAsync(user.Id, "changed"));
            Assert.True(await manager.ValidatePasswordAsync(user.Account, "changed"));
            Assert.False(await manager.ValidatePasswordAsync(user.Account, "密码🔒"));
            Assert.False((await manager.TryGetLoginedInfoAsync("session")).success);

            Assert.True(await mqtt.AddAsync(new MqttAuthUserInfo { UserName = "device", Password = "secret" }));
            var originalHash = (await mqttUsers.GetByUserNameAsync("device"))!.PasswordHash;
            Assert.True(PasswordHasher.IsHash(originalHash));
            Assert.True(await mqtt.ValidateAsync("device", "secret"));
            Assert.False(await mqtt.ValidateAsync("device", originalHash));
            Assert.True(await mqtt.UpdateAsync(new MqttAuthUserInfo { UserName = "device", Password = "", Remark = "备注" }));
            Assert.Equal(originalHash, (await mqttUsers.GetByUserNameAsync("device"))!.PasswordHash);
            Assert.All(await mqtt.GetAllAsync(), info => Assert.Empty(info.Password));
            Assert.True(await mqtt.UpdateAsync(new MqttAuthUserInfo { UserName = "device", Password = "new-secret" }));
            Assert.False(await mqtt.ValidateAsync("device", "secret"));
            Assert.True(await mqtt.ValidateAsync("device", "new-secret"));

            await users.InsertAsync(new UserEntity { Account = "legacy", Password = "old-password" });
            await mqttUsers.InsertAsync(new MqttAuthUserEntity { UserName = "legacy", PasswordHash = "old-password" });
            await manager.LoginAsync("legacy-session", new UserInfo { Account = "legacy" });
            Assert.False(await manager.ValidatePasswordAsync("legacy", "old-password"));
            Assert.False(await mqtt.ValidateAsync("legacy", "old-password"));
            var admin = (await users.GetByAccountAsync("admin"))!;
            admin.Password = "old-admin";
            await users.UpdateAsync(admin);

            await initializer.InitializeAsync();
            Assert.Empty((await users.GetByAccountAsync("legacy"))!.Password);
            Assert.Empty((await mqttUsers.GetByUserNameAsync("legacy"))!.PasswordHash);
            Assert.Empty((await users.GetByAccountAsync("admin"))!.Password);
            Assert.False(await manager.ValidatePasswordAsync("admin", "admin123"));
            Assert.False((await manager.TryGetLoginedInfoAsync("legacy-session")).success);
            Assert.True(await manager.ValidatePasswordAsync(user.Account, "changed"));
            Assert.True(await mqtt.ValidateAsync("device", "new-secret"));

            Assert.True(await manager.ChangePasswordAsync(admin.Id, "reset-admin"));
            Assert.True(await mqtt.UpdateAsync(new MqttAuthUserInfo { UserName = "legacy", Password = "reset-device" }));
            await initializer.InitializeAsync();
            Assert.True(await manager.ValidatePasswordAsync("admin", "reset-admin"));
            Assert.True(await mqtt.ValidateAsync("legacy", "reset-device"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(path);
        }
    }
}

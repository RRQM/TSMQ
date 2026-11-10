// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TORM;
using TSMQServer.Db.Sqlite.Entities;

namespace TSMQServer.Db.Sqlite.Repositories;

/// <summary>
/// MQTT 连接授权用户仓储。
/// </summary>
public class MqttAuthUserRepository : RepositoryBase<MqttAuthUserEntity>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttAuthUserRepository(OrmClient db) : base(db)
    {
    }

    /// <summary>
    /// 通过用户名获取授权用户。
    /// </summary>
    public async Task<MqttAuthUserEntity?> GetByUserNameAsync(string userName)
    {
        return await this.Db.Queryable<MqttAuthUserEntity>()
            .Where(u => u.UserName == userName)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 保存授权用户（存在则更新，不存在则插入）。
    /// </summary>
    public async Task SaveAsync(MqttAuthUserEntity entity)
    {
        await this.Db.Savable(entity)
            .ExecuteAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 删除授权用户。
    /// </summary>
    public async Task<bool> DeleteAsync(string userName)
    {
        return await this.Db.Deletable<MqttAuthUserEntity>()
            .Where(u => u.UserName == userName)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }
}

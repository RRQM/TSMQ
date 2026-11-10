using System.Text.Json;
using Microsoft.Data.Sqlite;
using TSMQServer.Core;
using TSMQServer.Core.Json;

namespace TSMQServer.Db.Sqlite;

/// <summary>SQLite 主题消息持久化存储。</summary>
public sealed class SqliteMqttMessageStore(string connectionString) : IMqttMessageStore
{
    /// <summary>初始化消息表及索引。</summary>
    public void Initialize()
    {
        using var connection = this.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS MqttPersistencePolicy (Id INTEGER PRIMARY KEY, PolicyJson TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS MqttPersistedMessage (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp INTEGER NOT NULL,
                ClientId TEXT NOT NULL, UserName TEXT NOT NULL, Topic TEXT NOT NULL,
                Qos INTEGER NOT NULL, Retain INTEGER NOT NULL, Payload BLOB NOT NULL);
            CREATE INDEX IF NOT EXISTS IX_MqttPersistedMessage_Time ON MqttPersistedMessage(Timestamp);
            CREATE INDEX IF NOT EXISTS IX_MqttPersistedMessage_Client ON MqttPersistedMessage(ClientId, Id);
            CREATE INDEX IF NOT EXISTS IX_MqttPersistedMessage_Topic ON MqttPersistedMessage(Topic, Id);
            """;
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<MqttPersistencePolicy> GetPolicyAsync()
    {
        using var connection = this.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT PolicyJson FROM MqttPersistencePolicy WHERE Id=1";
        var json = await command.ExecuteScalarAsync() as string;
        return json == null ? new() : JsonSerializer.Deserialize(json, TSMQServerCoreJsonSerializerContext.Default.MqttPersistencePolicy)
            ?? throw new InvalidDataException("消息持久化策略为空");
    }

    /// <inheritdoc/>
    public async Task SavePolicyAsync(MqttPersistencePolicy policy)
    {
        using var connection = this.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO MqttPersistencePolicy(Id,PolicyJson) VALUES(1,$json) ON CONFLICT(Id) DO UPDATE SET PolicyJson=excluded.PolicyJson";
        command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(policy, TSMQServerCoreJsonSerializerContext.Default.MqttPersistencePolicy));
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task AppendAsync(PersistedMqttMessage message)
    {
        using var connection = this.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO MqttPersistedMessage(Timestamp,ClientId,UserName,Topic,Qos,Retain,Payload)
            VALUES($time,$client,$user,$topic,$qos,$retain,$payload)
            """;
        command.Parameters.AddWithValue("$time", message.Timestamp.ToUniversalTime().Ticks);
        command.Parameters.AddWithValue("$client", message.ClientId);
        command.Parameters.AddWithValue("$user", message.UserName);
        command.Parameters.AddWithValue("$topic", message.Topic);
        command.Parameters.AddWithValue("$qos", message.Qos);
        command.Parameters.AddWithValue("$retain", message.Retain ? 1 : 0);
        command.Parameters.AddWithValue("$payload", Convert.FromBase64String(message.PayloadBase64));
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<QueryMqttMessagesResponse> QueryAsync(QueryMqttMessagesRequest request)
    {
        using var connection = this.Open();
        connection.CreateFunction<string, string, bool>("mqtt_matches", Matches, isDeterministic: true);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id,Timestamp,ClientId,UserName,Topic,Qos,Retain,Payload FROM MqttPersistedMessage
            WHERE ($before=0 OR Id<$before) AND ($client='' OR ClientId=$client)
              AND Timestamp >= $start AND Timestamp <= $end
              AND ($filter='' OR mqtt_matches($filter,Topic))
            ORDER BY Id DESC LIMIT $limit
            """;
        var limit = Math.Clamp(request.Limit, 1, 200);
        command.Parameters.AddWithValue("$before", request.BeforeId);
        command.Parameters.AddWithValue("$client", request.ClientId);
        command.Parameters.AddWithValue("$filter", request.TopicFilter);
        command.Parameters.AddWithValue("$start", request.StartTime?.ToUniversalTime().Ticks ?? 0);
        command.Parameters.AddWithValue("$end", request.EndTime?.ToUniversalTime().Ticks ?? DateTime.MaxValue.Ticks);
        command.Parameters.AddWithValue("$limit", limit + 1);
        var response = new QueryMqttMessagesResponse();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (response.Messages.Count == limit) { response.HasMore = true; break; }
            var payload = (byte[])reader[7];
            response.Messages.Add(new PersistedMqttMessage
            {
                Id = reader.GetInt64(0), Timestamp = new DateTime(reader.GetInt64(1), DateTimeKind.Utc),
                ClientId = reader.GetString(2), UserName = reader.GetString(3), Topic = reader.GetString(4),
                Qos = reader.GetInt32(5), Retain = reader.GetInt32(6) != 0,
                PayloadBase64 = Convert.ToBase64String(payload), PayloadLength = payload.Length
            });
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task DeleteBeforeAsync(DateTime cutoff)
    {
        using var connection = this.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM MqttPersistedMessage WHERE Timestamp < $cutoff";
        command.Parameters.AddWithValue("$cutoff", cutoff.ToUniversalTime().Ticks);
        await command.ExecuteNonQueryAsync();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    private static bool Matches(string filter, string topic)
    {
        if (topic.StartsWith('$') && !filter.StartsWith('$')) return false;
        var filters = filter.Split('/');
        var topics = topic.Split('/');
        for (var i = 0; i < filters.Length; i++)
        {
            if (filters[i] == "#") return true;
            if (i >= topics.Length || (filters[i] != "+" && filters[i] != topics[i])) return false;
        }
        return filters.Length == topics.Length;
    }
}

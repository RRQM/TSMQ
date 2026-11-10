namespace TSMQServer.Core.Imp.Services;

/// <summary>MQTT 主题过滤器与访问策略计算。</summary>
public static class MqttTopicAccess
{
    /// <summary>验证主题或主题过滤器。</summary>
    public static bool IsValid(string? topic, bool filter)
    {
        if (string.IsNullOrEmpty(topic) || topic.Contains('\0') || Encoding.UTF8.GetByteCount(topic) > 65535 || topic.StartsWith("$share/", StringComparison.Ordinal))
            return false;
        var levels = topic.Split('/');
        for (var i = 0; i < levels.Length; i++)
        {
            if (!levels[i].Contains('+') && !levels[i].Contains('#')) continue;
            if (!filter || (levels[i] != "+" && (levels[i] != "#" || i != levels.Length - 1))) return false;
        }
        return true;
    }

    /// <summary>检查主题是否匹配过滤器。</summary>
    public static bool Matches(string filter, string topic) => Covers(filter, topic);

    /// <summary>检查目标过滤器是否完全包含于允许过滤器。</summary>
    public static bool Covers(string allowed, string requested)
    {
        if (!IsValid(allowed, true) || !IsValid(requested, true)) return false;
        if (requested.StartsWith('$') && !allowed.StartsWith('$')) return false;
        var a = allowed.Split('/');
        var b = requested.Split('/');
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] == "#") return true;
            if (i >= b.Length || b[i] == "#") return false;
            if (a[i] != "+" && a[i] != b[i]) return false;
        }
        return a.Length == b.Length;
    }

    /// <summary>检查两个过滤器是否可能匹配同一主题。</summary>
    public static bool Overlaps(string left, string right)
    {
        if (!IsValid(left, true) || !IsValid(right, true)) return false;
        if ((left.StartsWith('$') && !right.StartsWith('$')) || (right.StartsWith('$') && !left.StartsWith('$'))) return false;
        var a = left.Split('/');
        var b = right.Split('/');
        var i = 0;
        for (; i < a.Length && i < b.Length; i++)
        {
            if (a[i] == "#" || b[i] == "#") return true;
            if (a[i] != "+" && b[i] != "+" && a[i] != b[i]) return false;
        }
        return a.Length == b.Length || (i < a.Length && a[i] == "#") || (i < b.Length && b[i] == "#");
    }

    /// <summary>判断发布或订阅是否被允许，拒绝规则优先。</summary>
    public static bool IsAllowed(MqttAccessPolicy policy, string userName, string clientId, string topic, string action)
    {
        if (action is not ("Publish" or "Subscribe") || !IsValid(topic, action == "Subscribe")) return false;
        if (!policy.AclEnabled) return true;
        var rules = policy.Rules.Where(r => (r.UserName.Length == 0 || r.UserName == userName)
            && (r.ClientId.Length == 0 || r.ClientId == clientId) && (r.Action == "Both" || r.Action == action));
        if (rules.Any(r => !r.Allow && (action == "Subscribe" ? Overlaps(r.TopicFilter, topic) : Matches(r.TopicFilter, topic)))) return false;
        return rules.Any(r => r.Allow && Covers(r.TopicFilter, topic));
    }
}

namespace TSMQServer.Core.Imp.Services;

// 消息并行处理；策略更新独占，等待已授权消息处理完成后再撤销连接。
internal sealed class MqttOperationGate
{
    private readonly SemaphoreSlim m_turnstile = new(1, 1);
    private readonly SemaphoreSlim m_readersLock = new(1, 1);
    private readonly SemaphoreSlim m_resource = new(1, 1);
    private int m_readers;

    public async Task<IDisposable> EnterAsync(bool exclusive = false)
    {
        await this.m_turnstile.WaitAsync();
        if (exclusive)
        {
            await this.m_resource.WaitAsync();
            return new Lease(this, true);
        }
        this.m_turnstile.Release();
        await this.m_readersLock.WaitAsync();
        try
        {
            if (this.m_readers == 0) await this.m_resource.WaitAsync();
            this.m_readers++;
        }
        finally { this.m_readersLock.Release(); }
        return new Lease(this, false);
    }

    private void Exit(bool exclusive)
    {
        if (exclusive)
        {
            this.m_resource.Release();
            this.m_turnstile.Release();
            return;
        }
        this.m_readersLock.Wait();
        try { if (--this.m_readers == 0) this.m_resource.Release(); }
        finally { this.m_readersLock.Release(); }
    }

    private sealed class Lease(MqttOperationGate owner, bool exclusive) : IDisposable
    {
        private MqttOperationGate? m_owner = owner;
        public void Dispose() => Interlocked.Exchange(ref this.m_owner, null)?.Exit(exclusive);
    }
}

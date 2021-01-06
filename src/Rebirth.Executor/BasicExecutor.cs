using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Rebirth.Executor
{
    public class BasicExecutor : BaseExecutor
    {
        private readonly ManualResetEvent m_event;
        private readonly ConcurrentQueue<Action> m_queue;

        public event Action<Exception> OnException;

        public BasicExecutor(string name) : base(name)
        {
            m_event = new ManualResetEvent(false);
            m_queue = new ConcurrentQueue<Action>();
        }
               
        protected override void Update()
        {
            m_event.Reset();

            while (m_queue.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(ex);
                }
            }

            m_event.WaitOne();
        }

        public void Enqueue(Action action)
        {
            m_queue.Enqueue(action);
            m_event.Set();
        }

        public override void Stop()
        {
            base.Stop();
            m_event.Set();
        }

        public override void Dispose()
        {
            base.Dispose();
            m_event.Dispose();
        }
    }
}

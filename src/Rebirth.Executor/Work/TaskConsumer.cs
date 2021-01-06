using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Executor
{
    public class TaskConsumer
    {
        private ConcurrentQueue<Action> m_actionQueue;

        public event Action<Exception> OnException;

        public int Count => m_actionQueue.Count;

        public TaskConsumer()
        {
            m_actionQueue = new ConcurrentQueue<Action>();
        }

        public void Update()
        {
            while (m_actionQueue.TryDequeue(out var action))
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
        }

        public void Enqueue(Action action) => m_actionQueue.Enqueue(action);
        public void Clear() => m_actionQueue.Clear();
    }
}

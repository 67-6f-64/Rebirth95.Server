using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rebirth.Tools
{
    public sealed class AwaitValue<T>
    {
        private const int WAITING = 1;

        private TaskCompletionSource<T> m_source;
        private int m_waiting;

        public bool Waiting => m_waiting != 0;

        public AwaitValue()
        {
            m_source = new TaskCompletionSource<T>();
            m_waiting = 0;
        }

        public void Set(T value)
        {
            if (m_waiting != WAITING)
                throw new InvalidOperationException();

            m_source.SetResult(value);
        }

        public Task<T> Get()
        {
            if (Interlocked.CompareExchange(ref m_waiting, WAITING, 0) == 0)
            {
                m_source = new TaskCompletionSource<T>();

                return m_source.Task;
            }

            throw new InvalidOperationException();
        }
    }
}

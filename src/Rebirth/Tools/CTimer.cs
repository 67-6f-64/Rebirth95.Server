using System;
using Rebirth.Executor;

namespace Rebirth.Tools
{
    public class CTimer : IDisposable
    {
        private readonly ServerAppThread m_thread;
        private RebirthTask m_task;

        public Action Elapsed { get; set; }
        public bool Running { get; private set; }

        public int Interval { get; set; }

        public CTimer(ServerAppThread thread)
        {
            m_thread = thread;
            Running = false;
        }

        public void Start()
        {
            if (!Running)
            {
                if (Elapsed == null)
                    throw new InvalidOperationException();

                m_task = new RebirthTask(Elapsed, Interval, true);
                
                m_thread.AddScheduledTask(m_task);

                Running = true;
            }
        }

        public void Stop()
        {
            if (Running)
            {
                m_thread.RemoveScheduledTask(m_task);
                Running = false;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}

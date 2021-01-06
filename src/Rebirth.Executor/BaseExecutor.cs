using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rebirth.Executor
{
    public abstract class BaseExecutor : IDisposable
    {
        private readonly Thread m_thread;

        public string Name { get; private set; }
        public bool Active { get; private set; }

        public BaseExecutor(string name)
        {
            Name = name;
            Active = false;

            var thread = new Thread(UpdateLoop);
            ConfigureThread(thread);

            m_thread = thread;
        }

        protected virtual void ConfigureThread(Thread thread)
        {
            thread.IsBackground = false;
            thread.Name = $"Executor-{Name}";
            thread.Priority = ThreadPriority.Highest;
        }

        protected abstract void Update();

        private void UpdateLoop()
        {
            Active = true;

            while(Active)
            {
                Update();
            }

            Active = false;
        }

        public virtual void Start()
        {
            m_thread.Start();
        }
        public virtual void Stop()
        {
            Active = false;
        }
        public virtual void Join()
        {
            m_thread.Join();
        }

        public virtual void Dispose()
        {
        }
    }
}

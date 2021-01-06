using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Rebirth.Executor
{
    public class TickExecutor : BaseExecutor
    {
        private Stopwatch m_timer;
        private long m_interval;

        public event Action OnUpdate;
        public event Action<long> OnOverload;
        public event Action<Exception> OnException;
               
        public TickExecutor(string name, int fps) : base(name)
        {
            m_timer = new Stopwatch();
            m_interval = 1000 / fps;
        }

        protected override void Update()
        {
            m_timer.Restart();

            try
            {
                OnUpdate?.Invoke();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }

            var elapsed = m_timer.ElapsedMilliseconds;

            if (elapsed >= m_interval)
            {
                if (elapsed != m_interval)
                {
                    try
                    {
                        OnOverload?.Invoke(elapsed);
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(ex);
                    }
                }
            }
            else
            {
                var elapseSpan = TimeSpan.FromMilliseconds(elapsed);
                var intervalSpan = TimeSpan.FromMilliseconds(m_interval);

                var span = intervalSpan.Subtract(elapseSpan);
                
                Thread.Sleep(span);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

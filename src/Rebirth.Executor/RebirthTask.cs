using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Executor
{
    public class RebirthTask
    {
        public Action Task { get; private set; }
        public int Delay { get; private set; }
        public bool Repeat { get; private set; }

        public bool Completed { get; private set; }
        public DateTime LastUpdate { get; private set; }

        public RebirthTask(Action action,int nDelay,bool bRepeat)
        {
            Task = action;
            Delay = nDelay;
            Repeat = bRepeat;

            Completed = false;
            SetLastUpdate(); //TODO: Ensure this called before Update, but not in ctor
        }

        public void SetLastUpdate() => LastUpdate = DateTime.Now;
        public void SetCompleted() => Completed = true;

        public void Update()
        {
            if(Completed == false)
            {
                var diff = DateTime.Now - LastUpdate;

                if(diff.TotalMilliseconds >= Delay)
                {
                    Task();
                    SetLastUpdate();

                    if(Repeat == false)
                    {
                        Completed = true;
                    }
                }
            }
        }

    }
}

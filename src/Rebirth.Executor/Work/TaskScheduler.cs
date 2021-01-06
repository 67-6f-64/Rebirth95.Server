using Rebirth.Executor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Executor
{
    public class TaskScheduler
    {
        private List<RebirthTask> m_taskList;
        private ConcurrentQueue<Action> m_actionQueue;

        public int Count => m_taskList.Count;
        public int Pending => m_actionQueue.Count;

        public TaskScheduler()
        {
            m_actionQueue = new ConcurrentQueue<Action>();
            m_taskList = new List<RebirthTask>();
        }

        public void Update()
        {
            UpdateActionQueue();
            UpdateTaskList();
        }

        private void UpdateActionQueue()
        {
            while (m_actionQueue.TryDequeue(out var action))
            {
                action();
            }
        }
        private void UpdateTaskList()
        {
            int length = m_taskList.Count - 1;

            for (int i = length; i >= 0; i--)
            {
                var task = m_taskList[i];

                task.Update();

                if (task.Completed)
                {
                    m_taskList.RemoveAt(i);
                }
            }
        }

        public void Add(RebirthTask task)
        {
            m_actionQueue.Enqueue(() =>
            {
                m_taskList.Add(task);
            });
        }
        public void Remove(RebirthTask task)
        {
            //Should i just mark it as Completed and let UpdateTaskList remove it ?
            
            m_actionQueue.Enqueue(() =>
            {
                m_taskList.Remove(task);
            });
        }

        //TODO: Impl that stops tasks first ?
        //public void Clear()
        //{
        //    m_actionQueue.Enqueue(() => m_taskList.Clear());
        //}
    }
}

using log4net;
using Rebirth.Client;
using Rebirth.Executor;
using System;

namespace Rebirth
{
    /// <summary>
    /// This task represents tasks that are "Async Producer , Sync Consumer"
    /// This is only a draft for now
    /// </summary>
    public class ServerAppTask<T> where T: ClientBase
    {
        public delegate void ServerAppTaskDelegate(int socketID);

        private T m_client;
        private ServerAppThread m_parent;

        public event Action<T> OnExecute;
        public event Action<T> OnCompleted;
        public event Action<T,Exception> OnException;

        public ServerAppTask(T client,ServerAppThread parent)
        {
            m_client = client;
            m_parent = parent;
        }

        public void Start()
        {
            System.Threading.Tasks.Task.Factory.StartNew(ExecuteProc);
        }

        private void ExecuteProc()
        {
            try
            {
                OnExecute?.Invoke(m_client);
                OnCompleted?.Invoke(m_client);
            }
            catch(Exception)
            {
                OnCompleted?.Invoke(m_client);
            }
        }

    }
    public class ServerAppThread
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof(ServerAppThread));

        //-----------------------------------------------------------------------------

        private TickExecutor    m_pExecutor;
        private TaskScheduler   m_pScheduler;

        private TaskConsumer    m_pGeneralConsumer;

        private TaskConsumer    m_pConnectConsumer;
        private TaskConsumer    m_pReceiveConsumer;
        private TaskConsumer    m_pDisconnectConsumer;

        //private TaskConsumer m_pFieldMan;
        //private TaskConsumer m_pEventMan;
        //private TaskConsumer m_pSecurityMan;
        //private TaskConsumer m_pFlushPacketMan;

        //-----------------------------------------------------------------------------

        public ServerAppThread(int dwFPS)
        {
            m_pExecutor = new TickExecutor("ServerAppThread", dwFPS);
            m_pExecutor.OnUpdate += OnUpdate;
            m_pExecutor.OnOverload += OnOverload;
            m_pExecutor.OnException += OnException;

            m_pScheduler = new TaskScheduler();

            m_pGeneralConsumer = new TaskConsumer();
            m_pGeneralConsumer.OnException += OnException;

            m_pConnectConsumer = new TaskConsumer();
            m_pConnectConsumer.OnException += OnException;

            m_pReceiveConsumer = new TaskConsumer();
            m_pReceiveConsumer.OnException += OnException;

            m_pDisconnectConsumer = new TaskConsumer();
            m_pReceiveConsumer.OnException += OnException;
        }

        //-----------------------------------------------------------------------------

        public void Start() => m_pExecutor.Start();
        public void Stop() => m_pExecutor.Stop();
        public void Join() => m_pExecutor.Join();

        //-----------------------------------------------------------------------------

        private void OnUpdate()
        {
            m_pConnectConsumer.Update();
            m_pReceiveConsumer.Update();
            m_pDisconnectConsumer.Update();

            //TODO: Verify where this is correct place for general consumer
            m_pGeneralConsumer.Update();

            m_pScheduler.Update();
        }
               
        private void OnOverload(long tElapsed)
        {
            Log.WarnFormat("[ServerAppThread] Overloaded @ {0}ms", tElapsed);
        }

        private void OnException(Exception ex)
        {
            Log.WarnFormat("[ServerAppThread] Exception:\r\n{0}", ex);
        }

        //-----------------------------------------------------------------------------

        public void AddScheduledTask(RebirthTask task) => m_pScheduler.Add(task);
        public void RemoveScheduledTask(RebirthTask task) => m_pScheduler.Remove(task);

        public void AddGeneralTask(Action action) => m_pGeneralConsumer.Enqueue(action);

        public void AddConnectTask(Action action) => m_pConnectConsumer.Enqueue(action);
        public void AddReceiveTask(Action action) => m_pReceiveConsumer.Enqueue(action);
        public void AddDisconnectTask(Action action) => m_pDisconnectConsumer.Enqueue(action);

        //-----------------------------------------------------------------------------

        private void Demo()
        {
            while (true)
            {
                //HandleAccepter()
                //HandlePackets()

                //FieldSetUpdate()
                //EventsUpdate()
                //SecurityUpdate()

                //CheckAndExecuteDelayedTask()
                //CheckAndExecuteReleatedTask()

                //FlushOutPackets()
               }
        }

    }
}

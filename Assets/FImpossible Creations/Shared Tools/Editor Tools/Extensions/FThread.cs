using System.Collections;
using System.Threading;

namespace FIMSpace.FTex
{
    /// <summary>
    /// FM: Base class for asynchronous thread operations
    /// </summary>
    public class FThread
    {
        private bool done = false;
        private bool fail = false;
        public bool Failed { get { return fail; } }
        private object handle = new object();
        private Thread fThread = null;

        public bool IsDone
        {
            get
            {
                bool temp;
                lock (handle) temp = done;
                return temp;
            }

            set
            {
                lock (handle) done = value;
            }
        }

        public virtual void Start()
        {
            fThread = new Thread(Run);
            fail = false;
            fThread.Start();
        }

        public virtual void Abort()
        {
            fThread.Abort();
        }

        protected virtual void ThreadOperations() { }

        protected virtual void OnFinished() { }

        public virtual bool Update()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Run()
        {
            try
            {
                ThreadOperations();
                IsDone = true;
            }
            catch (System.Exception exc)
            {
                fail = true;
                IsDone = true;
                UnityEngine.Debug.Log("Error occured during Async method execution, you probably used some unity core classes which are not allowed in async execution. Switch to not async executing if you really need this UnityEditor classes usage.");
                UnityEngine.Debug.LogException(exc);
            }
        }
    }
}

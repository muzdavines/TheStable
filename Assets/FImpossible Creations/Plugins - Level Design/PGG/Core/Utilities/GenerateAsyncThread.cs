using FIMSpace.FTex;
using System;

namespace FIMSpace.Generating
{
    public class GenerateAsyncThread : FThread
    {
        public CellsController Scheme { get; private set; }
        public Action ToCall;
        public bool Fail = false;

        public GenerateAsyncThread(Action toCall)
        {
            ToCall = toCall;
            Fail = false;
        }

        protected override void ThreadOperations()
        {
            try
            {
                ToCall.Invoke();
            }
            catch (Exception)
            {
                Fail = true;
                throw;
            }
        }
    }
}

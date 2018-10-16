namespace UltiDev.WebServer.Core
{
    using System;
    using System.Net;
    using System.Text;

    internal class RequestMethodTracer : IDisposable
    {
        private static bool doTrace = false;
        private static object locker = new object();
        internal string methodOrPropName = null;
        private static ushort nesting = 0;
        internal HttpListenerContext requestCtx = null;
        internal object retVal = null;
        private DateTime startTimeUTC;

        internal RequestMethodTracer(HttpListenerContext request, string format, params object[] args)
        {
            if (doTrace)
            {
                this.startTimeUTC = DateTime.UtcNow;
                this.methodOrPropName = string.Format(format, args);
                this.requestCtx = request;
                lock (locker)
                {
                    nesting = (ushort) (nesting + 1);
                    this.Trace(true, "Entering {0}", new object[] { this.methodOrPropName });
                }
            }
        }

        public void Dispose()
        {
            try
            {
                if (doTrace)
                {
                    lock (locker)
                    {
                        this.Trace(false, "Leaving {0}, RetVal: [{1}], \tExec time: {2}", new object[] { this.methodOrPropName, this.retVal, (TimeSpan) (DateTime.UtcNow - this.startTimeUTC) });
                        nesting = (ushort) (nesting - 1);
                    }
                }
            }
            finally
            {
                this.retVal = null;
            }
        }

        private void Trace(bool? enterTrueLeaveFalse, string format, params object[] args)
        {
            StringBuilder builder = new StringBuilder();
            if (enterTrueLeaveFalse.HasValue)
            {
                char ch = enterTrueLeaveFalse.Value ? '>' : '<';
                for (int i = 0; i < nesting; i++)
                {
                    builder.Append(ch);
                }
                builder.Append(' ');
            }
            RequestDispatcher.LifecycleTrace(this.requestCtx, builder.ToString() + format, args);
        }
    }
}


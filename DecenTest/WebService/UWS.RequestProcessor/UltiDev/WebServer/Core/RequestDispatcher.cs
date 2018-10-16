namespace UltiDev.WebServer.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    public abstract class RequestDispatcher
    {
        private static bool lifecycleTracing = true;
        protected HttpListener listener = new HttpListener();
        private volatile bool listenerInstanceDestroyed = false;
        internal ManualResetEvent noRequestsInProgress = new ManualResetEvent(true);
        private readonly string processUserName = null;
        public static readonly IntPtr processUserToken;
        private static int requestCount = 0;
        private int requestsBeingProcessed = 0;
        public static readonly string ServerName = string.Format("UltiDev Web Server Pro ({0})", VersionString);
        private static readonly string VersionString = typeof(RequestDispatcher).Assembly.GetName().Version.ToString();
        private volatile bool waitingForRequest = false;

        static RequestDispatcher()
        {
            if (ImpersonateSelf(2))
            {
                OpenThreadToken(GetCurrentThread(), 0xf01ff, true, ref processUserToken);
                RevertToSelf();
            }
        }

        public RequestDispatcher(bool initListeners)
        {
            this.processUserName = WindowsIdentity.GetCurrent().Name;
            if (initListeners)
            {
                this.InitListeneners();
            }
        }

        private void AcceptRequest(IAsyncResult ar)
        {
            Exception exception;
            this.waitingForRequest = false;
            HttpListenerContext context = null;
            lock (this.listener)
            {
                if (!this.listener.IsListening)
                {
                    return;
                }
                try
                {
                    context = this.listener.EndGetContext(ar);
                    if (context != null)
                    {
                        LifecycleTrace(context, "Got context from this.listener.EndGetContext(ar);", new object[0]);
                    }
                }
                catch (Exception exception1)
                {
                    exception = exception1;
                    Trace.TraceError(GetExceptionMessage(exception), new object[0]);
                }
                this.StartRequestWait();
            }
            if (context != null)
            {
                this.noRequestsInProgress.Reset();
                Interlocked.Increment(ref requestCount);
                Interlocked.Increment(ref this.requestsBeingProcessed);
                try
                {
                    this.HandleRequest(context);
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    try
                    {
                        Trace.TraceError(GetExceptionMessage(exception), new object[0]);
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    if (0 == Interlocked.Decrement(ref this.requestsBeingProcessed))
                    {
                        this.noRequestsInProgress.Set();
                    }
                    LifecycleTrace(context, "Leaving AcceptRequest();", new object[0]);
                }
            }
        }

        [DllImport("KERNEL32.DLL", SetLastError=true)]
        private static extern IntPtr GetCurrentThread();
        public static string GetExceptionMessage(Exception ex)
        {
            return ex.ToString();
        }

        protected abstract ICollection<string> GetHttpListenerUrls();
        private void HandleRequest(HttpListenerContext requestContext)
        {
            LifecycleTrace(requestContext, "Entered HandleRequest(requestContext);", new object[0]);
            this.LogRequest(requestContext);
            requestContext.Response.Headers["Server"] = ServerName;
            try
            {
                WaitHandle handle = this.ProcessRequest(requestContext);
                if (handle != null)
                {
                    handle.WaitOne();
                }
                requestContext.Response.Close();
            }
            catch
            {
                requestContext.Response.Abort();
            }
            finally
            {
                LifecycleTrace(requestContext, "Leaving HandleRequest(requestContext);", new object[0]);
            }
        }

        [DllImport("ADVAPI32.DLL", SetLastError=true)]
        private static extern bool ImpersonateSelf(int level);
        protected virtual void InitListeneners()
        {
            lock (this.listener)
            {
                foreach (string str in this.GetHttpListenerUrls())
                {
                    this.listener.Prefixes.Add(str);
                }
                this.listener.IgnoreWriteExceptions = true;
                this.listener.UnsafeConnectionNtlmAuthentication = true;
                this.listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            }
        }

        internal static void LifecycleTrace(HttpListenerContext context, string format, params object[] args)
        {
            if (lifecycleTracing)
            {
                string str = ((args == null) || (args.Length == 0)) ? format : string.Format(format, args);
                string str2 = context.Request.RequestTraceIdentifier.ToString().ToUpperInvariant();
                Trace.TraceInformation("@@@@ LT [{0}] for {2}: {1}", new object[] { str2, str, context.Request.RawUrl });
            }
        }

        private void LogRequest(HttpListenerContext requestContext)
        {
            Trace.TraceInformation("Received {0} request of size {1} for: \"{2}\".", new object[] { requestContext.Request.HttpMethod, requestContext.Request.ContentLength64, requestContext.Request.RawUrl });
        }

        [DllImport("ADVAPI32.DLL", SetLastError=true)]
        private static extern int OpenThreadToken(IntPtr thread, int access, bool openAsSelf, ref IntPtr hToken);
        protected abstract WaitHandle ProcessRequest(HttpListenerContext requestContext);
        public void Resume()
        {
            lock (this.listener)
            {
                if (this.IsSuspended)
                {
                    try
                    {
                        this.listener.Start();
                    }
                    catch (HttpListenerException exception)
                    {
                        if (exception.ErrorCode != 0x20)
                        {
                            throw;
                        }
                        StringBuilder builder = new StringBuilder();
                        foreach (string str in this.GetHttpListenerUrls())
                        {
                            builder.AppendLine(str);
                        }
                        throw new ApplicationException(string.Format(Resources.PortAlreadyInUseMsg, builder));
                    }
                    if (!this.waitingForRequest)
                    {
                        this.StartRequestWait();
                    }
                }
            }
        }

        [DllImport("ADVAPI32.DLL", SetLastError=true)]
        private static extern int RevertToSelf();
        public void Start()
        {
            this.Resume();
        }

        private IAsyncResult StartRequestWait()
        {
            lock (this.listener)
            {
                this.waitingForRequest = true;
                return this.listener.BeginGetContext(new AsyncCallback(this.AcceptRequest), null);
            }
        }

        public void Stop()
        {
            lock (this.listener)
            {
                if (!this.listenerInstanceDestroyed)
                {
                    this.listenerInstanceDestroyed = true;
                    this.Suspend();
                    this.listener.Close();
                    ((IDisposable) this.listener).Dispose();
                }
            }
        }

        public void Suspend()
        {
            lock (this.listener)
            {
                if (!this.IsSuspended)
                {
                    this.listener.Stop();
                }
            }
        }

        public bool IsSuspended
        {
            get
            {
                lock (this.listener)
                {
                    return !this.listener.IsListening;
                }
            }
        }

        protected string ProcessUserName
        {
            get
            {
                return this.processUserName;
            }
        }

        public static int RequestCount
        {
            get
            {
                return requestCount;
            }
        }

        public int RequestsInProgress
        {
            get
            {
                return this.requestsBeingProcessed;
            }
        }
    }
}


namespace UltiDev.WebServer.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Web;
    using UWS.Configuration;

    public class ApplicationRequestDispatcher : RequestDispatcher
    {
        private readonly WebAppDomain app;

        public ApplicationRequestDispatcher(WebAppDomain app) : base(false)
        {
            this.app = null;
            this.app = app;
            this.InitListeneners();
        }

        protected override ICollection<string> GetHttpListenerUrls()
        {
            return this.AppSettings.GetHttpListenerUrls();
        }

        protected override void InitListeneners()
        {
            lock (base.listener)
            {
                base.InitListeneners();
                base.listener.AuthenticationSchemes = this.AppSettings.AuthenicationMode;
                base.listener.Realm = this.AppSettings.BasicAndDigestRealm;
            }
        }

        protected override WaitHandle ProcessRequest(HttpListenerContext requestContext)
        {
            WaitHandle completionSignal;
            RequestDispatcher.LifecycleTrace(requestContext, "Entered ApplicationRequestDispatched.ProcessRequest(), about to construct AspNetRequest object.", new object[0]);
            try
            {
                AspNetRequest wr = new AspNetRequest(this.AppDomain, requestContext, base.ProcessUserName);
                RequestDispatcher.LifecycleTrace(requestContext, "ProcessRequest: AspNetRequest object instantiated.", new object[0]);
                if (!wr.InitRequest())
                {
                    RequestDispatcher.LifecycleTrace(requestContext, "ProcessRequest: AspNetRequest.InitRequest() returned false.", new object[0]);
                    return null;
                }
                if (this.app.AppSettings.BypassAppServerForStaticContent && wr.IsStaticContent())
                {
                    Trace.TraceInformation("ProcessRequest: Bypassing application server - serving \"{0}\" straight up.", new object[] { requestContext.Request.Url.AbsolutePath });
                    wr.SendStaticFile();
                    return null;
                }
                wr.completionSignal = new ManualResetEvent(false);
                RequestDispatcher.LifecycleTrace(requestContext, "ProcessRequest: Before HttpRuntime.ProcessRequest().", new object[0]);
                HttpRuntime.ProcessRequest(wr);
                RequestDispatcher.LifecycleTrace(requestContext, "ProcessRequest: HttpRuntime.ProcessRequest() completed successfully.", new object[0]);
                completionSignal = wr.completionSignal;
            }
            catch (Exception exception)
            {
                RequestDispatcher.LifecycleTrace(requestContext, "ProcessRequest: Error (unhandled exception) in HttpRuntime.ProcessRequest(): " + exception.ToString(), new object[0]);
                completionSignal = null;
            }
            finally
            {
                RequestDispatcher.LifecycleTrace(requestContext, "ProcessRequest: Leaving ApplicationRequestDispatched.ProcessRequest().", new object[0]);
            }
            return completionSignal;
        }

        public WebAppDomain AppDomain
        {
            get
            {
                return this.app;
            }
        }

        public WebAppConfigEntry AppSettings
        {
            get
            {
                return this.AppDomain.AppSettings;
            }
        }
    }
}


namespace UltiDev.WebServer.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;
    using UWS.Configuration;

    public class RedirectRequestDispatcher : RequestDispatcher
    {
        public readonly RedirectionConfigEntry RedirectionConfig;

        public RedirectRequestDispatcher(RedirectionConfigEntry redirector) : base(false)
        {
            this.RedirectionConfig = null;
            if (redirector == null)
            {
                throw new ArgumentNullException("RedirectionConfigEntry redirector");
            }
            this.RedirectionConfig = redirector;
            this.InitListeneners();
        }

        protected override ICollection<string> GetHttpListenerUrls()
        {
            return this.RedirectionConfig.GetHttpListenerUrls();
        }

        private string GetRedirectionUrl(Uri urlToRedirect, out int httpStatusCode)
        {
            httpStatusCode = 0x194;
            string redirectionUrlFor = this.RedirectionConfig.GetRedirectionUrlFor(urlToRedirect);
            if (string.IsNullOrEmpty(redirectionUrlFor))
            {
                return null;
            }
            httpStatusCode = (int) this.RedirectionConfig.RedirectionCode;
            return redirectionUrlFor;
        }

        protected override WaitHandle ProcessRequest(HttpListenerContext requestContext)
        {
            WaitHandle handle;
            RequestDispatcher.LifecycleTrace(requestContext, "Entered RedirectRequestDispatcher.ProcessRequest().", new object[0]);
            try
            {
                int num;
                string redirectionUrl = this.GetRedirectionUrl(requestContext.Request.Url, out num);
                requestContext.Response.StatusCode = num;
                if (redirectionUrl != null)
                {
                    requestContext.Response.RedirectLocation = redirectionUrl;
                }
                Trace.TraceInformation(string.Format("Redirected \"{0}\" to \"{1}\" with status code {2}", requestContext.Request.Url, redirectionUrl, num), new object[0]);
                handle = null;
            }
            finally
            {
                RequestDispatcher.LifecycleTrace(requestContext, "Leaving RedirectRequestDispatcher.ProcessRequest().", new object[0]);
            }
            return handle;
        }
    }
}


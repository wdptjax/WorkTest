namespace UltiDev.WebServer.Core
{
    using System;
    using System.Diagnostics;

    public class Trace
    {
        private static string Format(string format, params object[] args)
        {
            string str;
            if ((args == null) || (args.Length == 0))
            {
                str = format;
            }
            else
            {
                str = string.Format(format, args);
            }
            return string.Format("{0} {1}", GetTimeStamp(), str);
        }

        private static string GetTimeStamp()
        {
            return DateTime.Now.ToString("hh:mm:ss.ffff tt");
        }

        private static void RecordTrace(string prefix, string message)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine(prefix + message);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Hidden exception during tracing:\r\n" + exception.ToString());
            }
        }

        public static void TraceError(string format, params object[] args)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!  [*ERROR*]: !!!!!!!!!!!!!!!!!!!!!!!!\r\n" + Format(format, args));
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Hidden exception during tracing:\r\n" + exception.ToString());
            }
        }

        public static void TraceInformation(string format, params object[] args)
        {
            System.Diagnostics.Trace.WriteLine(Format(format, args));
        }

        public static void TraceWarning(string format, params object[] args)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("???????????????????????? [*WARNING*]: ????????????????????????\r\n" + Format(format, args));
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Hidden exception during tracing:\r\n" + exception.ToString());
            }
        }

        public static TraceListenerCollection Listeners
        {
            get
            {
                return System.Diagnostics.Trace.Listeners;
            }
        }
    }
}


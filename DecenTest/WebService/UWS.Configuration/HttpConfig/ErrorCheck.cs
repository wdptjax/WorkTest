namespace HttpConfig
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    internal static class ErrorCheck
    {
        internal static void VerifySuccess(HttpApi.Error error, string format, params object[] args)
        {
            if (error != HttpApi.Error.NO_ERROR)
            {
                format = string.Format("Error {0}. ", error) + format;
                VerifySuccess(false, format, args);
            }
        }

        internal static void VerifySuccess(bool success, string format, params object[] args)
        {
            if (!success)
            {
                string str = ((args == null) || (args.Length == 0)) ? format : string.Format(format, args);
                int error = Marshal.GetLastWin32Error();
                Exception innerException = new Win32Exception(error);
                throw new ApplicationException(string.Format("{2} while {0}: LastError = {1} (0x{1:X})", str, error, innerException.Message), innerException);
            }
        }

        internal static void VerifySuccess(int success, string format, params object[] args)
        {
            VerifySuccess(success != 0, format, args);
        }

        internal static void VerifySuccess(IntPtr pointer, string format, params object[] args)
        {
            VerifySuccess(pointer.ToInt32() != 0, format, args);
        }
    }
}


namespace UltiDev.WebServer.Core
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Guid("023B8498-88A4-4D82-867D-A1A25444E619")]
    public class ProcessEnvironment
    {
        public string ClrVersion;
        public string CommandLine;
        public bool HostProcessRetired;
        public int MaxIoThreads = -1;
        public int MaxRequestThreads = -1;
        public int ProcessBitness;
        public int ProcessID = -1;
        public DateTime? ProcessStartTime = null;
        public string ProcessUserName;
        public int UsedIoThreads = -1;
        public int UsedRequestThreads = -1;

        public override string ToString()
        {
            return string.Format("Command line: {0}\r\nProcess ID: {1}\r\nProcess user name: {2}\r\nBitness: {3}\r\nCLR version {4}\r\nProcess retired: {5}\r\nMax request threads: {6}\r\nMax I/O threads: {7}\r\n", new object[] { this.CommandLine, this.ProcessID, this.ProcessUserName, this.ProcessBitness, this.ClrVersion, this.HostProcessRetired, this.MaxRequestThreads, this.MaxIoThreads });
        }
    }
}


using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenXmlCidGenerator
{
    public class OleObjectHelper
    {
        public static bool ExportOleFile(string inputFileName, string oleOutputFileName, out StringBuilder errorMessages)
        {
            errorMessages = new StringBuilder();
            CoUninitialize();
            CoInitializeEx((IntPtr)null, 2);
            IStorage storage;
            Guid IID_IStorage = new Guid("0000000b-0000-0000-C000-000000000046");
            Guid IID_IOleObject = new Guid("00000112-0000-0000-C000-000000000046");

            var result = StgCreateStorageEx(
                oleOutputFileName,
                Convert.ToUInt32(STGM.STGM_READWRITE | STGM.STGM_SHARE_EXCLUSIVE | STGM.STGM_CREATE | STGM.STGM_TRANSACTED),
                (uint)STGFMT.STGFMT_STORAGE,
                0,
                IntPtr.Zero,
                IntPtr.Zero,
                ref IID_IStorage,
                out storage
            );

            if (result != 0)
            {
                errorMessages.AppendLine(String.Format("StgCreateStorageEx Error - result: {0}",
                    result.ToString()));
            }

            var CLSID_NULL = Guid.Empty;

            IOleObject Ole = null;
            object pOle = (object)Ole;

            result = OleCreateFromFile(
                ref CLSID_NULL,
                inputFileName,
                ref IID_IOleObject,
                (uint)OLERENDER.OLERENDER_NONE,
                IntPtr.Zero,
                null,
                storage,
                out pOle
            );

            if (result != 0)
            {
                errorMessages.AppendLine(String.Format("OleCreateFromFile Error - result: {0}",
                    result.ToString()));
            }

            storage.Commit((int)STGC.STGC_DEFAULT);

            if (pOle != null)
            {
                Marshal.FinalReleaseComObject(pOle);
                pOle = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            if (storage != null)
            {
                Marshal.FinalReleaseComObject(storage);
                storage = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return File.Exists(oleOutputFileName);
        }

        [DllImport("ole32.dll")]
        public static extern int CoInitializeEx(IntPtr pvReserved, int dwCoInit);

        [DllImport("ole32.dll")]
        public static extern void CoUninitialize();

        [DllImport("ole32.dll")]
        public static extern int OleCreateFromFile([In] ref Guid rclsid,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszFileName,
        [In] ref Guid riid, uint renderopt, [In] IntPtr pFormatEtc,
        IOleClientSite pClientSite, IStorage pStg,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppvObj);

        [DllImport("ole32.dll")]
        public static extern int StgCreateStorageEx([MarshalAs(UnmanagedType.LPWStr)] string
        pwcsName, uint grfMode, uint stgfmt, uint grfAttrs, [In] IntPtr
        pStgOptions, IntPtr reserved2, [In] ref Guid riid,
        out IStorage ppObjectOpen);

        [Flags]
        public enum STGM
        {
            STGM_READ = 0x0,
            STGM_WRITE = 0x1,
            STGM_READWRITE = 0x2,
            STGM_SHARE_DENY_NONE = 0x40,
            STGM_SHARE_DENY_READ = 0x30,
            STGM_SHARE_DENY_WRITE = 0x20,
            STGM_SHARE_EXCLUSIVE = 0x10,
            STGM_PRIORITY = 0x40000,
            STGM_CREATE = 0x1000,
            STGM_CONVERT = 0x20000,
            STGM_FAILIFTHERE = 0x0,
            STGM_DIRECT = 0x0,
            STGM_TRANSACTED = 0x10000,
            STGM_NOSCRATCH = 0x100000,
            STGM_NOSNAPSHOT = 0x200000,
            STGM_SIMPLE = 0x8000000,
            STGM_DIRECT_SWMR = 0x400000,
            STGM_DELETEONRELEASE = 0x4000000
        }
        [Flags]
        public enum STGFMT
        {
            STGFMT_STORAGE = 0,
            STGFMT_FILE = 3,
            STGFMT_ANY = 4,
            STGFMT_DOCFILE = 5
        }
    }
}


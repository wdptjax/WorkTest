namespace UltiDev.WebServer.Core
{
    using System;
    using System.Collections.Generic;

    public static class CompressionStats
    {
        public static bool ignoreLOHthreshold = false;
        private const int lohThreshold = 0x14bfb;
        private const int minContentLengthWorthCompressing = 0x1000;
        private const double minWorthCompressionRatio = 0.75;
        private static readonly Dictionary<string, CompressionData> stats = new Dictionary<string, CompressionData>();

        private static bool IsMimeTypeCompressible(ref string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
            {
                return false;
            }
            mimeType = MassageMimeType(mimeType);
            return (((mimeType.StartsWith("text/") || mimeType.StartsWith("application/x-javascript")) || mimeType.Contains("+xml")) || mimeType.StartsWith("application/json"));
        }

        private static string MassageMimeType(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
            {
                return mimeType;
            }
            return mimeType.Split(new char[] { ';', ' ' })[0].ToLowerInvariant();
        }

        public static double RecordSample(string mimeType, long uncompressedSize, long compressedSize)
        {
            CompressionData data;
            if ((uncompressedSize <= 0L) || (compressedSize <= 0L))
            {
                return 1.0;
            }
            double sample = ((double) uncompressedSize) / ((double) compressedSize);
            mimeType = MassageMimeType(mimeType);
            lock (stats)
            {
                if (!stats.TryGetValue(mimeType, out data))
                {
                    data = new CompressionData();
                    stats.Add(mimeType, data);
                }
            }
            return data.AddSample(sample);
        }

        public static CompressionAction ShouldCompress(string mimeType, long uncompressedSize)
        {
            CompressionData data;
            bool flag;
            if (uncompressedSize < 0x1000L)
            {
                return CompressionAction.DontCompress;
            }
            if (!IsMimeTypeCompressible(ref mimeType))
            {
                return CompressionAction.DontCompress;
            }
            lock (stats)
            {
                flag = stats.TryGetValue(mimeType, out data);
            }
            double num = flag ? data.GetCompressionRatio(false) : 3.0;
            long num2 = (long) ((((double) uncompressedSize) / num) + 0.5);
            if ((((double) num2) / ((double) uncompressedSize)) > 0.75)
            {
                return CompressionAction.DontCompress;
            }
            if (!(ignoreLOHthreshold || (num2 <= 0x11a2eL)))
            {
                return CompressionAction.CompressChunked;
            }
            return CompressionAction.CompressBuffered;
        }

        public enum CompressionAction
        {
            DontCompress,
            CompressBuffered,
            CompressChunked
        }
    }
}


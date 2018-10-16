namespace UltiDev.WebServer.Core
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class CompressionData : List<double>
    {
        private double compressionRatio = -1.0;
        internal const double defaultCompressionRatio = 3.0;
        private const uint sampleSize = 100;

        internal double AddSample(double sample)
        {
            lock (this)
            {
                if (base.Count >= 100L)
                {
                    base.RemoveAt(0);
                }
                base.Add(sample);
                return this.GetCompressionRatio(true);
            }
        }

        internal double GetCompressionRatio(bool forceRecalc = false)
        {
            lock (this)
            {
                if ((this.compressionRatio < 0.0) || forceRecalc)
                {
                    this.compressionRatio = 0.0;
                    foreach (double num in this)
                    {
                        this.compressionRatio += num;
                    }
                    if (base.Count > 1)
                    {
                        this.compressionRatio /= (double) base.Count;
                    }
                    if (this.compressionRatio <= 0.0)
                    {
                        this.compressionRatio = 3.0;
                    }
                }
                return this.compressionRatio;
            }
        }
    }
}


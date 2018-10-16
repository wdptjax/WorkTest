namespace UltiDev.WebServer.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public class ChunkedTransferDecoder
    {
        private List<byte[]> chunks = new List<byte[]>();
        private int chunkSize = 0;
        private ChunkPart expectedChunkPart = ChunkPart.SizeOrTrailer;
        private int index = 0;
        private Stream outStream = null;
        private ChunkPart partAfterLF = ChunkPart.Body;
        private int totalSize = 0;

        public ChunkedTransferDecoder(Stream outputStream)
        {
            this.outStream = outputStream;
        }

        private void AddChunk(byte[] chunk)
        {
            this.chunks.Add(chunk);
            this.totalSize += chunk.Length;
        }

        private void AttemptDecodeWithCr(bool writeBytes)
        {
            int byteIndex = this.GetByteIndex(13);
            if (byteIndex >= 0)
            {
                byte[] bytes = null;
                if (writeBytes)
                {
                    bytes = this.GetChunkBytesAsArray(byteIndex);
                }
                else
                {
                    this.SkipChunkBytes(byteIndex);
                }
                this.SkipChunkBytes(this.index + 1);
                if (!writeBytes)
                {
                    this.expectedChunkPart = ChunkPart.Lf;
                }
                else
                {
                    string s = Encoding.UTF8.GetString(bytes);
                    int index = s.IndexOf(';');
                    if (index >= 0)
                    {
                        s = s.Substring(0, index).Trim();
                    }
                    this.chunkSize = int.Parse(s, NumberStyles.HexNumber);
                    this.expectedChunkPart = (this.chunkSize == 0) ? ChunkPart.Nothing : ChunkPart.Lf;
                }
            }
        }

        private void AttemptDecodeWithLf()
        {
            int byteIndex = this.GetByteIndex(10);
            if (byteIndex >= 0)
            {
                this.SkipChunkBytes(byteIndex + 1);
                this.expectedChunkPart = this.partAfterLF;
            }
        }

        private byte[] ConsumeChunksTillIndex(int stopIndex, ByteConsumptionType byteConsumptionAction, out int consumedBytes)
        {
            int num = stopIndex - this.index;
            byte[] destinationArray = null;
            if (byteConsumptionAction == ByteConsumptionType.ReturnAsArray)
            {
                destinationArray = new byte[num];
            }
            int index = this.index;
            consumedBytes = 0;
            int num3 = 0;
            for (int i = 0; (i < this.chunks.Count) && (consumedBytes < num); i++)
            {
                int num5;
                byte[] sourceArray = this.chunks[i];
                int num6 = num - consumedBytes;
                int num7 = sourceArray.Length - index;
                if (num6 < num7)
                {
                    num5 = num6;
                }
                else
                {
                    num5 = num7;
                    num3++;
                }
                if (byteConsumptionAction == ByteConsumptionType.ReturnAsArray)
                {
                    Array.Copy(sourceArray, index, destinationArray, consumedBytes, num5);
                }
                else if (byteConsumptionAction == ByteConsumptionType.WriteToStream)
                {
                    this.outStream.Write(sourceArray, index, num5);
                }
                consumedBytes += num5;
                index = 0;
            }
            Debug.Assert((this.index + consumedBytes) == stopIndex);
            this.index = stopIndex;
            for (int j = 0; j < num3; j++)
            {
                this.RemoveChunk();
            }
            return destinationArray;
        }

        private int DecodeChunkBody()
        {
            this.partAfterLF = ChunkPart.SizeOrTrailer;
            if (this.chunks.Count == 0)
            {
                return 0;
            }
            int num = 0;
            while ((this.chunkSize != 0) && (this.chunks.Count > 0))
            {
                int stopIndex = this.index + this.chunkSize;
                int length = this.chunks[0].Length;
                if (stopIndex > length)
                {
                    stopIndex = length;
                }
                int num4 = stopIndex - this.index;
                int num5 = this.WriteChunkBytes(stopIndex);
                Debug.Assert(num5 == num4);
                num += num5;
                this.chunkSize -= num5;
            }
            Debug.Assert(this.chunkSize >= 0);
            Debug.Assert(this.index >= 0);
            Debug.Assert(((this.chunks.Count == 0) && (this.index == 0)) || (this.index <= this.chunks[0].Length));
            if (this.chunkSize == 0)
            {
                this.expectedChunkPart = ChunkPart.Cr;
            }
            return num;
        }

        public void Flush()
        {
            this.outStream.Flush();
        }

        private int GetByteIndex(int byteToFind)
        {
            int index = this.index;
            int num2 = this.index;
            for (int i = 0; i < this.chunks.Count; i++)
            {
                byte[] buffer = this.chunks[i];
                for (int j = index; j < buffer.Length; j++)
                {
                    if (buffer[j] == byteToFind)
                    {
                        return num2;
                    }
                    num2++;
                }
                index = 0;
            }
            return -1;
        }

        private byte[] GetChunkBytesAsArray(int stopIndex)
        {
            int num;
            return this.ConsumeChunksTillIndex(stopIndex, ByteConsumptionType.ReturnAsArray, out num);
        }

        private void RemoveChunk()
        {
            byte[] buffer = this.chunks[0];
            this.chunks.RemoveAt(0);
            this.totalSize -= buffer.Length;
            this.index -= buffer.Length;
        }

        private int SkipChunkBytes(int stopIndex)
        {
            int num;
            this.ConsumeChunksTillIndex(stopIndex, ByteConsumptionType.ThrowAway, out num);
            return num;
        }

        public int Write(byte[] data, int length)
        {
            ChunkPart expectedChunkPart;
            if (this.expectedChunkPart == ChunkPart.Nothing)
            {
                return 0;
            }
            this.AddChunk(data);
            int num = 0;
            do
            {
                expectedChunkPart = this.expectedChunkPart;
                switch (this.expectedChunkPart)
                {
                    case ChunkPart.SizeOrTrailer:
                        this.partAfterLF = ChunkPart.Body;
                        this.AttemptDecodeWithCr(true);
                        break;

                    case ChunkPart.Body:
                        num += this.DecodeChunkBody();
                        break;

                    case ChunkPart.Cr:
                        this.AttemptDecodeWithCr(false);
                        break;

                    case ChunkPart.Lf:
                        this.AttemptDecodeWithLf();
                        break;
                }
            }
            while ((expectedChunkPart != this.expectedChunkPart) && (this.expectedChunkPart != ChunkPart.Nothing));
            return num;
        }

        private int WriteChunkBytes(int stopIndex)
        {
            int num;
            this.ConsumeChunksTillIndex(stopIndex, ByteConsumptionType.WriteToStream, out num);
            return num;
        }

        private enum ByteConsumptionType
        {
            ReturnAsArray,
            WriteToStream,
            ThrowAway
        }

        private enum ChunkPart
        {
            SizeOrTrailer,
            Body,
            Cr,
            Lf,
            Nothing
        }
    }
}


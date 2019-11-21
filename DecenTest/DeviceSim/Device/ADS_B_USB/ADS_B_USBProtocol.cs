using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceSim.Device
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal class PacketHeader
    {
        [MarshalAs(UnmanagedType.I1)]
        public byte PacketStartFlag;

        [MarshalAs(UnmanagedType.I1)]
        public byte PayloadLength;

        [MarshalAs(UnmanagedType.I1)]
        public byte PacketSequence;

        [MarshalAs(UnmanagedType.I1)]
        public byte SystemID;

        [MarshalAs(UnmanagedType.I1)]
        public byte Component;

        [MarshalAs(UnmanagedType.I1)]
        public byte MessageID;

        public PacketHeader(byte[] value, int startIndex)
        {
            PacketStartFlag = value[startIndex];
            PayloadLength = value[startIndex + 1];
            PacketSequence = value[startIndex + 2];
            SystemID = value[startIndex + 3];
            Component = value[startIndex + 4];
            MessageID = value[startIndex + 5];
        }

        public PacketHeader()
        {
            PacketStartFlag = 0xfe;
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[6];
            buffer[0] = PacketStartFlag;
            buffer[1] = PayloadLength;
            buffer[2] = PacketSequence;
            buffer[3] = SystemID;
            buffer[4] = Component;
            buffer[5] = MessageID;
            return buffer;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal class TrafficReportMessage
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 ICAO_address;

        [MarshalAs(UnmanagedType.I4)]
        public Int32 lat;

        [MarshalAs(UnmanagedType.I4)]
        public Int32 lon;

        [MarshalAs(UnmanagedType.I4)]
        public Int32 altitude;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 heading;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 hor_velocity;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 ver_velocity;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 validFlags;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 squawk;

        [MarshalAs(UnmanagedType.U1)]
        public byte altitude_type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string callsign;

        [MarshalAs(UnmanagedType.U1)]
        public byte emitter_type;

        [MarshalAs(UnmanagedType.U1)]
        public byte tslc;

        public TrafficReportMessage(byte[] value, int startIndex)
        {
            ICAO_address = BitConverter.ToUInt32(value, startIndex);
            lat = BitConverter.ToInt32(value, startIndex + 4);
            lon = BitConverter.ToInt32(value, startIndex + 8);
            altitude = BitConverter.ToInt32(value, startIndex + 12);
            heading = BitConverter.ToUInt16(value, startIndex + 16);
            hor_velocity = BitConverter.ToUInt16(value, startIndex + 18);
            ver_velocity = BitConverter.ToInt16(value, startIndex + 20);
            validFlags = BitConverter.ToUInt16(value, startIndex + 22);
            squawk = BitConverter.ToUInt16(value, startIndex + 24);
            altitude_type = value[26];
            callsign = ASCIIEncoding.ASCII.GetString(value, startIndex + 27, 9).TrimEnd('\0');
            emitter_type = value[36];
            tslc = value[37];
        }

        public TrafficReportMessage()
        {

        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[Marshal.SizeOf(this)];
            byte[] data = BitConverter.GetBytes(ICAO_address);
            int offset = 0;
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(lat);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(lon);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(altitude);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(heading);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(hor_velocity);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(ver_velocity);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(validFlags);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(squawk);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            buffer[offset++] = altitude_type;
            data = ASCIIEncoding.ASCII.GetBytes(callsign);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += 9;
            buffer[offset++] = emitter_type;
            buffer[offset++] = tslc;
            return buffer;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal class OwnshipMessage
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 utcTime;

        [MarshalAs(UnmanagedType.I4)]
        public Int32 latitude;

        [MarshalAs(UnmanagedType.I4)]
        public Int32 longitude;

        [MarshalAs(UnmanagedType.I4)]
        public Int32 altPres;

        [MarshalAs(UnmanagedType.I4)]
        public Int32 altGNSS;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 accHoriz;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 accVert;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 accVel;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 velVert;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 nsVog;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 ewVog;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 state;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 squawk;

        [MarshalAs(UnmanagedType.U1)]
        public byte fixType;

        [MarshalAs(UnmanagedType.U1)]
        public byte numSats;

        [MarshalAs(UnmanagedType.U1)]
        public byte emStatus;

        [MarshalAs(UnmanagedType.U1)]
        public byte control;

        public OwnshipMessage(byte[] value, int startIndex)
        {
            utcTime = BitConverter.ToUInt32(value, startIndex);
            latitude = BitConverter.ToInt32(value, startIndex + 4);
            longitude = BitConverter.ToInt32(value, startIndex + 8);
            altPres = BitConverter.ToInt32(value, startIndex + 12);
            altGNSS = BitConverter.ToInt32(value, startIndex + 16);
            accHoriz = BitConverter.ToUInt32(value, startIndex + 20);
            accVert = BitConverter.ToUInt16(value, startIndex + 24);
            accVel = BitConverter.ToUInt16(value, startIndex + 26);
            velVert = BitConverter.ToInt16(value, startIndex + 28);
            nsVog = BitConverter.ToInt16(value, startIndex + 30);
            ewVog = BitConverter.ToInt16(value, startIndex + 32);
            state = BitConverter.ToUInt16(value, startIndex + 34);
            squawk = BitConverter.ToUInt16(value, startIndex + 36);
            fixType = value[startIndex + 38];
            numSats = value[startIndex + 39];
            emStatus = value[startIndex + 40];
            control = value[startIndex + 41];
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal class StatusMessage
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte status;

        public StatusMessage(byte[] value, int startIndex)
        {
            status = value[startIndex];
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal class MessageHeader
    {
        [MarshalAs(UnmanagedType.I1)]
        public byte MessageID;

        [MarshalAs(UnmanagedType.I1)]
        public byte PayloadLength;

        [MarshalAs(UnmanagedType.I1)]
        public byte CRC_EXTRA;

        public MessageHeader(byte[] value,int startIndex)
        {
            MessageID = value[startIndex];
            PayloadLength = value[startIndex+1];
            CRC_EXTRA = value[startIndex + 2];
        }
    }
    internal enum MessageID
    {
        DataStreamRequestMessagID = 66,
        DynamicMessageID = 202,
        StatusMessageID = 203,
        TrafficeReportMessageID = 246
    }
}

namespace UltiDev.Framework
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;

    public static class XmlSerializationHelper
    {
        public static FileStream OpenFileWithRetry(string filePath, FileMode openMode, FileAccess accessMode, FileShare sharingMode, int retryForSeconds = 5)
        {
            TimeSpan span = new TimeSpan(0, 0, retryForSeconds);
            DateTime utcNow = DateTime.UtcNow;
            do
            {
                try
                {
                    return File.Open(filePath, openMode, accessMode, sharingMode);
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
            while ((DateTime.UtcNow - utcNow) <= span);
            throw new Exception(string.Format("Failed multiple attempts to open file \"{0}\". Been trying for {1}.", filePath, span));
        }

        public static T XmlDesrializeFromFile<T>(string filePath) where T: class
        {
            T local2;
            FileInfo info = new FileInfo(filePath);
            if (!info.Exists || (info.Length == 0L))
            {
                return default(T);
            }
            using (FileStream stream = OpenFileWithRetry(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 5))
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    local2 = (T) serializer.Deserialize(reader);
                }
            }
            return local2;
        }

        public static void XmlSerializeToFile(object data, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(data.GetType());
            using (FileStream stream = OpenFileWithRetry(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 5))
            {
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8) {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize((XmlWriter) writer, data);
                stream.SetLength(stream.Position);
                stream.Flush();
            }
        }
    }
}


namespace ObjectiveXML.Utilities
{
    using System.IO;
    using System.Xml;

    public static class XmlExtensions
    {
        public static void SaveToFile(this XmlDocument doc, string filePath)
        {
            using (FileStream fs = new FileStream(filePath,
            FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                doc.Save(fs);
            }
        }

        public static void SaveToFile(this XmlDocument doc, DirectoryInfo directoryPath)
        {
            doc.SaveToFile(directoryPath.FullName);
        }

        internal static MemoryStream ToStream(this XmlReader reader)
        {
            MemoryStream ms = new MemoryStream();
            reader.CopyTo(ms);
            return ms;
        }

        internal static FileStream ToStream(this XmlReader reader, string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            reader.CopyTo(fs);
            return fs;
        }

        internal static void CopyTo(this XmlReader reader, Stream s)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CheckCharacters = false; // don't get hung up on technically invalid XML characters
            settings.CloseOutput = false; // leave the stream open
            using (XmlWriter writer = XmlWriter.Create(s, settings))
            {
                writer.WriteNode(reader, true);
            }
        }
    }
}

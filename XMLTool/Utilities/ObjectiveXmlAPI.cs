namespace ObjectiveXML.Utilities
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using Services.Serialization;

    public class ObjectiveXmlApi
    {
        public static dynamic FromFile(FileInfo file)
        {
            return new XmlProcessor().FromFile(file.FullName);
        }

        public static dynamic FromFile(string path)
        {
            return new XmlProcessor().FromFile(path);
        }

        public static dynamic FromResources(string source)
        {
            return new XmlProcessor().FromResources(source);
        }

        public static dynamic FromResources<T>(string source, Expression<Func<T, bool>> predicate)
        {
            return new XmlProcessor().FromResources(source, predicate);
        }

        public static dynamic FromStream(Stream source)
        {
            return new XmlProcessor().FromStream(source);
        }

        public static dynamic FromFile<T>(string source, Expression<Func<T, bool>> predicate)
        {
            return new XmlProcessor().FromFile(source, predicate);
        }

        public static dynamic ToXmlDocument(object obj)
        {
            return new XmlProcessor().ToXmlDocument(obj);
        }

        public static dynamic ToXmlDocument(params object[] objects)
        {
            return new XmlProcessor().ToXmlDocument(objects);
        }
    }
}

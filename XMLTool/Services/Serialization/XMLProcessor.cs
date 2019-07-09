namespace ObjectiveXML.Services.Serialization
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using System.Xml;
    using MEFLight.Attributes;
    using Models;
    using Utilities;

    internal class XmlProcessor
    {
        public XmlProcessor()
        {
            MefLightFactory.Singleton.ResolveImports(this);
        }

        [Import]
        public ModelFactory ModelFactory { get; set; }

        public dynamic FromFile<T>(string source, Expression<Func<T, bool>> predicate)
        {
            Stream stream = OpenXmlStream(source);
            return FromStream(stream, predicate);
        }

        public dynamic FromFile(string path)
        {
            Stream stream = OpenFileStream(path);
            return FromStream(stream);
        }

        public dynamic FromResources(string source)
        {
            Stream stream = OpenXmlStream(source);
            return FromStream(stream);
        }

        public dynamic FromResources<T>(string source, Expression<Func<T, bool>> predicate)
        {
            Stream stream = OpenXmlStream(source);
            return FromStream(stream, predicate);
        }

        public dynamic FromStream(Stream source)
        {
            return ModelFactory.Create(source);
        }

        public dynamic FromStream<T>(Stream source, Expression<Func<T, bool>> predicate)
        {
            return ModelFactory.Create(source, predicate);
        }

        public dynamic ToXmlDocument(params object[] objects)
        {
            return ModelFactory.Create(objects);
        }

        private Stream OpenXmlStream(string source)
        {
            using (var stringReader = new StringReader(source))
            {
                using (var reader = XmlReader.Create(stringReader))
                {
                    return reader.ToStream();
                }
            }      
        }

        private Stream OpenFileStream(string path)
        {
            using (var stream = new StreamReader(path))
            {
                using (var reader = XmlReader.Create(stream))
                {
                    return reader.ToStream();
                }
            }
        }
    }
}

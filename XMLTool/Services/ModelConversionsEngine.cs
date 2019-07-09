namespace ObjectiveXML.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Json;
    using MEFLight.Attributes;
    using Utilities;

    [Export]
    internal class ModelConversionsEngine
    {
        private JsonParser _parser;

        [ImportingConstructor]
        public ModelConversionsEngine(JsonParser parser)
        {
            _parser = parser;
        }

        public XmlDocument CreateXmlDocument(IDictionary<string, object> dataCollection)
        {
            List<XmlDocument> nodes = new List<XmlDocument>();

            dataCollection.ToList().ForEach(e =>
            {
                XmlDocument doc = _parser.SerializeToXml(e.ToDictionary());
                nodes.Add(doc);
            });

            if(nodes.Count > 1)
            {
                XmlDocument aggregateDocument = new XmlDocument();
                XmlElement root = aggregateDocument.CreateElement(nodes.First().DocumentElement.Name + 's');
                aggregateDocument.AppendChild(root);

                nodes.ForEach(d => 
                {
                    XmlNode nextElement = aggregateDocument.ImportNode(d.DocumentElement, true);
                    aggregateDocument.DocumentElement.AppendChild(nextElement);
                });

                return aggregateDocument;
            }
            else if(nodes.Count == 0)
            {
                throw new InvalidOperationException("Could not convert to XDocument, elligable Xml nodes were not found");
            }

            return nodes.FirstOrDefault();
        }

        public  XmlDocument CreateXmlDocument(KeyValuePair<string, object> singleValue)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            data.Add(singleValue);

            return CreateXmlDocument(data);
        }

        public IEnumerable<TObject> CreateCollection<TObject>(IEnumerable<dynamic> dataCollection)
             where TObject : class
        {
            List<TObject> collection = new List<TObject>();

            IDictionary<string, object> unwrappedData;

            foreach (var data in dataCollection)
            {
                unwrappedData = data;

                TObject instance = CustomActivator.CreateInstanceAndPopulate<TObject>(unwrappedData);
                collection.Add(instance);
            }

            return collection;
        }

        public TObject[] CreateArray<TObject>(IEnumerable<dynamic> dataCollection)
            where TObject : class
        {
            return this.CreateCollection<TObject>(dataCollection).ToArray();
        }

        public object CreateArray(Type type, IEnumerable<dynamic> dataCollection)
        {
            return ((Func<IEnumerable<dynamic>, object>)CreateArray<object>).Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { dataCollection });
        }

        public object CreateCollection(Type type, IEnumerable<dynamic> dataCollection)
        {
            return ((Func<IEnumerable<dynamic>, object>)CreateCollection<object>).Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { dataCollection });
        }
    }
}

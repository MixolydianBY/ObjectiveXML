namespace ObjectiveXML.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Xml;
    using System.Xml.Linq;
    using MEFLight.Attributes;
    using Services.Json;
    using Utilities;

    [Export]
    internal class ModelFactory
    {
        private readonly JsonParser _parser;

        [ImportingConstructor]
        public ModelFactory(JsonParser parser)
        {
            _parser = parser;
        }

        public object Create(Stream stream)
        {
            return Create(stream, null);
        }

        public object Create<T>(Stream stream, Expression<Func<T, bool>> predicate)
        {
            IDictionary<string, object> filters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            //Convert Expression to filters for the XDocument node
            if (predicate.Body is BinaryExpression be)
            {
                string key = (string)be.Left.GetMemberIdentity();
                object value = be.Right.GetMemberIdentity();

                filters.Add(key, value);
            }
   
            return Create(stream, filters);
        }

        private object Create(Stream stream, IDictionary<string, object> filters)
        {          
            stream.Position = 0; //Make sure we are at the beginning
            IDictionary<string, object> collectiveData = default(IDictionary<string, object>);
            object result = default(object);

            //Accessing elements in series
            using (var reader = XmlReader.Create(stream))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        //element processing
                        var element = XElement.ReadFrom(reader) as XElement;

                        if (filters != null)
                        {
                            if(!ApplyFilters(element, filters))
                                continue;
                        }                      

                        //Element deserialization
                        if(result == null)
                        {
                            result = _parser.ToDynamicModel(element);
                            collectiveData = result.As<dynamic>();
                            continue;
                        }

                        //Collect and store objects
                        IDictionary<string, object> nextResult = (dynamic) _parser.ToDynamicModel(element);
                        collectiveData.AddRange(nextResult);
                    }
                }

                reader.Close();
            }

            return result;
        }

        public object Create(params object[] parts)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            int count = 0;

            foreach (var part in parts)
            {
                string key = $"{part.GetType().Name}_{count}";
                data.Add(key, part);
                count++;
            }
            
            return _parser.ToSerializationDynamicModel(data);
        }

        private bool ApplyFilters(XElement element, IDictionary<string, object> filters)
        {
            bool flag = false;

            foreach (var filter in filters)
            {
                //attributes check
                var attributes = element.Attributes().Where(a => a.Name.ToString().Equals(filter.Key, StringComparison.OrdinalIgnoreCase)).ToList();

                if (attributes.Count > 0)
                {
                    var flag1 = attributes.Any(a => a.Value == filter.Value.ToString());
                    if (flag1)
                    {
                        flag = true;
                        continue;
                    }               
                }

                //Elements check
                var elements = element.Descendants().Where(e => e.Name.ToString().Equals(filter.Key, StringComparison.OrdinalIgnoreCase)).ToList();

                if (elements.Count > 0)
                {
                    flag = elements.Any(e => e.Value ==  filter.Value.ToString());
                }
            }

            return flag;
        }
    }
}

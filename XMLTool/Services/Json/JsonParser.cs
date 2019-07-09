namespace ObjectiveXML.Services.Json
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml;
    using System.Xml.Linq;
    using Converters;
    using MEFLight.Attributes;
    using Models;
    using Newtonsoft.Json;
    using Utilities;

    [Export]
    internal class JsonParser
    {
        private readonly JsonSerializerSettings _jsonDeserializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new JsonConverterCollection { new XmlToObjectConverter() }
        };

        private readonly JsonSerializerSettings _objSerializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new JsonConverterCollection { new ObjectToXmlConverter() }
        };

        public object ToDynamicModel(string jsonString)
        {
            if (!jsonString.IsValidJson(out var token))
            {
                dynamic obj = new ExpandoObject();
                obj.Value = jsonString;
                return obj;
            }

            return JsonConvert.DeserializeObject<DynamicModel>(token.ToString(), _jsonDeserializationSettings);
        }

        public object ToDynamicModel(XObject obj)
        {
            string jString = JsonConvert.SerializeXNode(obj);
            return ToDynamicModel(jString);
        }

        public object ToSerializationDynamicModel(object obj)
        {
            string jString = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<DynamicSerializationModel>(jString, _objSerializationSettings);
        }

        public XmlDocument SerializeToXml(IDictionary<string, object> data)
        {
            string jString = JsonConvert.SerializeObject(data, new JsonConverter[1] { new PreXmlConverter()});
            return JsonConvert.DeserializeXmlNode(jString);
        }

        public string SerializeToJString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}

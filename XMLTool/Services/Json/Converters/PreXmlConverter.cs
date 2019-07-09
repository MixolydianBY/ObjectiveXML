namespace ObjectiveXML.Services.Json.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PreXmlConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable).IsAssignableFrom(objectType) && objectType.IsGenericType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IDictionary<string, object> data = (IDictionary<string, object>)value;

            data = data.ToDictionary(e =>
            {
                int index = e.Key.IndexOf('_');
                string nodeName = e.Key.Substring(0, index);
                return nodeName;
            }, i => i.Value);

            string jString = JsonConvert.SerializeObject(data);
            JToken token = JToken.Parse(jString);

            writer.WriteToken(token.CreateReader());
        }
    }
}

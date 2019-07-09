namespace ObjectiveXML.Services.Json.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PreserveRootConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsClass;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            data.Add(value.GetType().Name, value);

            string jString = JsonConvert.SerializeObject(data);
            JToken token = JToken.Parse(jString);

            writer.WriteToken(token.CreateReader());
        }
    }
}

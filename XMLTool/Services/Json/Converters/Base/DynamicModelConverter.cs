namespace ObjectiveXML.Services.Json.Converters.Base
{
    using System;
    using System.Dynamic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public abstract class DynamicModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DynamicObject).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
            var token = JToken.Parse(value.ToString());
            writer.WriteToken(token.CreateReader());
        }
    }
}

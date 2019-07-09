namespace ObjectiveXML.Services.Json.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Base;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ObjectToXmlConverter : DynamicModelConverter
    {

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token is JArray)
            {
                IDictionary<string, object> data = new Dictionary<string, object>();

                token.Children().Where(x => x.Children<JProperty>().Any())
                    .Select(x => x.Children())
                    .SelectMany(x => x.OfType<JProperty>())
                    .ToList().ForEach(c => data.Add(c.Name, c.Value));


                var temp = JsonConvert.SerializeObject(data);
                return JsonConvert.DeserializeObject<DynamicModel>(temp);
            }
            else
            {
                return token.ToObject(objectType);
            }
        }
    }
}

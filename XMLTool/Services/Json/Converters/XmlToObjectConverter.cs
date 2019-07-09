namespace ObjectiveXML.Services.Json.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Base;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Utilities;

    public class XmlToObjectConverter : DynamicModelConverter
    {   
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token.Type == JTokenType.Object)
            {
                token.SelectTokens("$..#text")
                     .Concat(token.SelectTokens("$..?xml"))
                     .ToList()
                     .ForEach(t => t.Parent.Remove());

               ((JObject)token).Descendants().OfType<JProperty>().Where(w => w.Name.Contains("@"))
                   .ToList().ForEach(e =>
                   {
                       JProperty mainProp = (JProperty)e.Ancestors()?.FirstOrDefault(x => x.First().Equals(e))?.Parent;
                       e.Rename(e.Name.Replace("@", ""));

                       if (mainProp != null 
                           && mainProp.Root.Type != JTokenType.Array 
                           && mainProp.Root.First().Equals(mainProp))
                       {
                           mainProp.Rename($"{mainProp.Name}_{e.Value}");
                       }

                   });
            }

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

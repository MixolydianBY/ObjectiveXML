namespace ObjectiveXML.Tests.Entities
{
    using Newtonsoft.Json;
    using Services.Json.Converters;

    public class Book
    {
        [JsonProperty("@number")]
        public int Number { get; set; }

        [JsonConverter(typeof(PreserveRootConverter))]
        public Author[] Authors { get; set; }

        public Publisher Publisher { get; set; }

        public int Pages { get; set; }

        public long Isbn { get; set; }

        public string Annotation { get; set; }
    }
}

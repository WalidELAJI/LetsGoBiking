namespace CsharpServer.Suggestions
{
    public class SuggestionDetails
    {
        [Newtonsoft.Json.JsonProperty("display_name")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("lon")]
        public string Longitude { get; set; }

        [Newtonsoft.Json.JsonProperty("lat")]
        public string Latitude { get; set; }

    }
}

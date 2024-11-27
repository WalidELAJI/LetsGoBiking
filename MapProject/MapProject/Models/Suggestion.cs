using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapProject.Models
{
    public class Suggestion
    {
        [Newtonsoft.Json.JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [Newtonsoft.Json.JsonProperty("lat")]
        public string Latitude { get; set; }

        [Newtonsoft.Json.JsonProperty("lon")]
        public string Longitude { get; set; }
    }
}

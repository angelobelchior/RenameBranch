using System.Text.Json.Serialization;
using System.Linq;

namespace RenameBranch.GitHub.Models
{
    public class Branch
    {

        [JsonPropertyName("ref")]
        public string Ref { get; set; }        

        [JsonPropertyName("object")]
        public BranchObject Object { get; set; }

        public string Name => this.Ref?.Split("/").LastOrDefault();

        public class BranchObject
        {
            [JsonPropertyName("sha")]
            public string SHA { get; set; }
        }
    }
}
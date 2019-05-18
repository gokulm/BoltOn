using Newtonsoft.Json;

namespace BoltOn.Data
{
    public class BaseCosmosEntity : BaseEntity<string>
    {
        [JsonIgnore]
        public object RequestOptions { get; set; }

        [JsonIgnore]
        public object FeedOptions { get; set; }
    }
}

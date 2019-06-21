using BoltOn.Data;
using Newtonsoft.Json;

namespace BoltOn.Samples.Domain.Entities
{
    public class Grade : BaseEntity<string>
    {
        [JsonProperty("studentId")] //if you are using any property in cosmosdb query make sure propertyname matches exactly as it is in stored in document collection. 
        public int StudentId { get; set; }
        public string CourseName { get; set; }
        public int Year { get; set; }
        public string Score { get; set; }
    }
}

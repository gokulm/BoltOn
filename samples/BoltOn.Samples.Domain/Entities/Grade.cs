using BoltOn.Data;
using Newtonsoft.Json;

namespace BoltOn.Samples.Domain.Entities
{
    public class Grade : BaseEntity<string>
    {
        //while using any property in cosmosdb query make sure propertyname matches exactly as it is in stored in document collection. 
        [JsonProperty("studentId")]
        public int StudentId { get; set; }
        public string CourseName { get; set; }
        public int Year { get; set; }
        public string Score { get; set; }
    }
}

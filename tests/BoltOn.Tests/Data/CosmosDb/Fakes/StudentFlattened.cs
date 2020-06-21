using System;
using BoltOn.Cqrs;
using Newtonsoft.Json;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
{
    public class StudentFlattened : BaseCqrsEntity
    {
        [JsonProperty("id")]
        public override Guid Id { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("studentType")]
        public string StudentType { get; set; }

        [JsonProperty("studentTypeId")]
        public int StudentTypeId { get; set; }
    }
}

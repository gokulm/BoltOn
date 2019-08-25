using System;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using Newtonsoft.Json;

namespace BoltOn.Tests.Other
{
    public class CollegeDbContext : BaseCosmosDbContext<CollegeDbContext>
    {
        public CollegeDbContext(CosmosDbContextOptions<CollegeDbContext> options) : base(options)
        {
        }
    }

    public class Grade : BaseEntity<string>
    {
        //while using any property in cosmosdb query make sure propertyname matches exactly as it is in stored in document collection.
        [JsonProperty("id")]
        public override string Id { get; set; }
        [JsonProperty("studentId")]
        public int StudentId { get; set; }
        public string CourseName { get; set; }
        public int Year { get; set; }
        public string Score { get; set; }
    }
}

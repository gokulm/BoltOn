namespace BoltOn.Tests.Other
{
	public static class IntegrationTestHelper
	{
		public static bool IsSqlRunning { get; set; }
		public static bool IsRabbitMqRunning { get; set; }
		public static bool IsSqlServer { get; set; }
		public static bool IsCosmosDbServer { get; set; }
		public static bool IsSeedData { get; set; }
		public static bool IsRedisCache { get; set; }
		public static bool IsElasticsearchServer { get; set; }
		public static bool IsSeedElasticsearch { get; set; }
	}
}

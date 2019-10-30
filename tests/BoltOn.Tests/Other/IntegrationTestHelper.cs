namespace BoltOn.Tests.Other
{
	public static class IntegrationTestHelper
	{
		public static bool IsSqlRunning { get; set; } = false;
		public static bool IsRabbitMqRunning { get; set; } = false;
		public static bool IsSqlServer { get; set; }
		public static bool IsSeedData { get; set; } = true;
	}
}

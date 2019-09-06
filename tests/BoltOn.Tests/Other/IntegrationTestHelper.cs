namespace BoltOn.Tests.Other
{
	public static class IntegrationTestHelper
	{
		public static bool IsSqlRunning { get; set; } = false;
		public static bool IsRabbitMqRunning { get; set; } = false;
	}
}

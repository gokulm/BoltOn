using System.Collections.Generic;

namespace BoltOn.Tests.Mediator
{
    public class MediatorTestHelper
    {
        public static List<string> LoggerStatements { get; set; } = new List<string>();
        public static bool IsClearMiddlewares { get; set; }
        public static bool IsCustomizeIsolationLevel { get; set; }
		public static bool IsSeedData { get; set; } = true;
	}
}

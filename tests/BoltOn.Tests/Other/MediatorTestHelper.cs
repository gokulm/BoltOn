using System.Collections.Generic;

namespace BoltOn.Tests.Other
{
	public class MediatorTestHelper
    {
        public static List<string> LoggerStatements { get; set; } = new List<string>();
        public static bool IsClearInterceptors { get; set; }
        public static bool IsCustomizeIsolationLevel { get; set; }
		public static bool IsSeedData { get; set; } = true;
		public static bool IsSqlServer { get; set; } 
	}
}

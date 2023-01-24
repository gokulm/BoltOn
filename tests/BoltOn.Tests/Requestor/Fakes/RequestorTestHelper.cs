using System.Collections.Generic;

namespace BoltOn.Tests.Requestor.Fakes
{
	public class RequestorTestHelper
    {
        public static List<string> LoggerStatements { get; set; } = new List<string>();
        public static bool IsCustomizeIsolationLevel { get; set; }
	}
}

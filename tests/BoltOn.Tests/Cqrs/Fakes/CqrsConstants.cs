using System;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public static class CqrsConstants
    {
        public static Guid EventId = Guid.Parse("42bc65b2-f8a6-4371-9906-e7641d9ae9cb");
        public static Guid Event2Id = Guid.Parse("1e5f4d8d-5a3c-478c-a2f5-6bbb43c39a4a");
		public static Guid Event3Id = Guid.Parse("8228f234-af13-4695-90eb-4c2472882d7f");
		public const string AlreadyProcessedEventId = "90f7f995-c930-4f2c-9621-7c3763a4df1d";
        public static Guid EntityId = Guid.Parse("b33cac30-5595-4ada-97dd-f5f7c35c0f4c");
    }
}

using System;
using BoltOn.Cache;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Cache.Fakes
{
	public class TestRequest : IRequest<string>, ICacheResponse
	{
		public string CacheKey => "TestKey";

		public TimeSpan? AbsoluteExpiration => null;
	}

	public class TestClearCacheRequest : IRequest<string>, IClearCachedResponse
	{
		public string CacheKey => "TestKey";
	}
}

BoltOn Cache is just an adapter for .NET Core's `IDistributedCache`.

In order to use it, you have to do the following:

* Install **BoltOn.Cache** NuGet package in the host project.
* Call `BoltOnCacheModule()` in your startup's BoltOn() method. 
* Install the appropriate NuGet package depending on the `IDistributedCache` implementations that .NET Core provides - Memory, Redis and SQL Server. Use the appropriate extension methods like `AddDistributedMemoryCache` or `AddDistributedSqlServerCache` or `AddStackExchangeRedisCache` to configure the underlying cache. 
* Finally, inject [IBoltOnCache](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cache/IBoltOnCache.cs) whereever you want in your application.

Example:

    var serviceCollection = new ServiceCollection();
    serviceCollection.BoltOn(b =>
    {
        b.BoltOnCacheModule();
    });

    serviceCollection.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost:6379";
    });

ICacheResponse and IClearCachedResponse
---------------------------------------
The BoltOnCacheModule comes with 2 other marker interfaces - [ICacheResponse](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cache/ICacheResponse.cs) and [IClearCachedResponse](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cache/IClearCachedResponse.cs) to cache and clear [Requestor](../requestor)'s response automatically. It's done using [CacheResponseInterceptor](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cache/CacheResponseInterceptor.cs). 

If the request implements [ICacheResponse](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cache/ICacheResponse.cs), the cache is checked before entering the pipeline, and if the response exists in the cache, the response is returned without executing the request's handler. If the response is not in the cache, the handler is executed and the response is cached before returning it, so that subsequent requests will be served by the cache. 

Example:

    public class GetAllStudentsRequest : IQuery<IEnumerable<StudentDto>>, ICacheResponse
	{
		public string CacheKey => "Students";

		public TimeSpan? SlidingExpiration => TimeSpan.FromHours(2);
	}

If the request implements [IClearCachedResponse](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cache/IClearCachedResponse.cs), the cache is cleared before returning the response, and thus subsequent requests will enter the pipeline. 

Example:

    public class CreateStudentRequest : IRequest<Student>, IClearCachedResponse
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public int StudentTypeId { get; set; }

		public string CacheKey => "Students";
	}

**Note:**
<br />
[BoltOnCache](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Cache/BoltOnCache.cs) uses .NET Core's built-in serializer (System.Text.Json) for serializing cache values to byte array. You can switch the serializer by writing your own implemenation for [IBoltOnCacheSerializer](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Cache/BoltOnCacheSerializer.cs) and registering the implementation while bootstrapping your application.

Check out .NET Core's IDistributedCache documentation to know more about the other configurations and usage.
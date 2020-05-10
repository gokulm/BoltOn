BoltOn Cache is just an adapter for .NET Core's `IDistributedCache`.

In order to use it, you have to do the following:

* Install **BoltOn.Cache** NuGet package in the host project and the other projects where [IBoltOnCache](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cache/IBoltOnCache.cs) interface will be used.
* Call `BoltOnCache()` in your startup's BoltOn() method. 
* Install the appropriate NuGet package depending on the `IDistributedCache` implementations that .NET Core provides - Memory, Redis and SQL Server. Use the appropriate extension methods like `AddDistributedMemoryCache` or `AddDistributedSqlServerCache` or `AddStackExchangeRedisCache` to configure the underlying cache. 
* Finally, inject `IBoltOnCache` whereever you want in your application.

Example:

    var serviceCollection = new ServiceCollection();
    serviceCollection.BoltOn(b =>
    {
        b.BoltOnCache();
    });

    serviceCollection.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost:6379";
    });

**Note:**

[BoltOnCache](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Cache/BoltOnCache.cs) uses .NET Core's built-in serializer (System.Text.Json) for serializing cache values to byte array. You can switch the serializer by writing your own implemenation for [IBoltOnCacheSerializer](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Cache/BoltOnCacheSerializer.cs) and registering the implementation while bootstrapping your application.

Check out .NET Core's IDistributedCache documentation to know more about the other configurations and usage.
```csharp
services.AddSqliteCache(cachePath);
		services.AddFusionCacheSystemTextJsonSerializer();
		services.AddFusionCache()
			.WithSerializer(sp => sp.GetRequiredService<IFusionCacheSerializer>())
			.WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>())
			.AsHybridCache();
		return services;
```
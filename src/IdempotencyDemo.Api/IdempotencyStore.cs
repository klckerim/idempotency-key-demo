using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IdempotencyDemo.Api;

public sealed record CachedResult(int StatusCode, string JsonBody, string BodyHash, DateTimeOffset CreatedAt);


public sealed class IdempotencyStore
{
    private readonly ConcurrentDictionary<string, CachedResult> _store = new();

    public bool TryGet(string key, out CachedResult result) => _store.TryGetValue(key, out result!);

    public bool TryPut(string key, CachedResult result) => _store.TryAdd(key, result);

    public static string ComputeBodyHash<T>(T body)
    {
        
        // demo amaçlıdır. production için farklı yaklaşımlarla hash üretimi yapılmalı
        var json = JsonSerializer.Serialize(body);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }
}

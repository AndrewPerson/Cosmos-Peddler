using System.Collections.Concurrent;

namespace CosmosPeddler;

public class CacheDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue> where TKey : notnull
{
    public bool Complete { get; set; }
}
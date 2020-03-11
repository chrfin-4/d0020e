using System;
using System.Collections.Generic;

// Note that the Map methods do not mutate the dictionary - they return a brand
// new instance.
// The other methods that return a dict mutate the dict instance - they do
// NOT return a new dict. (The same (modified) instance is returned for
// convenience to allow chaining.)
public static class DictExtension
{

    // Same as dict[k] = v, but returns the dictionary.
    public static Dictionary<K,V> Put<K,V>(this Dictionary<K,V> dict, K k, V v)
    {
        dict[k] = v;
        return dict;
    }

    /*
    // Return dict[k] if present, or v otherwise.
    public static V GetValueOrDefault<K,V>(this Dictionary<K,V> dict, K k, V v)
    {
        if (dict.ContainsKey(k))
            return dict[k];
        return v;
    }

    // Return dict[k] if present, or alt[k] otherwise.
    public static V GetValueOrDefault<K,V>(this Dictionary<K,V> dict, K k,
            Dictionary<K,V> alt)
    {
        if (dict.ContainsKey(k))
            return dict[k];
        return alt[k];
    }
    */

    // Return dict[k] if present, or v otherwise.
    public static V GetValueOrDefault<K,V>(this Dictionary<K,V> dict, K k, V v)
        => dict.ContainsKey(k) ? dict[k] : v;

    // Return dict[k] if present, or alt[k] otherwise.
    public static V GetValueOrDefault<K,V>(this Dictionary<K,V> dict, K k,
            Dictionary<K,V> alt)
        => dict.ContainsKey(k) ? dict[k] : alt[k];

    // Set dict[k] = v if not already present. Then return dict.
    public static Dictionary<K,V> SetIfMissing<K,V>(this Dictionary<K,V> dict,
            K k, V v)
    {
        if (!dict.ContainsKey(k))
            dict[k] = v;
        return dict;
    }

    // Set dict[k] = alt[k] if not already present. Then return dict.
    public static Dictionary<K,V> SetIfMissing<K,V>(this Dictionary<K,V> dict,
            K k, Dictionary<K,V> alt)
    {
        if (!dict.ContainsKey(k))
            dict[k] = alt[k];
        return dict;
    }

    // Add all key-value pairs from other to dict. Then return dict.
    // If replace is set to false, keys that exist in dict are ignored
    // (existing mappings not updated).
    public static Dictionary<K,V> MergeWith<K,V>(this Dictionary<K,V> dict,
            Dictionary<K,V> other, bool replace=true)
    {
        foreach (KeyValuePair<K,V> p in other)
            if (replace || !dict.ContainsKey(p.Key))
                dict.Put(p.Key, p.Value);
        return dict;
    }

    // ((k -> v), (k -> r)) -> (r -> v)
    public static Dictionary<R,V> MapKeys<K,V,R>(this Dictionary<K,V> dict, Func<K,R> f)
        => dict.Map(f, v => v);

    // ((k -> v), ((k,v) -> r)) -> (r -> v)
    public static Dictionary<R,V> MapKeys<K,V,R>(this Dictionary<K,V> dict, Func<K,V,R> f2)
        => dict.Map(f2, (k,v) => v);

    // ((k -> v), (v -> r)) -> (k -> r)
    public static Dictionary<K,R> MapValues<K,V,R>(this Dictionary<K,V> dict, Func<V,R> f)
        => dict.Map(k => k, f);

    // ((k -> v), ((k,v) -> r)) -> (k -> r)
    public static Dictionary<K,R> MapValues<K,V,R>(this Dictionary<K,V> dict, Func<K,V,R> f2)
        => dict.Map((k,v) => k, f2);

    // ((k -> v), (k -> k2), (v -> v2)) -> (k2 -> v2)
    public static Dictionary<K2,V2> Map<K,V,K2,V2>(this Dictionary<K,V> dict, Func<K,K2> keyFun, Func<V,V2> valFun)
        => dict.Map((k,v) => keyFun(k), (k,v) => valFun(v));

    // ((k -> v), ((k,v) -> k2), ((k,v) -> v2)) -> (k2 -> v2)
    public static Dictionary<K2,V2> Map<K,V,K2,V2>(this Dictionary<K,V> dict, Func<K,V,K2> keyFun2, Func<K,V,V2> valFun2)
    {
        Dictionary<K2,V2> result = new Dictionary<K2,V2>(dict.Count);
        foreach (KeyValuePair<K,V> p in dict)
            result.Add(keyFun2(p.Key, p.Value), valFun2(p.Key, p.Value));
        return result;
    }

    public static Dictionary<K,V> ForEachKey<K,V>(this Dictionary<K,V> dict, Action<K> f)
    {
        foreach (K k in dict.Keys)
            f(k);
        return dict;
    }

    public static Dictionary<K,V> ForEachValue<K,V>(this Dictionary<K,V> dict, Action<V> f)
    {
        foreach (V v in dict.Values)
            f(v);
        return dict;
    }

}

public static class ListExtension
{

    public static List<R> Map<T,R>(this List<T> list, Func<T,R> f)
    {
        List<R> result = new List<R>(list.Count);
        foreach (T t in list)
            result.Add(f(t));
        return result;
    }

    public static List<T> Filter<T>(this List<T> list, Func<T,bool> f)
    {
        List<T> result = new List<T>();
        foreach (T t in list)
            if (f(t))
              result.Add(t);
        return result;
    }

    public static T Reduce<T>(this List<T> list, Func<T,T,T> f, T id)
    {
        T result = id;
        foreach (T t in list)
            result = f(result, t);
        return result;
    }

    public static List<T> ToSorted<T>(this List<T> list)
    {
        list.Sort();
        return list;
    }

    public static List<T> ToSorted<T,R>(this List<T> list, Func<T,R> fieldAccessor) where R : IComparable<R>
    {
        Comparison<T> cmp = (T t1, T t2) => fieldAccessor(t1).CompareTo(fieldAccessor(t2));
        list.Sort(cmp);
        return list;
    }

    public static Dictionary<K,V> ToDict<K,V,T>(this List<T> list, Func<T,K> keyFun, Func<T,V> valFun)
    {
        Dictionary<K,V> dict = new Dictionary<K,V>(list.Count);
        foreach (T t in list)
            dict.Add(keyFun(t), valFun(t));
        return dict;
    }

}

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization.Formatters.Binary;

public class Serial
{

    public static byte[] SerializableToByteArray<T>(T obj)
    {
        BinaryFormatter binary = new BinaryFormatter();
        using(var ms = new MemoryStream()) {
          binary.Serialize(ms, obj);
          return ms.ToArray();
        }
    }

    public static T DeserializeByteArray<T>(byte[] bytes)
    {
        BinaryFormatter binary = new BinaryFormatter();
        using(var ms = new MemoryStream(bytes)) {
          return (T) binary.Deserialize(ms);
        }
    }

    // Works with built-in types K and V (or other types that have the
    // DataContract attribute).
    // (k -> v) -> j
    public static string DictToJSON<K,V>(Dictionary<K,V> obj) =>
        ToJSON<Dictionary<K,V>>(obj);

    // Works with built-in types K and V (or other types that have the
    // DataContract attribute).
    // j -> (k -> v)
    public static Dictionary<K,V> DictFromJSON<K,V>(string json) =>
        FromJSON<Dictionary<K,V>>(json);

    // For K and V that do not have the DataContract attribute and therefore
    // require an explicit conversion function.
    // (k -> v) -> j
    public static string DictToJSON<K,V>(Dictionary<K,V> dict, Func<K,string> keyToString, Func<V,string> valToString)
        => DictToJSON(dict.Map(keyToString, valToString));

    // For K and V that do not have the DataContract attribute and therefore
    // require an explicit conversion function.
    // j -> (k -> v)
    public static Dictionary<K,V> DictFromJSON<K,V>(string json, Func<string,K> key, Func<string,V> val)
        => FromJSON<Dictionary<string,string>>(json).Map(key, val);

    // (s -> s) -> j
    public static string DictToJSON(Dictionary<string,string> obj) =>
        ToJSON(obj);

    // j -> (s -> s)
    public static Dictionary<string,string> DictFromJSON(string json) =>
        FromJSON<Dictionary<string,string>>(json);

    public static void DictToJSONFile(Dictionary<string,string> obj, string file) =>
        File.WriteAllText(file, DictToJSON(obj));

    //public static Dictionary<string,string> DictFromJSONFile(string file) =>
    // f -> (k -> v)
    public static Dictionary<K,V> DictFromJSONFile<K,V>(string file) =>
        Util.IsFile(file) ? DictFromJSON<K,V>(File.ReadAllText(file)) : new Dictionary<K,V>();

    // (t,f) -> IO
    public static void ToJSONFile<T>(T obj, string file) =>
        File.WriteAllText(file, ToJSON<T>(obj));

    // Low level. Converts any built-in type T (or any other with a
    // DataContract attribute) to a JSON string.
    // t -> j
    public static string ToJSON<T>(T obj)
    {
        DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
        MemoryStream ms = new MemoryStream();
        js.WriteObject(ms, obj);
        ms.Position = 0;
        StreamReader sr = new StreamReader(ms);
        return sr.ReadToEnd();
    }

    // Low level. For Ts *without* the DataContract attribute.
    // t -> j
    public static string ToJSON<T>(T obj, Func<T,string> toString)
        => ToJSON<string>(toString(obj));

    // Low level. For Ts *without* the DataContract attribute.
    // t -> j
    public static string ListToJSON<T>(List<T> list, Func<T,string> toString)
        => ToJSON<List<string>>(list.Map(toString));

    // f -> t
    public static T FromJSONFile<T>(string file) =>
        FromJSON<T>(File.ReadAllText(file));

    // Low level. Imports any built-in type T (or any other with a
    // DataContract attribute) from a JSON string.
    // j -> t
    public static T FromJSON<T>(string json)
    {
        DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
        MemoryStream inStream = new MemoryStream(Encoding.Unicode.GetBytes(json));
        return (T) js.ReadObject(inStream);
    }

    // Low level. For Ts *without* the DataContract attribute.
    // j -> t
    public static T FromJSON<T>(string json, Func<string,T> fromString)
        => fromString(json);

    // Low level. For Ts *without* the DataContract attribute.
    // j -> t
    public static List<T> ListFromJSON<T>(string json, Func<string,T> fromString)
        => FromJSON<List<string>>(json).Map(fromString);

}

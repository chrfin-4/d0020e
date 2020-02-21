using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

public class Util
{

    public static byte[] ComputeSHA256(byte[] bytes)
    {
        SHA256 sha = SHA256.Create();
        return sha.ComputeHash(bytes);
    }

    public static byte[] ComputeSHA256(string absolutePath)
    {
        SHA256 sha = SHA256.Create();
        FileStream fileStream = File.OpenRead(absolutePath);
        return sha.ComputeHash(fileStream);
    }

    public static string BytesToHex(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder(bytes.Length);
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("X"));
        }
        return sb.ToString();
    }

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

}

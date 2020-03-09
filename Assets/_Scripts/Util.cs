using System;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;

public class Util
{

    //*
    public static byte[] ZipAsset(ArtMetaData meta)
    {
        // Either use ZipDirectory to zip all files in the directory if 3D,
        // or ZipFile to zip a single image file if 2D.
        if (meta.IsPainting)
            return ZipFile(meta.AbsolutePath);
        else if (meta.IsSculpture)
            return ZipDirectory(meta.AbsolutePath);
        else
            throw new ArgumentException("Invalid meta data with checksum " + meta.Checksum.ToString());
    }
    //*/

    // Zip one (or more) explicitly specified file(s).
    // Note that the path is stripped from files. So the zip will contain
    // entries consisting only of the (relative) file name.
    public static byte[] ZipFile(params string[] paths)
        => ZipFiles(paths);

    // Note that the path is stripped off. So the zip will contain entries
    // consisting only of the (relative) file name.
    public static byte[] ZipFiles(IEnumerable<string> paths)
    {
        MemoryStream zip = new MemoryStream();
        ZipArchive archive = new ZipArchive(zip, ZipArchiveMode.Create);
        foreach (string file in paths)
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
        archive.Dispose();
        return zip.ToArray();
    }

    // Note that the path is stripped from files. So the zip will contain
    // entries consisting only of the (relative) file name.
    // If path is a file, uses the path to the containing directory.
    // So `some/path/to/a/file.ext` is treated as `some/path/to/a/`
    public static byte[] ZipDirectory(string path)
        => ZipFiles(Directory.EnumerateFiles(GetDirectory(path)));

    // Zip all files in path. Optionally prefix the entries with a directory
    // name.

    //*
    public static void UnzipAsset(byte[] zip, ArtMetaData meta)
        => Unzip(zip, GetDirectory(meta.AbsolutePath));
    //*/

    public static void Unzip(byte[] zip, string dir)
    {
        ZipArchive archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
        archive.ExtractToDirectory(dir);
    }

    public static byte[] ComputeSHA256(byte[] bytes)
    {
        SHA256 sha = SHA256.Create();
        return sha.ComputeHash(bytes);
    }

    // Works both for files and directories.
    public static byte[] ComputeSHA256(string path)
    {
        if (IsFile(path))
            return ComputeSHA256OfFile(path);
        else if (IsDirectory(path))
            return ComputeSHA256OfDirectory(path);
        else
            throw new ArgumentException("Path is neither a file nor a directory: " + path);
    }

    public static byte[] ComputeSHA256OfFile(string path)
    {
        SHA256 sha = SHA256.Create();
        using (FileStream fileStream = File.OpenRead(path)) {
          return sha.ComputeHash(fileStream);
        }
    }

    // Computes the SHA256 hash of a directory by hashing all its files (and
    // subdirectories), concatenating the hashes in sorted
    // order, and then hashing the concatenation.
    // If path is a file, computes the hash of the containing directory.
    // So `some/path/to/a/file.ext` is treated as `some/path/to/a/`
    public static byte[] ComputeSHA256OfDirectory(string path)
    {
        path = GetDirectory(path);
        List<string> shas = new List<string>();
        foreach (string dir in Directory.EnumerateDirectories(path))
            shas.Add(BytesToHex(ComputeSHA256OfDirectory(dir)));
        foreach (string file in Directory.EnumerateFiles(path))
            shas.Add(BytesToHex(ComputeSHA256OfFile(file)));
        shas.Sort();
        return ComputeSHA256(HexToBytes(String.Concat(shas)));
    }

    public static bool IsDirectory(string path) => Directory.Exists(path);

    public static bool IsFile(string path) => File.Exists(path);

    public static string GetDirectory(string path)
        => IsDirectory(path) ? path : Path.GetDirectoryName(path);

    // []           -> ""
    // [0x01]       -> "01"
    // [0x12]       -> "12"
    // [0x01, 0x02] -> "0102"
    public static string BytesToHex(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder(bytes.Length);
        foreach (byte b in bytes)
            sb.Append(b.ToString("X2"));
        return sb.ToString();
    }

    // ""     -> []
    // "1"    -> [0x00]
    // "12"   -> [0x12]
    // "123"  -> [0x01, 0x23]
    // "0102" -> [0x01, 0x02]
    // Forces the hex string to have even length by prefixing it with "0" if
    // necessary.
    public static byte[] HexToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
            hex = "0" + hex;
        byte[] bytes = new byte[hex.Length/2];
        for (int i = 0; i < hex.Length; i+=2)
        {
            byte highNibble = (byte) (hexCharToByte(hex[i]) << 4);
            byte lowNibble = hexCharToByte(hex[i+1]);
            bytes[i/2] = (byte) (highNibble | lowNibble);
        }
        return bytes;
    }

    private static byte hexCharToByte(char c)
    {
        switch (c)
        {
            case '0': return 0;
            case '1': return 1;
            case '2': return 2;
            case '3': return 3;
            case '4': return 4;
            case '5': return 5;
            case '6': return 6;
            case '7': return 7;
            case '8': return 8;
            case '9': return 9;
            case 'A': return 10;
            case 'B': return 11;
            case 'C': return 12;
            case 'D': return 13;
            case 'E': return 14;
            case 'F': return 15;
            case 'a': return 10;
            case 'b': return 11;
            case 'c': return 12;
            case 'd': return 13;
            case 'e': return 14;
            case 'f': return 15;
            default: throw new ArgumentOutOfRangeException("c", c.ToString() + " is not a hex digit");
        }
    }

}

using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.IO;
using UnityEngine;

// Uses SHA256
[Serializable]
public class Checksum
{
    private byte[] bytes;

    private Checksum(byte[] bytes)
    {
        this.bytes = bytes;
    }

    public static Checksum Compute(byte[] bytes) =>
        new Checksum(Util.ComputeSHA256(bytes));

    public static Checksum Compute(string absolutePath, ArtType type = ArtType.Painting) =>
        type.IsPainting() ? Compute2D(absolutePath) : Compute3D(absolutePath);

    public static Checksum Compute2D(string absolutePath) =>
        new Checksum(Util.ComputeSHA256(absolutePath));

    public static Checksum Compute3D(string absolutePath) =>
        new Checksum(Util.ComputeSHA256OfDirectory(absolutePath));

    public static Checksum FromString(string hex) =>
        new Checksum(Util.HexToBytes(hex));

    public override string ToString() => Util.BytesToHex(bytes);

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (!this.GetType().Equals(obj.GetType()))
            return false;
        Checksum other = (Checksum) obj;
        if (bytes.Length != other.bytes.Length)
            return false;
        for (int i = 0; i < bytes.Length; i++)
            if (bytes[i] != other.bytes[i])
                return false;
        return true;
    }

    public override int GetHashCode() =>
        (bytes[0] << 3) + (bytes[1] << 2) + (bytes[2] << 1) + bytes[3];

}

/* Room settings consist of settings specifically for that room, such as:
 * - max visitors allowed
 * - room password
 * - admin username (and password?)
 * - allow laser pointer?
 * - allow VoIP?
 *
 * and also per-slot settings, each of wich contain settings such as:
 * - id (name or number uniquely identifying this slot)
 * - whether to allow live update
 * - who is allowed to modify
 * - art piece to display in this slot
 *   (might consist of multiple pieces of meta data or possibly only the checksum identifying the art)
 */
public class RoomSettings
{
    public int MaxVisitors { get; set; }
    public string GalleryName { get; set; }
    public Dictionary<int,SlotSettings> Slots { get; }

    public RoomSettings() : this(new Dictionary<int,SlotSettings>())
    {}

    private RoomSettings(Dictionary<int,SlotSettings> settings)
    {
        Slots = settings;
    }

    public ArtManifest GetManifest()
    {
        List<SlotSettings> tmp = new List<SlotSettings>();
        foreach (SlotSettings slot in Slots.Values)
            tmp.Add(slot.WithMeta(slot.MetaData.MakeRelativePath()));
        return new ArtManifest(tmp);
    }

    public string ToJSON()
    {
        Dictionary<string,string> tmp = new Dictionary<string,string>();
        tmp.Add("MaxVisitors", MaxVisitors.ToString());
        tmp.Add("GalleryName", GalleryName);
        Dictionary<string,string> slots = new Dictionary<string,string>();
        foreach (KeyValuePair<int,SlotSettings> kv in Slots)
            slots.Add(kv.Key.ToString(), kv.Value.ToJSON());
        tmp.Add("Slots", Serial.DictToJSON(slots));
        return Serial.DictToJSON(tmp);
    }

    public static RoomSettings FromJSON(string json)
    {
        Dictionary<string,string> tmp = Serial.DictFromJSON(json);
        int maxVisitors = Int32.Parse(tmp["MaxVisitors"]);
        string name = tmp["GalleryName"];
        Dictionary<int,SlotSettings> slots = Serial.DictFromJSON(tmp["Slots"], Int32.Parse, SlotSettings.FromJSON);
        return new RoomSettings(slots);
    }

}

[Serializable]
public class SlotSettings
{
    public SlotSettings(int slot, ArtMetaData metaData)
    {
        SlotNumber = slot;
        MetaData = metaData;
    }

    public SlotSettings WithMeta(ArtMetaData metaData) =>
        new SlotSettings(SlotNumber, metaData);

    public int SlotNumber { get; }
    public ArtMetaData MetaData { get; }
    // Rotation/Orientation?
    // ...

    public string ToJSON()
    {
        Dictionary<string,string> tmp = new Dictionary<string,string>();
        tmp.Add("SlotNumber", SlotNumber.ToString());
        tmp.Add("Checksum", MetaData.Checksum.ToString());
        return Serial.DictToJSON(tmp);
    }

    public static SlotSettings FromJSON(string json)
        => FromJSON(json, AppSettings.GetAppSettings().ArtRegistry);

    public static SlotSettings FromJSON(string json, ArtRegistry reg)
    {
        Dictionary<string,string> tmp = Serial.DictFromJSON(json);
        int slotNumber = Int32.Parse(tmp["SlotNumber"]);
        ArtMetaData metaData = reg.Get(Checksum.FromString(tmp["Checksum"]));
        return new SlotSettings(slotNumber, metaData);
    }

}

// TODO: Maybe add some "about" field?
// Note that the path needs to be translated/converted when exported to a
// visitor.
[Serializable]
public class ArtMetaData
{
    public ArtMetaData(string title, string artist, string absolutePath, ArtType type) :
        this(title, artist, absolutePath, type, Checksum.Compute(absolutePath, type))
    { }

    private ArtMetaData(string title, string artist, string absolutePath, ArtType type, Checksum checksum)
    {
        Checksum = checksum;
        ArtTitle = title;
        ArtistName = artist;
        AbsolutePath = absolutePath;
        Type = type;
    }

    public Checksum Checksum { get; }
    public string ArtTitle { get; }
    public string ArtistName { get; }
    // Note: AbsolutePath is a slightly misleading name.
    // When exporting to visitors, the "absolute path" is only the file name.
    public string AbsolutePath { get; }
    public string FileName { get => Path.GetFileName(AbsolutePath); }
    public ArtType Type { get; }
    public bool IsPainting { get => Type == ArtType.Painting; }
    public bool IsSculpture { get => Type == ArtType.Sculpture; }

    // TODO: rename?
    public ArtMetaData MakeRelativePath()
        => new ArtMetaData(ArtTitle, ArtistName, FileName, Type, Checksum);

    // TODO: rename?
    // For importing by visitors.
    // Uses the filename AND a directory named after the checksum.
    // Before the call, AbsolutePath should be just the file name (`file.ext`).
    // After the call, AbsolutePath will be `root/CHECKSUM/file.ext`.
    // TODO: should check to make sure the current path is not already absolute.
    public ArtMetaData MakeAbsolutePath(string root)
    {
        string absolutePath = Path.Combine(root, Checksum.ToString(), FileName);
        return new ArtMetaData(ArtTitle, ArtistName, absolutePath, Type, Checksum);
    }

    private Dictionary<string,string> ToDict()
    {
        Dictionary<string,string> tmp = new Dictionary<string,string>();
        tmp.Add("ArtTitle", ArtTitle);
        tmp.Add("ArtistName", ArtistName);
        tmp.Add("Path", AbsolutePath);
        tmp.Add("ArtType", Type.ToString());
        tmp.Add("Checksum", Checksum.ToString());
        return tmp;
    }

    private static ArtMetaData FromDict(Dictionary<string,string> dict)
    {
        string title = dict["ArtTitle"];
        string name = dict["ArtistName"];
        string path = dict["Path"];
        ArtType type = dict["ArtType"].ToArtType();
        Checksum checksum = Checksum.FromString(dict["Checksum"]);
        return new ArtMetaData(title, name, path, type, checksum);
    }

    public string ToJSON()
        => Serial.DictToJSON(ToDict());

    public static ArtMetaData FromJSON(string json)
        => FromDict(Serial.DictFromJSON(json));

}

// Global registry. Contains metadata for all known (local) art assets.
// Paths must(?) be absolute, but only locally. So when transferring to
// visitors, filename should be made absolute relative to some local
// root. In other words, the absolute path will be different for different
// clients.
[Serializable]
public class ArtRegistry
{

    private string RegFile { get; }
    private Dictionary<Checksum,ArtMetaData> Metadata { get; }

    private ArtRegistry()
    {
        Metadata = new Dictionary<Checksum,ArtMetaData>();
    }

    private ArtRegistry(string file)
    {
        Metadata = new Dictionary<Checksum,ArtMetaData>();
        RegFile = file;
    }

    private ArtRegistry(Dictionary<Checksum,ArtMetaData> meta)
    {
        Metadata = meta;
    }

    public static ArtRegistry Load(string file)
    {
        ArtRegistry reg = new ArtRegistry(file);
        if (!Util.IsFile(file))
            return reg;
        string json = File.ReadAllText(file);
        Serial.ListFromJSON<ArtMetaData>(json, ArtMetaData.FromJSON).ForEach(art => reg.AddArt(art));
        return reg;
    }

    // XXX: both deprecated?
    //public static ArtRegistry GetArtRegistry() => instance;
    public static ArtRegistry GetEmptyArtRegistry() => new ArtRegistry();

    public ArtMetaData Get(Checksum checksum) => Metadata[checksum];
    public bool HasArt(Checksum checksum) => Metadata.ContainsKey(checksum);
    public List<ArtMetaData> GetAll() =>
        new List<ArtMetaData>(Metadata.Values);

    // TODO: automatically write to file when a change is made?
    // Can also be used to update meta data of existing art.
    // As long as the asset is the same so that the checksum matches, the new
    // metadata will replace the old.
    public ArtRegistry AddArt(ArtMetaData metaData)
    {
        Metadata.Put(metaData.Checksum, metaData);
        //return Save(); // TODO: ???
        return this;
    }

    public ArtRegistry Add2DArt(string title, string artistName, string absolutePath)
        => AddArt(title, artistName, absolutePath, ArtType.Painting);

    public ArtRegistry Add3DArt(string title, string artistName, string absolutePath)
        => AddArt(title, artistName, absolutePath, ArtType.Sculpture);

    public ArtRegistry AddArt(string title, string artistName, string absolutePath, ArtType type)
        => AddArt(new ArtMetaData(title, artistName, absolutePath, type));

    public ArtRegistry Save()
        => Save(RegFile);

    public ArtRegistry Save(string file)
    {
        string json = Serial.ListToJSON<ArtMetaData>(GetAll(), art => art.ToJSON());
        File.WriteAllText(RegFile, json);
        return this;
    }

}

// Global settings.
// Should include the path to the art registry?
// Or maybe the art registry itself (as an object)?
// Maybe hold a directory where assets are downloaded to?
// Relative file paths are relative to the root. (Absolute paths are absolute.)
public class AppSettings
{

    public static readonly string RootPathKey = "root";
    public static readonly string SettingsKey = "settings";
    public static readonly string GalleriesKey = "galleries";
    public static readonly string ArtRegPathKey = "artregfile";
    public static readonly string DownloadsPathKey = "downloads";

    private static AppSettings instance = null;
    private static readonly Dictionary<string, string> defaults = new Dictionary<string,string>();

    private Dictionary<string, string> settings = new Dictionary<string,string>();

    // XXX: should not be public. Either
    //      - Use a property,
    //      - extra methods for accessing the dictionary, or
    //      - a separate gallery registry class.
    public Dictionary<string,RoomSettings> galleries;

    // Set up default values.
    static AppSettings()
    {
        defaults.Add(ArtRegPathKey, "art.json");
        defaults.Add(DownloadsPathKey, "downloads");
        defaults.Add(SettingsKey, "settings.json");
        defaults.Add(GalleriesKey, "galleries.json");
        // Application.persistentDataPath is not available at compile time
        // and cannot be set as a default here. Must be added in the
        // constructor instead.
        // TODO: However, that also means the AppSettings constructor cannot
        // be used in a static context. So maybe don't add it to defaults at all?
    }

    private AppSettings()
    {
        defaults.SetIfMissing(RootPathKey, Application.persistentDataPath);
    }

    public static AppSettings GetAppSettings()
    {
        if (instance != null)
            return instance;
        instance = new AppSettings();
        return instance.Load();
    }

    private AppSettings Save()
    {
        ArtRegistry.Save();
        return this;
    }

    private AppSettings Load()
    {
        LoadSettings();
        LoadArtRegistry();
        LoadGalleries();
        return this;
    }

    private AppSettings LoadSettings()
    {
        string jsonFile = settingsFile();
        Dictionary<string,string> dict = Serial.DictFromJSONFile<string,string>(jsonFile);
        settings.MergeWith(dict);
        // TODO: Could point to a different root and/or settings file?
        //        Recursively load again?
        return this;
    }

    private AppSettings LoadArtRegistry()
    {
        ArtRegistry = ArtRegistry.Load(registryFile());
        return this;
    }

    public AppSettings AddGallery(RoomSettings gallery)
    {
        galleries[gallery.GalleryName] = gallery;
        return this;
    }

    private AppSettings LoadGalleries()
    {
        string json = File.ReadAllText(galleriesFile());
        galleries = Serial.DictFromJSON(json, s => s, RoomSettings.FromJSON);
        return this;
    }

    private AppSettings SaveGalleries()
    {
        string json = Serial.DictToJSON(galleries, s => s, s => s.ToJSON());
        File.WriteAllText(galleriesFile(), json);
        return this;
    }

    private string registryFile()
        => getAbsolute(GetString(ArtRegPathKey));

    private string settingsFile()
        => getAbsolute(GetString(SettingsKey));

    private string galleriesFile()
        => getAbsolute(GetString(GalleriesKey));

    private string GetString(string key) =>
        settings.GetValueOrDefault(key, defaults);

    private int GetInt(string key) =>
        Int32.Parse(GetString(key));

    private AppSettings SetString(string key, string val)
    {
        settings[key] = val;
        return this;
    }

    public AppSettings SetInt(string key, int val) =>
        SetString(key, val.ToString());

    public ArtRegistry ArtRegistry { get; set; }

    private string getPath(string key)
        => getAbsolute(GetString(key));

    public string RootPath
    {
        get => getPath(RootPathKey);
    }

    public string DownloadPath
    {
        get => getPath(DownloadsPathKey);
    }

    // TODO: *IF* it should be possible to point to a different root directory,
    // that needs to be taken into account here.
    private string getAbsolute(string path)
    {
        if (Path.IsPathRooted(path))
            return path;
        else
            return Path.Combine(RootPath, path);
    }

}

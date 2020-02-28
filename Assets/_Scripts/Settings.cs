using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.IO;

public enum ArtType
{
    Painting, Sculpture
}

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

    public static Checksum Compute(string absolutePath) =>
        new Checksum(Util.ComputeSHA256(absolutePath));

    public override string ToString() => Util.BytesToHex(bytes);

    public override bool Equals(Object obj)
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
    public int MaxVisitors { get; }
    public Dictionary<int,SlotSettings> Slots { get; }

    private RoomSettings(Dictionary<int,SlotSettings> settings)
    {
        Slots = settings;
    }

    public static RoomSettings GetTestRoomSettings()
    {
        Dictionary<int,SlotSettings> settings = new Dictionary<int,SlotSettings>();
        ArtRegistry reg = ArtRegistry.GetArtRegistry();
        int nr = 1;
        foreach (ArtMetaData meta in reg.GetAll())
        {
            settings.Add(nr, new SlotSettings(nr, meta));
            nr++;
        }
        return new RoomSettings(settings);
    }

    public ArtManifest GetManifest()
    {
        List<SlotSettings> tmp = new List<SlotSettings>();
        foreach (SlotSettings slot in Slots.Values)
        {
            tmp.Add(slot.WithMeta(slot.MetaData.MakeRelativePath()));
        }
        return new ArtManifest(tmp);
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
}

// Note that the path needs to be translated/converted when exported to a
// visitor.
[Serializable]
public class ArtMetaData
{
    public ArtMetaData(string title, string artist, string absolutePath, ArtType type) : this(title, artist, absolutePath, type, Checksum.Compute(absolutePath))
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
    // When exporting to visitors, the "absolute path" only the file name.
    public string AbsolutePath { get; }
    public string FileName { get => Path.GetFileName(AbsolutePath); }
    public ArtType Type { get; }

    // TODO: rename?
    // For export to visitors.
    // Changes the file name to the file's checksum (as a hex string).
    // Makes the filename the whole path (so strips off the prefix).
    public ArtMetaData MakeRelativePath() {
        string ext = Path.GetExtension(FileName);
        string fileName = Path.ChangeExtension(Checksum.ToString(), ext);
        return new ArtMetaData(ArtTitle, ArtistName, fileName, Type, Checksum);
    }

    // TODO: rename?
    // For importing by visitors.
    // Uses the filename (which is the checksum).
    // When called, AbsolutePath should be just the file name (hex.ext).
    public ArtMetaData MakeAbsolutePath(string root)
    {
        string absolutePath = Path.Combine(root, FileName);
        return new ArtMetaData(ArtTitle, ArtistName, absolutePath, Type, Checksum);
    }

}

// Global registry. Contains metadata for all known (local) art assets.
// Paths must(?) be absolute, but only locally. So when transferring to
// visitors, filename should be made absolute relative to some local
// root. In other words, the absolute path will be different for different
// clients.
[Serializable]
public class ArtRegistry
{
    private static ArtRegistry instance = new ArtRegistry();

    private Dictionary<Checksum,ArtMetaData> Metadata { get; }

    // XXX: Creates a hard-coded registry.
    static ArtRegistry()
    {/*
        string path = "art/";
        string artist = "Pixabay";
        string title1 = "beautiful-calm-clouds-dark-206359";
        string path1 = path + title1 + ".jpg";
        string title2 = "flight-landscape-nature-sky-36717";
        string path2 = path + title2 + ".jpg";
        instance.AddArt(title1, artist, path1, ArtType.Painting);
        instance.AddArt(title2, artist, path2, ArtType.Painting);
        string artist3 = "Philip Ackermann";
        string title3 = "scenic-view-of-mountain-1666021";
        string path3 = path + title3 + ".jpg";
        string artist4 = "Bell Co";
        string title4 = "photo-of-a-turtle-underwater-847393";
        string path4 = path + title4 + ".jpg";
        instance.AddArt(title3, artist3, path3, ArtType.Painting);
        instance.AddArt(title4, artist4, path4, ArtType.Painting);*/
    }

    private ArtRegistry()
    {
        Metadata = new Dictionary<Checksum,ArtMetaData>();
    }

    public static ArtRegistry GetArtRegistry() => instance;
    public static ArtRegistry GetEmptyArtRegistry() => new ArtRegistry();

    public ArtMetaData Get(Checksum checksum) => Metadata[checksum];
    public bool HasArt(Checksum checksum) => Metadata.ContainsKey(checksum);
    public List<ArtMetaData> GetAll() =>
        new List<ArtMetaData>(Metadata.Values);

    public ArtRegistry AddArt(ArtMetaData metaData)
    {
        Metadata.Add(metaData.Checksum, metaData);
        return this;
    }

    // TODO: automatically write to file when a change is made?
    public ArtRegistry AddArt(string title, string artistName, string absolutePath, ArtType type)
    {
        ArtMetaData metadata = new ArtMetaData(title, artistName, absolutePath, type);
        return AddArt(metadata);
    }

    // TODO: automatically write to file when a change is made?
    // TODO: should be possible to specify path?
    public void WriteToFile()
    {
    }

}

// Global settings.
// Should include the path to the art registry?
// Or maybe the art registry itself (as an object)?
// Maybe hold a directory where assets are downloaded to?
public class AppSettings
{

    private static AppSettings instance = new AppSettings();

    public static AppSettings GetAppSettings() => instance;

    public ArtRegistry ArtRegistry { get; }
    public string PathRoot { get; }

    private AppSettings()
    {
        ArtRegistry = ArtRegistry.GetArtRegistry();
        PathRoot = "/home/finkn/d0020e/";
    }

}

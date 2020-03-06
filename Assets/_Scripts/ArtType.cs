using System.Collections.Generic;

public enum ArtType
{
    Painting, Sculpture
}

// Working around limitations of enums in C#.
// Adds
//  - a ToString method to an ArtType, and
//  - a ToArtType method to a string.
public static class ArtTypeExtension
{
    private static Dictionary<ArtType,string> toString = new Dictionary<ArtType,string>();
    private static Dictionary<string,ArtType> fromString = new Dictionary<string,ArtType>();

    static ArtTypeExtension()
    {
        toString.Add(ArtType.Painting, "Painting");
        toString.Add(ArtType.Sculpture, "Sculpture");
        fromString.Add(toString[ArtType.Painting], ArtType.Painting);
        fromString.Add(toString[ArtType.Sculpture], ArtType.Sculpture);
    }

    public static string ToString(this ArtType type) => toString[type];

    public static ArtType ToArtType(this string type) => fromString[type];

    public static bool IsPainting(this ArtType type) => type == ArtType.Painting;

    public static bool IsSculpture(this ArtType type) => type == ArtType.Sculpture;

}

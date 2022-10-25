using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace b7.Xabbo.Model;

[XmlRoot("wardrobe")]
public class Wardrobe
{
    private static XmlSerializer _serializer = new XmlSerializer(typeof(Wardrobe));

    [XmlArray("figures")]
    [XmlArrayItem("figure")]
    public List<string> Figures { get; set; }

    public Wardrobe()
    {
        Figures = new List<string>();
    }

    public static Wardrobe Load(Stream stream) => (Wardrobe)(_serializer.Deserialize(stream)
        ?? throw new FormatException());

    public static Wardrobe Load(string path)
    {
        using Stream stream = File.OpenRead(path);
        return Load(stream);
    }

    public void Save(Stream stream) => _serializer.Serialize(stream, this);
    public void Save(string path)
    {
        using Stream stream = File.OpenWrite(path);
        Save(stream);
    }
}

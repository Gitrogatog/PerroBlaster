using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace ldtk;
public readonly record struct Point(int X, int Y);



[JsonSerializable(typeof(Rootobject))]
internal partial class RootobjectContext : JsonSerializerContext
{
}

public static class RootobjectReader
{
	static JsonSerializerOptions options = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	static RootobjectContext context = new RootobjectContext(options);

	public static Rootobject ReadJson(string path)
	{
        string s = File.ReadAllText(path);
		return (Rootobject)JsonSerializer.Deserialize(s, typeof(Rootobject), context);
		// return new TexturePage(new CramTextureAtlasFile(new FileInfo(path), data));
	}
}

// [JsonSerializable(typeof(CramTextureAtlasData))]
// internal partial class CramTextureAtlasDataContext : JsonSerializerContext
// {
// }

// public static class CramAtlasReader
// {
// 	static JsonSerializerOptions options = new JsonSerializerOptions
// 	{
// 		PropertyNameCaseInsensitive = true
// 	};

// 	static CramTextureAtlasDataContext context = new CramTextureAtlasDataContext(options);

// 	public static TexturePage ReadTextureAtlas(string path)
// 	{
// 		var data = (CramTextureAtlasData)JsonSerializer.Deserialize(File.ReadAllText(path), typeof(CramTextureAtlasData), context);
// 		return new TexturePage(new CramTextureAtlasFile(new FileInfo(path), data));
// 	}
// }
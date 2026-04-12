namespace ldtk;
public class Rootobject
{
    public __Header__ __header__ { get; set; }
    public string iid { get; set; }
    public string jsonVersion { get; set; }
    public int appBuildId { get; set; }
    public int nextUid { get; set; }
    public string identifierStyle { get; set; }
    public Toc[] toc { get; set; }
    public string worldLayout { get; set; }
    public int worldGridWidth { get; set; }
    public int worldGridHeight { get; set; }
    public int defaultLevelWidth { get; set; }
    public int defaultLevelHeight { get; set; }
    public int defaultPivotX { get; set; }
    public int defaultPivotY { get; set; }
    public int defaultGridSize { get; set; }
    public int defaultEntityWidth { get; set; }
    public int defaultEntityHeight { get; set; }
    public string bgColor { get; set; }
    public string defaultLevelBgColor { get; set; }
    public bool minifyJson { get; set; }
    public bool externalLevels { get; set; }
    public bool exportTiled { get; set; }
    public bool simplifiedExport { get; set; }
    public string imageExportMode { get; set; }
    public bool exportLevelBg { get; set; }
    public object pngFilePattern { get; set; }
    public bool backupOnSave { get; set; }
    public int backupLimit { get; set; }
    public object backupRelPath { get; set; }
    public string levelNamePattern { get; set; }
    public object tutorialDesc { get; set; }
    public object[] customCommands { get; set; }
    public object[] flags { get; set; }
    public Defs defs { get; set; }
    public Level[] levels { get; set; }
    public object[] worlds { get; set; }
    public string dummyWorldIid { get; set; }
}

public class __Header__
{
    public string fileType { get; set; }
    public string app { get; set; }
    public string doc { get; set; }
    public string schema { get; set; }
    public string appAuthor { get; set; }
    public string appVersion { get; set; }
    public string url { get; set; }
}

public class Defs
{
    public Layer[] layers { get; set; }
    public Entity[] entities { get; set; }
    public Tileset[] tilesets { get; set; }
    public Enum[] enums { get; set; }
    public object[] externalEnums { get; set; }
    public object[] levelFields { get; set; }
}

public class Layer
{
    public string __type { get; set; }
    public string identifier { get; set; }
    public string type { get; set; }
    public int uid { get; set; }
    public object doc { get; set; }
    public object uiColor { get; set; }
    public int gridSize { get; set; }
    public int guideGridWid { get; set; }
    public int guideGridHei { get; set; }
    public float displayOpacity { get; set; }
    public float inactiveOpacity { get; set; }
    public bool hideInList { get; set; }
    public bool hideFieldsWhenInactive { get; set; }
    public bool canSelectWhenInactive { get; set; }
    public bool renderInWorldView { get; set; }
    public int pxOffsetX { get; set; }
    public int pxOffsetY { get; set; }
    public int parallaxFactorX { get; set; }
    public int parallaxFactorY { get; set; }
    public bool parallaxScaling { get; set; }
    public object[] requiredTags { get; set; }
    public object[] excludedTags { get; set; }
    public object autoTilesKilledByOtherLayerUid { get; set; }
    public object[] uiFilterTags { get; set; }
    public bool useAsyncRender { get; set; }
    public Intgridvalue[] intGridValues { get; set; }
    public object[] intGridValuesGroups { get; set; }
    public object[] autoRuleGroups { get; set; }
    public object autoSourceLayerDefUid { get; set; }
    public int? tilesetDefUid { get; set; }
    public int tilePivotX { get; set; }
    public int tilePivotY { get; set; }
    public object biomeFieldUid { get; set; }
}

public class Intgridvalue
{
    public int value { get; set; }
    public string identifier { get; set; }
    public string color { get; set; }
    public Tile tile { get; set; }
    public int groupUid { get; set; }
}

public class Tile
{
    public int tilesetUid { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int w { get; set; }
    public int h { get; set; }
}

public class Entity
{
    public string identifier { get; set; }
    public int uid { get; set; }
    public object[] tags { get; set; }
    public bool exportToToc { get; set; }
    public bool allowOutOfBounds { get; set; }
    public object doc { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public bool resizableX { get; set; }
    public bool resizableY { get; set; }
    public int? minWidth { get; set; }
    public object maxWidth { get; set; }
    public int? minHeight { get; set; }
    public object maxHeight { get; set; }
    public bool keepAspectRatio { get; set; }
    public float tileOpacity { get; set; }
    public float fillOpacity { get; set; }
    public float lineOpacity { get; set; }
    public bool hollow { get; set; }
    public string color { get; set; }
    public string renderMode { get; set; }
    public bool showName { get; set; }
    public int? tilesetId { get; set; }
    public string tileRenderMode { get; set; }
    public Tilerect tileRect { get; set; }
    public object uiTileRect { get; set; }
    public object[] nineSliceBorders { get; set; }
    public int maxCount { get; set; }
    public string limitScope { get; set; }
    public string limitBehavior { get; set; }
    public int pivotX { get; set; }
    public int pivotY { get; set; }
    public Fielddef[] fieldDefs { get; set; }
}

public class Tilerect
{
    public int tilesetUid { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int w { get; set; }
    public int h { get; set; }
}

public class Fielddef
{
    public string identifier { get; set; }
    public object doc { get; set; }
    public string __type { get; set; }
    public int uid { get; set; }
    public string type { get; set; }
    public bool isArray { get; set; }
    public bool canBeNull { get; set; }
    public object arrayMinLength { get; set; }
    public object arrayMaxLength { get; set; }
    public string editorDisplayMode { get; set; }
    public int editorDisplayScale { get; set; }
    public string editorDisplayPos { get; set; }
    public string editorLinkStyle { get; set; }
    public object editorDisplayColor { get; set; }
    public bool editorAlwaysShow { get; set; }
    public bool editorShowInWorld { get; set; }
    public bool editorCutLongValues { get; set; }
    public object editorTextSuffix { get; set; }
    public object editorTextPrefix { get; set; }
    public bool useForSmartColor { get; set; }
    public bool exportToToc { get; set; }
    public bool searchable { get; set; }
    public object min { get; set; }
    public object max { get; set; }
    public object regex { get; set; }
    public object acceptFileTypes { get; set; }
    public object defaultOverride { get; set; }
    public object textLanguageMode { get; set; }
    public bool symmetricalRef { get; set; }
    public bool autoChainRef { get; set; }
    public bool allowOutOfLevelRef { get; set; }
    public string allowedRefs { get; set; }
    public int? allowedRefsEntityUid { get; set; }
    public object[] allowedRefTags { get; set; }
    public object tilesetUid { get; set; }
}

public class Tileset
{
    public int __cWid { get; set; }
    public int __cHei { get; set; }
    public string identifier { get; set; }
    public int uid { get; set; }
    public string relPath { get; set; }
    public object embedAtlas { get; set; }
    public int pxWid { get; set; }
    public int pxHei { get; set; }
    public int tileGridSize { get; set; }
    public int spacing { get; set; }
    public int padding { get; set; }
    public object[] tags { get; set; }
    public int? tagsSourceEnumUid { get; set; }
    public Enumtag[] enumTags { get; set; }
    public Customdata[] customData { get; set; }
    public object[] savedSelections { get; set; }
    public Cachedpixeldata cachedPixelData { get; set; }
}

public class Cachedpixeldata
{
    public string opaqueTiles { get; set; }
    public string averageColors { get; set; }
}

public class Enumtag
{
    public string enumValueId { get; set; }
    public int?[] tileIds { get; set; }
}

public class Customdata
{
    public int tileId { get; set; }
    public string data { get; set; }
}

public class Enum
{
    public string identifier { get; set; }
    public int uid { get; set; }
    public Value[] values { get; set; }
    public object iconTilesetUid { get; set; }
    public object externalRelPath { get; set; }
    public object externalFileChecksum { get; set; }
    public object[] tags { get; set; }
}

public class Value
{
    public string id { get; set; }
    public object tileRect { get; set; }
    public int color { get; set; }
}

public class Toc
{
    public string identifier { get; set; }
    public object[] instances { get; set; }
    public Instancesdata[] instancesData { get; set; }
}

public class Instancesdata
{
    public Iids iids { get; set; }
    public int worldX { get; set; }
    public int worldY { get; set; }
    public int widPx { get; set; }
    public int heiPx { get; set; }
    public Fields fields { get; set; }
}

public class Iids
{
    public string worldIid { get; set; }
    public string levelIid { get; set; }
    public string layerIid { get; set; }
    public string entityIid { get; set; }
}

public class Fields
{
}

public class Level
{
    public string identifier { get; set; }
    public string iid { get; set; }
    public int uid { get; set; }
    public int worldX { get; set; }
    public int worldY { get; set; }
    public int worldDepth { get; set; }
    public int pxWid { get; set; }
    public int pxHei { get; set; }
    public string __bgColor { get; set; }
    public object bgColor { get; set; }
    public bool useAutoIdentifier { get; set; }
    public object bgRelPath { get; set; }
    public object bgPos { get; set; }
    public float bgPivotX { get; set; }
    public float bgPivotY { get; set; }
    public string __smartColor { get; set; }
    public object __bgPos { get; set; }
    public object externalRelPath { get; set; }
    public object[] fieldInstances { get; set; }
    public Layerinstance[] layerInstances { get; set; }
    public object[] __neighbours { get; set; }
}

public class Layerinstance
{
    public string __identifier { get; set; }
    public string __type { get; set; }
    public int __cWid { get; set; }
    public int __cHei { get; set; }
    public int __gridSize { get; set; }
    public float __opacity { get; set; }
    public int __pxTotalOffsetX { get; set; }
    public int __pxTotalOffsetY { get; set; }
    public int? __tilesetDefUid { get; set; }
    public string __tilesetRelPath { get; set; }
    public string iid { get; set; }
    public int levelId { get; set; }
    public int layerDefUid { get; set; }
    public int pxOffsetX { get; set; }
    public int pxOffsetY { get; set; }
    public bool visible { get; set; }
    public object[] optionalRules { get; set; }
    public int?[] intGridCsv { get; set; }
    public object[] autoLayerTiles { get; set; }
    public int seed { get; set; }
    public object overrideTilesetUid { get; set; }
    public Gridtile[] gridTiles { get; set; }
    public Entityinstance[] entityInstances { get; set; }
}

public class Gridtile
{
    public int[] px { get; set; }
    public int[] src { get; set; }
    public int f { get; set; }
    public int t { get; set; }
    public int[] d { get; set; }
    public int a { get; set; }
}

public class Entityinstance
{
    public string __identifier { get; set; }
    public int[] __grid { get; set; }
    public int[] __pivot { get; set; }
    public object[] __tags { get; set; }
    public __Tile __tile { get; set; }
    public string __smartColor { get; set; }
    public string iid { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int defUid { get; set; }
    public int[] px { get; set; }
    public Fieldinstance[] fieldInstances { get; set; }
    public int __worldX { get; set; }
    public int __worldY { get; set; }
}

public class __Tile
{
    public int tilesetUid { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int w { get; set; }
    public int h { get; set; }
}

public class Fieldinstance
{
    public string __identifier { get; set; }
    public string __type { get; set; }
    public object __value { get; set; }
    public object __tile { get; set; }
    public int defUid { get; set; }
    public Realeditorvalue[] realEditorValues { get; set; }
}

public class Realeditorvalue
{
    public string id { get; set; }
    public string[] _params { get; set; }
}

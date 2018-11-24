---- Mapfile: C:\Users\riivo.kolka\Documents\src\Manifold_Dump_Code\Test\testmap.map
---- DATASOURCE: Bing Maps Hybrid
--DROP DATASOURCE [Bing Maps Hybrid];
CREATE DATASOURCE [Bing Maps Hybrid] (
  PROPERTY 'Source' '{ "Source": "http:\\/\\/ecn.t{switch}.tiles.virtualearth.net\\/tiles\\/h{cell}?g=516&mkt=en-us&n=z&key={key}", "SourceCache": true, "SourceSubtype": "binghybrid", "SourceUuid": "93b77480-d71f-4b78-a189-071d7e21b9e1" }',
  PROPERTY 'Type' 'imageserver'
);


---- TABLE: binghybrid
--DROP TABLE [binghybrid];
CREATE TABLE [binghybrid] (
  [Level] INT32,
  [Y] INT32,
  [X] INT32,
  [Tile] TILE,
  INDEX [Level_Y_X_x] BTREE ([Level], [Y], [X]),
  PROPERTY 'FieldCoordSystem.Tile' 'EPSG:3857,mfd:{ "LocalOffsetX": -20037508.342789244, "LocalOffsetY": -20037508.342789244, "LocalScaleX": 0.29858214173896974, "LocalScaleY": 0.29858214173896974 }',
  PROPERTY 'FieldLevel' 'Level',
  PROPERTY 'FieldTile' 'Tile',
  PROPERTY 'FieldTileSize.Tile' '[ 256, 256 ]',
  PROPERTY 'FieldTileType.Tile' 'uint8x4',
  PROPERTY 'FieldX' 'X',
  PROPERTY 'FieldY' 'Y',
  PROPERTY 'Source' 'http://ecn.t{switch}.tiles.virtualearth.net/tiles/a{cell}?g=516&mkt=en-us&n=z&key={key}',
  PROPERTY 'SourceServer' 'bingsatellite',
  PROPERTY 'Type' 'imageserver'
);


---- IMAGE: binghybrid Image
--DROP IMAGE [binghybrid Image];
CREATE IMAGE [binghybrid Image] (
  PROPERTY 'FieldLevel' 'Level',
  PROPERTY 'FieldTile' 'Tile',
  PROPERTY 'FieldX' 'X',
  PROPERTY 'FieldY' 'Y',
  PROPERTY 'LevelBase' '1',
  PROPERTY 'LevelMax' '19',
  PROPERTY 'Rect' '[ 0, 0, 134217728, 134217728 ]',
  PROPERTY 'Table' '[binghybrid]'
);


---- QUERY: Create_Inventory
--DROP QUERY [Create_Inventory];
CREATE QUERY [Create_Inventory] (
  PROPERTY 'Text' 'CREATE TABLE [Inventory] (
  [mfd_id] INT64,
  [Stock] INT32,
  [ExtraStock] INT32
    WITH [[ FUNCTION f(@x INT32) INT32 AS @x+100 END; ]]
    AS [[ f([Stock]) ]],
  [insertdate] DATETIME
    WITH [[ SCRIPT funcs ENGINE \'c#\' [[
  class Script
  {
    static System.DateTime F() { return System.DateTime.Now; }
  }
]];

FUNCTION currentdate() DATETIME AS SCRIPT INLINE funcs ENTRY \'Script.F\'; ]]
    AS [[ currentdate() ]],  
  [insertdate2] DATETIME
    WITH [[ SCRIPT funcs ENGINE \'c#\' [[
  class Script
  {
    static System.DateTime F(int a) { return System.DateTime.Now; }
  }
]];

FUNCTION currentdate(@a INT32) DATETIME AS SCRIPT INLINE funcs ENTRY \'Script.F\'; ]]
    AS [[ currentdate(1) ]],
  INDEX [mfd_id_x] BTREE ([mfd_id])
);
'
);


---- QUERY: Create_My Components
--DROP QUERY [Create_My Components];
CREATE QUERY [Create_My Components] (
  PROPERTY 'Text' 'CREATE TABLE [My Components] (
  [mfd_id] INT64,
  [Component Name] VARCHAR,
  INDEX [mfd_id_x] BTREE ([mfd_id]),
  CONSTRAINT [Ncomp]
    AS [[ (

    [Component Name] IN (SELECT [Name] FROM [mfd_root])

  ) ]]
);
'
);


---- QUERY: Create_Prices
--DROP QUERY [Create_Prices];
CREATE QUERY [Create_Prices] (
  PROPERTY 'Text' 'CREATE TABLE [Prices] (
  [mfd_id] INT64,
  [Cost] INT32,
  [Retail] INT32,
  INDEX [mfd_id_x] BTREE ([mfd_id]),
  CONSTRAINT [Profitable]
    AS [[ (
    [Retail] > [COST]*2
  ) ]]
);
'
);


---- TABLE: Inventory
--DROP TABLE [Inventory];
CREATE TABLE [Inventory] (
  [mfd_id] INT64,
  [Stock] INT32,
  [ExtraStock] INT32 AS [[ f([Stock]) ]],
  [insertdate] DATETIME AS [[ currentdate() ]],
  [insertdate2] DATETIME AS [[ currentdate(1) ]],
  INDEX [mfd_id_x] BTREE ([mfd_id])
);


---- DATASOURCE: Link_to_Code
--DROP DATASOURCE [Link_to_Code];
CREATE DATASOURCE [Link_to_Code] (
  PROPERTY 'Source' '{ "Source": "Test.cs" }',
  PROPERTY 'Type' 'cs'
);


---- MAP: Map
--DROP MAP [Map];
CREATE MAP [Map] (
  PROPERTY 'CoordSystem' '{ "Axes": "XY", "Base": "WGS 84 (EPSG:4326)", "CenterLat": 0, "CenterLon": 0, "Eccentricity": 0.08181919084262149, "MajorAxis": 6378137, "Name": "WGS 84 \\/ Pseudo-Mercator (EPSG:3857)", "System": "Pseudo Mercator", "Unit": "Meter", "UnitScale": 1, "UnitShort": "m" }',
  PROPERTY 'Item.0' '{ "Entity": "[binghybrid Image]", "Z": 2 }',
  PROPERTY 'Item.1' '{ "Entity": "[Test_Table Drawing]", "Z": 1 }',
  PROPERTY 'Item.2' '{ "Entity": "[Test_Table Labels]", "Hidden": true, "Z": 0 }',
  PROPERTY 'Item.3' '{ "Entity": "[Bing Maps Hybrid]::[Bing Maps Hybrid Image]", "Hidden": true, "Z": 3 }'
);


---- MAP: Map Lat/Lon
--DROP MAP [Map Lat/Lon];
CREATE MAP [Map Lat/Lon] (
  PROPERTY 'CoordSystem' '{ "Name": "Universal Transverse Mercator Zone 35 (N)", "System": "Transverse Mercator", "CenterLat": 0, "CenterLon": 27, "FalseEasting": 500000, "ScaleX": 0.9996, "ScaleY": 0.9996, "Axes": "XYH", "Base": "World Geodetic 1984 (WGS84)", "MajorAxis": 6378137, "Eccentricity": 0.08181919084262149, "Unit": "Meter", "UnitScale": 1, "UnitShort": "m" }',
  PROPERTY 'Folder' 'TestFolder\\Folder 2',
  PROPERTY 'Item.0' '{ "Entity": "[Test_Table Drawing]", "Z": 2 }',
  PROPERTY 'Item.2' '{ "Entity": "[binghybrid Image]", "Z": 1 }'
);


---- TABLE: mfd_meta fr-BE
--DROP TABLE [mfd_meta fr-BE];
CREATE TABLE [mfd_meta fr-BE] (
  [mfd_id] INT64,
  [Name] NVARCHAR,
  [Property] NVARCHAR,
  [Value] NVARCHAR,
  INDEX [mfd_id_x] BTREE ([mfd_id]),
  INDEX [Name_Property_x] BTREE ([Name] NOCASE, [Property] NOCASE),
  INDEX [Property_Name_x] BTREE ([Property] NOCASE, [Name] NOCASE),
  INDEX [value_x] BTREE ([Value] COLLATE 'fr-BE'),
  INDEX [value_x_2] BTREE ([Value] COLLATE 'et-EE' NOCASE NOACCENT NOSYMBOLS DESC),
  PROPERTY 'FieldItem.mfd_id' '{ "Size": 72, "Z": 0 }',
  PROPERTY 'FieldItem.Name' '{ "Size": 212, "Z": 1 }',
  PROPERTY 'FieldItem.Property' '{ "Size": 96, "Z": 2 }',
  PROPERTY 'FieldItem.Value' '{ "Size": 545, "Z": 3 }'
);


---- TABLE: My Components
--DROP TABLE [My Components];
CREATE TABLE [My Components] (
  [mfd_id] INT64,
  [Component Name] VARCHAR,
  INDEX [mfd_id_x] BTREE ([mfd_id]),
  [Ncomp] AS [[ (

    [Component Name] IN (SELECT [Name] FROM [mfd_root])

  ) ]]
);


---- TABLE: Prices
--DROP TABLE [Prices];
CREATE TABLE [Prices] (
  [mfd_id] INT64,
  [Cost] INT32,
  [Retail] INT32,
  INDEX [mfd_id_x] BTREE ([mfd_id]),
  [Profitable] AS [[ (
    [Retail] > [COST]*2
  ) ]]
);


---- DATASOURCE: Temp_Datasource
--DROP DATASOURCE [Temp_Datasource];
CREATE DATASOURCE [Temp_Datasource] (
  PROPERTY 'Type' 'manifold'
);


---- IMAGE: Test Image
--DROP IMAGE [Test Image];
CREATE IMAGE [Test Image] (
  PROPERTY 'FieldTile' 'Tile',
  PROPERTY 'FieldX' 'X',
  PROPERTY 'FieldY' 'Y',
  PROPERTY 'Rect' '[ 0, 0, 1024, 1024 ]',
  PROPERTY 'Table' '[Test Image Table]'
);


---- TABLE: Test Image Table
--DROP TABLE [Test Image Table];
CREATE TABLE [Test Image Table] (
  [mfd_id] INT64,
  [X] INT32,
  [Y] INT32,
  [Tile] TILE,
  INDEX [mfd_id_x] BTREE ([mfd_id]),
  INDEX [X_Y_Tile_x] RTREE ([X], [Y], [Tile] TILESIZE (128,128) TILETYPE UINT8),
  PROPERTY 'FieldCoordSystem.Tile' '{ "Axes": "XYH", "Base": "World Geodetic 1984 (WGS84)", "Eccentricity": 0.08181919084262149, "MajorAxis": 6378137, "Name": "Latitude \\/ Longitude", "System": "Latitude \\/ Longitude", "Unit": "Degree", "UnitLatLon": true, "UnitScale": 1, "UnitShort": "deg" }',
  PROPERTY 'FieldTileSize.Tile' '[ 128, 128 ]',
  PROPERTY 'FieldTileType.Tile' 'uint8'
);


---- COMMENTS: Test_Comments
--DROP COMMENTS [Test_Comments];
CREATE COMMENTS [Test_Comments] (
  PROPERTY 'Text' 'Ei 
kommentääri
43qy&:_E_:%YjE \\3w4y q35y 


h54wq h\\w6+ 
'
);


---- SCRIPT: Test_CS
--DROP SCRIPT [Test_CS];
CREATE SCRIPT [Test_CS] (
  PROPERTY 'Folder' 'TestFolder\\Folder 2',
  PROPERTY 'Text' '// C#

class Script
{

static Manifold.Context Manifold;
static void Main()
{
	Manifold.Application.Log("Hello, World!");
	Manifold.Application.OpenLog();
}

}
',
  PROPERTY 'Type' 'C#'
);


---- SCRIPT: Test_IPY
--DROP SCRIPT [Test_IPY];
CREATE SCRIPT [Test_IPY] (
  PROPERTY 'Folder' 'TestFolder',
  PROPERTY 'Text' '# IronPython
#
# Note: running script requires IronPython (IronPython.dll)

def Main():
	Manifold.Application.Log("Hello, World!")
	Manifold.Application.OpenLog()
',
  PROPERTY 'Type' 'IronPython'
);


---- SCRIPT: Test_JS
--DROP SCRIPT [Test_JS];
CREATE SCRIPT [Test_JS] (
  PROPERTY 'Folder' 'TestFolder',
  PROPERTY 'Text' '// JavaScript

var Main = function()
{
	Manifold.Application.Log("Hello, World!");
	Manifold.Application.Log(\'Hello, World!\');
	Manifold.Application.OpenLog();
}
',
  PROPERTY 'Type' 'JavaScript'
);


---- LAYOUT: Test_Layout
--DROP LAYOUT [Test_Layout];
CREATE LAYOUT [Test_Layout] (
  PROPERTY 'Item.0' '{ "Center": [ 0, 0 ], "Entity": "[Test_Table Drawing]", "Rect": [ 25.4, 142.3038942976356, 116.35952712100139, 271.6 ], "Scale": 0, "Type": "entity", "Z": 1 }',
  PROPERTY 'Item.1' '{ "Background": 6591981, "Center": [ 512, 512 ], "Entity": "[Test Image]", "Rect": [ 87.44436717663422, 107.605702364395, 184.6, 255.48609179415854 ], "Scale": 6.432160804020101, "Type": "entity", "Z": 0 }'
);


---- LOCATION: Test_Location
--DROP LOCATION [Test_Location];
CREATE LOCATION [Test_Location] (
  PROPERTY 'Text' '{ "Center": [ -269.56268602944965, 1.2390172697885337e-11 ], "Entity": "[Map Lat\\/Lon]", "Scale": 5672220232.70756 }'
);


---- QUERY: Test_Query
--DROP QUERY [Test_Query];
CREATE QUERY [Test_Query] (
  PROPERTY 'Text' '-- $manifold$

SELECT * FROM [mfd_root];'
);


---- TABLE: Test_Table
--DROP TABLE [Test_Table];
CREATE TABLE [Test_Table] (
  [mfd_id] INT64,
  [geom] GEOM,
  [mfd_id morethan 5] BOOLEAN AS [[ [mfd_id] > 5 ]],
  INDEX [mfd_id_x] BTREE ([mfd_id]),
  INDEX [geom_x] RTREE ([geom]),
  PROPERTY 'FieldCoordSystem.geom' '{ "Axes": "XYH", "Base": "World Geodetic 1984 (WGS84)", "Eccentricity": 0.08181919084262149, "MajorAxis": 6378137, "Name": "Latitude \\/ Longitude", "System": "Latitude \\/ Longitude", "Unit": "Degree", "UnitLatLon": true, "UnitScale": 1, "UnitShort": "deg" }',
  PROPERTY 'FieldItem.geom' '{ "Size": 96, "Z": 1 }',
  PROPERTY 'FieldItem.mfd_id' '{ "Size": 72, "Z": 0 }',
  PROPERTY 'FieldItem.mfd_id morethan 5' '{ "Size": 118, "Z": 2 }',
  PROPERTY 'Folder' 'TestFolder\\Folder 2'
);


---- DRAWING: Test_Table Drawing
--DROP DRAWING [Test_Table Drawing];
CREATE DRAWING [Test_Table Drawing] (
  PROPERTY 'FieldGeom' 'geom',
  PROPERTY 'Folder' 'TestFolder\\Folder 2',
  PROPERTY 'Table' '[Test_Table]'
);


---- LABELS: Test_Table Labels
--DROP LABELS [Test_Table Labels];
CREATE LABELS [Test_Table Labels] (
  PROPERTY 'Drawing' '[Test_Table Drawing]',
  PROPERTY 'FieldText' 'mfd_id',
  PROPERTY 'Folder' 'TestFolder\\Folder 2'
);



# Customizer
### Custom content consumption library

| Package                | Release |
|------------------------|---------|
| `Customizer`           | [![NuGet](https://img.shields.io/nuget/v/Customizer.svg)](https://www.nuget.org/packages/Customizer/) |
| `Customizer.Authoring` | [![NuGet](https://img.shields.io/nuget/v/Customizer.Authoring.svg)](https://www.nuget.org/packages/Customizer.Authoring/) |

This library is meant to provide a basic common custom content consumption platform.

Given a standard set of resources and additional user data, users either write JSON text or use external tools to generate definitions referencing named resources by path and defining properties (e.g. custom armor attach points and layered texture generation) that can then be loaded into shared formats for consumption by other libraries.

**Note:** The Customizer assembly does not understand JSON encoding or FBX models. If content is authored by hand in these formats, the following conversions must be made:
* Content definitions written in JSON must be converted to CzBox containers with CzTool's [Pack](#Pack) command.
* Translation dictionaries written in JSON must be converted to deflate compressed translation dictionaries with CzTool's [MakeTl](#MakeTl) command.
* FBX models must be converted to LiveMesh containers with CzTool's [MakeMesh](#MakeMesh) command.

## Content paths

Content paths in content definition files are specified as URIs.

* The URI's scheme (`scheme://xxx/xxx`) determines how the input stream is transformed when read. Supported values are:
  - `file`: No transformation
  - `deflate`: Decompress using deflate algorithm
* The URI's host (`xxx://host/xxx`) determines which data providers will look for the content.
  - `local`: CzBox and filesystem providers
  - `proc`: Procedural data (built-in for `DataManager`)
    - `file://proc/color:RRGGBB`: generate 1x1 PNG stream for RGB color from hex color
* The URI's absolute path (`xxx://xxx/path`) specifies the name the provider should check against.
  - e.g. if a data manager `dm` has a registered filesystem provider based on a folder that contains a file `res.txt`, then a call to `dm.GetStream("file://local/res.txt")` will return a stream with the file's data

## Customizer

**Framework:** .NET Standard 2.0

Provides base API and data structures for loading resources using a content definition into a live object ready for consumption and functions for data serialization.

```csharp
using Customizer.Box;
using Customizer.Content;
using Customizer.Utility;
using System.IO;

public class SomeClass {
    public void LoadAssets(string pathToCzBox, string someContentFolder) {
        var dm = new DataManager();
        dm.RegisterDataProvider(
            new FileDataProvider(someContentFolder));
        LiveContent content;
        using (var box = CzBox.Load(new FileInfo(pathToCzBox).OpenRead())) {
            content = box.LoadLiveContent(dm, Defaults.LiveLoadOptions);
        }

        foreach (var e in content.Meshes) {
            var mesh = e.Value;
            foreach (var subMesh in mesh.SubMeshes) {
                // Do something with each sub-mesh
            }
        }

        foreach (var e in content.RenderedCoTextures) {
            var image = e.Value;
            // Do something with this texture
        }

        // Do something with the rest of the data in the content object
    }
}
```

## Customizer.Authoring

**Framework:** .NET Standard 2.0

Provides a few functions for content authoring:
* Load FBX models into a LiveMesh object (wrapper around AssimpNet)
* Load FBX animations into a LiveAnim object (wrapper around AssimpNet)
* Deserialize UTF-8 JSON from stream (wrapper around System.Text.Json)
* Serialize UTF-8 JSON to stream (wrapper around System.Text.Json)

**Note:** this assembly can only be used on Windows/OSX/Linux due to an unmanaged library dependency in assimpnet

## CzTool

**Framework:** .NET Core 2.2

Command-line program that provides some utilities:

### Template

Generate template file

Usage: `CzTool template definition|matchfile <targetFile>`

### MakeMesh

Generate LiveMesh from FBX

Usage:

`CzTool makemesh <sourceFile> <targetFile> [-m|--matchFile FILE] [-u|--uniqueName NAME] [-v|--variantTypeName NAME] [-f|--fitUniqueName NAME] [-c|--compress]`

`matchFile` is a JSON text file for assigning [bone](Customizer/Modeling/BoneType.cs) / [attach point](Customizer/Modeling/AttachPointType.cs) types to bones that follows this structure:

```json
{
  "Bones": {
    "(boneNameRegex)": "(boneEnumValueName)"
  },
  "AttachPoints": {
    "(boneNameRegex)": "(attachPointEnumValueName)"
  }
}
```
A template matchfile can be generated with `CzTool template matchfile <targetFile>`

`uniqueName` is the unique name that should be assigned to the mesh for cases where loose clothing will be fit against the mesh or any mesh with the same variant type name.

`variantTypeName` is a name used to associate "morphed" meshes sharing the same vertex structure.

`fitUniqueName` is the unique name of the mesh this mesh was fit against (i.e. this mesh is a piece of loose clothing, and fitUniqueName is the body mesh modeled against).

`compress` enables deflate compression of the serialized data. The mesh must be referenced with the `deflate://` schema as opposed to the normal `file://` schema in content definitions.

### MakeAnim

Generate LiveAnim from FBX

Usage:

`CzTool makeanim <sourceFile> <targetFile> [-m|--matchFile FILE] [-s|--sourceUniqueName NAME] [-c|--compress]`

`matchFile` is a JSON text file for assigning [bone](Customizer/Modeling/BoneType.cs) types to bones that follows this structure:

```json
{
  "Bones": {
    "(boneNameRegex)": "(boneEnumValueName)"
  }
}
```
A template matchfile can be generated with `CzTool template matchfile <targetFile>`

`sourceUniqueName` is the unique name of the mesh this mesh was animated against (i.e. this FBX file is animated, and sourceUniqueName is the uniqueName of the LiveMesh of the base non-animated FBX file).

`compress` enables deflate compression of the serialized data. The animation must be referenced with the `deflate://` schema as opposed to the normal `file://` schema in content definitions.

### MakeTl

Generate translation dictionary from JSON

Usage:

`CzTool maketl <sourceFile> <targetFile> [-c|--compress]`

Source JSON must be an object with key-value pairs.

`compress` enables deflate compression of the serialized data. The translation dictionary must be referenced with the `deflate://` schema as opposed to the normal `file://` schema in content definitions.

### Composite

Generate composite images defined in content definition

Usage:

`CzTool composite [-d|--directory <DIRECTORY>] <contentDefinition> <targetDir>`

`directory` is an optional directory to create a filesystem data provider with (will attempt to load resources in the content definition in that directory if other data providers fail). If not specified, the program uses the `contentDefinition` file's parent directory.

### Pack

Write CzBox container

Usage:

`CzTool pack [-d|--definition <FILE>] <targetFile> [resources...]`

`definition` is an optional JSON content definition to embed.

### Unpack

Unpack CzBox container

Usage:

`CzTool unpack <sourceFile> <targetDir>`

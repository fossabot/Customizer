# Customizer
### Custom content consumption library

| Package                | Release |
|------------------------|---------|
| `Customizer`           | [![NuGet](https://img.shields.io/nuget/v/Customizer.svg)](https://www.nuget.org/packages/Customizer/) |
| `Customizer.Authoring` | [![NuGet](https://img.shields.io/nuget/v/Customizer.Authoring.svg)](https://www.nuget.org/packages/Customizer.Authoring/) |

This library is meant to provide a basic common custom content consumption platform.

Given a standard set of resources and additional user data, users either write JSON text or use external tools to generate definitions referencing named resources by path and defining properties (e.g. custom armor attach points and layered texture generation) that can then be loaded into shared formats for consumption by other libraries.

Content paths are specified as URIs.

* Scheme determines how the input stream is transformed when read. Supported values are:
  - `file`: No transformation
  - `deflate`: Decompress using deflate algorithm
* Host determines which data providers will look for the content.
  - `local`: CzBox and filesystem providers
  - `proc`: Procedural data (built-in for `DataManager`)
    - `file://proc/color:RRGGBB`: generate 1x1 PNG stream for RGB color from hex color

**Note:** The Customizer assembly does not understand JSON encoding or FBX models. If content is authored by hand in these formats, the following conversions must be made:
* Content definitions written in JSON must be converted to CzBox containers with CzTool's [Pack](#Pack) command.
* Translation dictionaries written in JSON must be converted to deflate compressed translation dictionaries with CzTool's [MakeTl](#MakeTl) command.
* FBX models must be converted to LiveMesh containers with CzTool's [MakeMesh](#MakeMesh) command.

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

Provides a few functions for content authors:
* Load FBX models into a LiveMesh object (wrapper around AssimpNet)
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

Generate deflate compressed LiveMesh from FBX

Usage:

`CzTool makemesh <sourceFile> <targetFile> [-m|--matchFile FILE] [-u|--uniqueName NAME] [-v|--variantTypeName NAME] [-f|--fitUniqueName NAME]`

matchFile is a JSON text file for assigning [bone](Customizer/Modeling/BoneType.cs) / [attach point](Customizer/Modeling/AttachPointType.cs) types to bones that follows this structure:

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

uniqueName is the unique name that should be assigned to the mesh for cases where loose clothing will be fit against the mesh or any mesh with the same variant type name.

variantTypeName is a name used to associate "morphed" meshes sharing the same vertex structure.

fitUniqueName is the unique name of the mesh this mesh was fit against (i.e. this mesh is a piece of loose clothing, and fitUniqueName is the body mesh modeled against).

### MakeTl

Generate deflate compressed translation dictionary from JSON

Usage:

`CzTool maketl <sourceFile> <targetFile>`

Source JSON must be an object with key-value pairs.

### Composite

Generate composite images defined in content definition

Usage:

`CzTool composite [-d|--directory <DIRECTORY>] <contentDefinition> <targetDir>`

### Pack

Write CzBox container

Usage:

`CzTool pack [-d|--definition <FILE>] <targetFile> [resources...]`

### Unpack

Unpack CzBox container

Usage:

`CzTool unpack <sourceFile> <targetDir>`

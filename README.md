# CustomNavi
### Custom Navi tooling

This library is meant to provide a basic common custom character creation platform.

Given a standard set of resources and additional user data, users either write JSON text or use external tools to generate definitions referencing named resources by path and defining properties (e.g. custom armor attach points and layered texture generation) that can then be loaded into shared formats for consumption by other libraries.

**Note:** The CustomNavi assembly does not understand JSON encoding or FBX models. If content is authored by hand in these formats, the following conversions must be made:
* Content definitions written in JSON must be converted to CnBox containers with CNATool's [Pack](#Pack) command.
* Translation dictionaries written in JSON must be converted to deflate compressed translation dictionaries with CNATool's [MakeTl](#MakeTl) command.
* FBX models must be converted to LiveMesh containers with CNATool's [MakeMesh](#MakeMesh) command.

## CustomNavi

**Framework:** .NET Standard 2.0

Provides base API and data structures for loading resources using a content definition into a live object ready for consumption and functions for data serialization.

```csharp
using CustomNavi.Box;
using CustomNavi.Content;
using CustomNavi.Modeling;
using CustomNavi.Texturing;
using CustomNavi.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

public class SomeClass {
  public void LoadAssets(string pathToCnBox, string someContentFolder){
    var rm = new DataManager();
    rm.RegisterDataProvider(
      new FileDataProvider(someContentFolder));
    var box = CnBox.Load(new FileInfo(pathToCnBox).OpenRead());
    var content = box.LoadLiveContent(rm, Defaults.LiveLoadOptions);
    foreach(var e in content.Meshes){
      LiveMesh mesh = e.Value;
      foreach(LiveSubMesh submesh in mesh.SubMeshes){
        // Do something with each sub-mesh
      }
    }
    foreach(var e in content.RenderedCoTextures){
      Image<Rgba32> image = e.Value;
      // Do something with this image
    }
    // Do something with the rest of the data in the content object
  }
}
```

## CustomNavi.Authoring

**Framework:** .NET Standard 2.0

Provides a few functions for content authors:
* Load FBX models into a LiveMesh object (wrapper around AssimpNet)
* Deserialize UTF-8 JSON from stream (wrapper around System.Text.Json)
* Serialize UTF-8 JSON to stream (wrapper around System.Text.Json)

**Note:** this assembly can only be used on Windows/OSX/Linux due to an unmanaged library dependency in assimpnet

## CNATool

**Framework:** .NET Core 2.2

Command-line program that provides some utilities:

### Template

Generate template file

Usage: `CNATool template definition|matchfile <targetFile>`

### MakeMesh

Generate deflate compressed LiveMesh from FBX

Usage: `CNATool makemesh <sourceFile> <targetFile> [-m|--matchFile FILE]`

matchFile is a JSON text file for assigning [bone](CustomNavi/Modeling/BoneType.cs) / [attach point](CustomNavi/Modeling/AttachPointType.cs) types to bones that follows this structure:

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
A template matchfile can be generated with `CNATool template matchfile <targetFile>`

### MakeTl

Generate deflate compressed translation dictionary from JSON

Usage: `CNATool maketl <sourceFile> <targetFile>`

Source JSON must be an object with key-value pairs.

### Composite

Generate composite images defined in content definition

Usage: `CNATool composite [-d|--directory <DIRECTORY>] <contentDefinition> <targetDir>`

### Pack

Write CnBox container

Usage: `CNATool pack [-d|--definition <FILE>] <targetFile> [resources...]`

### Unpack

Unpack CnBox container

Usage: `CNATool unpack <sourceFile> <targetDir>`

## TO-DO

* Mesh combining function

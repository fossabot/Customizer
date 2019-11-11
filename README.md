# Customizer
### Custom content consumption library

This library is meant to provide a basic common custom content consumption platform.

Given a standard set of resources and additional user data, users either write JSON text or use external tools to generate definitions referencing named resources by path and defining properties (e.g. custom armor attach points and layered texture generation) that can then be loaded into shared formats for consumption by other libraries.

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
using Customizer.Modeling;
using Customizer.Texturing;
using Customizer.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

public class SomeClass {
  public void LoadAssets(string pathToCzBox, string someContentFolder){
    var rm = new DataManager();
    rm.RegisterDataProvider(
      new FileDataProvider(someContentFolder));
    var box = CzBox.Load(new FileInfo(pathToCzBox).OpenRead());
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

Usage: `CzTool makemesh <sourceFile> <targetFile> [-m|--matchFile FILE]`

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

### MakeTl

Generate deflate compressed translation dictionary from JSON

Usage: `CzTool maketl <sourceFile> <targetFile>`

Source JSON must be an object with key-value pairs.

### Composite

Generate composite images defined in content definition

Usage: `CzTool composite [-d|--directory <DIRECTORY>] <contentDefinition> <targetDir>`

### Pack

Write CzBox container

Usage: `CzTool pack [-d|--definition <FILE>] <targetFile> [resources...]`

### Unpack

Unpack CzBox container

Usage: `CzTool unpack <sourceFile> <targetDir>`

## TO-DO

* Mesh combining function

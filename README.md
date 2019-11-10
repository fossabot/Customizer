# CustomNavi
### Custom Navi tooling

This library is meant to provide a basic common custom character creation toolkit.

Given a certain standard library of resources, users can write JSON text referencing named resources by path and defining certain custom properties (e.g. custom armor attach points and layered texture generation) that can then be loaded into common formats for consumption by any libraries that use this library.

## CustomNavi

**Framework:** .NET Standard 2.0

This library provides the base API and functions for loading resources using a content definition into a live object ready for consumption and functions for data serialization.

## CustomNavi.Authoring

**Framework:** .NET Standard 2.0

This library currently just provides a function for loading FBX models into a LiveMesh model.

**Note:** this library can only be used on Windows/OSX/Linux due to an unmanaged library dependency in assimpnet

## CNATool

**Framework:** .NET Core 2.2

This command-line program provides some utilities:
* Template
  - Generate template ContentDefinition json
  - Usage: `CNATool template <targetFile>`
* MakeMesh
  - Generate binary-serialized LiveMesh from FBX
  - Usage: `CNATool makemesh <sourceFile> <targetFile> [-m|--matchFile FILE]`
    - matchFile is a JSON text file for assigning bone / attach point types to bones that follows this structure:
    ```
    {
        "bones": {
            "(boneEnumValueName)": "(boneNameRegex)"
        },
        "attachPoints": {
            "(attachPointEnumValueName)": "(boneNameRegex)"
        }
    }
    ```
* Pack
  - Write CnBox container
  - Usage: `CNATool pack [-d|--definition <targetFile>] <contentDefinition> [resources...]`
* Unpack
  - Unpack CnBox container
  - Usage: `CNATool unpack <sourceFile> <targetDir>`
* Composite
  - Generate composite images from content definition
  - Usage: `CNATool composite [-d|--directory <DIRECTORY>] <contentDefinition> <targetDir>`
## TO-DO

* Add live content manipulation (add/update/remove texture/mesh)
* Add content packing structure and utility

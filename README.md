# UE3 Shader Cache Patcher

Patches UE3 cooked script packages so that their shader caches
are automatically loaded in game. Tested with Rising Storm 2: Vietnam
and Killing Floor 2.

## Use Case

Anyone who has tried using custom master materials in UnrealScript mods for
Rising Storm 2 or Killing Floor 2 (and perhaps other UE3 games) has seen this
dreaded message after loading the cooked mod in game.

```
[0037.39] ScriptLog: Mutators shaderloader.shaderloadermutator
[0037.39] Log: Missing cached shader map for material M_VN_MASTER_PBR_copy, compiling.
[0037.39] Log: Can't compile M_VN_MASTER_PBR_copy with seekfree loading path on console, will attempt to use default material instead
[0037.39] Warning: Warning, Failed to compile Material M_SLM_Test.Materials.M_VN_MASTER_PBR_copy for platform PC-D3D-SM5, Default Material will be used in game.
[0037.39] Warning: Warning, Failed to compile Material Instance M_SLM_Test.Materials.M_CustomTest_INST with Base M_VN_MASTER_PBR_copy for platform PC-D3D-SM5, Default Material will be used in game.
```

After patching the cooked script package (ShaderLoader.u in this case), the
custom materials work in game!

```
[0043.63] ScriptLog: Mutators shaderloader.shaderloadermutator
[0043.63] DevShaders: ... Loaded 230 shaders (0 legacy, 230 redundant)
[0043.63] DevShaders: Shader cache for shaderloader contains 2 materials (0 redundant)
```

## Download

Download from releases: https://github.com/tuokri/UE3ShaderCachePatcher/releases

## Examples

Killing Floor 2 demo repo: https://github.com/tuokri/KF2CustomMaterialTest

Rising Storm 2: Vietnam demo repo: https://github.com/tuokri/RS2-ShaderLoaderMutator

Video demo: https://youtu.be/u-m0W_edhj8

## Command Line Tool

A simple command line tool is also included. See [UE3ShaderCachePatcherCLI](./UE3ShaderCachePatcherCLI/)
for details and usage instructions.

## Notes

[Material-Message-Box](https://github.com/tuokri/Material-Message-Box) is used as a submodule
to patch it to work properly with MaterialDesignInXamlToolkit >= 5.0.0.

## Credits

Powered by Unreal-Library from EliotVU: https://github.com/EliotVU/Unreal-Library

## Development TODOs

- Write some (unit) tests for the CLI app.
- Write some (unit) tests for the model part of the GUI app.
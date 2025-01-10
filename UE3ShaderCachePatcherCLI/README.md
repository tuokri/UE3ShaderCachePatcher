# UE3ShaderCachePatcherCLI

## Quickstart

Show help:

```bash
UE3ShaderCachePatcherCLI.exe -h
```

Patch `MyMod.MyGameInfo.DummyObject` to refer to `MyMod.SeekFreeShaderCache`.

```bash
UE3ShaderCachePatcherCLI.exe -f C:\Path\To\My\Mod\MyMod.u -s "SeekFreeShaderCache" -p "MyGameInfo.DummyObject"
# Or (both commands are equivalent):
UE3ShaderCachePatcherCLI.exe -f C:\Path\To\My\Mod\MyMod.u -s "MyMod.SeekFreeShaderCache" -p "MyMod.MyGameInfo.DummyObject"
```

using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using UELib;
using UELib.Core;

var fileOption = new Option<FileInfo>(
    aliases: ["--file", "-f"],
    description: "The UE3 script package (.u) file to patch.")
{
    IsRequired = true
};

var shaderCacheOption = new Option<string>(
    aliases: ["--shader-cache-path", "-s"],
    description: "Full path to the shader cache object within the .u script package."
)
{
    IsRequired = true
};

var objectToPatchOption = new Option<string>(
    aliases: ["--patch-object", "-p"],
    description: "Full path to the object to patch within the .u script package."
)
{
    IsRequired = true
};

var assembly = Assembly.GetExecutingAssembly();
var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
var rootCommand = new RootCommand($"UE3 Shader Cache Patcher CLI {fvi.FileVersion}");

rootCommand.AddOption(fileOption);
rootCommand.AddOption(shaderCacheOption);
rootCommand.AddOption(objectToPatchOption);
rootCommand.SetHandler(PatchFile, fileOption, shaderCacheOption, objectToPatchOption);

return await rootCommand.InvokeAsync(args);

string GetObjPath(UObject? obj, bool withClassPrefix = false)
{
    if (withClassPrefix)
    {
        return obj == null ? "UNKNOWN" : $"{obj.Class.Name}'{obj.Package.PackageName}.{obj.GetPath()}'";
    }

    return obj == null ? "UNKNOWN" : $"{obj.Package.PackageName}.{obj.GetPath()}";
}

void PatchFile(FileInfo file, string shaderCachePath, string objectToPatchPath)
{
    if (string.CompareOrdinal(shaderCachePath, objectToPatchPath) == 0)
    {
        throw new ArgumentException("Shader cache path and object to patch path cannot be the same object.");
    }

    Console.WriteLine($"Patching file '{file.FullName}'");
    var pkg = UnrealLoader.LoadPackage(file.FullName, FileAccess.ReadWrite);
    pkg.InitializePackage();
    pkg.InitializeExportObjects();

    var pkgName = Path.GetFileNameWithoutExtension(file.Name);

    var parts = objectToPatchPath.Split('.');
    if (string.CompareOrdinal(parts[0], pkgName) == 0)
    {
        parts = parts.Skip(1).ToArray();
    }

    var patchObjectParts = new string[parts.Length];
    parts.CopyTo(patchObjectParts, 0);

    // TODO: expecting this to be in the format: MyPatchableClass.DummyObjectVarToPatch.
    //   Is there a case where this patch can be longer than 2 parts?
    if (patchObjectParts.Length != 2)
    {
        throw new ArgumentException(
            "Expecting patchable object to be in the format 'MyPatchableClass.MyObjectVarToPatch'," +
            $" actual object path resolved to '{string.Join('.', patchObjectParts)}'."
        );
    }

    if (shaderCachePath.Contains('.'))
    {
        parts = shaderCachePath.Split('.');
        if (string.CompareOrdinal(parts[0], pkgName) == 0)
        {
            shaderCachePath = string.Join('.', parts.Skip(1));
        }
    }

    var topLevelShaderCache = !shaderCachePath.Contains('.');
    // TODO: support searching for nested shader cache!
    if (!topLevelShaderCache)
    {
        const string msg = "Only top level shader cache objects are currently supported.";
        Console.WriteLine(msg);
        throw new Exception(msg);
    }

    UObject? shaderCache = null;
    UObject? objectToPatch = null;
    UDefaultProperty? propToPatch = null;
    long patchablePropValuePosition = 0;

    foreach (var item in pkg.Exports)
    {
        if (shaderCache == null && item.Object.Name == shaderCachePath)
        {
            shaderCache = item.Object;
            Console.WriteLine($"Found shader cache object: '{GetObjPath(shaderCache)}'");
        }

        // TODO: do we need support for varying lengths of patchObjectParts?
        if (objectToPatch == null && patchObjectParts[0] == item.Object.Name)
        {
            item.Object.BeginDeserializing();
            item.Object.Default?.BeginDeserializing();

            var defaultProps = item.Object.Default?.Properties;
            if (defaultProps == null)
            {
                return;
            }

            propToPatch = defaultProps.Find(
                p => p.Name == patchObjectParts.Last()
            );

            if (propToPatch != null)
            {
                var dPath = GetObjPath(item.Object.Default);
                Console.WriteLine($"Found property to patch: '{dPath}.{propToPatch.Name}'");

                // Get private UDefaultProperty member _PropertyValuePosition.
                var field = propToPatch.GetType().GetProperty(
                    "_PropertyValuePosition", BindingFlags.NonPublic | BindingFlags.Instance);
                var fieldValue = field?.GetValue(propToPatch);
                if (fieldValue == null)
                {
                    throw new ArgumentException(
                        $"invalid fieldValue (_PropertyValuePosition) in '{dPath}.{propToPatch.Name}'");
                }

                patchablePropValuePosition = (long)fieldValue;
                Debug.Assert(patchablePropValuePosition > 0);

                Debug.Assert(item.Object.Default != null);
                objectToPatch = item.Object.Default;
            }
        }

        if (shaderCache != null && objectToPatch == null)
        {
            break;
        }
    }

    if (shaderCache == null)
    {
        throw new ArgumentException("Unable to find shader cache object");
    }

    if (objectToPatch == null || propToPatch == null)
    {
        throw new ArgumentException("Unable to find object to patch.");
    }

    Console.WriteLine(
        $"Patching object: '{objectToPatch}': " +
        $"{propToPatch.Type}'{GetObjPath(objectToPatch)}.{propToPatch.Name}' (Value={propToPatch.Value}) " +
        $"--> {shaderCache.Class}");

    // Patch objectToPatch to point to the ShaderCache object to
    // force automatic loading of custom shaders when the mod script
    // package .u file is loaded.
    var stream = objectToPatch.Package.Stream;
    stream.Seek(objectToPatch.GetBufferPosition(), SeekOrigin.Begin);
    stream.Seek(patchablePropValuePosition, SeekOrigin.Current);
    var oldObjIndex = stream.ReadInt32(); // TODO: maybe validate something with this?
    stream.Seek(-4, SeekOrigin.Current);
    stream.Write(shaderCache);

    if (Debugger.IsAttached)
    {
        var shaderCacheIdx = (int)shaderCache;

        stream.Seek(-4, SeekOrigin.Current);
        Debug.Assert(stream.ReadInt32() == shaderCacheIdx);

        var group = $"{objectToPatch.GetPath()}.{propToPatch.Name}";
        // Re-read package to verify patching was successful.
        var debugPkg = UnrealLoader.LoadPackage(file.FullName);
        debugPkg.InitializePackage();
        debugPkg.InitializeExportObjects();
        var checkObj = debugPkg.FindObjectByGroup(group) as UObjectProperty;
        Debug.Assert(checkObj != null);

        checkObj.Outer.Default.BeginDeserializing();
        foreach (var prop in checkObj.Outer.Default.Properties)
        {
            if (prop.Name == checkObj.Name)
            {
                Debug.Assert(prop.Value == $"{shaderCache.Class.Name}'{shaderCache.Name}'");
                break;
            }
        }
    }

    Console.WriteLine("Done.");
}
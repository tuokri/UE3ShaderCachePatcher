using System.Collections.Generic;
using UELib;
using UELib.Core;
using UELib.Types;

namespace UE3ShaderCachePatcher;

public class ObjectDataModel : BaseDataModel
{
    private const string TargetObjectTextDefault = "Waiting for object selection...";

    private UnrealPackage? _package;
    private UObject? _shaderCacheObject;
    private UObject? _targetObject;
    private UDefaultProperty? _targetProperty;
    private DefaultPropertiesCollection _defaultProperties;
    private List<UName> _defaultPropertiesNames;

    private string _targetObjectBeforeText = TargetObjectTextDefault;
    private string _targetObjectAfterText = TargetObjectTextDefault;

    public List<UName> DefaultPropertiesNames
    {
        get => _defaultPropertiesNames;
        private set
        {
            _defaultPropertiesNames = value;
            NotifyPropertyChanged();
        }
    }

    public DefaultPropertiesCollection DefaultProperties
    {
        get => _defaultProperties;
        set
        {
            _defaultProperties = value;
            NotifyPropertyChanged();

            var nameList = new List<UName>();
            _defaultProperties.ForEach(prop => nameList.Add(prop.Name));
            DefaultPropertiesNames = nameList;
        }
    }

    public UObject? ShaderCacheObject
    {
        get => _shaderCacheObject;
        set
        {
            _shaderCacheObject = value;
            EvaluateTargetObjectTextValues();
            NotifyPropertyChanged();
        }
    }

    public UObject? TargetObject
    {
        get => _targetObject;
        set
        {
            _targetObject = value;
            EvaluateTargetObjectTextValues();
            NotifyPropertyChanged();
        }
    }

    public UDefaultProperty? TargetProperty
    {
        get => _targetProperty;
        set
        {
            _targetProperty = value;
            EvaluateTargetObjectTextValues();
            NotifyPropertyChanged();
        }
    }

    public UnrealPackage? Package
    {
        get => _package;
        set
        {
            _package = value;
            EvaluateTargetObjectTextValues();
            NotifyPropertyChanged();
        }
    }

    private void EvaluateTargetObjectTextValues()
    {
        var pkgName = _package?.PackageName ?? "???";
        var objName = _targetObject?.Name ?? "???";
        var shaderObjName = _shaderCacheObject?.Name ?? "???";
        var shaderClass = _shaderCacheObject?.Class?.Name ?? "ShaderCache";
        var targetPropName = _targetProperty?.Name ?? "???";

        var targetPropValue = $"{targetPropName}=???";

        if (_targetProperty?.Type == PropertyType.ObjectProperty)
        {
            targetPropValue = _targetProperty.Decompile();
        }

        TargetObjectBeforeText =
            $"{pkgName}.{objName}.{targetPropValue}";

        TargetObjectAfterText =
            $"{pkgName}.{objName}.{targetPropName}" +
            $"={shaderClass}'{pkgName}.{shaderObjName}'";
    }

    public string TargetObjectBeforeText
    {
        get => _targetObjectBeforeText;
        private set
        {
            _targetObjectBeforeText = value;
            NotifyPropertyChanged();
        }
    }

    public string TargetObjectAfterText
    {
        get => _targetObjectAfterText;
        private set
        {
            _targetObjectAfterText = value;
            NotifyPropertyChanged();
        }
    }

    public void ResetTargetObjectTextValues()
    {
        TargetObjectBeforeText = TargetObjectTextDefault;
        TargetObjectAfterText = TargetObjectTextDefault;
    }

    public ObjectDataModel()
    {
        _defaultProperties = new DefaultPropertiesCollection();
        _defaultPropertiesNames = new List<UName>();

        // EvaluateTargetObjectTextValues();
    }
}

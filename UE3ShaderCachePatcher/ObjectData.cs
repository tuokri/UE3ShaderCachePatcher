using System.Collections.Generic;
using UELib;
using UELib.Core;

namespace UE3ShaderCachePatcher;

public class ObjectDataModel : BaseDataModel
{
    private UnrealPackage? _package;
    private UObject? _shaderCacheObject;
    private UObject? _targetObject;
    private UDefaultProperty? _targetProperty;
    private DefaultPropertiesCollection _defaultProperties;
    private List<UName> _defaultPropertiesNames;

    private string _targetObjectBeforeText;
    private string _targetObjectAfterText;

    public List<UName> DefaultPropertiesNames
    {
        get => _defaultPropertiesNames;
        set
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
        var shaderClass = _shaderCacheObject?.GetClassName() ?? "ShaderCache";
        var targetPropName = _targetProperty?.Name ?? "???";
        var targetPropValue = _targetProperty?.Value ?? "???";

        TargetObjectBeforeText =
            $"{pkgName}.{objName}.{targetPropName} = {targetPropValue}";

        TargetObjectAfterText =
            $"{pkgName}.{objName}.{targetPropName}" +
            $" = {shaderClass}'{pkgName}.{shaderObjName}'";
    }

    public string TargetObjectBeforeText
    {
        get => _targetObjectBeforeText;
        set
        {
            _targetObjectBeforeText = value;
            NotifyPropertyChanged();
        }
    }

    public string TargetObjectAfterText
    {
        get => _targetObjectAfterText;
        set
        {
            _targetObjectAfterText = value;
            NotifyPropertyChanged();
        }
    }

    public ObjectDataModel()
    {
        _targetObjectBeforeText = "Waiting for object selection...";
        _targetObjectAfterText = "Waiting for object selection...";

        _defaultProperties = new DefaultPropertiesCollection();
        _defaultPropertiesNames = new List<UName>();

        // EvaluateTargetObjectTextValues();
    }
}

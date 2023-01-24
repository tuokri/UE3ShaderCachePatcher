using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using UELib;
using UELib.Core;

namespace UE3ShaderCachePatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObjectViewModel _objectViewModel;

        public string WindowTitle
        {
            get => (string)GetValue(TitleProperty);
            init => SetValue(TitleProperty, value);
        }

        public new static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(WindowTitle), typeof(string),
                typeof(MainWindow), new UIPropertyMetadata("", null));

        public MainWindow(ObjectViewModel objectViewModel)
        {
            _objectViewModel = objectViewModel;

            InitializeComponent();

            WindowMainGrid.DataContext = _objectViewModel.ObjectData;
            DataContext = this;

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            WindowTitle = $"UE3 Shader Cache Patcher {version}";
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".u",
                Filter = "UE3 script packages (.u)|*.u"
            };

            var result = dialog.ShowDialog();

            if (result == true)
            {
                _objectViewModel.ObjectData.Package = null;
                _objectViewModel.ObjectData.ShaderCacheObject = null;
                _objectViewModel.ObjectData.TargetObject = null;
                _objectViewModel.ObjectData.TargetProperty = null;
                _objectViewModel.ObjectData.DefaultProperties.Clear();
                _objectViewModel.ObjectData.DefaultPropertiesNames.Clear();

                LstShaderCacheObjects.Items.Clear();
                LstTargetObjects.Items.Clear();

                FileNameBlock.Text = $"Selected file: '{dialog.FileName}'";

                _objectViewModel.ObjectData.Package = UnrealLoader.LoadFullPackage(dialog.FileName);
                var pkgShaderCacheObjects = _objectViewModel.ObjectData.Package.Exports.FindAll(item =>
                    item.ClassName.Equals("ShaderCache", StringComparison.OrdinalIgnoreCase));
                pkgShaderCacheObjects.Sort((item1, item2) =>
                    string.Compare(item1.ObjectName, item2.ObjectName, StringComparison.Ordinal));
                pkgShaderCacheObjects.ForEach(obj => LstShaderCacheObjects.Items.Add(obj));

                var targetObjects = _objectViewModel.ObjectData.Package.Objects.FindAll(obj =>
                    !obj.GetClassName().Equals("ShaderCache", StringComparison.OrdinalIgnoreCase));
                targetObjects.Sort((item1, item2) =>
                    string.Compare(item1.Name, item2.Name, StringComparison.Ordinal));
                targetObjects.ForEach(obj => LstTargetObjects.Items.Add(obj));
            }
        }

        private void LstShaderCacheObjects_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstShaderCacheObjects.SelectedItem == null)
            {
                _objectViewModel.ObjectData.ShaderCacheObject = null;
                return;
            }

            var exportTableItem = (UExportTableItem)LstShaderCacheObjects.SelectedItem;
            var obj = exportTableItem.Object;

            if (obj == null)
            {
                _objectViewModel.ObjectData.ShaderCacheObject = null;
                return;
            }

            obj.BeginDeserializing();

            _objectViewModel.ObjectData.ShaderCacheObject = obj;
        }

        private void LstTargetObjects_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _objectViewModel.ObjectData.DefaultProperties = new DefaultPropertiesCollection();

            if (LstTargetObjects.SelectedItem == null)
            {
                _objectViewModel.ObjectData.TargetObject = null;
                _objectViewModel.ObjectData.TargetProperty = null;
                return;
            }

            var obj = (UObject)LstTargetObjects.SelectedItem;

            obj.BeginDeserializing();

            var props = obj.Properties;

            if (props == null)
            {
                return;
            }

            props.Sort((prop1, prop2) =>
                string.Compare(prop1.Name, prop2.Name, StringComparison.Ordinal));
            // props.ForEach(prop => LstTargetObjectDefaultProperties.Items.Add(prop));

            _objectViewModel.ObjectData.TargetObject = obj;
            _objectViewModel.ObjectData.DefaultProperties = props;
        }

        private void LstTargetObjectDefaultProperties_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstTargetObjectDefaultProperties.SelectedItem == null)
            {
                _objectViewModel.ObjectData.TargetProperty = null;
                return;
            }

            var propName = (UName)LstTargetObjectDefaultProperties.SelectedItem;
            var prop = _objectViewModel.ObjectData.DefaultProperties.Find(
                property => string.Equals(property.Name, propName,
                    StringComparison.OrdinalIgnoreCase));

            if (prop == null)
            {
                return;
            }

            _objectViewModel.ObjectData.TargetProperty = prop;
        }

        private void ValidatePatchButton_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstTargetObjects.SelectedItem != null &&
                           LstTargetObjectDefaultProperties.SelectedItem != null &&
                           LstShaderCacheObjects.SelectedItem != null;
        }

        private void ValidatePatchButton_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Disable all UI selections.
            // Patch file.
            // Re-load file and update UI.
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value

            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo(e.Uri.AbsoluteUri)
                {
                    UseShellExecute = true
                };

                proc.Start();
            }

            e.Handled = true;
        }
    }

    public static class Commands
    {
        public static readonly RoutedUICommand ValidatePatchButton =
            new("Validate Patch Button", "ValidatePatchButton",
                typeof(MainWindow));
    }
}

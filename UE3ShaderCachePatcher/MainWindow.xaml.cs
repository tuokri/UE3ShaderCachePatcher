using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using BespokeFusion;
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

        private const string TopBarDefaultStatus =
            "Select a script package file (.u) to begin the patching process.";

        public string TopBarStatusString
        {
            get => (string)GetValue(SelectFileToBeginProperty);
            set => SetValue(SelectFileToBeginProperty, value);
        }

        public static readonly DependencyProperty SelectFileToBeginProperty =
            DependencyProperty.Register(
                nameof(TopBarStatusString),
                typeof(string),
                typeof(MainWindow),
                new UIPropertyMetadata(TopBarDefaultStatus, null));

        public string WindowTitle
        {
            get => (string)GetValue(TitleProperty);
            init => SetValue(TitleProperty, value);
        }

        public new static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(WindowTitle), typeof(string),
                typeof(MainWindow),
                new UIPropertyMetadata("", null));

        public MainWindow(ObjectViewModel objectViewModel)
        {
            _objectViewModel = objectViewModel;

            InitializeComponent();

            WindowMainGrid.DataContext = _objectViewModel.ObjectData;
            DataContext = this;
            TopBarStatusTextBlock.DataContext = this;

            var assembly = Assembly.GetExecutingAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            WindowTitle = $"UE3 Shader Cache Patcher {fvi.FileVersion}";
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            // var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // var myGames = Path.Combine(docPath, "My Games");
            // var initialDir = Path.Exists(myGames) ? myGames : docPath;
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                // InitialDirectory = initialDir,
                RestoreDirectory = true,
                DefaultExt = ".u",
                Filter = "UE3 script packages (.u)|*.u"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _objectViewModel.ObjectData.Package = null;
                _objectViewModel.ObjectData.ShaderCacheObject = null;
                _objectViewModel.ObjectData.TargetObject = null;
                _objectViewModel.ObjectData.TargetProperty = null;
                _objectViewModel.ObjectData.DefaultProperties.Clear();
                _objectViewModel.ObjectData.DefaultPropertiesNames.Clear();

                LstShaderCacheObjects.Items.Clear();
                LstTargetObjects.Items.Clear();

                TopBarStatusString = $"Selected file: '{dialog.FileName}'";

                var pkg = UnrealLoader.LoadPackage(dialog.FileName, FileAccess.ReadWrite);
                pkg.InitializePackage();
                pkg.InitializeExportObjects();

                _objectViewModel.ObjectData.Package = pkg;

                var objects = new List<UObject>();
                pkg.Exports.ForEach(item =>
                {
                    if (item.Object?.Class != null)
                    {
                        item.Object.BeginDeserializing();
                        objects.Add(item.Object);
                    }
                });

                var pkgShaderCacheObjects = objects.FindAll(obj =>
                    obj.Class.Name.Equals("ShaderCache", StringComparison.OrdinalIgnoreCase));
                pkgShaderCacheObjects.Sort((item1, item2) =>
                    string.Compare(item1.ToString(), item2.ToString(), StringComparison.Ordinal));
                pkgShaderCacheObjects.ForEach(obj => LstShaderCacheObjects.Items.Add(obj));

                var targetObjects = objects.FindAll(obj =>
                    !obj.Class.Name.Equals("ShaderCache", StringComparison.OrdinalIgnoreCase));
                targetObjects.Sort((item1, item2) =>
                    string.Compare(item1.ToString(), item2.ToString(), StringComparison.Ordinal));
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

            var obj = (UObject)LstShaderCacheObjects.SelectedItem;

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
            e.CanExecute =
                LstTargetObjects.SelectedItem != null
                && LstTargetObjectDefaultProperties.SelectedItem != null
                && LstShaderCacheObjects.SelectedItem != null;

            // TODO: move everything to data model.
            if (e.CanExecute)
            {
                Debug.Assert(LstTargetObjects.SelectedItem
                             == _objectViewModel.ObjectData.TargetObject);
                Debug.Assert(LstTargetObjectDefaultProperties.SelectedItem != null
                             && (UName)LstTargetObjectDefaultProperties.SelectedItem
                             == _objectViewModel.ObjectData.TargetProperty?.Name);
                Debug.Assert(LstShaderCacheObjects.SelectedItem != null
                             && (UObject)LstShaderCacheObjects.SelectedItem
                             == _objectViewModel.ObjectData.ShaderCacheObject);
            }
        }

        private void ValidatePatchButton_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Disable all UI selections?
            // Patch file.
            // Re-load file and update UI? Or just exit?

            var msg = "Warning! Patching:\n\n" +
                      $"{_objectViewModel.ObjectData.TargetObjectBeforeText}\n\n" +
                      "to:\n\n" +
                      $"{_objectViewModel.ObjectData.TargetObjectAfterText}\n\n" +
                      $"in package '{_objectViewModel.ObjectData.Package?.FullPackageName}'!\n\n" +
                      "Please confirm patch operation.";

            var box = new CustomMaterialMessageBox
            {
                TxtTitle = { Text = "Confirm Patch Operation" },
                TxtMessage = { Text = msg },
                BtnOk = { Content = "OK" },
                BtnCancel = { Content = "Cancel" },
                BtnCopyMessage = { IsEnabled = false, Visibility = Visibility.Collapsed },
                Height = 500,
                Width = 500,
            };

            box.Show();
            if (box.Result != MessageBoxResult.OK)
            {
                return;
            }

            var targetObj = _objectViewModel.ObjectData.TargetObject;
            var shaderObj = _objectViewModel.ObjectData.ShaderCacheObject;
            var targetProp = _objectViewModel.ObjectData.TargetProperty;

            // TODO: crash here?
            if (targetObj == null || shaderObj == null || targetProp == null)
            {
                return;
            }

            var prop = targetObj.Properties.Find(targetProp.Name);

            // Get private UDefaultProperty member _PropertyValuePosition.
            var field = prop.GetType().GetProperty(
                "_PropertyValuePosition", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = field?.GetValue(prop);
            if (fieldValue == null)
            {
                throw new ArgumentException(
                    $"invalid fieldValue in {targetObj}.{prop.Name}");
            }

            var propertyValuePosition = (long)fieldValue;
            Debug.Assert(propertyValuePosition > 0);

            // var buffer = targetObj.CopyBuffer();
            // var stream = new UObjectStream(targetObj.GetBuffer());
            var stream = targetObj.Package.Stream;
            stream.Seek(targetObj.GetBufferPosition(), SeekOrigin.Begin);
            stream.Seek(propertyValuePosition, SeekOrigin.Current);
            var oldObjIndex = stream.ReadInt32(); // TODO: maybe validate something with this?
            stream.Seek(-4, SeekOrigin.Current);
            stream.Write(shaderObj);

            if (Debugger.IsAttached)
            {
                stream.Seek(-4, SeekOrigin.Current);
                Debug.Assert(stream.ReadInt32() == (int)shaderObj);
            }

            _objectViewModel.ObjectData.ShaderCacheObject = null;
            _objectViewModel.ObjectData.TargetObject = null;
            _objectViewModel.ObjectData.TargetProperty = null;
            _objectViewModel.ObjectData.DefaultProperties.Clear();
            _objectViewModel.ObjectData.DefaultPropertiesNames.Clear();
            _objectViewModel.ObjectData.Package?.Dispose();
            _objectViewModel.ObjectData.Package = null;
            TopBarStatusString = TopBarDefaultStatus;
            LstTargetObjects.Items.Clear();
            LstShaderCacheObjects.Items.Clear();
            _objectViewModel.ObjectData.ResetTargetObjectTextValues();

            var successMsg = new CustomMaterialMessageBox
            {
                TxtTitle = { Text = "Success!" },
                TxtMessage = { Text = "Package was patched successfully!" },
                BtnOk = { Content = "OK" },
                BtnCancel = { IsEnabled = false, Visibility = Visibility.Collapsed },
                BtnCopyMessage = { IsEnabled = false, Visibility = Visibility.Collapsed },
            };
            successMsg.Show();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
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
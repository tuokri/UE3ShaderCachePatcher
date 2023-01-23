using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UELib;
using UELib.Core;

namespace UE3ShaderCachePatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UnrealPackage? _package;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                // dialog.FileName = "Document"; // Default file name
                DefaultExt = ".u", // Default file extension
                Filter = "UE3 script packages (.u)|*.u" // Filter files by extension
            };

            // Show open file dialog box
            var result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                LstShaderCacheObjects.Items.Clear();
                LstTargetObjects.Items.Clear();
                LstTargetObjectDefaultProperties.Items.Clear();

                FileNameBlock.Text = dialog.FileName;

                _package = UnrealLoader.LoadFullPackage(dialog.FileName);
                var pkgShaderCacheObjects = _package.Exports.FindAll(item =>
                    item.ClassName.Equals("ShaderCache", StringComparison.OrdinalIgnoreCase));
                pkgShaderCacheObjects.Sort((item1, item2) =>
                    string.Compare(item1.ObjectName, item2.ObjectName, StringComparison.Ordinal));
                pkgShaderCacheObjects.ForEach(obj => LstShaderCacheObjects.Items.Add(obj));

                var targetObjects = _package.Objects.FindAll(obj =>
                    !obj.GetClassName().Equals("ShaderCache", StringComparison.OrdinalIgnoreCase));
                targetObjects.Sort((item1, item2) =>
                    string.Compare(item1.Name, item2.Name, StringComparison.Ordinal));
                targetObjects.ForEach(obj => LstTargetObjects.Items.Add(obj));
            }
        }

        private void LstTargetObjects_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LstTargetObjectDefaultProperties.Items.Clear();

            if (LstTargetObjects.SelectedItem == null)
            {
                return;
            }

            var obj = (UObject)LstTargetObjects.SelectedItem;

            if (obj.Properties == null)
            {
                obj.BeginDeserializing();
            }

            var props = obj.Properties;
            props?.Sort((prop1, prop2) =>
                string.Compare(prop1.Name, prop2.Name, StringComparison.Ordinal));
            props?.ForEach(prop => LstTargetObjectDefaultProperties.Items.Add(prop.Name));
        }

        // https://stackoverflow.com/questions/9288507/disabling-button-based-on-multiple-properties-im-using-multidatatrigger-and-mu

        private void BtnPatch_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}

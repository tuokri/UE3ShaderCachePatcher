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

namespace UE3ShaderCachePatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            // dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".u"; // Default file extension
            dialog.Filter = "UE3 script packages (.u)|*.u"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                lstShaderCacheObjects.Items.Clear();
                lstTargetObjects.Items.Clear();

                fileNameBlock.Text = dialog.FileName;

                lstShaderCacheObjects.Items.Add("SeekFreeShaderCache");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");
                lstShaderCacheObjects.Items.Add("asd");

                lstTargetObjects.Items.Add("someTargetObject");
            }
        }
    }
}
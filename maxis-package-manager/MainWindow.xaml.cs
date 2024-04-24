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
using System.IO;
using Microsoft.Win32;
using static maxis_package_manager.PackageIndexTable;

namespace maxis_package_manager
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

        public Package package;

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true) {
                package = new Package(openFileDialog.OpenFile());
                loadInterfaceFromPackageContents();
            }
        }

        private void loadInterfaceFromPackageContents() {
            FileListView.Items.Clear();
            foreach (FileEntry fileEntry in package.indexTable.files){
                FileListView.Items.Add(fileEntry.hash + " ("+fileEntry.typeID.name+")");
            }
        }
    }
}

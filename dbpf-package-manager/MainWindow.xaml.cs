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
using static DBPF_package_manager.PackageIndexTable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static DBPF_package_manager.TypeIDs;


namespace DBPF_package_manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2) {
                openPackageAtPath(args[1]);
            }
        }

        public static Package package;

        Dictionary<ListViewItem, FileEntry> fileEntryLookupForListView = new Dictionary<ListViewItem, FileEntry>();

        private void OpenPackage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DBPF Package (*.package) | *.package";

            if (openFileDialog.ShowDialog() == true)
            {
                openPackageAtPath(openFileDialog.FileName);
            }
        }

        private void openPackageAtPath(string path)
        {
            if (package != null) { //for the package that is already open, if there is one
                package.reader.Close();
            }

            package = new Package(File.OpenRead(path)); //for the current package that we just opened
            loadInterfaceFromPackageContents();
        }

        public string getHexStringFromHash(ulong hash) {
            return "0x" + Convert.ToHexString(BitConverter.GetBytes(hash).Reverse().ToArray());
        }

        private void loadInterfaceFromPackageContents()
        {
            FileListView.Items.Clear();

            fileEntryLookupForListView = new Dictionary<ListViewItem, FileEntry>();

            if (package.indexTable.files == null)
            {
                MessageBox.Show("Couldn't load file list, because file list was null. This shouldn't have happened, so if you've got to this point, hopefully you've already received another warning message explaining why.");
            }
            else
            {
                FileEntry[] sortedFileEntries = package.indexTable.files.OrderBy(f => f.hash).ToArray(); // by sorting, we abandon the original order, which in some cases I have good reason to believe is the original alphabetical order (before the filenames were turned into hashes)

                foreach (FileEntry fileEntry in sortedFileEntries)
                {
                    ListViewItem newItem = new ListViewItem();
                    string s = getHexStringFromHash(fileEntry.hash) + " (" + fileEntry.typeID.name + ")"; //the Reverse() is there because we want to display it as big-endian, it's nothing to do with the game being big-endian.
                    createContextMenuForListViewItem(newItem);
                    newItem.Content = s;
                    fileEntryLookupForListView.Add(newItem, fileEntry);
                    FileListView.Items.Add(newItem);
                }
            }
            packageInfoLabel.Text = "Local filename: " + System.IO.Path.GetFileName(((FileStream)package.reader.BaseStream).Name) + "\n";
            packageInfoLabel.Text += "Package version: v " + package.packageHeader.majorVersion + "." + package.packageHeader.minorVersion;
            packageInfoLabel.Text += "\nIndex version: v " + package.packageHeader.indexMajorVersion + "." + package.packageHeader.indexMinorVersion + (package.packageHeader.indexSubVariant == 256 ? " (No subvariant)" : " (Subvariant " + package.packageHeader.indexSubVariant + ")");
            packageInfoLabel.Text += "\nEndian: " + (package.packageHeader.isBigEndian ? "Big-endian" : "Little-endian");
            packageInfoLabel.Text += "\nLast modified: " + package.packageHeader.lastModified;
        }

        private void SavePackage(object sender, RoutedEventArgs e)
        {
            if (package != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save package file";
                saveFileDialog.Filter = "DBPF Package (*.package) | *.package";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string oldPackagePath = ((FileStream)package.reader.BaseStream).Name; //remember the path of the currently open package

                    byte[] output = package.dumpToByteArray();

                    if (oldPackagePath == saveFileDialog.FileName)
                    {
                        package.reader.Close(); //if user saved to the same package that is currently open, close the binaryread on that package so that they can actually write to it, which is fine because its contents are about to change so we would've had to reload anyway
                    }

                    File.WriteAllBytes(saveFileDialog.FileName, output);

                    MessageBox.Show("Package saved.", "Task complete");

                    if (oldPackagePath == saveFileDialog.FileName)
                    {
                        openPackageAtPath(oldPackagePath);  //and finally, if the user just saved to the same package that is currently open, reload it.
                    }
                }
            }
        }

        private void exportCurrentlySelectedItem(object sender, RoutedEventArgs e)
        {
            if (FileListView.SelectedItem == null)
            {
                return;
            }

            FileEntry f = fileEntryLookupForListView[(ListViewItem)FileListView.SelectedItem];

            ExportDetails details = f.typeID.typeIDSpecificExport(f);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export file";
            saveFileDialog.Filter = details.filter;
            saveFileDialog.FileName = getHexStringFromHash(f.hash) + "." + details.extension;

            if (saveFileDialog.ShowDialog() == true) {
                File.WriteAllBytes(saveFileDialog.FileName, details.content);
            }
        }

        private void replaceCurrentlySelectedItem(object sender, RoutedEventArgs e)
        {
            if (FileListView.SelectedItem == null)
            {
                return;
            }

            FileEntry f = fileEntryLookupForListView[(ListViewItem)FileListView.SelectedItem];

            f.typeID.typeIDSpecificReplace(f);
        }

        private void createContextMenuForListViewItem(ListViewItem listItem)
        {
            ContextMenu contextMenu = new ContextMenu();
            listItem.ContextMenu = contextMenu;

            MenuItem export = new MenuItem();
            export.Header = "Export";
            export.Click += exportCurrentlySelectedItem;
            contextMenu.Items.Add(export);

            MenuItem replace = new MenuItem();
            replace.Header = "Replace";
            replace.Click += replaceCurrentlySelectedItem;
            contextMenu.Items.Add(replace);
        }

        private void fileListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = (ListView)sender;

            if (list.SelectedItem == null)
            {
                return;
            }

            FileEntry fileEntry = fileEntryLookupForListView[(ListViewItem)list.SelectedItem];       
        }

        private void MassExport(object sender, RoutedEventArgs e)
        {
            if (package != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "";
                saveFileDialog.Title = "Select a folder to export package contents to";
                saveFileDialog.FileName = "Save here";

                if (saveFileDialog.ShowDialog() == true)
                {
                    MessageBox.Show("Ready to begin mass export - you will be notified when the export is complete.\nIt should only take a couple of minutes at most, for large packages. Press OK to start.");

                    string dir = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);

                    foreach (FileEntry f in package.indexTable.files)
                    {
                        ExportDetails details = f.typeID.typeIDSpecificExport(f);
                        if (details == null) {
                            Debug.WriteLine("Skipping export of " + f.hash + " because its ExportDetails were null.");
                            continue;
                        }

                        string silentExportPath = System.IO.Path.Combine(dir, getHexStringFromHash(f.hash)+"."+details.extension);
                        File.WriteAllBytes(silentExportPath, details.content);
                    }

                    MessageBox.Show("Mass export complete.");
                }
            }
        }
    }
}

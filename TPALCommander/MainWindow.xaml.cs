using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
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

public enum EntryType
{
    Dir,
    File
}

public class DirectoryEntry
{
    public string Name { get; set; }

    public string Extension { get; set; }

    public string Size { get; set; }

    public DateTime Date { get; set; }

    public string Imagepath { get; set; }

    public EntryType Type { get; set; }

    public string Fullpath { get; set; }
}


namespace TPALCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    partial class MainWindow : Window
    {

        ObservableCollection<DirectoryEntry> entries = new ObservableCollection<DirectoryEntry>();
        ObservableCollection<DirectoryEntry> subEntries = new ObservableCollection<DirectoryEntry>();
        private int ListHeaderSize = 50;
        public MainWindow()
        {
            InitializeComponent();
            ObservableCollection<DirectoryEntry> Collection = new ObservableCollection<DirectoryEntry>();
            Collection.Add(new DirectoryEntry() { Name = "Xd", Date = DateTime.Now, Extension = "exe", Fullpath = "C:\\Fraps", Size = "1 000 kB", Type = EntryType.Dir, Imagepath = "Assets/Folder-icon.png" });
            //this.LeftView.ActualWidth = LeftGrid.ActualWidth;
            LeftView.ItemsSource = subEntries;
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = e.Source as ListViewItem;

            var entry = item.DataContext as DirectoryEntry;

            if (entry.Type == EntryType.Dir)
            {
                subEntries.Clear();

                foreach (string s in Directory.GetDirectories(entry.Fullpath))
                {
                    DirectoryInfo dir = new DirectoryInfo(s);
                    DirectoryEntry d = new DirectoryEntry()
                    {
                        Date = Directory.GetLastWriteTime(s),
                        Fullpath = dir.FullName,
                        Name = dir.Name,
                        Imagepath = "Assets/Folder-icon.png",
                        Type = EntryType.Dir
                    };

                    subEntries.Add(d);
                }
                foreach (string f in Directory.GetFiles(entry.Fullpath))
                {
                    FileInfo file = new FileInfo(f);
                    DirectoryEntry d = new DirectoryEntry()
                    {
                        Date = Directory.GetLastWriteTime(f),
                        Extension = file.Extension,
                        Name = file.Name,
                        Type = EntryType.File,
                        /*Imagepath = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName),*/
                        Fullpath = file.FullName
                    };
                    subEntries.Add(d);
                }

                item.DataContext = subEntries;
            }
        }

        private void List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - 10; // take into account vertical scrollbar

            gView.Columns[0].Width = 30.0d;

            gView.Columns[1].Width = (workingWidth - (gView.Columns[2].ActualWidth + gView.Columns[0].Width + gView.Columns[3].Width)) > ListHeaderSize 
                ? workingWidth - (gView.Columns[2].ActualWidth + gView.Columns[0].Width + gView.Columns[3].Width) : ListHeaderSize;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string s in Directory.GetLogicalDrives())
            {
                DirectoryEntry d = new DirectoryEntry() { Size = "<DIR>", Fullpath = s, Imagepath = "/Assets/Folder-icon.png", Name = s, Date = Directory.GetLastWriteTime(s), Type = EntryType.Dir };
                entries.Add(d);
            }
            this.RightView.DataContext = entries;
        }
    }
}
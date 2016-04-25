using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
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
    File,
    Up
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
            //LeftView.ItemsSource = Collection;
            LeftView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            RightView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            RightView.SetValue(ListViewItem.NameProperty, "RightItemView");
            AddHotKeys();
        }

        private void AddHotKeys()
        {
            try
            {
                RoutedCommand firstSettings = new RoutedCommand();
                firstSettings.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(firstSettings, copy_handler));

                //RoutedCommand secondSettings = new RoutedCommand();
                //secondSettings.InputGestures.Add(new KeyGesture(Key.V, ModifierKeys.Control));
                //CommandBindings.Add(new CommandBinding(secondSettings, My_second_event_handler));
            }
            catch (Exception exception)
            {
                //handle exception error
            }
        }

        private void copy_handler(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Copy!", "Copy");
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = e.Source as ListViewItem;

            var entry = item.DataContext as DirectoryEntry;

           doWork(entry);

        }

        private void List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - 10; // take into account vertical scrollbar

            gView.Columns[0].Width = 30.0d;


            gView.Columns[1].Width = (workingWidth - (gView.Columns[2].ActualWidth + gView.Columns[0].ActualWidth + gView.Columns[3].ActualWidth)) > ListHeaderSize
                ? workingWidth - (gView.Columns[2].ActualWidth + gView.Columns[0].ActualWidth + gView.Columns[3].ActualWidth) : ListHeaderSize;

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] logicalDrives = Directory.GetLogicalDrives();
            leftComboBox.ItemsSource = logicalDrives;
            leftComboBox.SelectedIndex = 0;
            rightComboBox.ItemsSource = logicalDrives;
            rightComboBox.SelectedIndex = 0;

            doWork(new DirectoryEntry()
            {
                Fullpath = logicalDrives[0],
                Type = EntryType.Dir
            });

            this.RightView.DataContext = subEntries;
            this.LeftView.DataContext = subEntries;
        }

        public void doWork(DirectoryEntry entry)
        {
            try
            {

                if ((entry.Type == EntryType.Up))
                {
                    if (entry.Fullpath.EndsWith("\\"))
                    {
                        int i = entry.Fullpath.Length - 2;
                        entry.Fullpath = entry.Fullpath.Substring(0, entry.Fullpath.Length - 2);
                    }
                    int index = entry.Fullpath.LastIndexOf("\\");
                    entry.Fullpath = entry.Fullpath.Substring(0, index + 1);
                    entry.Type = EntryType.Dir;
                }

                if (entry.Type == EntryType.Dir)
                {
                    subEntries.Clear();
                    if (entry.Fullpath.Length > 3)
                    {
                        subEntries.Add(new DirectoryEntry()
                        {
                            Imagepath = "Assets/arrow_up_left.png",
                            Extension = entry.Extension,
                            Date = entry.Date,
                            Fullpath = entry.Fullpath,
                            Name = "...",
                            Type = EntryType.Up
                        });
                    }


                    foreach (string s in Directory.GetDirectories(entry.Fullpath))
                    {
                        //try
                        //{
                            DirectoryInfo dir = new DirectoryInfo(s);
                            //System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(dir.FullName);
                            DirectoryEntry d = new DirectoryEntry()
                            {
                                Date = Directory.GetLastWriteTime(s),
                                Fullpath = dir.FullName,
                                Name = dir.Name,
                                Imagepath = "Assets/Folder-icon.png",
                                Type = EntryType.Dir
                            };

                            subEntries.Add(d);
                        //}
                        //catch (UnauthorizedAccessException exception)
                        //{
                            
                        //}
                    }
                    foreach (string f in Directory.GetFiles(entry.Fullpath))
                    {
                        FileInfo file = new FileInfo(f);
                        DirectoryEntry d = new DirectoryEntry()
                        {
                            Date = Directory.GetLastWriteTime(f),
                            Name = file.Name,
                            Type = EntryType.File,
                            /*Imagepath = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName),*/
                            Fullpath = file.FullName
                        };
                        if (file.Extension.Length > 1)
                            d.Extension = file.Extension.Substring(1);
                        subEntries.Add(d);
                    }

                    //if(sender)

                    RightView.DataContext = subEntries;
                    leftPathLabel.Content = String.Format(entry.Fullpath + "*.*");

                }
            }
            catch (UnauthorizedAccessException exception)
            {
                //MessageBox.Show(exception.Message, exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.IO.IOException exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (entry.Type == EntryType.File)
            {
                try
                {
                    System.Diagnostics.Process.Start(entry.Fullpath);
                }
                catch (System.ComponentModel.Win32Exception exception)
                {
                    MessageBox.Show(exception.Message, exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void rightComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            String path = (String) cb.SelectedItem;
            if(path != String.Empty)
            {
                doWork(new DirectoryEntry()
                {
                    Fullpath = path,
                    Type = EntryType.Dir
                }); 
            }
        }
    }
}
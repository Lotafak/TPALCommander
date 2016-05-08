using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TPALCommander.Properties;
using ComboBox = System.Windows.Controls.ComboBox;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;

public enum EntryType
{
    Dir,
    File,
    Up
}

public class DirectoryEntry
{
    public bool OrderBy { get; set; }
    public String DateFormatter { get; set; }

    public string Name { get; set; }

    public string Extension { get; set; }

    public string Size { get; set; }

    public String StringDate
    {
        get
        {
            return String.Format(this.DateFormatter, this.Date);
        }
        set { StringDate = value; }
    }

    public DateTime Date { get; set; }

    public string Imagepath { get; set; }

    public EntryType Type { get; set; }

    public string Fullpath { get; set; }

    public bool View { get; set; }

    public DirectoryEntry()
    {
        DateFormatter = Resources.Culture.Name == "pl" ? "{0:dd/MM/yyyy HH:mm}" : "{0:MM/dd/yyyy hh:mm tt}";
        OrderBy = true;
    }
}


namespace TPALCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    partial class MainWindow
    {

        private ObservableCollection<DirectoryEntry> leftEntries = new ObservableCollection<DirectoryEntry>();
        private ObservableCollection<DirectoryEntry> rightEntries = new ObservableCollection<DirectoryEntry>();
        private ObservableCollection<DirectoryEntry> subEntries = new ObservableCollection<DirectoryEntry>();
        private List<DirectoryEntry> listToCopy = new List<DirectoryEntry>();
        private List<DirectoryEntry> listToCut = new List<DirectoryEntry>();
        private DirectoryEntry leftPreviousEntry = null;
        private DirectoryEntry rightPreviousEntry = null;
        private int ListHeaderSize = 50;

        private BackgroundWorker backgroundWorker1;

        public MainWindow()
        {
            CultureResources.ChangeCulture(Properties.Settings.Default.DefaultCulture);

            InitializeComponent();
            LeftView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            RightView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusBar.Visibility = Visibility.Hidden;
            DoWork((DirectoryEntry) e.Result);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            DirectoryEntry destinationPath = (DirectoryEntry) e.Argument;
            long copyingSize = 0;
            long copiedSize = 0;

            //Calculating size
            foreach (DirectoryEntry item in listToCopy)
            {
                if (item.Type == EntryType.Dir)
                {
                    DirectoryInfo di = new DirectoryInfo(item.Fullpath);
                    copyingSize += DirSize(di);
                }
                else if (item.Type == EntryType.File)
                {
                    FileInfo fi = new FileInfo(item.Fullpath);
                    copyingSize += fi.Length;
                }
            }

            foreach (DirectoryEntry item in listToCopy)
            {

                if (item.Type == EntryType.Dir)
                {
                    CopyFolder(item.Fullpath, destinationPath.Fullpath + "\\" + item.Name);
                    DirectoryInfo di = new DirectoryInfo(item.Fullpath);
                    //copiedSize += DirSize(di);
                    //backgroundWorker1.ReportProgress((int)(copiedSize/copyingSize)*100);
                }
                else if (item.Type == EntryType.File)
                {
                    FileInfo fi = new FileInfo(item.Fullpath);
                    copiedSize += fi.Length;
                    //File.Copy(item.Fullpath, Path.Combine(destinationPath.Fullpath, item.Name));
                    fi.CopyTo(Path.Combine(destinationPath.Fullpath, item.Name));
                    backgroundWorker1.ReportProgress((int) (((float) copiedSize/(float) copyingSize)*100));
                }
            }
            e.Result = destinationPath;

        }

        private void copy_handler(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Copy!", "Copy");
            ListView listView = sender as ListView;
            //listView.
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = e.Source as ListViewItem;
            var entry = item.DataContext as DirectoryEntry;

            if (LeftView.ItemContainerGenerator.IndexFromContainer(item) >= 0)
            {
                entry.View = false;
            }
            else
            {
                entry.View = true;
            }

            DoWork(entry);
        }

        private void List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null)
            {
                GridView gView = listView.View as GridView;

                var workingWidth = listView.ActualWidth - 20; // take into account vertical scrollbar

                gView.Columns[0].Width = 30.0d;

                gView.Columns[1].Width = (workingWidth -
                                          (gView.Columns[2].ActualWidth + gView.Columns[0].ActualWidth +
                                           gView.Columns[3].ActualWidth + 10 + gView.Columns[4].ActualWidth)) >
                                         ListHeaderSize
                    ? workingWidth -
                      (gView.Columns[2].ActualWidth + gView.Columns[0].ActualWidth + gView.Columns[3].ActualWidth +
                       gView.Columns[4].ActualWidth)
                    : ListHeaderSize;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] logicalDrives = Directory.GetLogicalDrives();
            LeftComboBox.ItemsSource = logicalDrives;
            LeftComboBox.SelectedIndex = 0;
            RightComboBox.ItemsSource = logicalDrives;
            RightComboBox.SelectedIndex = 0;

            DoWork(new DirectoryEntry()
            {
                Fullpath = logicalDrives[0],
                Type = EntryType.Dir
            });

            this.RightView.DataContext = rightEntries;
            this.LeftView.DataContext = leftEntries;
        }

        public void DoWork(DirectoryEntry entry)
        {
            try
            {
                if ((entry.Type == EntryType.Up))
                {
                    if (entry.Fullpath.EndsWith("\\"))
                    {
                        entry.Fullpath = entry.Fullpath.Substring(0, entry.Fullpath.Length - 2);
                    }
                    int index = entry.Fullpath.LastIndexOf("\\", StringComparison.Ordinal);
                    entry.Fullpath = entry.Fullpath.Substring(0, index + 1);
                    entry.Type = EntryType.Dir;
                }

                if (entry.Type == EntryType.Dir)
                {
                    if (!entry.View)
                    {
                        FillContainer(leftEntries, entry);
                    }
                    else
                    {
                        FillContainer(rightEntries, entry);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                //MessageBox.Show(exception.Message, exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                foreach (var item in RightComboBox.Items)
                {
                    if (!entry.View)
                    {
                        if (item.Equals(leftPreviousEntry.Fullpath))
                            LeftComboBox.SelectedItem = item;
                    }
                    else
                    {
                        if (item.Equals(rightPreviousEntry.Fullpath))
                            RightComboBox.SelectedItem = item;
                    }
                }
            }

            if (entry.Type == EntryType.File)
            {
                try
                {
                    System.Diagnostics.Process.Start(entry.Fullpath);
                }
                catch (Win32Exception exception)
                {
                    MessageBox.Show(exception.Message, exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void FillContainer(ObservableCollection<DirectoryEntry> collection, DirectoryEntry entry)
        {
            collection.Clear();
            if (entry.Fullpath.Length > 3)
            {
                collection.Add(new DirectoryEntry()
                {
                    Imagepath = "Assets/arrow_up_left.png",
                    Extension = entry.Extension,
                    Date = entry.Date,
                    Fullpath = entry.Fullpath,
                    Name = "...",
                    Type = EntryType.Up
                });
            }

            foreach (string s in Directory.EnumerateDirectories(entry.Fullpath))
            {
                //Directory.GetAccessControl(s);
                DirectoryInfo dir = new DirectoryInfo(s);
                DirectoryEntry d = new DirectoryEntry()
                {
                    Fullpath = dir.FullName,
                    Name = dir.Name,
                    Imagepath = "Assets/Folder-icon.png",
                    Type = EntryType.Dir,
                    Date = Directory.GetCreationTime(s)
                };
                //d.Date = String.Format(d.DateFormatter, Directory.GetCreationTime(s), CultureInfo.CreateSpecificCulture(Properties.Resources.Culture.Name));

                collection.Add(d);
            }

            //try
            //{
            //    collection = new ObservableCollection<DirectoryEntry>(collection.OrderBy(i => i.Name));

            //    collection.Move(collection.IndexOf(collection.First(x => x.Name == "...")), 0);

            //}
            //catch (InvalidOperationException) { }

            //ObservableCollection<DirectoryEntry> subCollection = new ObservableCollection<DirectoryEntry>();
            //ObservableCollection<DirectoryEntry> wholeCollection = new ObservableCollection<DirectoryEntry>();

            foreach (string f in Directory.GetFiles(entry.Fullpath))
            {

                FileInfo file = new FileInfo(f);
                NumberFormatInfo nfi = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = " ";
                nfi.NumberDecimalDigits = 0;
                DirectoryEntry d = new DirectoryEntry()
                {
                    Name = file.Name,
                    Type = EntryType.File,
                    /*Imagepath = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName),*/
                    Size = Math.Round(file.Length/1024.0).ToString("n", nfi),
                    //ToString("n", nfi)
                    Fullpath = file.FullName,
                    Date = Directory.GetCreationTime(f)
                };

                if (file.Extension.Length > 1)
                    d.Extension = file.Extension.Substring(1);
                collection.Add(d);
            }

            //try
            //{
            //    subCollection = new ObservableCollection<DirectoryEntry>(subCollection.OrderBy(i => i.Name));
            //    foreach (var c in collection.Union(subCollection))
            //    {
            //        wholeCollection.Add(c);
            //    }
            //}
            //catch (InvalidOperationException) { }

            //if(sender)
            if (!entry.View)
            {
                LeftView.DataContext = collection;
                LeftPathLabel.Content =
                    String.Format(entry.Fullpath.EndsWith("\\") ? (entry.Fullpath) : entry.Fullpath + "\\");
                leftPreviousEntry = entry;
            }
            else
            {
                RightView.DataContext = collection;
                RightPathLabel.Content =
                    String.Format(entry.Fullpath.EndsWith("\\") ? (entry.Fullpath) : entry.Fullpath + "\\");
                rightPreviousEntry = entry;
            }
        }

        private void rightComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            String path = (String) cb.SelectedItem;
            bool view = false;

            if (Equals(cb, objB: LeftComboBox))
                view = false;
            else
                view = true;

            if (path != String.Empty)
            {
                DoWork(new DirectoryEntry()
                {
                    Fullpath = path,
                    Type = EntryType.Dir,
                    View = view
                });
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool setPolishCulture = (Equals(sender, PolishMenuItem));

            CultureResources.ChangeCulture(new CultureInfo(setPolishCulture ? "pl" : "en"));
            PolishMenuItem.IsChecked = setPolishCulture;
            EnglishMenuItem.IsChecked = !setPolishCulture;
            Refresh();
        }

        public void Refresh()
        {
            DoWork(new DirectoryEntry()
            {
                Type = EntryType.Dir,
                Fullpath = LeftPathLabel.Content.ToString(),
                View = false
            });
            DoWork(new DirectoryEntry()
            {
                Type = EntryType.Dir,
                Fullpath = RightPathLabel.Content.ToString(),
                View = true
            });
        }

        private void CopyCommandBinding(object sender, ExecutedRoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            listToCopy.Clear();
            if (lv != null)
                foreach (DirectoryEntry item in lv.SelectedItems)
                {
                    listToCopy.Add(item);
                }
        }

        private void CutCommandBinding(object sender, ExecutedRoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            listToCopy.Clear();
            listToCut.Clear();
            if (lv != null)
                foreach (DirectoryEntry item in lv.SelectedItems)
                {
                    listToCopy.Add(item);
                    MessageBox.Show(item.Fullpath);
                }
        }

        private void PasteCommandBinding(object sender, ExecutedRoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            DirectoryEntry destinationPath = new DirectoryEntry();
            if (listToCopy != null && (listToCopy.Count > 0))
            {
                if (lv != null)
                    switch (lv.Name)
                    {
                        case "LeftView":
                            destinationPath = leftPreviousEntry;
                            break;
                        case "RightView":
                            destinationPath = rightPreviousEntry;
                            break;
                    }
                try
                {
                    StatusBar.Visibility = Visibility.Visible;
                    backgroundWorker1.RunWorkerAsync(destinationPath);
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message, ex.Source);
                }
                catch (System.IO.IOException ex)
                {
                    MessageBox.Show("yhm");
                }
            }
        }

        private void SetStatus(long l)
        {
            if (!ProgressBar.Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<long>(SetStatus), l);
            }
            else
            {
                ProgressBar.Value = l;
                ProgressBar.UpdateLayout();
            }
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        public void CopyFolder(String sourcePath, String destinationPath)
        {
            DirectoryInfo dir = new DirectoryInfo(sourcePath);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourcePath);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destinationPath, file.Name);
                file.CopyTo(temppath, true);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destinationPath, subdir.Name);
                CopyFolder(subdir.FullName, temppath);
            }
        }

        private void DeleteCommandBinding(object sender, ExecutedRoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv != null && lv.SelectedItems.Count > 1)
            {

                StringBuilder sb = new StringBuilder();
                sb.Append("\n");
                foreach (DirectoryEntry directoryEntry in lv.SelectedItems)
                {
                    sb.Append("\n").Append(directoryEntry.Name);
                }

                var messageBoxResult = MessageBox.Show(Properties.Resources.DeleteFilesWarning + sb, "TPALCommander",
                    MessageBoxButton.OKCancel);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    foreach (DirectoryEntry directoryEntry in lv.SelectedItems)
                    {
                        File.Delete(directoryEntry.Fullpath);
                    }
                }
            }
            else if (lv != null && lv.SelectedItem != null)
            {
                DirectoryEntry file = (DirectoryEntry) lv.SelectedItem;
                MessageBoxResult messageBoxResult =
                    MessageBox.Show(Properties.Resources.DeleteFileWarning + file.Name + " ?", "TPALCommander",
                        MessageBoxButton.OKCancel);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    if (file.Type == EntryType.Dir)
                        DeleteFolder(file.Fullpath);
                    else
                        File.Delete(file.Fullpath);
                }
            }
            DirectoryEntry de = new DirectoryEntry()
            {
                Type = EntryType.Dir,
                Imagepath = "Assets/Folder-icon.png"
            };
            if (lv != null)
                switch (lv.Name)
                {
                    case "LeftView":
                        de = leftPreviousEntry;
                        break;
                    case "RightView":
                        de = rightPreviousEntry;
                        break;
                }
            DoWork(de);
        }

        public void DeleteFolder(String sourcePath)
        {
            DirectoryInfo dir = new DirectoryInfo(sourcePath);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourcePath);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();
            try
            {
                foreach (FileInfo file in files)
                {
                    //file.SetAccessControl(new FileSecurity(file.FullName, AccessControlSections.All));
                    file.Delete();
                }

                foreach (DirectoryInfo subdir in dirs)
                {
                    DeleteFolder(subdir.FullName);
                }
                //dir.SetAccessControl(new DirectorySecurity(new D/*)*/);
                dir.Delete();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader gridViewColumnHeader = sender as GridViewColumnHeader;
            if (gridViewColumnHeader != null && leftPreviousEntry != null && gridViewColumnHeader.Content != null &&
                gridViewColumnHeader.Content.Equals("Name"))
            {
                leftPreviousEntry.OrderBy = !leftPreviousEntry.OrderBy;
                DoWork(leftPreviousEntry);
            }
            //gridViewColumnHeader.DataContext
        }

        private void RightHeaderClickHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader gridViewColumnHeader = sender as GridViewColumnHeader;
            if (gridViewColumnHeader != null && rightPreviousEntry != null && gridViewColumnHeader.Content != null)
            {
                ObservableCollection<DirectoryEntry> fileTemp = new ObservableCollection<DirectoryEntry>();
                ObservableCollection<DirectoryEntry> folderTemp = new ObservableCollection<DirectoryEntry>();
                ObservableCollection<DirectoryEntry> orderedTemp = new ObservableCollection<DirectoryEntry>();
                ObservableCollection<DirectoryEntry> whole = new ObservableCollection<DirectoryEntry>();

                rightPreviousEntry.OrderBy = !rightPreviousEntry.OrderBy;

                fileTemp =
                    new ObservableCollection<DirectoryEntry>(((ObservableCollection<DirectoryEntry>)
                        gridViewColumnHeader.DataContext).Where(
                            i => i.Type == EntryType.File));

                folderTemp =
                    new ObservableCollection<DirectoryEntry>(((ObservableCollection<DirectoryEntry>)
                        gridViewColumnHeader.DataContext).Where(
                            i => i.Type == EntryType.Dir));

                if (gridViewColumnHeader.Content.Equals(Properties.Resources.ListHeaderName))
                {
                    orderedTemp = rightPreviousEntry.OrderBy
                               ? new ObservableCollection<DirectoryEntry>(fileTemp.OrderBy(i => i.Name))
                               : new ObservableCollection<DirectoryEntry>(fileTemp.OrderByDescending(i => i.Name));
                }

                else if (gridViewColumnHeader.Content.Equals(Properties.Resources.ListHeaderDate))
                {
                    orderedTemp = rightPreviousEntry.OrderBy
                        ? new ObservableCollection<DirectoryEntry>(fileTemp.OrderBy(i => i.Date))
                        : new ObservableCollection<DirectoryEntry>(fileTemp.OrderByDescending(i => i.Date));
                }
                else if (gridViewColumnHeader.Content.Equals(Properties.Resources.ListHeaderExtension))
                {
                    orderedTemp = rightPreviousEntry.OrderBy
                        ? new ObservableCollection<DirectoryEntry>(fileTemp.OrderBy(i => i.Extension))
                        : new ObservableCollection<DirectoryEntry>(fileTemp.OrderByDescending(i => i.Extension));
                }
                else if (gridViewColumnHeader.Content.Equals(Properties.Resources.ListHeaderSize))
                {
                    orderedTemp = rightPreviousEntry.OrderBy
                        ? new ObservableCollection<DirectoryEntry>(fileTemp.OrderBy(i => i.Size, new IntComparer()))
                        : new ObservableCollection<DirectoryEntry>(fileTemp.OrderByDescending(i => i.Size, new IntComparer()));
                }

                try
                {
                    whole.Add(
                        ((ObservableCollection<DirectoryEntry>) gridViewColumnHeader.DataContext).First(
                            i => i.Type == EntryType.Up));
                }

                catch (InvalidOperationException) { }

                foreach (var o in folderTemp.Union(orderedTemp))
                {
                    whole.Add(o);
                }

                RightView.DataContext = whole;
            }
        }
    }
}
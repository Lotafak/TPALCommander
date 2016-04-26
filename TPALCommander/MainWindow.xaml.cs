using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

    public bool View { get; set; }
}


namespace TPALCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    partial class MainWindow : Window
    {

        private ObservableCollection<DirectoryEntry> leftEntries = new ObservableCollection<DirectoryEntry>();
        private ObservableCollection<DirectoryEntry> rightEntries = new ObservableCollection<DirectoryEntry>();
        private ObservableCollection<DirectoryEntry> subEntries = new ObservableCollection<DirectoryEntry>();
        private List<DirectoryEntry> listToCopy = new List<DirectoryEntry>();
        private List<DirectoryEntry> listToCut = new List<DirectoryEntry>();
        private DirectoryEntry leftPreviousEntry = null;
        private DirectoryEntry rightPreviousEntry = null;
        private int ListHeaderSize = 50;
        public MainWindow()
        {
            CultureResources.ChangeCulture(Properties.Settings.Default.DefaultCulture);

            InitializeComponent();
            //UpdateStatusLabel();
            ObservableCollection<DirectoryEntry> Collection = new ObservableCollection<DirectoryEntry>();
            //Collection.Add(new DirectoryEntry() { Name = "Xd", Date = DateTime.Now, Extension = "exe", Fullpath = "C:\\Fraps", Size = "1 000 kB", Type = EntryType.Dir, Imagepath = "Assets/Folder-icon.png" });
            //LeftView.ItemsSource = Collection;
            LeftView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            RightView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            //RightView.SetValue(ListViewItem.NameProperty, "RightItemView");
            AddHotKeys();
        }

        private void AddHotKeys()
        {
            try
            {
                //RoutedCommand firstSettings = new RoutedCommand();
                //firstSettings.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
                //CommandBindings.Add(new CommandBinding(firstSettings, copy_handler));

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

            doWork(entry);
        }

        private void List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - 10; // take into account vertical scrollbar

            gView.Columns[0].Width = 30.0d;

            gView.Columns[1].Width = (workingWidth - (gView.Columns[2].ActualWidth + gView.Columns[0].ActualWidth + gView.Columns[3].ActualWidth + 10)) > ListHeaderSize
                ? workingWidth - (gView.Columns[2].ActualWidth + gView.Columns[0].ActualWidth + gView.Columns[3].ActualWidth) : ListHeaderSize;
            //gView.Columns[1].Width
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] logicalDrives = Directory.GetLogicalDrives();
            LeftComboBox.ItemsSource = logicalDrives;
            LeftComboBox.SelectedIndex = 0;
            RightComboBox.ItemsSource = logicalDrives;
            RightComboBox.SelectedIndex = 0;

            doWork(new DirectoryEntry()
            {
                Fullpath = logicalDrives[0],
                Type = EntryType.Dir
            });

            this.RightView.DataContext = rightEntries;
            this.LeftView.DataContext = leftEntries;
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
            catch (UnauthorizedAccessException exception)
            {
                //MessageBox.Show(exception.Message, exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.IO.IOException exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                //doWork(new DirectoryEntry()
                //{
                //    Fullpath = "C:\\",
                //    Type = EntryType.Dir
                //});
                //doWork(previous);
                //LeftComboBox.SelectedIndex = 1;
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
                catch (System.ComponentModel.Win32Exception exception)
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
                    Date = Directory.GetLastWriteTime(s),
                    Fullpath = dir.FullName,
                    Name = dir.Name,
                    Imagepath = "Assets/Folder-icon.png",
                    Type = EntryType.Dir
                };

                collection.Add(d);
            }
            foreach (string f in Directory.GetFiles(entry.Fullpath))
            {
                FileInfo file = new FileInfo(f);
                NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = " ";
                nfi.NumberDecimalDigits = 0;
                DirectoryEntry d = new DirectoryEntry()
                {
                    Date = Directory.GetLastWriteTime(f),
                    Name = file.Name,
                    Type = EntryType.File,
                    /*Imagepath = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName),*/
                    Size = file.Length.ToString("n", nfi),
                    Fullpath = file.FullName
                };
                if (file.Extension.Length > 1)
                    d.Extension = file.Extension.Substring(1);
                collection.Add(d);
            }

            //if(sender)
            if (!entry.View)
            {
                LeftView.DataContext = collection;
                LeftPathLabel.Content = String.Format(entry.Fullpath.EndsWith("\\") ? (entry.Fullpath) : entry.Fullpath + "\\");
                leftPreviousEntry = entry;
            }
            else
            {
                RightView.DataContext = collection;
                RightPathLabel.Content = String.Format(entry.Fullpath.EndsWith("\\") ? (entry.Fullpath) : entry.Fullpath + "\\");
                rightPreviousEntry = entry;
            }
        }

        private void rightComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            String path = (String)cb.SelectedItem;
            bool view = false;

            if (cb == LeftComboBox)
                view = false;
            else
                view = true;

            if (path != String.Empty)
            {
                doWork(new DirectoryEntry()
                {
                    Fullpath = path,
                    Type = EntryType.Dir,
                    View = view
                });
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool setPolishCulture = (sender == PolishMenuItem);

            CultureResources.ChangeCulture(new CultureInfo(setPolishCulture ? "pl" : "en"));
            PolishMenuItem.IsChecked = setPolishCulture;
            EnglishMenuItem.IsChecked = !setPolishCulture;
        }

        private void CopyCommandBinding(object sender, ExecutedRoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            listToCopy.Clear();
            foreach (DirectoryEntry item in lv.SelectedItems)
            {
                listToCopy.Add(item);
                MessageBox.Show(item.Fullpath);
            }
        }


        private void CutCommandBinding(object sender, ExecutedRoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            listToCopy.Clear();
            listToCut.Clear();
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
            if (listToCopy != null)
            {
                switch (lv.Name)
                {
                    case "LeftView":
                        destinationPath = leftPreviousEntry;
                        break;
                    case "RightView":
                        destinationPath = rightPreviousEntry;
                        break;
                }
                foreach (DirectoryEntry item in listToCopy)
                {
                    File.Copy(item.Fullpath, Path.Combine(destinationPath.Fullpath, item.Name));
                }
            }
            doWork(destinationPath);
        }
    }

    // Create a class that implements ICommand and accepts a delegate.
    //public class SimpleDelegateCommand : ICommand
    //{
    //    // Specify the keys and mouse actions that invoke the command. 
    //    public Key GestureKey { get; set; }
    //    public ModifierKeys GestureModifier { get; set; }
    //    public MouseAction MouseGesture { get; set; }

    //    Action<object> _executeDelegate;

    //    public SimpleDelegateCommand(Action<object> executeDelegate)
    //    {
    //        _executeDelegate = executeDelegate;
    //    }

    //    public void Execute(object parameter)
    //    {
    //        _executeDelegate(parameter);
    //    }

    //    public bool CanExecute(object parameter) { return true; }
    //    public event EventHandler CanExecuteChanged;

    //    public SimpleDelegateCommand ChangeColorCommand
    //    {
    //        get { return changeColorCommand; }
    //    }

    //    private SimpleDelegateCommand changeColorCommand;
    //}

    //private void InitializeCommand()
    //{
    //    originalColor = this.Background;

    //    changeColorCommand = new SimpleDelegateCommand(x => this.ChangeColor(x));

    //    DataContext = this;
    //    changeColorCommand.GestureKey = Key.C;
    //    changeColorCommand.GestureModifier = ModifierKeys.Control;
    //    ChangeColorCommand.MouseGesture = MouseAction.RightClick;
    //}

    //private Brush originalColor, alternateColor;

    //// Switch the Background color between
    //// the original and selected color.
    //private void ChangeColor(object colorString)
    //{
    //    if (colorString == null)
    //    {
    //        return;
    //    }

    //    Color newColor =
    //        (Color)ColorConverter.ConvertFromString((String)colorString);

    //    alternateColor = new SolidColorBrush(newColor);

    //    if (this.Background == originalColor)
    //    {
    //        this.Background = alternateColor;
    //    }
    //    else
    //    {
    //        this.Background = originalColor;
    //    }
    //}

    //public class AddToInputBinding
    //{
    //    public InputBinding Binding { get; set; }
    //    public static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached(
    //      "Binding", typeof(InputBinding), typeof(AddToInputBinding), new PropertyMetadata
    //      {
    //          PropertyChangedCallback = (obj, e) =>
    //          {
    //              ((UIElement)obj).InputBindings.Add((InputBinding)e.NewValue);
    //          }
    //      });
    //}
}
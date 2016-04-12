using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public MainWindow()
        {
            InitializeComponent();
            ObservableCollection<DirectoryEntry> Collection = new ObservableCollection<DirectoryEntry>();
            Collection.Add(new DirectoryEntry() {Name = "Xd", Date  = DateTime.Now, Extension = "exe", Fullpath = "C:/Fraps", Size = "1 000 kB", Type = EntryType.Dir, Imagepath = "s"});
            //this.LeftView.ActualWidth = LeftGrid.ActualWidth;
            LeftView.ItemsSource = Collection;
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - 10; // take into account vertical scrollbar
            var col1 = 0.40;
            var col2 = 0.20;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth - gView.Columns[1].Width - gView.Columns[0].Width;
        }
    }
}

using Microsoft.Win32;
using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Xml.Linq;

namespace ResxLocalizer {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : RibbonWindow {
        public MainWindow() {
            InitializeComponent();

            cvs.Source = oc;
            lvItems.DataContext = cvs;
        }

        CollectionViewSource cvs = new CollectionViewSource();
        ObservableCollection<Item> oc = new ObservableCollection<Item>();

        public class Item : INotifyPropertyChanged {
            String _Name;
            public String Name { get { return _Name; } set { _Name = value; Fire("Name"); } }

            public Caset c1, c2;

            public String Disp1 {
                get { return c1.Gets(Name); }
                set { c1.Sets(Name, value); Fire("Disp1"); }
            }
            public String Disp2 {
                get { return c2.Gets(Name); }
                set { c2.Sets(Name, value); Fire("Disp2"); }
            }

            protected void Fire(String s) {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(s));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void lvItems_ItemActivation(object sender, MouseButtonEventArgs e) {
            EditIt();
        }

        private void EditIt() {
            Item it = lvItems.SelectedItem as Item;
            if (it == null) return;

            EdWin form = new EdWin();
            form.l1.Content = "" + ((GridViewColumn)FindName("h1")).Header;
            form.l2.Content = "" + ((GridViewColumn)FindName("h2")).Header;
            form.DataContext = it;

            form.ShowDialog();
        }

        Caset c1 = new Caset(), c2 = new Caset();

        private void mOpenResx_Click(object sender, RoutedEventArgs e) {
            CloseMe();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.resx|*.resx";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() ?? false) {
                String[] alfp = ofd.FileNames;
                OpenResx(alfp);
            }
        }

        private void mSaveResx_Click(object sender, RoutedEventArgs e) {
            c1.Save();
            c2.Save();
            MessageBox.Show("保存しました。", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public class Caset {
            XDocument x;
            String fp;

            public void Load(String fp) {
                x = XDocument.Load(this.fp = fp);
            }

            public IEnumerable<String> Names {
                get {
                    if (x != null) {
                        foreach (var s in x
                            .Elements("root")
                            .Elements("data")
                            .Where(eld => eld.Attribute("type") == null || eld.Attribute("type").Value == "System.String")
                            .Where(eld => eld.Attribute("mimetype") == null)
                            .Attributes("name")
                            .Select(p => p.Value)
                            .Where(p => !p.StartsWith(">>"))
                            .OrderBy(p => p)
                        ) {
                            yield return s;
                        }
                    }
                }
            }

            public string Gets(string Name) {
                if (x != null) {
                    foreach (var elv in x.Elements("root").Elements("data").Where(eld => eld.Attribute("name") != null && eld.Attribute("name").Value == Name).Elements("value")) {
                        return elv.Value;
                    }
                }
                return String.Empty;
            }

            public void Sets(string Name, string value) {
                if (x == null) return;
                XElement eld = null;
                var elr = x.Elements("root").First();
                eld = elr.Elements("data").Where(el => el.Attribute("name") != null && el.Attribute("name").Value == Name).FirstOrDefault();
                if (eld == null) {
                    elr.Add(eld = new XElement("data"
                        , new XAttribute(XNamespace.Xml + "space", "preserve")
                        , new XAttribute("name", Name)
                        ));
                }
                var elv = eld.Element("value");
                if (elv == null) {
                    eld.Add(elv = new XElement("value"));
                }
                elv.RemoveAll();
                elv.Add(new XText(value));
            }

            public void Save() {
                if (x == null) return;
                x.Save(fp);
            }
        }

        private void mSwap_Click(object sender, RoutedEventArgs e) {
            foreach (Item it in lvItems.SelectedItems.OfType<Item>()) {
                String v = it.Disp2;
                it.Disp2 = it.Disp1;
                it.Disp1 = v;
            }
        }

        private void mEdit_Click(object sender, RoutedEventArgs e) {
            EditIt();
        }

        public void CloseMe() {
            oc.Clear();
            c1 = new Caset();
            c2 = new Caset();
        }

        public void OpenResx(String[] alfp) {
            CloseMe();

            if (alfp.Length >= 1) c1.Load(alfp[0]);
            if (alfp.Length >= 2) c2.Load(alfp[1]);

            ((GridViewColumn)FindName("h1")).Header = (alfp.Length >= 1) ? System.IO.Path.GetFileName(alfp[0]) : "無し";
            ((GridViewColumn)FindName("h2")).Header = (alfp.Length >= 2) ? System.IO.Path.GetFileName(alfp[1]) : "無し";

            foreach (String k in c1.Names.Concat(c2.Names).Distinct()) {
                oc.Add(new Item { c1 = c1, c2 = c2, Name = k });
            }
        }
    }
}

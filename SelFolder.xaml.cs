using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ResxLocalizer {
    /// <summary>
    /// SelFolder.xaml の相互作用ロジック
    /// </summary>
    public partial class SelFolder : Window {
        public SelFolder() {
            InitializeComponent();

            Listup();
        }

        private void Listup() {
            var isf = IsolatedStorageFile.GetUserStoreForAssembly();
            isf.CreateDirectory("conf");
            cbConf.DataContext = isf.GetFileNames("conf\\*").Select(fp => System.IO.Path.GetFileName(fp));
        }

        private void bRefDir_Click(object sender, RoutedEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = tbDir.Text;
            if (fbd.ShowDialog(null) != System.Windows.Forms.DialogResult.OK) return;
            tbDir.Text = fbd.SelectedPath;

            Refr();
        }

        ResName[] alrn;
        IGrouping<String, ResName>[] alg;
        PerLang[] alpl;

        private void Refr() {
            if (String.IsNullOrEmpty(tbDir.Text)) return;

            alrn = Directory.GetFiles(tbDir.Text, "*.resx").Select(fp => new ResName(fp)).ToArray();
            alg = alrn.GroupBy(p => p.Language).ToArray();
            lbLang.DataContext = alpl = alg.Select(g => new PerLang { g = g }).ToArray();

            int n = 0;
            foreach (var pl in alpl) {
                pl.IsSelected = true;
                n++;
                if (n == 2) break;
            }
        }

        class PerLang : INotifyPropertyChanged {
            public IGrouping<String, ResName> g;

            bool _IsSelected;
            public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; Fires("IsSelected"); } }

            public String Disp { get { return String.IsNullOrEmpty(g.Key) ? "(Default)" : g.Key; } }

            public String Lang { get { return g.Key; } }

            public event PropertyChangedEventHandler PropertyChanged;

            void Fires(string props) {
                if (PropertyChanged != null) foreach (String prop in props.Split(',')) PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        private void tbDir_LostFocus(object sender, RoutedEventArgs e) {
            Refr();
        }

        public IEnumerable<IEnumerable<ResName>> sels;

        private void bOk_Click(object sender, RoutedEventArgs e) {
            if (alpl == null || alpl.Count(pl => pl.IsSelected) != 2) return;
            sels = alpl.Where(p => p.IsSelected).Select(p => p.g);
            DialogResult = true;
        }

        private void bReadConf_Click(object sender, RoutedEventArgs e) {
            var isf = IsolatedStorageFile.GetUserStoreForAssembly();
            using (var si = isf.OpenFile("conf\\" + cbConf.Text, FileMode.Open)) {
                XDocument xd = XDocument.Load(si);
                var el = xd.Element("ResxLocalizer");
                tbDir.Text = el.Attribute("Dir").Value;
                Refr();
                String[] langs = el.Attribute("Langs").Value.Split(',');

                foreach (var pl in alpl) {
                    pl.IsSelected = langs.Contains(pl.Lang);
                }
            }
        }

        private void bSaveConf_Click(object sender, RoutedEventArgs e) {
            XDocument xd = new XDocument(
                new XElement("ResxLocalizer"
                    , new XAttribute("Dir", tbDir.Text)
                    , new XAttribute("Langs", String.Join(",", alpl.Where(p => p.IsSelected).Select(p => p.Lang).ToArray()))
                    )
                );

            var isf = IsolatedStorageFile.GetUserStoreForAssembly();
            using (var os = isf.CreateFile("conf\\" + cbConf.Text)) {
                xd.Save(os);
            }
            Listup();
        }

        private void bDelConf_Click(object sender, RoutedEventArgs e) {
            var isf = IsolatedStorageFile.GetUserStoreForAssembly();
            isf.DeleteFile("conf\\" + cbConf.Text);
        }
    }
}

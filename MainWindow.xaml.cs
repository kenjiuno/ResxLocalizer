using Microsoft.Win32;
using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;
using Viewer;

namespace ResxLocalizer {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : RibbonWindow {
        public MainWindow() {
            InitializeComponent();

            cvs.Source = oc;
            lvItems.DataContext = cvs;
            tbDisp1.DataContext = cvs;
            tbDisp2.DataContext = cvs;
        }

        CollectionViewSource cvs = new CollectionViewSource();
        ObservableCollection<Item> oc = new ObservableCollection<Item>();

        public class Item : INotifyPropertyChanged {
            String _Name;
            public String Name { get { return _Name; } set { _Name = value; Fires("Name"); } }

            public Item(CasetBase c1, CasetBase c2) {
                this.c1 = c1; c1.StringChanged += c1_StringChanged; c1.PropertyChanged += c1_PropertyChanged;
                this.c2 = c2; c2.StringChanged += c2_StringChanged; c2.PropertyChanged += c2_PropertyChanged;
            }

            void c1_PropertyChanged(object sender, PropertyChangedEventArgs e) {
                if (e != null) {
                    if ("IsMod".Equals(e.PropertyName)) {
                        SetMod1(c1.IsChanged(Name));
                        Fires("Disp1");
                    }
                }
            }

            void c2_PropertyChanged(object sender, PropertyChangedEventArgs e) {
                if (e != null) {
                    if ("IsMod".Equals(e.PropertyName)) {
                        SetMod2(c2.IsChanged(Name));
                        Fires("Disp2");
                    }
                }
            }

            void c1_StringChanged(object sender, PropertyChangedEventArgs e) {
                SetMod1(c1.IsChanged(Name));
                Fires("Disp1");
            }
            void c2_StringChanged(object sender, PropertyChangedEventArgs e) {
                SetMod2(c2.IsChanged(Name));
                Fires("Disp2");
            }

            public CasetBase c1 { get; private set; }
            public CasetBase c2 { get; private set; }

            public String Disp1 {
                get { return c1.Gets(Name); }
                set { c1.Sets(Name, value); Fires("Disp1"); }
            }
            public String Disp2 {
                get { return c2.Gets(Name); }
                set { c2.Sets(Name, value); Fires("Disp2"); }
            }

            protected void Fires(String props) {
                foreach (String prop in props.Split(','))
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }

            bool _IsMod1 = false;
            public bool IsMod1 { get { return _IsMod1; } }

            bool _IsMod2 = false;
            public bool IsMod2 { get { return _IsMod2; } }

            void SetMod1(bool f) {
                if (_IsMod1 != f) {
                    _IsMod1 = f; Fires("IsMod1");
                }
            }

            void SetMod2(bool f) {
                if (_IsMod2 != f) {
                    _IsMod2 = f; Fires("IsMod2");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void lvItems_ItemActivation(object sender, MouseButtonEventArgs e) {

        }

        CasetBase c1 = new Caset(), c2 = new Caset();

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

        private void mSwap_Click(object sender, RoutedEventArgs e) {
            foreach (Item it in lvItems.SelectedItems.OfType<Item>()) {
                String v1 = it.Disp1;
                String v2 = it.Disp2;
                it.Disp2 = v1;
                it.Disp1 = v2;
                if (it.Disp2 == v1 && it.Disp1 == v2) continue;
                it.Disp1 = v1;
                it.Disp2 = v2;
            }
        }

        public void CloseMe() {
            oc.Clear();
            c1 = new Caset();
            c2 = new Caset();
        }

        public void OpenResx(String[] alfp) {
            CloseMe();

            if (alfp.Length >= 1) c1 = Caset.LoadFrom(alfp[0]);
            if (alfp.Length >= 2) c2 = Caset.LoadFrom(alfp[1]);

            ((GridViewColumn)FindName("h1")).Header = (alfp.Length >= 1) ? System.IO.Path.GetFileName(alfp[0]) : "無し";
            ((GridViewColumn)FindName("h2")).Header = (alfp.Length >= 2) ? System.IO.Path.GetFileName(alfp[1]) : "無し";

            foreach (String k in c1.Names.Concat(c2.Names).Distinct()) {
                oc.Add(new Item(c1, c2) { Name = k });
            }
        }

        List<Sel> alSels = new List<Sel>();

        public void AddSel(Sel sel) {
            alSels.Add(sel);
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e) {
            SelNow();
        }

        private void SelNow() {
            mSel.Visibility = (alSels.Count != 0) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            if (alSels.Count == 0) return;
            if (!Saved()) return;
            SelWin form = new SelWin();
            form.DataContext = alSels;

            if (form.ShowDialog() ?? false) {
                var seled = form.Seled;
                OpenResx(seled.Files);
            }
        }

        bool Saved() {
            if (!IsMod) return true;
            switch (MessageBox.Show("保存?", Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation)) {
                case MessageBoxResult.Yes:
                    mSaveResx_Click(this, new RoutedEventArgs());
                    return true;
                case MessageBoxResult.No:
                    return true;
                default:
                    return false;
            }
        }

        bool IsMod {
            get {
                return c1.IsMod || c2.IsMod;
            }
        }

        private void mSel_Click(object sender, RoutedEventArgs e) {
            SelNow();
        }

        private void RibbonWindow_Closing(object sender, CancelEventArgs e) {
            e.Cancel = !Saved();
        }

        Proj1 proj = new Proj1();

        private void mMany_Click(object sender, RoutedEventArgs e) {
            SelFolder form = new SelFolder();
            form.DataContext = proj.Clone();
            form.Left = this.Left + 24;
            form.Top = this.Top + 24;
            if (!(form.ShowDialog() ?? false)) return;

            proj = (Proj1)form.DataContext;

            CloseMe();

            c1 = new MCaset(proj.GetLang1Files(), proj.IsMulti.Value);
            c2 = new MCaset(proj.GetLang2Files(), proj.IsMulti.Value);

            ((GridViewColumn)FindName("h1")).Header = lLangDisp1.Text = proj.LangDisp1;
            ((GridViewColumn)FindName("h2")).Header = lLangDisp2.Text = proj.LangDisp2;

            foreach (String k in c1.Names.Concat(c2.Names).Distinct()) {
                oc.Add(new Item(c1, c2) { Name = k });
            }
        }

        void results_Click(object sender, RoutedEventArgs e) {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked == null) return;
            if (object.ReferenceEquals(headerClicked.Column, FindName("hr"))) {
                cvs.SortDescriptions.Clear();
                cvs.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
            if (object.ReferenceEquals(headerClicked.Column, FindName("h1"))) {
                cvs.SortDescriptions.Clear();
                cvs.SortDescriptions.Add(new SortDescription("Disp1", ListSortDirection.Ascending));
            }
            if (object.ReferenceEquals(headerClicked.Column, FindName("h2"))) {
                cvs.SortDescriptions.Clear();
                cvs.SortDescriptions.Add(new SortDescription("Disp2", ListSortDirection.Ascending));
            }
        }

        private void bSearchNext_Click(object sender, RoutedEventArgs e) {
            bool ifNext = object.ReferenceEquals(bSearchNext, sender);

            int x = oc.IndexOf(cvs.View.CurrentItem as Item);
            int n = oc.Count;
            var kws = tbKws.Text;
            for (int i = 0; i < n; i++) {
                var it = oc[(x + (ifNext ? +i + 1 : -i - 1) + n + n) % n];

                String src = it.Disp1 + " " + it.Disp2;
                bool all = true;
                foreach (String kw in Regex.Replace(kws, "\\s+", " ").Trim().Split(' ')) {
                    all &= src.IndexOf(kw, StringComparison.CurrentCultureIgnoreCase) >= 0;
                }
                if (all) {
                    cvs.View.MoveCurrentTo(it);
                    lvItems.ScrollIntoView(it);
                    break;
                }
            }
        }

        private void mExp_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.csv SJIS|*.csv|*.csv UTF-8|*.csv";
            if (sfd.ShowDialog() ?? false) {
                using (StreamWriter wr = new StreamWriter(sfd.FileName, false, (sfd.FilterIndex == 2) ? Encoding.UTF8 : Encoding.GetEncoding(932))) {
                    Csvw w = new Csvw(wr, ',', '"');
                    w.Write("リソース名");
                    w.Write(proj.LangDisp1);
                    w.Write(proj.LangDisp2);
                    w.NextRow();

                    foreach (Item item in oc) {
                        w.Write(item.Name);
                        w.Write(item.Disp1);
                        w.Write(item.Disp2);
                        w.NextRow();
                    }
                }
            }
        }

        private void mImp_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.csv SJIS|*.csv|*.csv UTF-8|*.csv";
            if (ofd.ShowDialog() ?? false) {
                Csvr r = new Csvr();
                r.ReadStr(File.ReadAllText(ofd.FileName, (ofd.FilterIndex == 2) ? Encoding.UTF8 : Encoding.GetEncoding(932)), ',', '"');
                var rows = r.alrow;
                for (int y = 1; y < rows.Count; y++) {
                    var cols = rows[y];
                    if (cols.Length == 3) {
                        var item = oc.FirstOrDefault(p => p.Name.Equals(cols[0]));
                        if (item != null) {
                            if (!item.Disp1.Equals(cols[1])) item.Disp1 = cols[1];
                            if (!item.Disp2.Equals(cols[2])) item.Disp2 = cols[2];
                        }
                    }
                }
            }
        }
    }

    public class MCaset : CasetBase {
        List<Caset> alc = new List<Caset>();

        public ISep Sep;

        bool includeFolderName;

        public MCaset(String[] alfp, bool includeFolderName) {
            foreach (String fp in alfp) {
                alc.Add(Eat(Caset.LoadFrom(fp)));
            }
            Sep = new PostNameSep("@", "/", this.includeFolderName = includeFolderName);
        }

        private Caset Eat(Caset caset) {
            caset.PropertyChanged += caset_PropertyChanged;
            return caset;
        }

        void caset_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e != null) {
                if ("IsMod".Equals(e.PropertyName)) Fires("IsMod");
            }
        }

        public override IEnumerable<string> Names {
            get {
                foreach (var c in alc) {
                    foreach (String a in c.Names) {
                        yield return Sep.Build(a, c.ResName.BaseName, c.ResName.FolderName);
                    }
                }
            }
        }

        public interface ISep {
            String Build(String key, String baseName, String folder);
            bool TryParse(String built, out String key, out String baseName, out String folderName);
        }

        public class PostNameSep : ISep {
            String nameSep;
            String projSep;
            bool includeFolderName;

            public PostNameSep(String nameSep, String projSep, bool includeFolderName) {
                this.nameSep = nameSep;
                this.projSep = projSep;
                this.includeFolderName = includeFolderName;
            }

            public string Build(string key, string baseName, string folder) {
                if (includeFolderName)
                    return key + nameSep + folder + projSep + baseName;
                return key + nameSep + baseName;
            }

            public bool TryParse(string built, out string key, out string baseName, out string folderName) {
                String[] cols = built.Split(new String[] { nameSep }, StringSplitOptions.None);
                if (cols.Length == 2) {
                    String[] pcols = cols[1].Split(new String[] { projSep }, StringSplitOptions.None);
                    if (pcols.Length == 2) {
                        key = cols[0];
                        folderName = pcols[0];
                        baseName = pcols[1];
                        return true;
                    }
                    else {
                        key = cols[0];
                        folderName = String.Empty;
                        baseName = cols[1];
                        return true;
                    }
                }
                else {
                    key = String.Empty;
                    folderName = String.Empty;
                    baseName = String.Empty;
                    return false;
                }
            }
        }

        public override bool IsChanged(string Name) {
            String key, baseName, folderName;
            if (Sep.TryParse(Name, out key, out baseName, out folderName)) {
                var c1 = alc.FirstOrDefault(c => c.ResName.BaseName.Equals(baseName) && (!includeFolderName || c.ResName.FolderName.Equals(folderName)));
                if (c1 != null)
                    return c1.IsChanged(key);
            }
            return false;
        }

        public override string Gets(string Name) {
            String key, baseName, folderName;
            if (Sep.TryParse(Name, out key, out baseName, out folderName)) {
                var c1 = alc.FirstOrDefault(c => c.ResName.BaseName.Equals(baseName) && (!includeFolderName || c.ResName.FolderName.Equals(folderName)));
                if (c1 != null)
                    return c1.Gets(key);
            }
            return String.Empty;
        }

        public override void Sets(string Name, string value) {
            String key, baseName, folderName;
            if (Sep.TryParse(Name, out key, out baseName, out folderName)) {
                var c1 = alc.FirstOrDefault(c => c.ResName.BaseName.Equals(baseName) && (!includeFolderName || c.ResName.FolderName.Equals(folderName)));
                if (c1 != null)
                    c1.Sets(key, value);
            }
        }

        public override void Save() {
            foreach (var c in alc) {
                c.Save();
            }
        }

        public override bool IsMod {
            get { return alc.Any(c => c.IsMod); }
        }
    }

    public abstract class CasetBase : INotifyPropertyChanged {
        public abstract IEnumerable<String> Names { get; }
        public abstract string Gets(string Name);
        public abstract void Sets(string Name, string value);
        public abstract void Save();
        public abstract bool IsChanged(string Name);

        public abstract bool IsMod { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler StringChanged;

        protected void Fires(string props) {
            foreach (String prop in props.Split(','))
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        protected void Str(string prop) {
            if (StringChanged != null)
                StringChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class Caset : CasetBase {
        XDocument x;
        String fp;

        bool _IsMod = false;
        public override bool IsMod { get { return _IsMod; } }

        protected void SetMod(bool v) {
            if (_IsMod != v) {
                _IsMod = v; Fires("IsMod");
            }
        }

        public void Load(String fp) {
            x = XDocument.Load(this.fp = fp);
            SetMod(false);
            Fires("Names");
        }

        SortedDictionary<string, string> changedNames = new SortedDictionary<string, string>();

        public override IEnumerable<String> Names {
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

        public ResName ResName {
            get {
                return new ResName(fp);
            }
        }

        public override string Gets(string Name) {
            if (x != null) {
                foreach (var elv in x.Elements("root").Elements("data").Where(eld => eld.Attribute("name") != null && eld.Attribute("name").Value == Name).Elements("value")) {
                    return elv.Value;
                }
            }
            return String.Empty;
        }

        public override void Sets(string Name, string value) {
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
            changedNames[Name] = null;
            Str(Name);
            SetMod(true);
        }

        public override void Save() {
            if (x == null) return;
            x.Save(fp);
            changedNames.Clear();
            SetMod(false);
        }

        public override bool IsChanged(string Name) {
            return changedNames.ContainsKey(Name);
        }

        public static Caset LoadFrom(String fp) {
            Caset c = new Caset();
            c.Load(fp);
            return c;
        }
    }

    public class 変更Converter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is bool && (bool)value)
                return Brushes.Tomato;
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}

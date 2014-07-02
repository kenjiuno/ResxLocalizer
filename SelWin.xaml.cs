using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Viewer;

namespace ResxLocalizer {
    /// <summary>
    /// SelWin.xaml の相互作用ロジック
    /// </summary>
    public partial class SelWin : Window {
        public SelWin() {
            InitializeComponent();
        }

        public Sel Seled;

        private void bSel_Click(object sender, RoutedEventArgs e) {
            Seled = ((Button)sender).DataContext as Sel;
            if (Seled != null) DialogResult = true;
        }

        private void bExp_Click(object sender, RoutedEventArgs e) {
            List<Caset> alc = new List<Caset>();
            foreach (Sel sel in (List<Sel>)DataContext) {
                foreach (String fp in sel.Files) {
                    Caset c = new Caset();
                    c.Load(fp);
                    alc.Add(c);
                }
            }
            String[] xItems = alc.Select(p => p.ResName.Language).Distinct().ToArray();
            ResEnt[] yItems = alc.SelectMany(p => p.Names.Select(pp => new ResEnt { Name = pp, BaseName = p.ResName.BaseName })).ToArray();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.csv|*.csv";
            if (sfd.ShowDialog() ?? false) {
                using (StreamWriter wr = new StreamWriter(sfd.FileName, false, Encoding.GetEncoding(932))) {
                    Csvw w = new Csvw(wr, ',', '"');
                    w.Write("ファイル名");
                    w.Write("リソース名");
                    foreach (String lang in xItems) {
                        w.Write(lang);
                    }
                    w.NextRow();
                    foreach (ResEnt re in yItems) {
                        w.Write(re.BaseName);
                        w.Write(re.Name);
                        foreach (String lang in xItems) {
                            var c = alc.FirstOrDefault(p => p.ResName.BaseName == re.BaseName && p.ResName.Language == lang);
                            if (c != null) {
                                w.Write(c.Gets(re.Name));
                            }
                            else {
                                w.Write("");
                            }
                        }
                        w.NextRow();
                    }
                }
            }
        }

        class ResEnt {
            public String BaseName { get; set; }
            public String Name { get; set; }

        }

        public class SortResName : IEqualityComparer<ResName>, IComparer<ResName> {
            public bool Equals(ResName x, ResName y) {
                return Compare(x, y) == 0;
            }

            public int GetHashCode(ResName obj) {
                return (obj.BaseName + "." + obj.Language).GetHashCode();
            }

            public int Compare(ResName x, ResName y) {
                int t;
                if (0 != (t = x.BaseName.CompareTo(y.BaseName))) return t;
                if (0 != (t = x.Language.CompareTo(y.Language))) return t;
                return 0;
            }
        }

        private void bImp_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.csv|*.csv";
            if (ofd.ShowDialog() ?? false) {
            }
            else return;

            Csvr csv = new Csvr();
            csv.ReadStr(File.ReadAllText(ofd.FileName, Encoding.GetEncoding(932)), ',', '"');

            List<Caset> alc = new List<Caset>();
            foreach (Sel sel in (List<Sel>)DataContext) {
                foreach (String fp in sel.Files) {
                    Caset c = new Caset();
                    c.Load(fp);
                    alc.Add(c);
                }
            }

            var rows = csv.alrow;
            if (rows.Count > 1) {
                int n = 0;
                String[] alhdr = rows[0];
                for (int x = 2; x < alhdr.Length; x++) {
                    String lang = alhdr[x];
                    for (int y = 1; y < rows.Count; y++) {
                        String[] cols = rows[y];
                        if (x < cols.Length) {
                            var c = alc.FirstOrDefault(p => p.ResName.BaseName == cols[0] && p.ResName.Language == lang);
                            if (c != null) {
                                String nv = cols[x];
                                String cv = c.Gets(cols[1]);
                                if (cv != nv) {
                                    c.Sets(cols[1], nv);
                                    n++;
                                }
                            }
                        }
                    }
                }
                if (n == 0) {
                    MessageBox.Show("変更されていません。", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (MessageBox.Show(String.Format("{0:#,##0}件変更しました。保存しますか?", n), Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes) {
                    foreach (Caset c in alc) if (c.IsMod) c.Save();
                }
            }
        }
    }

    public class Sel {
        public String Disp { get; set; }
        public String[] Files { get; set; }
    }
}

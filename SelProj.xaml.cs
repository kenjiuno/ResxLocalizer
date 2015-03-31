using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ResxLocalizer {
    /// <summary>
    /// SelProj.xaml の相互作用ロジック
    /// </summary>
    public partial class SelProj : Window {
        public SelProj() {
            InitializeComponent();

            isf = IsolatedStorageFile.GetUserStoreForAssembly();
            isf.CreateDirectory("conf");
            Refr();
        }

        IsolatedStorageFile isf;

        Proj1 proj { get { return (Proj1)DataContext; } }

        private void bLoad_Click(object sender, RoutedEventArgs e) {
            using (var si = isf.OpenFile("conf\\" + tbProj.Text, FileMode.Open)) {
                XDocument xd = XDocument.Load(si);
                var el = xd.Element("ResxLocalizer");
                proj.Dirs = (el.Attribute("Dirs") ?? el.Attribute("Dir")).Value;
                proj.Lang1 = (el.Attribute("Lang1") != null) ? el.Attribute("Lang1").Value : "";
                proj.Lang2 = (el.Attribute("Lang2") != null) ? el.Attribute("Lang2").Value : "";
                proj.LangDisp1 = (el.Attribute("LangDisp1") != null) ? el.Attribute("LangDisp1").Value : "";
                proj.LangDisp2 = (el.Attribute("LangDisp2") != null) ? el.Attribute("LangDisp2").Value : "";
                proj.IsMulti = (el.Attribute("IsMulti") != null) ? el.Attribute("IsMulti").Value.Equals("1") : false;
            }
            proj.RefrDirs();
            Close();
        }

        private void bSave_Click(object sender, RoutedEventArgs e) {
            XDocument xd = new XDocument(
                new XElement("ResxLocalizer"
                    , new XAttribute("Dirs", proj.Dirs)
                    , new XAttribute("Lang1", proj.Lang1)
                    , new XAttribute("Lang2", proj.Lang2)
                    , new XAttribute("LangDisp1", proj.LangDisp1)
                    , new XAttribute("LangDisp2", proj.LangDisp2)
                    , new XAttribute("IsMulti", (proj.IsMulti ?? false) ? 1 : 0)
                    )
                );

            var isf = IsolatedStorageFile.GetUserStoreForAssembly();
            using (var os = isf.CreateFile("conf\\" + tbProj.Text)) {
                xd.Save(os);
            }
            Close();
        }

        private void bDelete_Click(object sender, RoutedEventArgs e) {
            isf.DeleteFile("conf\\" + tbProj.Text);
            Refr();
        }

        private void Refr() {
            lbProj.DataContext = isf.GetFileNames("conf\\*").Select(fp => System.IO.Path.GetFileName(fp));
        }

        private void lbProj_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            proj.ProjName = "" + lbProj.SelectedItem;
        }
    }
}

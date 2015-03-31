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
        }

        private void bRefDir_Click(object sender, RoutedEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = tbDir.Text;
            if (fbd.ShowDialog(null) != System.Windows.Forms.DialogResult.OK) return;
            tbDir.Text = tbDir.Text.TrimEnd() + "\r\n" + fbd.SelectedPath;

            Refr();
        }

        Proj1 proj { get { return (Proj1)DataContext; } }

        private void Refr() {
            if (String.IsNullOrEmpty(tbDir.Text)) return;

            proj.RefrDirs();
        }

        private void tbDir_LostFocus(object sender, RoutedEventArgs e) {
            Refr();
        }

        private void bOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void bProjs_Click(object sender, RoutedEventArgs e) {
            SelProj form = new SelProj();
            form.Left = this.Left + 24;
            form.Top = this.Top + 24;
            form.DataContext = DataContext;
            form.ShowDialog();
        }
    }
}

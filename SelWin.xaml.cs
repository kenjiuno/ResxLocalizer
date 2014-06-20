using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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
    }

    public class Sel {
        public String Disp { get; set; }
        public String[] Files { get; set; }
    }
}

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
    /// SearchWin.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchWin : Window {
        public SearchWin() {
            InitializeComponent();
        }

        public ISearchText St { get; set; }

        private void bSearch_Click(object sender, RoutedEventArgs e) {
            if (St != null)
                St.SearchText(tb.Text);
        }
    }

    public interface ISearchText {
        void SearchText(String kws);
    }
}

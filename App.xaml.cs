﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ResxLocalizer {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            var mform = new MainWindow();
            MainWindow = mform;

            Sel Seled = null;
            foreach (String fp in e.Args) {
                if (File.Exists(fp)) {
                    List<Sel> sels = new List<Sel>();
                    foreach (String row in File.ReadAllLines(fp, Encoding.UTF8)) {
                        String[] cols = row.Split('\t');
                        if (cols.Length <= 1) continue;
                        sels.Add(new Sel { Disp = cols[0], Files = cols.Skip(1).Select(p => Path.Combine(Path.GetDirectoryName(fp), p)).ToArray() });
                    }
                    if (sels.Count != 0) {
                        var form = new SelWin();
                        form.DataContext = sels;
                        if (form.ShowDialog() ?? false) {
                            Seled = form.Seled;
                        }
                    }
                }
            }

            {
                if (Seled != null) mform.OpenResx(Seled.Files);
                mform.Show();
            }
        }
    }
}

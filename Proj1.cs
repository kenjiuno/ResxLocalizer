using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ResxLocalizer {
    public class Proj1 : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Fires(String props) {
            if (PropertyChanged != null)
                foreach (String prop in props.Split(','))
                    PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        String _ProjName;

        public String ProjName {
            get { return _ProjName; }
            set { _ProjName = value; Fires("ProjName"); }
        }

        String _LangDisp1 = String.Empty;

        public String LangDisp1 {
            get { return _LangDisp1; }
            set { _LangDisp1 = value; Fires("LangDisp1"); }
        }

        String _LangDisp2 = String.Empty;

        public String LangDisp2 {
            get { return _LangDisp2; }
            set { _LangDisp2 = value; Fires("LangDisp2"); }
        }

        String _Lang1 = String.Empty;

        public String Lang1 {
            get { return _Lang1; }
            set { _Lang1 = value; Fires("Lang1"); }
        }

        String _Lang2 = String.Empty;

        public String Lang2 {
            get { return _Lang2; }
            set { _Lang2 = value; Fires("Lang2"); }
        }

        String _Dirs = String.Empty;

        public String Dirs {
            get { return _Dirs; }
            set { _Dirs = value; Fires("Dirs"); }
        }

        public bool _IsMulti = false;

        public bool? IsMulti {
            get { return _IsMulti; }
            set { _IsMulti = value ?? false; Fires("IsMulti"); }
        }

        public void RefrDirs() {
            List<string> alfp = new List<string>();
            foreach (String dir in Dirs.Replace("\r\n", "\n").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)) {
                foreach (String fp in Directory.GetFiles(dir, "*.resx")) {
                    alfp.Add(fp);
                }
            }
            _Files = alfp.ToArray();
            _ResNames = Files.Select(p => new ResName(p)).ToArray();
            Fires("Files,ResNames");

            AvailLangs.Clear();
            foreach (var s in ResNames.Select(p => p.Language).OrderBy(p => p).Distinct().ToArray()) AvailLangs.Add(s);
        }

        String[] _Files = new String[0];

        public String[] Files {
            get { return _Files.ToArray(); }
        }

        ResName[] _ResNames = new ResName[0];

        public ResName[] ResNames {
            get { return _ResNames.ToArray(); }
        }

        public string[] GetLang1Files() { return ResNames.Where(p => Lang1.Equals(p.Language)).Select(p => p.FilePath).ToArray(); }
        public string[] GetLang2Files() { return ResNames.Where(p => Lang2.Equals(p.Language)).Select(p => p.FilePath).ToArray(); }

        ObservableCollection<String> _AvailLangs = new ObservableCollection<String>();

        public Collection<String> AvailLangs { get { return _AvailLangs; } }

        public Proj1 Clone() {
            var v = (Proj1)MemberwiseClone();
            return v;
        }
    }
}

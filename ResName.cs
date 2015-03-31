using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ResxLocalizer {
    public class ResName {
        public ResName(String fp) {
            Avail = false;
            Dir = String.Empty;
            BaseName = String.Empty;
            Language = String.Empty;
            FolderName = String.Empty;

            if (fp != null) {
                Dir = Path.GetDirectoryName(fp);
                String fn = Path.GetFileName(fp);
                Match M;
                if ((M = Regex.Match(fn, "^(?<b>.+?)\\.(?<a>[a-z]+\\-[a-z]+)\\.resx", RegexOptions.IgnoreCase)).Success) {
                    BaseName = M.Groups["b"].Value.ToLowerInvariant();
                    Language = M.Groups["a"].Value;
                    Avail = true;
                }
                else if ((M = Regex.Match(fn, "^(?<b>.+?)\\.resx", RegexOptions.IgnoreCase)).Success) {
                    BaseName = M.Groups["b"].Value.ToLowerInvariant();
                    Avail = true;
                }
                {
                    String[] dirs = Dir.Split(Path.DirectorySeparatorChar);
                    FolderName = (dirs.Length == 0) ? "" : dirs[dirs.Length - 1].ToLowerInvariant();
                }
                FilePath = fp;
            }
        }

        public bool Avail { get; set; }
        public String Dir { get; set; }
        public String BaseName { get; set; }
        public String Language { get; set; }
        public String FilePath { get; set; }
        public String FolderName { get; set; }

        public override string ToString() {
            return BaseName + "." + Language;
        }

    }
}

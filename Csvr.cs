using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Viewer {
    public class Csvr {
        public List<String[]> alrow = new List<String[]>();

        class Reader {
            int x, cx;
            String s;
            char Sep, Quote;

            public Reader(String s, char sep, char quote) {
                this.s = s;
                this.x = 0;
                this.cx = s.Length;
                this.Sep = sep;
                this.Quote = quote;
            }

            public bool EOF { get { return x >= cx; } }

            public ReadRes ReadStr(out String res) {
                String temp = "";
                if (EOF) {
                    res = null;
                    return ReadRes.EOF;
                }
                if (s[x] == Quote) {
                    x++;
                    while (x < cx) {
                        if (s[x] == Quote) {
                            if (x + 1 < cx && s[x + 1] == Quote) {
                                x += 2;
                                temp += Quote;
                            }
                            else {
                                x++;
                                break;
                            }
                        }
                        else {
                            temp += s[x];
                            x++;
                        }
                    }
                }
                else if (s[x] == Sep) {
                    x++;
                    res = "";
                    return ReadRes.Ok;
                }
                else if (CHTr.IsBR(s[x])) {
                    ReadBR();
                    res = null;
                    return ReadRes.EOL;
                }

                while (x < cx) {
                    if (CHTr.IsBR(s[x])) {
                        break;
                    }
                    else if (s[x] == Sep) {
                        x++;
                        break;
                    }
                    else {
                        temp += s[x];
                        x++;
                    }
                }
                res = temp;
                return ReadRes.Ok;
            }

            private bool ReadBR() {
                if (x < cx) {
                    if (s[x] == '\r') {
                        x++;
                        if (x < cx && s[x] == '\n') {
                            x++;
                        }
                        return true;
                    }
                    else if (s[x] == '\n') {
                        x++;
                        return true;
                    }
                }
                return false;
            }

            class CHTr {
                public static bool IsBR(char c) {
                    switch (c) {
                        case '\r':
                        case '\n':
                            return true;
                    }
                    return false;
                }
            }
        }

        enum ReadRes {
            Ok, EOF, EOL,
        }

        public void Read(String fp) {
            ReadStr(File.ReadAllText(fp, Encoding.GetEncoding(932)), ',', '"');
        }

        public void ReadStr(String text, char sep, char quote) {
            Reader r = new Reader(text, sep, quote);
            List<String> alcol = new List<String>();
            while (true) {
                String col;
                ReadRes rr = r.ReadStr(out col);
                if (rr == ReadRes.Ok) {
                    alcol.Add(col);
                }
                else if (rr == ReadRes.EOL) {
                    alrow.Add(alcol.ToArray());
                    alcol.Clear();
                }
                else if (rr == ReadRes.EOF) {
                    if (alcol.Count != 0) {
                        alrow.Add(alcol.ToArray());
                        alcol.Clear();
                    }
                    break;
                }
            }
        }
    }
}

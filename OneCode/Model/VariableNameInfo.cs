using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCode {
    class VariableNameInfo {
        private string content;
        private string prefix;

        public VariableNameInfo(string _content) {
            this.prefix = "";
            this.Content = _content;
        }

        public VariableNameInfo(string _content, string _prefix) {
            this.prefix = _prefix;
            this.content = _content;
        }

        public string GetContentWithPrefix() {
            return prefix + content;
        }

        public string Content {
            get {
                return content;
            }

            set {
                int index = 0;
                foreach (Char c in value) {
                    if (!Char.IsLetter(c)) {
                        prefix += c.ToString();
                    } else {
                        index = value.IndexOf(c);
                        break;
                    }
                }

                this.content = value.Substring(index);
            }
        }

        public string Prefix {
            get {
                return prefix;
            }
        }
    }
}

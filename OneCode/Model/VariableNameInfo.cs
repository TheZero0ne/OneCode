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
            this.content = _content;
            this.prefix = "";
        }

        public VariableNameInfo(string _content, string _prefix) {
            this.content = _content;
            this.prefix = _prefix;
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
                string tmpPrefix  = "";
                foreach (Char c in value) {
                    if (!Char.IsLetter(c)) {
                        tmpPrefix += c.ToString();
                    } else {
                        index = value.IndexOf(c);
                        break;
                    }
                }
                this.content = value.Substring(index);
                if (this.prefix.Length == 0 || !this.prefix.Equals(tmpPrefix))
                {
                    this.prefix = tmpPrefix;
                }
            }
        }

        public string Prefix {
            get {
                return prefix;
            }
        }
    }
}

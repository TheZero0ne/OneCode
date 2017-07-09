using System;

namespace OneCode {
    /// <summary>
    /// The VariableNameInfo holds the Name and an optional prefix of a Variable. It also provides methods to split the Name for translation.
    /// </summary>
    class VariableNameInfo {
        private string content;
        private string prefix;

        public VariableNameInfo(string _content) {
            this.prefix = "";
            this.Content = _content;
        }

        public VariableNameInfo(string _content, string _prefix) {
            this.content = _content;
            this.prefix = _prefix;
        }

        /// <summary>
        /// Provides the Name with prefix.
        /// </summary>
        /// <returns>The Name with prefix</returns>
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

                // Loop through the Name-string and detects the optional prefix
                foreach (Char c in value) {
                    if (!Char.IsLetter(c)) {
                        tmpPrefix += c.ToString();
                    } else {
                        index = value.IndexOf(c);
                        break;
                    }
                }

                this.content = value.Substring(index);

                if (this.prefix.Length == 0 || !this.prefix.Equals(tmpPrefix)) {
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

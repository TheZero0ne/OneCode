using OneCode.Properties;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OneCode {
    /// <summary>
    /// The VariableFormatter splits the variable Name like the user configured and splits the prefix and the namecontent into two fields.
    /// </summary>
    static class VariableFormatter {
        /// <summary>
        /// Splits a string with blanks and detects a prefix. 
        /// </summary>
        /// <param name="s">A string to split</param>
        /// <returns>The splitted string</returns>
        public static string SplitString(string s) {
            string splitString = "";

            if (Settings.Default.CodeStyle == CodeStyle.CamelCase) {
                splitString = Regex.Replace(s, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})", " ", RegexOptions.Compiled).Trim();
            } else if (Settings.Default.CodeStyle == CodeStyle.UnderLine) {
                splitString = s.Replace("_", " ").Trim();
            }

            string tmpSplit = splitString;
            foreach (Char c in splitString)
            {
                if (!Char.IsLetter(c) && c != ' ') { 
                    tmpSplit = tmpSplit.Insert(tmpSplit.IndexOf(c), " ");
                    tmpSplit = tmpSplit.Insert(tmpSplit.IndexOf(c) + 1, " ");
                }

            }
            splitString = tmpSplit;

            splitString = Regex.Replace(splitString, "  ", " ", RegexOptions.Compiled);

            return splitString.Length > 0 ? splitString : s;
        }

        /// <summary>
        /// Merges a string and removes all blanks.
        /// </summary>
        /// <param name="s">The string to merge</param>
        /// <returns>The merged string</returns>
        public static string MergeString(string s) {
            string mergedString = "";
            string[] parts = s.Split(' ');
            parts = parts.Where(x => x.Length > 0).ToArray();

            if (Settings.Default.CodeStyle == CodeStyle.CamelCase) {
                int i = 0;

                foreach (string p in parts) {

                    if (p.Length == 1 && !Char.IsLetter(p[0]))
                    {
                        mergedString += p;
                    } else { 
                        if (0 == i++) {
                            mergedString += p[0].ToString().ToLower() + p.Remove(0, 1);
                        } else {
                            mergedString += p[0].ToString().ToUpper() + p.Remove(0, 1);
                        }
                    }
                }
            } else if (Settings.Default.CodeStyle == CodeStyle.UnderLine) {
                foreach (string p in parts) {
                    mergedString += "_" + p.ToLower();
                }

                mergedString = mergedString.Remove(0, 1);
            }
            return mergedString;
        }
    }
}

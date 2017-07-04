﻿using OneCode.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneCode {
    static class VariableFormatter {
        public static string SplitString(string s) {
            string splitString = "";

            if (Settings.Default.CodeStyle == CodeStyle.CamelCase) {
                splitString = Regex.Replace(s, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})", " ", RegexOptions.Compiled).Trim();
            } else if (Settings.Default.CodeStyle == CodeStyle.UnderLine) {
                splitString = s.Replace("_", " ").Trim();
            }

            return splitString.Length > 0 ? splitString : s;
        }

        public static string MergeString(string s) {
            string mergedString = "";
            string[] parts = s.Split(' ');

            if (Settings.Default.CodeStyle == CodeStyle.CamelCase) {
                int i = 0;

                foreach (string p in parts) {
                    if (0 == i++) {
                        mergedString += p[0].ToString().ToLower() + p.Remove(0, 1);
                    } else {
                        mergedString += p[0].ToString().ToUpper() + p.Remove(0, 1);
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
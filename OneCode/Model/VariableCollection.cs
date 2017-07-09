using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OneCode {
    /// <summary>
    /// The VariableCollection is an ObservableCollection of Variable
    /// </summary>
    [Serializable]
    class VariableCollection : ObservableCollection<Variable> {
        private Dictionary<int, VariableNameInfo> translationMap;

        /// <summary>
        /// Provides a Dictionary for the translation of the data which the VariableCollection holds
        /// </summary>
        /// <returns>The Dictionary for translation</returns>
        public Dictionary<int, VariableNameInfo> GetNamesDictionaryForTranslation() {
            translationMap = new Dictionary<int, VariableNameInfo>();

            foreach(Variable v in this) {
                VariableNameInfo vni = new VariableNameInfo(VariableFormatter.SplitString(v.Name.Content), v.Name.Prefix);
                translationMap.Add(v.SpanStart, vni);
            }
            return translationMap;
        }

        /// <summary>
        /// Applies the translation from a given dictionary to the VariableCollection
        /// </summary>
        /// <param name="dic">The Dictionary to apply</param>
        public void ApplyTranslationDictionary(Dictionary<int, VariableNameInfo> dic) {
            foreach (var val in dic) {
                Variable variable = this.Where(x => x.SpanStart == val.Key).First();
                val.Value.Content = VariableFormatter.MergeString(val.Value.Content);
                variable.Translation = val.Value;
            }
        }
    }
}

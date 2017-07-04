using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCode {
    [Serializable]
    class VariableCollection : ObservableCollection<Variable> {
        private Dictionary<int, VariableNameInfo> translationMap;

        public Dictionary<int, VariableNameInfo> GetNamesDictionaryForTranslation() {
            translationMap = new Dictionary<int, VariableNameInfo>();

            foreach(Variable v in this) {
                VariableNameInfo vni = new VariableNameInfo(VariableFormatter.SplitString(v.Name.Content), v.Name.Prefix);
                translationMap.Add(v.SpanStart, vni);
            }
            return translationMap;
        }

        public void ApplyTranslationDictionary(Dictionary<int, VariableNameInfo> dic) {
            foreach (var val in dic) {
                Variable variable = this.Where(x => x.SpanStart == val.Key).First();
                val.Value.Content = VariableFormatter.MergeString(val.Value.Content);
                variable.Translation = val.Value;
            }
        }
    }
}

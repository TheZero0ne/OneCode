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

        private Dictionary<int, string> translationMap;

        public Dictionary<int, string> GetNamesDictionaryForTranslation()
        {
            translationMap = new Dictionary<int, string>();

            foreach(Variable v in this)
            {
                translationMap.Add(v.SpanStart, v.Name);
            }
            return translationMap;
        }

        public void ApplyTranslationDictionary(Dictionary<int, string> dic)
        {
            foreach (var val in dic)
            {
                Variable variable = this.Where(x => x.SpanStart == val.Key).First();
                variable.Translation = val.Value;
            }
        }

    }
}

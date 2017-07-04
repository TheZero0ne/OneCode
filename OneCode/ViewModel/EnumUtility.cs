using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCode {
    public static class EnumUtility {
        public static object[] GetValuesAndDescriptions(Type enumType) {
            var values = Enum.GetValues(enumType).Cast<object>();
            var valuesAndDescriptions = from value in values
                                        select new {
                                            Value = value,
                                            Description = value.GetType()
                                                    .GetMember(value.ToString())[0]
                                                    .GetCustomAttributes(true)
                                                    .OfType<DescriptionAttribute>()
                                                    .First()
                                                    .Description
                                        };
            return valuesAndDescriptions.ToArray();
        }

        public static List<CultureInfo> GetCultureInfo() {
            return new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures));
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace OneCode {
    /// <summary>
    /// A Helper-Class to Bind Enum-Data directly to the view
    /// </summary>
    public static class EnumUtility {
        /// <summary>
        /// Provides the Data of an Enum
        /// </summary>
        /// <param name="enumType">The Type of the desired Enum</param>
        /// <returns>An Array of the data of the desired Enum</returns>
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

        /// <summary>
        /// Provides the data of the cultureInfo
        /// </summary>
        /// <returns>A List of the CultureInfo</returns>
        public static List<CultureInfo> GetCultureInfo() {
            return new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures));
        }
    }
}

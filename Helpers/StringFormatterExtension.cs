using System.Collections.Generic;
using System.Linq;

namespace System.IO.BACnet.Helpers
{
    public static class StringFormatterExtension
    {
        public enum Casing
        {
            DontChange,
            FirstCharacterLowerCase,
        }

        public static IEnumerable<string> PropertiesWithValues<TType>(this TType obj, params string[] except)
            where TType : class
            => obj.PropertiesWithValues(Casing.FirstCharacterLowerCase, except);

        public static IEnumerable<string> PropertiesWithValues<TType>(this TType obj, Casing forcedCasing, params string[] except)
            where TType : class
        {
            if (obj == null)
                return new string[0];

            return obj.GetType().GetProperties()
                .Where(p => !except.Contains(p.Name, StringComparer.Ordinal))
                .Select(p =>
                {
                    string propertyName;
                    switch (forcedCasing)
                    {
                        case Casing.DontChange:
                            propertyName = p.Name;
                            break;

                        case Casing.FirstCharacterLowerCase:
                            propertyName = char.ToLower(p.Name[0]) + p.Name.Substring(1);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(forcedCasing), forcedCasing, null);
                    }                    
                    return $"{propertyName}: {p.GetValue(obj, null)}";
                });
        }
    }
}
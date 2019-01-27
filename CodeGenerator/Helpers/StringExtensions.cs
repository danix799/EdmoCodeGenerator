using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Helpers
{
    public static class StringExtensions
    {
        public static string ToLowerCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return Char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
        public static string ToUpperCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return Char.ToUpperInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
    }
}

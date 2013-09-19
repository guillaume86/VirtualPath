using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace VirtualPath.Common
{
    public static class StringExtensions
    {
        public static bool Glob(this string value, string pattern)
        {
            int pos;
            for (pos = 0; pattern.Length != pos; pos++)
            {
                switch (pattern[pos])
                {
                    case '?':
                        break;

                    case '*':
                        for (int i = value.Length; i >= pos; i--)
                        {
                            if (Glob(value.Substring(i), pattern.Substring(pos + 1)))
                                return true;
                        }
                        return false;

                    default:
                        if (value.Length == pos || Char.ToUpper(pattern[pos]) != Char.ToUpper(value[pos]))
                        {
                            return false;
                        }
                        break;
                }
            }

            return value.Length == pos;
        }
    }

}
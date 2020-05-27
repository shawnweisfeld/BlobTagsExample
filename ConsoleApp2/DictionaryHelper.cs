using PeanutButter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ConsoleApp2
{
    public static class DictionaryHelper
    {
        public static string ToJoinedString(this IDictionary<string, string> value)
        {
            return string.Join(" ", value.Select(x => $"{x.Key}:{x.Value}"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    public static class Util
    {
        public static bool SafeEqual(this string a, string b)
        {
            var sa = a.Trim().ToLower();
            var sb = b.Trim().ToLower();

            return sa.Equals(sb, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

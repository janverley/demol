using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static void SafeFileWithBackup(string path, object data)
        {
            var contents = JsonConvert.SerializeObject(data, Formatting.Indented);

            // todo first backup
            File.WriteAllText(path, contents);
        }

        public static T SafeReadJson<T>(string path)
        {
            string contents = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<T>(contents);

            return data;
        }
    }
}

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
        private static Dictionary<Type, string> Files => new Dictionary<Type, string>
        {
            { typeof(AdminData), @".\Files\admin.{0}.json" },
            { typeof(DagenData), @".\Files\dagen.json"},
            { typeof(SpelersData), @".\Files\spelers.json" },
            { typeof(VragenData), @".\Files\vragen.{0}.json" },
            { typeof(AntwoordenData), @".\Files\antwoorden.{0}.json" }
        };

        public static bool SafeEqual(this string a, string b)
        {
            var sa = a.Trim().ToLower();
            var sb = b.Trim().ToLower();

            return sa.Equals(sb, StringComparison.InvariantCultureIgnoreCase);
        }

        public static void SafeFileWithBackup(object data)
        {
            SafeFileWithBackup(Files[data.GetType()], data);
        }
        public static void SafeFileWithBackup(object data, int dag)
        {
            SafeFileWithBackup(string.Format(Files[data.GetType()], dag), data);
        }

        private static void SafeFileWithBackup(string path, object data)
        {
            var fileInfo = new FileInfo(path);
            // first backup
            if (fileInfo.Exists)
            {
                var index = 0;
                FileInfo backupFile;

                do
                {
                    backupFile = new FileInfo(Path.Combine(fileInfo.DirectoryName, "Backups", $"{index}.{fileInfo.Name}"));
                    index++;
                }
                while (backupFile.Exists);

                if (!Directory.Exists(backupFile.DirectoryName))
                {
                    Directory.CreateDirectory(backupFile.DirectoryName);
                }

                fileInfo.CopyTo(backupFile.FullName);
            }

            var contents = JsonConvert.SerializeObject(data, Formatting.Indented);

            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            File.WriteAllText(fileInfo.FullName, contents);
        }

        public static T SafeReadJson<T>(int dag) where T : new()
        {
            var path = string.Format(Files[typeof(T)], dag);
            return SafeReadJson<T>(path);
        }

        public static T SafeReadJson<T>() where T : new()
        {
            var path = Files[typeof(T)];
            return SafeReadJson<T>(path);
        }
        private static T SafeReadJson<T>(string path) where T : new()
        {
            T data = new T();

            if (File.Exists(path))
            {
                string contents = File.ReadAllText(path);
                data = JsonConvert.DeserializeObject<T>(contents);
            }

            return data;
        }


        internal static bool DataFileFoundAndValid<T>()
        {
            return DataFileFoundAndValid<T>(Files[typeof(T)]);
        }
        internal static bool DataFileFoundAndValid<T>(int dag)
        {
            return DataFileFoundAndValid<T>(string.Format(Files[typeof(T)], dag));
        }
        private static bool DataFileFoundAndValid<T>(string path)
        {
            var result = true;

            if (!File.Exists(path))
            {
                result = false;
            }
            else
            {
                string contents = File.ReadAllText(path);
                try
                {
                    T data = JsonConvert.DeserializeObject<T>(contents);
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    public static class Util
    {
        private struct FileData
        {
            public string Filename { get; set; }
            public bool Encrypted { get; set; }
        }

        private static Dictionary<Type, FileData> Files => new Dictionary<Type, FileData>
        {
            { typeof(AdminData), new FileData{ Filename =  @".\Files\admin.{0}.json", Encrypted = false } },
            { typeof(DagenData), new FileData{ Filename =  @".\Files\dagen.json", Encrypted = false } },
            { typeof(SpelersData), new FileData{ Filename =  @".\Files\spelers.json", Encrypted = false }  },
            { typeof(VragenData), new FileData{ Filename =  @".\Files\vragen.{0}.json", Encrypted = false }  },
            { typeof(AntwoordenData), new FileData{ Filename =  @".\Files\antwoorden.{0}.json", Encrypted = false }  },
            { typeof(MollenData), new FileData{ Filename =  @".\Files\mollen.json", Encrypted = false } },
            { typeof(OpdrachtVragenData), new FileData{ Filename =  @".\Files\OpdrachtVragen.json", Encrypted = false } }
        };

        public static bool SafeEqual(this string a, string b)
        {
            var sa = a.Trim().ToLower();
            var sb = b.Trim().ToLower();

            return sa.Equals(sb, StringComparison.InvariantCultureIgnoreCase);
        }

        public static void SafeFileWithBackup(object data)
        {
            SafeFileWithBackup(Files[data.GetType()].Filename, data, Files[data.GetType()].Encrypted);
        }
        public static void SafeFileWithBackup(object data, int dag)
        {
            SafeFileWithBackup(string.Format(Files[data.GetType()].Filename, dag), data, Files[data.GetType()].Encrypted);
        }

        private static void SafeFileWithBackup(string path, object data, bool encrypt)
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

            if (encrypt)
            {
                contents = Crypt(contents);
            }


            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            File.WriteAllText(fileInfo.FullName, contents);
        }

        public static T SafeReadJson<T>(int dag) where T : new()
        {
            var path = string.Format(Files[typeof(T)].Filename, dag);
            return SafeReadJson<T>(path, Files[typeof(T)].Encrypted);
        }

        public static T SafeReadJson<T>() where T : new()
        {
            var path = Files[typeof(T)].Filename;
            return SafeReadJson<T>(path, Files[typeof(T)].Encrypted);
        }
        private static T SafeReadJson<T>(string path, bool decrypt) where T : new()
        {
            T data = new T();

            if (File.Exists(path))
            {
                string contents = File.ReadAllText(path);

                if (decrypt)
                {
                    contents = Decrypt(contents);
                }

                data = JsonConvert.DeserializeObject<T>(contents);
            }

            return data;
        }


        internal static bool DataFileFoundAndValid<T>()
        {
            return DataFileFoundAndValid<T>(Files[typeof(T)].Filename, Files[typeof(T)].Encrypted);
        }
        internal static bool DataFileFoundAndValid<T>(int dag)
        {
            return DataFileFoundAndValid<T>(string.Format(Files[typeof(T)].Filename, dag), Files[typeof(T)].Encrypted);
        }
        private static bool DataFileFoundAndValid<T>(string path, bool decrypt)
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
                    if (decrypt)
                    {
                        contents = Decrypt(contents);
                    }


                    T data = JsonConvert.DeserializeObject<T>(contents);
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }

        private static byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public static string Crypt(this string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
            byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }

        public static string Decrypt(this string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
            byte[] inputbuffer = Convert.FromBase64String(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Encoding.Unicode.GetString(outputBuffer);
        }
    }
}

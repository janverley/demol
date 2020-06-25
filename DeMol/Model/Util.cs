using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Caliburn.Micro;
using DeMol.ViewModels;
using Newtonsoft.Json;

namespace DeMol.Model
{
    public static class Util
    {
        private static readonly byte[] key = new byte[8] {1, 2, 3, 4, 5, 6, 7, 8};
        private static readonly byte[] iv = new byte[8] {1, 2, 3, 4, 5, 6, 7, 8};

        private static Dictionary<Type, FileData> Files => new Dictionary<Type, FileData>
        {
            {typeof(AdminData), new FileData {Filename   = @".\Files\admin.{0}.json", Encrypted = false}},
            {typeof(DagenData), new FileData {Filename   = @".\Files\dagen.json", Encrypted     = false}},
            {typeof(SpelersData), new FileData {Filename = @".\Files\spelers.json", Encrypted   = false}},
            //{ typeof(VragenData), new FileData{ Filename =  @".\Files\vragen.{0}.json", Encrypted = false }  },
            {typeof(AntwoordenData), new FileData {Filename = @".\Files\antwoorden.{0}.json", Encrypted = false}},
            {typeof(MollenData), new FileData {Filename     = @".\Files\mollen.json", Encrypted         = false}},
            {
                typeof(OpdrachtData),
                new FileData {Filename = @".\Files\OpdrachtVragen.{0}.json", Encrypted = false}
            },
            {typeof(FinaleData), new FileData {Filename = @".\Files\finaleData.json", Encrypted = false}}
        };

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            var elements = source.ToArray();
            for (var i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                var swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

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
            SafeFileWithBackup(data, dag.ToString());
        }

        public static void SafeFileWithBackup(object data, string dag)
        {
            SafeFileWithBackup(String.Format(Files[data.GetType()].Filename, dag), data,
                Files[data.GetType()].Encrypted);
        }

        private static void SafeFileWithBackup(string path, object data, bool encrypt)
        {
            var fileInfo = new FileInfo(path);
            // first backup
            if (fileInfo.Exists)
            {
                var      index = 0;
                FileInfo backupFile;

                do
                {
                    backupFile =
                        new FileInfo(Path.Combine(fileInfo.DirectoryName, "Backups", $"{index}.{fileInfo.Name}"));
                    index++;
                } while (backupFile.Exists);

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

        internal static Tuple<string, Vraag> GetVraagAndCode(OpdrachtData extra, int r)
        {
            var vraag   = extra.Vragen[r];
            var vraagID = $"{extra.Opdracht.ToUpper()}{r + 1}";

            return new Tuple<string, Vraag>(vraagID, vraag);
        }

        internal static Vraag GetVraagFromCode(string code)
        {
            var opdrachtId  = code.Substring(0, 1);
            var vraagNummer = Int32.Parse(code.Substring(1)) - 1;

            var opdrachtVragen = SafeReadJson<OpdrachtData>(opdrachtId);

            var x = GetVraagAndCode(opdrachtVragen, vraagNummer);

            return x.Item2;
        }


        public static T SafeReadJson<T>(int dag) where T : new()
        {
            return SafeReadJson<T>(dag.ToString());
        }

        public static T SafeReadJson<T>(string dag) where T : new()
        {
            var path = String.Format(Files[typeof(T)].Filename, dag);
            return SafeReadJson<T>(path, Files[typeof(T)].Encrypted);
        }

        public static T SafeReadJson<T>() where T : new()
        {
            var path = Files[typeof(T)].Filename;
            return SafeReadJson<T>(path, Files[typeof(T)].Encrypted);
        }

        private static T SafeReadJson<T>(string path, bool decrypt) where T : new()
        {
            var data = new T();

            if (File.Exists(path))
            {
                var contents = File.ReadAllText(path);

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

        internal static bool DataFileFoundAndValid<T>(string dag)
        {
            return DataFileFoundAndValid<T>(String.Format(Files[typeof(T)].Filename, dag), Files[typeof(T)].Encrypted);
        }

        internal static bool DataFileFoundAndValid<T>(int dag)
        {
            return DataFileFoundAndValid<T>(dag.ToString());
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
                var contents = File.ReadAllText(path);
                try
                {
                    if (decrypt)
                    {
                        contents = Decrypt(contents);
                    }

                    var data = JsonConvert.DeserializeObject<T>(contents);
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }

        public static string Crypt(this string text)
        {
            SymmetricAlgorithm algorithm    = DES.Create();
            var                transform    = algorithm.CreateEncryptor(key, iv);
            var                inputbuffer  = Encoding.Unicode.GetBytes(text);
            var                outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }

        public static string Decrypt(this string text)
        {
            SymmetricAlgorithm algorithm    = DES.Create();
            var                transform    = algorithm.CreateDecryptor(key, iv);
            var                inputbuffer  = Convert.FromBase64String(text);
            var                outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Encoding.Unicode.GetString(outputBuffer);
        }

        public static IEnumerable<OpdrachtData> AlleOpdrachtData()
        {
            var result = new List<OpdrachtData>();

            var allChars = "abcdefghijklmnopqrstuvwyz";

            foreach (var @char in allChars.ToCharArray())
            {
                var opdrachtId = @char.ToString();
                if (DataFileFoundAndValid<OpdrachtData>(opdrachtId))
                {
                    var opdrachtVragenData = SafeReadJson<OpdrachtData>(opdrachtId);
                    result.Add(opdrachtVragenData);
                }
            }

            return result;
        }

        public static string OpdrachtUINaam(OpdrachtData opdrachtData)
        {
            return $"{opdrachtData.Opdracht.ToUpper()} - {opdrachtData.Description}";
        }

        public static AdminData GetAdminDataOfSelectedDag(SimpleContainer container)
        {
            return SafeReadJson<AdminData>(container.GetInstance<ShellViewModel>().Dag);
        }

        public static void SafeAdminData(SimpleContainer container, AdminData adminData)
        {
            SafeFileWithBackup(adminData, container.GetInstance<ShellViewModel>().Dag);
        }

        private struct FileData
        {
            public string Filename { get; set; }
            public bool Encrypted { get; set; }
        }
    }
}
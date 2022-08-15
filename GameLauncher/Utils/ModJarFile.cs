using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CmlLib.Utils
{
    public class ModJarFile : JarFile
    {
        public string FileName { get; set; }
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? MCVersion { get; set; }
        public string? Url { get; set; }
        public string?[]? AuthorList { get; set; }
        public string?[]? Dependencies { get; set; }
        public bool IsParse;

        public ModJarFile(string path) : base(path)
        {
            FileName = System.IO.Path.GetFileName(path);
            Id = FileName;
            ParseModInfo();
        }

        public void ParseModInfo()
        {
            string? mcmod = null;

            if (!File.Exists(Path))
            {
                return;
            }

            try
            {
                using (var fs = File.OpenRead(Path))
                using (var s = new ZipInputStream(fs))
                {
                    ZipEntry e;
                    while ((e = s.GetNextEntry()) != null)
                    {
                        if (e.Name == "mcmod.info")
                        {
                            mcmod = readStreamString(s);
                            break;
                        }
                    }
                }
            }
            catch
            {

            }

            if (string.IsNullOrEmpty(mcmod))
            {
                return;
            }

            mcmod = mcmod.Replace("\n", "");

            try
            {
                var arrEunm = JsonSerializer.Deserialize<JsonElement>(mcmod).EnumerateArray();
                if (!arrEunm.MoveNext())
                {
                    return;
                }

                JsonElement json = arrEunm.Current;
                Id = json.GetProperty("modid").GetString();
                Name = json.GetProperty("name").GetString();
                Description = json.GetProperty("description").GetString();
                Version = json.GetProperty("version").GetString();
                MCVersion = json.GetProperty("mcversion").GetString();
                AuthorList = json.GetProperty("authorList").Deserialize<string[]>();
                Dependencies = json.GetProperty("dependencies").Deserialize<string[]>();
                IsParse = true;

            }
            catch(Exception e)
            {
                Trace.WriteLine(e);
            }
            if (Id == null)
            {
                Id = FileName;
            }
        }

    }
}

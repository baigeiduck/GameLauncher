using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CmlLib.Utils
{
    public class ModUtils
    {
        /**
         * 遍历目录下所有模组文件
         */
        public static List<ModJarFile> DirMods(string dir) {
            List<ModJarFile> list = new List<ModJarFile> ();
            string[] files = Directory.GetFiles(dir);
            foreach (string file in files) { 
                if (!File.Exists(file))
                {
                    continue;
                }
                ModJarFile mod = new ModJarFile(file);
                if (mod.IsParse) { 
                    list.Add(mod);
                }
            }
            return list;
        }
    }
}

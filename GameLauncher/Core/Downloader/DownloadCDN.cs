using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TLSP.GameLauncher.Core.Downloader
{
    public class DownloadCDN
    {
        private static IDictionary<string,List<string>> _CDN = new ConcurrentDictionary<string,List<string>>();

        private const string MCBBS = "download.mcbbs.net";
        private const string BMCL = "bmclapi2.bangbang93.com";
        private static string[] DefaultServer = new string[] { MCBBS,BMCL};

        private static readonly string[] empty = new string[0];

        static DownloadCDN()
        {
            Default();
        }

        /// <summary>
        /// 使用默认配置
        /// </summary>
        public static void Default() {
            Clear();
            AddCDN("launchermeta.mojang.com");
            AddCDN("launcher.mojang.com");
            AddCDN("resources.download.minecraft.net", new string[]
            {
                MCBBS + "/assets",
                BMCL + "/assets"
            });
            AddCDN("libraries.minecraft.net", new string[]
            {
                BMCL + "/maven",
                MCBBS + "/maven"
            });
            AddCDN("files.minecraftforge.net/maven", new string[]
            {
                BMCL + "/maven",
                MCBBS + "/maven",
            });
            AddCDN("maven.minecraftforge.net", new string[]
            {
                BMCL + "/maven",
                MCBBS + "/maven",
            });
        }

        public static void AddCDN(string source)
        {
            AddCDN(source, DefaultServer);
        }

        /// <summary>
        /// 添加CDN
        /// </summary>
        /// <param name="source">源站</param>
        /// <param name="cdn">节点地址</param>
        public static void AddCDN(string source,params string[] cdn) { 
            if (_CDN.ContainsKey(source))
            {
                _CDN[source].AddRange(cdn);
            }
            else
            {
                _CDN[source] = new List<string>(cdn);
            }
        }

        public static void Remove(string source)
        {
            if (_CDN.ContainsKey(source))
            {
                _CDN[source].Remove(source);
            }
        }

        public static void Clear() { 
            _CDN.Clear();
        }

        public static string[] GetCDN(string source) {
            if (_CDN.ContainsKey(source))
            {
                return _CDN[source].ToArray();
            }
            return empty;
        }

        /// <summary>
        /// 应用CDN
        /// </summary>
        /// <param name="url">当前访问地址</param>
        /// <returns></returns>
        public static string[] AppleCDN(string url)
        {
            foreach(string k in _CDN.Keys)
            {
                //判断URL为目标Source
                if (url.Contains(k))
                {
                    List<string> result = new List<string>();
                    //替换Source为CDN
                    foreach (string cdn in _CDN[k]) {
                        result.Add(url.Replace(k,cdn));
                    }
                    //添加源站
                    result.Add(url);
                    return result.ToArray();
                }
            }
            return new string[] { url};
        }
    }
}

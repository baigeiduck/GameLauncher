﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace CmlLib.Launcher
{
    public partial class MProfileInfo
    {
        /// <summary>
        /// 웹, 로컬에 있는 모든 프로파일을 가져옵니다.
        /// </summary>
        /// <returns>프로파일 리스트</returns>
        public static MProfileInfo[] GetAllProfileList()
        {
            var list = new List<MProfileInfo>();
            list.AddRange(GetLocalProfileList(Minecraft.Versions)); //먼저 로컬 프로파일을 불러옴
            foreach (var item in GetWebProfiles()) //다음 웹 프로파일을 불러옴
            {
                var prof = from arr in list
                           where arr.Name == item.Name
                           select arr;
                if (prof.ToArray().Length == 0)
                    list.Add(item);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Minecraft.Versions 폴더에서 다운로드된 프로파일 목록을 검색하고 가져옵니다.
        /// </summary>
        /// <returns>프로파일 리스트</returns>
        public static MProfileInfo[] GetLocalProfileList()
        {
            return GetLocalProfileList(Minecraft.Versions);
        }

        /// <summary>
        /// 설정한 경로에서 다운로드된 프로파일 목록을 검색하고 반환합니다. 
        /// </summary>
        /// <param name="path">검색할 폴더의 경로</param>
        /// <returns>프로파일 리스트</returns>
        public static MProfileInfo[] GetLocalProfileList(string path)
        {
            var list = new List<MProfileInfo>();
            foreach (DirectoryInfo item in new DirectoryInfo(path).GetDirectories())
            {
                var filepath = item.FullName + "\\" + item.Name + ".json";
                if (File.Exists(filepath))
                {
                    list.Add(new MProfileInfo { IsWeb = false, Name = item.Name, Path = filepath });
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 모장 서버에서 존재하는 모든 프로파일을 가져와 반환합니다.
        /// </summary>
        /// <returns>프로파일 리스트</returns>
        public static MProfileInfo[] GetWebProfiles()
        {
            List<MProfileInfo> list = new List<MProfileInfo>();
            JArray jarr;
            using (WebClient wc = new WebClient())
            {
                var jobj = JObject.Parse(wc.DownloadString("https://launchermeta.mojang.com/mc/game/version_manifest.json"));
                jarr = JArray.Parse(jobj["versions"].ToString());
            }

            foreach (JObject item in jarr)
            {
                var obj = item.ToObject<MProfileInfo>();
                obj.IsWeb = true;
                list.Add(obj);
            }
            return list.ToArray();
        }
    }
}
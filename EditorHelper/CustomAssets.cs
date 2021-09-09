using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EditorHelper.Utils;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper {
    public static class CustomAssets {
        public struct CustomAssetData {
            public string Name;
            public string Creator;
            public string BaseURL;
            public string summary;
            public string description;
            public Texture2D Logo;
            public Texture2D SmallLogo;
            public List<string> TextureList;

            public static string CombineUrl(string url1, string url2)
            {
                url1 = url1.TrimEnd('/');
                url2 = url2.TrimStart('/');
                return (url1 + "/" + url2).Replace("#", "%23");
            }
            public CustomAssetData(Dictionary<string, object> data) {
                Name = (string) data["name"];
                Creator = (string) data["author"];
                summary = (string) data["summary"];
                description = (string) data["description"];
                BaseURL = CombineUrl(CustomAssetsUrl, Name);
                TextureList = ((List<object>) data["contents"]).Select(o => (string) o).ToList();
                try {
                    using var webclient = new WebClient();
                    var logoData = webclient.DownloadData(CombineUrl(BaseURL, "_logo.png"));
                    var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    tex.LoadImage(logoData);
                    Logo = tex;
                    SmallLogo = tex;
                    new TextureScale().Bilinear(SmallLogo, 64, tex.height * 64 / tex.width);
                } catch (Exception e) {
                    UnityModManager.Logger.Log($"Error loading logo: {e}");
                    Logo = null;
                    SmallLogo = DefaultLogo;
                }
            }

            public void Import(string path) {
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                using var webclient = new WebClient();
                foreach (var texture in TextureList) {
                    var url = CombineUrl(BaseURL, texture);
                    UnityModManager.Logger.Log(url);
                    var texturePath = Path.Combine(path, texture);
                    if (!Directory.Exists(Path.GetDirectoryName(texturePath))) Directory.CreateDirectory(Path.GetDirectoryName(texturePath)!);
                    webclient.DownloadFile(url, texturePath);
                }
            }
        }
        
        public const string CustomAssetsUrl =
            @"https://raw.githubusercontent.com/papertoy1127/EditorHelper_CustomAssets/master/";

        public static Texture2D DefaultLogo = CanvasDrawer.MakeTexture(1, 64, Color.clear);
        public static bool Inited = false;

        public static List<CustomAssetData> CustomAssetDatas { get; private set; } 

        public static void Load() {
            Inited = false;
            try {
                using var webclient = new WebClient();
                static IEnumerator DownloadStringCallback(object sender, DownloadStringCompletedEventArgs e) {
                    var data = e.Result;
                    UnityModManager.Logger.Log($"Loaded assets data: \n{data}");
                    var assets = (List<object>) GDMiniJSON.Json.Deserialize(data);
                    yield return null;
                    CustomAssetDatas = new List<CustomAssetData>();
                    foreach (Dictionary<string, object> asset in assets) {
                        CustomAssetDatas.Add(new CustomAssetData(asset));
                        yield return null;
                    }

                    Inited = true;
                };
                webclient.DownloadStringCompleted += (sender, e) =>
                    scnEditor.instance.StartCoroutine(DownloadStringCallback(sender, e));
                webclient.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/papertoy1127/EditorHelper_CustomAssets/master/packs.json"));
                
            } catch (Exception e) {
                UnityModManager.Logger.Log($"Cannot load custom assets data ({e})");
                CustomAssetDatas = new List<CustomAssetData>();
            }
        }
    }
}
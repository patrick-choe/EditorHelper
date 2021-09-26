/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EditorHelper.Components;
using EditorHelper.Utils;
using GDMiniJSON;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper {
    public static class CustomAssets {
        public struct CustomAssetData {
            public string Name;
            public string Creator;
            public string BaseURL;
            public string Summary;
            public string Description;
            public string LicenseKR;
            public string LicenseEN;
            public Texture2D Logo;
            public Texture2D SmallLogo;
            public List<string> TextureList;
            public Dictionary<string, bool> TextureToDownload;

            public static string CombineUrl(string url1, string url2)
            {
                url1 = url1.TrimEnd('/');
                url2 = url2.TrimStart('/');
                return (url1 + "/" + url2).Replace("#", "%23");
            }
            public CustomAssetData(Dictionary<string, object> data) {
                Name = (string) data["name"];
                Creator = (string) data["author"];
                Summary = (string) data["summary"];
                LicenseKR = (string) data["license_kr"];
                LicenseEN = (string) data["license_en"];
                Description = (string) data["description"];
                BaseURL = CombineUrl(CustomAssetsUrl, Name);
                TextureList = ((List<object>) data["contents"]).Select(o => (string) o).ToList();
                TextureToDownload = new Dictionary<string, bool>();
                foreach (var texture in TextureList) {
                    TextureToDownload.Add(texture, true);
                }
                
                try {
                    using var webclient = new WebClient();
                    var logoData = webclient.DownloadData(CombineUrl(BaseURL, "_logo.png"));
                    Logo = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    Logo.LoadImage(logoData);
                    SmallLogo = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    SmallLogo.LoadImage(logoData);
                    const int size = 64;
                    if (SmallLogo.width <= SmallLogo.height) {
                        new TextureScale().Bilinear(SmallLogo, SmallLogo.width * size / SmallLogo.height, size);
                    } else {
                        new TextureScale().Bilinear(SmallLogo, size, SmallLogo.height * size / SmallLogo.width);
                    }
                    new TextureScale().Bilinear(Logo, EditorHelperPanel.panelSizeX - 35, Logo.height * (EditorHelperPanel.panelSizeX - 35) / Logo.width);
                } catch (Exception e) {
                    UnityModManager.Logger.Log($"Error loading logo: {e}");
                    Logo = null;
                    SmallLogo = DefaultLogo;
                }
            }

            public static int count = -1;
            public static int total = -1;

            public IEnumerator Import(string path) {
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                var toDownload = TextureToDownload.Where(pair => pair.Value).Select(pair => pair.Key).ToArray();
                count = 0;
                total = toDownload.Length;
                foreach (var texture in toDownload) {
                    using var webclient = new WebClient();
                    yield return null;
                    var url = CombineUrl(BaseURL, texture);
                    UnityModManager.Logger.Log(url);
                    var texturePath = Path.Combine(path, texture);
                    if (!Directory.Exists(Path.GetDirectoryName(texturePath))) Directory.CreateDirectory(Path.GetDirectoryName(texturePath)!);
                    webclient.DownloadFileCompleted += (sender, args) => {
                        count += 1;
                        if (count == total) {
                            count = -1;
                            total = -1;
                        }
                    };
                    webclient.DownloadFileAsync(new Uri(url), texturePath);
                }
                
                if (count == total) {
                    count = -1;
                    total = -1;
                }
            }
            
            public IEnumerator ReImport(string path) {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
                yield return Import(path);
            }
        }
        
        public const string CustomAssetsUrl =
            @"https://raw.githubusercontent.com/papertoy1127/EditorHelper_CustomAssets/master/";

        public static Texture2D DefaultLogo = CanvasDrawer.MakeTexture(1, 64, Color.clear);
        public static bool Inited = false;

        public static List<CustomAssetData> CustomAssetDatas { get; private set; }

        private static List<object> assets;
                
        private delegate void AsyncMethodCaller(object sender, DownloadStringCompletedEventArgs e);

        private static void DownloadStringCallbackAsync(object sender, DownloadStringCompletedEventArgs e) {
            static void Callback(object sender, DownloadStringCompletedEventArgs e) {
                var data = e.Result;
                var o = Json.Deserialize(data);
                assets = (List<object>) o;
            }
            
            var caller = new AsyncMethodCaller(Callback);
            caller.BeginInvoke(sender, e, null, null);
        }

        private static IEnumerator DownloadStringCallback() {
            yield return new WaitUntil(() => assets != null);
            CustomAssetDatas = new List<CustomAssetData>();
            foreach (Dictionary<string, object> asset in assets) {
                CustomAssetDatas.Add(new CustomAssetData(asset));
            }   

            Inited = true;
        }

        public static void Load() {
            Inited = false;
            try {
                using var webclient = new WebClient();

                webclient.DownloadStringCompleted += DownloadStringCallbackAsync;
                webclient.DownloadStringCompleted += (_, _) => scnEditor.instance.StartCoroutine(DownloadStringCallback());
                webclient.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/papertoy1127/EditorHelper_CustomAssets/master/packs.json"));
                
            } catch (Exception e) {
                UnityModManager.Logger.Log($"Cannot load custom assets data ({e})");
                CustomAssetDatas = new List<CustomAssetData>();
            }
        }
    }
}
*/
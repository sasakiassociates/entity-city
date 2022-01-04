using System.IO;
using UnityEditor;
using UnityEngine;

namespace myScript.Editor {
    public class ImportTileTexture : EditorWindow{


        [MenuItem("Sasaki/Import Height Maps")]
        private static void ImportHeightMaps()
            {


            
                
//                TextureImporter importer = new TextureImporter
//                {
//                    isReadable = true,
//                    npotScale = TextureImporterNPOTScale.None,
//                    mipmapEnabled = false,
//                    wrapMode = TextureWrapMode.Clamp,
//                    filterMode = FilterMode.Point,
//                };
//                
//                
//                TextureImporterSettings settings = new TextureImporterSettings();
//                
//                importer.ReadTextureSettings(settings);
//                importer.SetTextureSettings(settings);
                
            
                string folderName = "Heightmaps200";
                if (!AssetDatabase.IsValidFolder("Assets/Resources/" + folderName)) {
                    AssetDatabase.CreateFolder("Assets/Resources/", folderName);
                }
                
                Texture2D texture  = new Texture2D(1,1);
                AssetDatabase.CreateAsset(texture, "Assets/Resources/" + folderName);
                
//                
//
//                string path = EditorUtility.OpenFolderPanel("Load Heightmaps from folder", "", "");
//
//                string[] files = Directory.GetFiles(path);
//
//                foreach (string file in files)
//                    if (file.EndsWith(".png")) {
//                        
//                    }
                
            }
        

    }
}
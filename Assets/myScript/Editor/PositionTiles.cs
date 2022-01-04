using UnityEditor;
using UnityEngine;

namespace myScript.Editor {
    public class PositionTiles : EditorWindow {

        private const string _letters = "ABCDEFGHIJKLMNOPQRSTUVW";

        [MenuItem("Sasaki/Place Tiles")]
        private static void PlaceTiles()
            {
                Texture2D[] tileTextures = Resources.LoadAll<Texture2D>("Tiles");

                Debug.Log("tiles loaded " + tileTextures.Length);

                GameObject prevContainer = GameObject.Find("container");

                if (prevContainer != null) {
                    Debug.Log("destroying previous container");
                    foreach (Transform t in prevContainer.transform) {
                        DestroyImmediate(t.gameObject);
                    }
                    DestroyImmediate(prevContainer);
                }

                //create prefab to use for each tiles 
                GameObject tilePrefab = new GameObject();

                GameObject container = new GameObject("container");
                //rotate tile to face up 
                tilePrefab.transform.rotation = Quaternion.Euler(90, 0, 0);
                //add sprite renderer 
                tilePrefab.AddComponent<SpriteRenderer>();
                //create rect for sprite 
                Rect tileRect = new Rect(0, 0 , tileTextures[0].width, tileTextures[0].height);

                foreach (var t in tileTextures) {
                    //create object in scene 
                    GameObject instance = Instantiate(tilePrefab, container.transform);
                    //set renderer to new sprite with this tiles texture 
                    instance.GetComponent<SpriteRenderer>().sprite = Sprite.Create(t, tileRect, Vector2.one * 0.5f, t.width);
                    //replace name 
                    instance.name = t.name;
                    //get letter
                    char tLetter = instance.name[5];
                    //compare letter to string for value
                    int xOffset = _letters.IndexOf(tLetter);
                    //get number value 
                    int zOffset = int.Parse(instance.name.Split('-')[1]);
                    instance.transform.position = new Vector3(xOffset, 0, -zOffset );
                }

                //delete prefab
                DestroyImmediate(tilePrefab);
            }

    }
}
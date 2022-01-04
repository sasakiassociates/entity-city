using System.Collections;
using System.Collections.Generic;
using myScript.TileImage;
using Unity.Mathematics;
using UnityEngine;

namespace myScript {
    public class LoadTesting : MonoBehaviour {

        public Texture2D testTexture;
        public bool loadImage = false;

        // Update is called once per frame
        void Update()
            {
                if (!loadImage) {
                    return;
                }

                var reader = new TileReader();
                reader.LoadTexture(Resources.Load<Texture2D>("HeightMaps/terrain_E-16"));

            
                loadImage = false;
            }

    }
}
using System.Collections.Generic;
using UnityEngine;

namespace myScript {
    public class TileReader {

        // script created for reading an image at runtime 
        public List<float> storedHeightsList;

        public int imageWidth;
        public int imageHeight;

        public void LoadTexture(Texture2D image)
            {
                storedHeightsList = ReadImage(image);
                imageWidth = image.width;
                imageHeight = image.height;
            }

        public List<float> GetHeights(int rX, int rY, int width, int height)
            {
                List<float> heights = new List<float>();
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        heights.Add(GetHeight(rX + x, rY + y));
                    }
                }
                return heights;
            }

        private float GetHeight(int x, int y)
            {
                if (y < 0 || y > imageHeight) {
                    return 0;
                }
                if (x < 0 || x > imageWidth) {
                    return 0;
                }
                if (y == 0) {
                    y = 1;
                }
                if (x == imageWidth) {
                    x = imageWidth - 1;
                }
                var index = imageWidth * (imageHeight - y) + x;
                if (index > storedHeightsList.Count - 1) {
                    return 0;
                }
                return storedHeightsList[index];
            }

        private List<float> ReadImage(Texture2D source)
            {
                List<float> pix = new List<float>();

                if (source == null) {
                    return pix;
                }
                Color32[] pixels = source.GetPixels32();
                var i = 0;

                for (int z = source.height - 1; z >= 0; z--) {
                    for (int x = 0; x < source.width; x++) {
                        pix.Add(RGBToHeight(pixels[i]));
                        i++;
                    }
                }

                return pix;
            }

        private static float RGBToHeight(Color32 color)
            {
                var r = color.r;
                var g = color.g;
                var b = color.b;
//                return r;
                return r * 256 + g + b / 256 - 32768l;
            }

    }
}
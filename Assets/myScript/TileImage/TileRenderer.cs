using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace myScript.TileImage {
    public class TileRenderer : MonoBehaviour {

        [Range(1f, 10f)]
        public float scaleOffset = 2f;

        public GameObject pinObject;

        private List<PixelData> _pins = new List<PixelData>();

        public Texture2D heightMap;

        //number of pins in viewer
        private int _pinAmountX = 100;
        private int _pinAmountY = 100;

        private float _scale = 4f;
        private float _lowestPoint = 0.00005f;

        private float2 _frameToImageOffset = new float2(0, 0); //in pixels
        private float2 _frameSize = new float2(1000, 1000); //in pixels

        private readonly float _imgSize = 0.25f;

        private readonly TileReader _tileReader = new TileReader();
        private Dictionary<string, float> _pixelValues = new Dictionary<string, float>();

        private void Start()
            {
                //create holder for scene
                GameObject pinHolder = new GameObject("Pin Holder");
                //get values of pixels from images
//                _pixelValues =  _tileReader.ReadImageToDict(heightMap);
                //get converted value to apply to objects
                float modelScale = _imgSize * _frameSize.x / 1000f / _pinAmountX;
                //add the tile position
                float3 tilePos = transform.position;
                //Convert Pixel values to units (Previously was GetPixelValues)
                var values = ConvertPixelsToFrame();
                foreach (var value in values) {
                    //create object
                    GameObject instance = Instantiate(pinObject, pinHolder.transform);
                    //set name 
                    instance.name = value.x + "_" + value.z + "_" + value.y;
                    //scale is related to overall height
                    float3 localScale = new float3(1f, value.y / _scale, 1f) * modelScale;
                    //if point is supppperr low
                    if (localScale.y < _lowestPoint) {
                        localScale.y = _lowestPoint;
                    }
                    instance.transform.localScale = localScale;
                    //create position
                    float3 pinPos = new float3(value.x, localScale.y, value.z) * modelScale;
                    //nudge pins over
                    instance.transform.position = new float3(
                                                      pinPos.x + modelScale / 2f + (tilePos.x - 0.125f),
                                                      localScale.y / 2f,
                                                      pinPos.z + modelScale / 2f + (tilePos.z - 0.125f));
                    //store into struct list
                    PixelData pixelData = new PixelData {depth = localScale.y, prefab = instance};
                    _pins.Add(pixelData);
                }
            }

        private void Update()
            {
                foreach (var pinPixel in _pins) {
                    float3 pinPos = pinPixel.prefab.transform.position;

                    float3 pinScale = pinPixel.prefab.transform.localScale;
                    pinScale = new float3(pinScale.x, pinPixel.depth * scaleOffset, pinScale.z);

                    pinPixel.prefab.transform.localScale = pinScale;
                    pinPixel.prefab.transform.position = new float3(pinPos.x, pinScale.y / 2f, pinPos.z);
                }
            }

        private struct PixelData {

            public GameObject prefab;
            public float depth;

        }

        private float3[] ConvertPixelsToFrame()
            {
                //density of viewer 
                float stepX = _frameSize.x / _pinAmountX;
                float stepY = _frameSize.y / _pinAmountY;
                //temp storage for values 
                float3[] tempValues = new float3[_pinAmountX * _pinAmountY];

                //increment for pixels
                var i = 0;
                //loop through each pin for X and Y
                for (int x = 0; x < _pinAmountX; x++) {
                    for (int y = 0; y < _pinAmountY; y++) {
                        //get the x and y key for accessing the pixel 
                        var pixelX = (int) math.floor(_frameToImageOffset.x + stepX * x);
                        var pixelY = (int) math.floor(_frameToImageOffset.y + stepY * y);
                        //store the accessed pixels value along with the pins position within the viewer
                        tempValues[i] = new float3(x , _pixelValues[pixelX + "_" + pixelY], y);
                        //increment pixels
                        i++;
                    }
                }
                //return array of values for pins 
                return tempValues;
            }

    }
}
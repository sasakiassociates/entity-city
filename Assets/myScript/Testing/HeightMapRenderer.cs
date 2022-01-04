using System.Collections.Generic;
using myScript.TileImage;
using Unity.Mathematics;
using UnityEngine;

namespace myScript {
    public class HeightMapRenderer : MonoBehaviour {

        public Material pinMaterial;
        public Mesh pinMesh;
        public GameObject pinObject;

        [Range(1f, 10f)]
        public float scaleOffset = 2f;

        public Texture2D imageTest;

        private List<PinPixel> _pins = new List<PinPixel>();

        //number of pins in viewer
        private int _pinAmountX = 100;
        private int _pinAmountY = 100;

        private float _scale = 400f;
        private float _lowestPoint = 0.00005f;

        private float _storedScale;

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
//                _pixelValues =  _tileReader.ReadImageToDict(imageTest);
                //get converted value to apply to objects
                float adjustedScale = _imgSize * _frameSize.x / 1000f / _pinAmountX;
                //Convert Pixel values to units (Previously was GetPixelValues)
                var values = ConvertPixelsToFrame();
                foreach (var value in values) {
                    //create object
                    GameObject instance = Instantiate(pinObject, pinHolder.transform);
                    //set name 
                    instance.name = value.x + "_" + value.z + "_" + value.y;
                    //scale is related to overall height
                    float3 localScale = new float3(1f, value.y / _scale, 1f) * adjustedScale;
                    if (localScale.y < _lowestPoint) {
                        localScale.y = _lowestPoint;
                    }
                    instance.transform.localScale = localScale;
                    //create position
                    float3 pinPos = new float3(value.x, localScale.y, value.z) * adjustedScale;
                    //nudge pins over
                    instance.transform.position = new float3(
                        pinPos.x + adjustedScale / 2f,
                        localScale.y / 2f,
                        pinPos.z + adjustedScale / 2f);

                    //store into struct list
                    PinPixel pinPixel = new PinPixel {depth = localScale.y, pin = instance};
                    _pins.Add(pinPixel);
                }
            }

        private void Update()
            {
                //for adjusted scaled
                //use image size multiplied by frame dimension
                //divided by amount of image size in pixels and amount of pins in one dimensions
                float adjustedScale = _imgSize * _frameSize.x / 1000f / _pinAmountX;

                var values = ConvertPixelsToFrame();

                foreach (var pinPixel in _pins) {
                    float3 pinPos = pinPixel.pin.transform.position;

                    float3 pinScale = pinPixel.pin.transform.localScale;
                    pinScale = new float3(pinScale.x,pinPixel.depth* scaleOffset,pinScale.z);
                 
                    pinPixel.pin.transform.localScale = pinScale;
                    pinPixel.pin.transform.position = new float3(pinPos.x, pinScale.y/2f, pinPos.z);
                }
                foreach (var v in values) {
                    var start = new Vector3(v.x, 0, v.z) * adjustedScale;
                    //scale is related to overall height
                    var end = new Vector3(v.x, v.y / _scale, v.z) * adjustedScale;
                    Debug.DrawLine(start, end, Color.blue);
                }
            }

        public struct PinPixel {

            public GameObject pin;
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
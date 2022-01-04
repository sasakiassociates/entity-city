using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace myScript.TileImage {
    public class TileManager : MonoBehaviour {

        private struct PinData {

            public GameObject prefab;
            public float depth;

        }

        private struct PinEntity : IComponentData {

            public float depth;

        }

        public static TileManager Instance { get; set; }

        public Mesh pinMesh;
        public Material pinMaterial;

        //TODO temp 
        public Texture2D testImage;

        public List<TileTracker> activeTiles = new List<TileTracker>();

        //number of pins in viewer
        private int _pinAmountX = 1000;
        private int _pinAmountY = 1000;

        private float _scale = 40f;
        private float _lowestPoint = 0.00005f;

        private float _modelScale;

        private float2 _frameToImageOffset = new float2(0, 0); //in pixels
        private float2 _frameSize = new float2(1000, 1000); //in pixels

        private readonly float _imgSize = 0.25f;

        private readonly TileReader _tileReader = new TileReader();
        private Dictionary<string, float> _pixelValues = new Dictionary<string, float>();

        private EntityManager _entityManager;
        private EntityArchetype _pintEntityArchetype;

        private static readonly int HeightMin = Shader.PropertyToID("_HeightMin");
        private static readonly int HeightMax = Shader.PropertyToID("_HeightMax");

        private IEnumerator SetTileWithEnum(float3 tilePos, float3[] values )
            {
                foreach (var value in values) {
                    //create entity
                    var instance = _entityManager.CreateEntity(_pintEntityArchetype);
                    //scale is related to overall height
                    float3 localScale = new float3(1f, value.y / _scale, 1f) * _modelScale;
                    //if scale is too small or negative
                    if (localScale.y < _lowestPoint) {
                        localScale.y = _lowestPoint;
                    }

                    //create position
                    float3 scalePos = new float3(value.x, localScale.y, value.z) * _modelScale;
                    //nudge pins over
                    float3 finalPos = new float3(
                        scalePos.x + _modelScale / 2f + (tilePos.x - 0.125f),
                        scalePos.y / 2f ,
                        scalePos.z + _modelScale / 2f + (tilePos.z - 0.125f));

                    _entityManager.SetComponentData(instance, new NonUniformScale {Value = localScale});
                    _entityManager.SetComponentData(instance, new Translation {Value = finalPos});
                    _entityManager.SetSharedComponentData(instance, new RenderMesh { mesh = pinMesh, material = pinMaterial});
                }
                yield return null;
            }

        private void Awake()
            {
                Instance = this;

                _entityManager = World.Active.EntityManager;
                _pintEntityArchetype = _entityManager.CreateArchetype(
                    typeof(Translation),
                    typeof(LocalToWorld),
                    typeof(NonUniformScale),
                    typeof(PinEntity),
                    typeof(RenderMesh));

                _modelScale = _imgSize * _frameSize.x / 1000f / _pinAmountX;
                CreateActiveTracker(testImage, transform.position);
            }

        public void CreateActiveTracker(Texture2D heightMap, float3 tilePos)
            {
//                _pixelValues =  _tileReader.ReadImageToDict(heightMap);
                var values = ConvertPixelsToFrame();
                SetTileWithEntities(tilePos, values);
//                StartCoroutine(SetTileWithEnum(tilePos, ConvertPixelsToFrame()));
            }

        private void SetTileWithEntities(float3 tilePos, float3[] values)
            {
                float maxHeight = 0f;
                float minHeight = 0f;

                foreach (var value in values) {
                    //create entity
                    var instance = _entityManager.CreateEntity(_pintEntityArchetype);
                    //scale is related to overall height
                    float3 convertedScale = new float3(1f, value.y / _scale, 1f) * _modelScale;
                   
                    //get tile heights
                    if (convertedScale.y > maxHeight ) {
                        maxHeight = convertedScale.y;
                    }
                    if (convertedScale.y < minHeight ) {
                        minHeight = convertedScale.y;
                    }
                    
                    //if scale is too small or negative
                    if (convertedScale.y < _lowestPoint) {
                        convertedScale.y = _lowestPoint;
                    }

                    //create position
                    float3 valuePos = new float3(value.x, convertedScale.y, value.z) * _modelScale;
                    //nudge pins over
                    float3 finalPos = new float3(
                        valuePos.x + _modelScale / 2f + (tilePos.x - 0.125f),
                        convertedScale.y,
                        valuePos.z + _modelScale / 2f + (tilePos.z - 0.125f));

                    _entityManager.SetComponentData(instance, new NonUniformScale {Value = convertedScale});
                    _entityManager.SetComponentData(instance, new Translation {Value = finalPos});
                    _entityManager.SetSharedComponentData(instance, new RenderMesh { mesh = pinMesh, material = pinMaterial});
                }

                pinMaterial.SetFloat(HeightMin, minHeight);
                pinMaterial.SetFloat(HeightMax, maxHeight);
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
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public struct PinVisualizer : IComponentData { }

namespace myScript {
    public class TileVisualizer : MonoBehaviour {

        public Texture2D tileHeightMap;

        //offset of viewer frame within image
        public float2 viewerOffset; //in pixels
        public float2 viewerSize; //in pixels

        public float scale = 0.05f;

        //number of pins in viewer
        private int _pinX = 50;
        private int _pinY = 50;

        //dictionary of heights in tile keyed by x and y of pixels
        private Dictionary<string, float> _pixelValues = new Dictionary<string, float>();
        private readonly TileReader _tileReader = new TileReader();

        private readonly float _imgSize = 0.25f;
        private List<GameObject> _pins = new List<GameObject>();

        private void Start()
            {
//            _pixelValues = _tileReader.ReadImageToDict(tileHeightMap);
            }


        public void UpdatePosition(float offset, float heightMod)
            {
                foreach (var pin in _pins) {
                    var pos = pin.transform.position;
                    var scale = pin.transform.localScale;
                    pin.transform.localScale = new Vector3(pos.x * offset, pos.y * heightMod, pos.z * offset);
                    pin.transform.position = new Vector3(scale.x * offset, pin.transform.localScale.y, scale.z * offset);
                }
            }

        private void Update()
            {
                var values = GetViewerPixels();

                float adjustedScale = .25f * viewerSize.x / 1000f/_pinX;

                foreach (var v in values) {
                    var start = new Vector3(v.x, 0, v.z) * adjustedScale;
                    //scale is related to overall height
                    var end = new Vector3(v.x, v.y / scale, v.z) * adjustedScale;
                    Debug.DrawLine(start, end, Color.blue);
                }
            }

        public void CreateTileEntity(float meshOffset, float heightMod, Mesh pinMesh, Material pinMaterial)
            {
//            _pixelValues =  _tileReader.ReadImageToDict(tileHeightMap);
                var values = GetViewerPixels();

                var entityManager = World.Active.EntityManager;
                var pinVisualType = entityManager.CreateArchetype(
                    typeof(Translation),
                    typeof(LocalToWorld),
                    typeof(NonUniformScale),
                    typeof(PinVisualizer),
                    typeof(RenderMesh));

                float2 offset = new float2(
                    viewerSize.x / _pinX * meshOffset,
                    viewerSize.y / _pinY * meshOffset
                );
                for (int i = 0, x = 0; x < _pinX; x++) {
                    for (int z = 0; z < _pinY; z++, i++) {
                        //create scale of visual
                        float3 scale = new float3(
                            1f / _pinX,
                            values[i].y * heightMod,
                            1f / _pinX
                        );
                        //set position of visual
                        float3 position = new float3(
                            x,
                            values[i].y,
                            z);
                        //create entity
                        var instance = entityManager.CreateEntity(pinVisualType); //set position
                        entityManager.SetComponentData(instance, new Translation {Value = position + (float3) transform.position}); //set scale
                        entityManager.SetComponentData(instance, new NonUniformScale {Value = scale}); //set mesh and material 
                        entityManager.SetSharedComponentData(instance, new RenderMesh { mesh = pinMesh, material = pinMaterial});
                    }
                }
            }

        private float3[] GetViewerPixels()
            {
                //density of viewer 
                float stepX = viewerSize.x / _pinX;
                float stepY = viewerSize.y / _pinY;
                //temp storage for values 
                float3[] tempValues = new float3[_pinX * _pinY];

                //increment for pixels
                var i = 0;
                //loop through each pin for X and Y
                for (int x = 0; x < _pinX; x++) {
                    for (int y = 0; y < _pinY; y++) {
                        //get the x and y key for accessing the pixel 
                        var pixelX = (int) math.floor(viewerOffset.x + stepX * x);
                        var pixelY = (int) math.floor(viewerOffset.y + stepY * y);
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
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Vuforia;

namespace myScript {
    public class TrackerDistance : MonoBehaviour {

        public GameObject vufCamera;
        public GameObject frame;

        [Range(1, 10)]
        public float scaleAdjust = 4f;

        public int smoothingPoints = 10;

        private Plane _plane = new Plane();

        private float3 _scale;
        private float3 _smoothPoint;
        private const float Tolerance = 0.01f;
        private readonly List<float3> _points = new  List<float3>();

        public static TrackerDistance Instance { get; set; }

        private void Awake()
            {
                Instance = this;
                _plane.SetNormalAndPosition(Vector3.up, Vector3.zero);
            }

        
        
        private void Start()
            {
                _scale = frame.transform.localScale;
                
            }


        public void CreatePlane(Vector3 pos, TrackableBehaviour trackable)
            {
                Debug.Log("plane is created by "+ trackable.name);
                _plane.SetNormalAndPosition(Vector3.up, pos);
                
            }

        public void SetTrackingStatus(Color color)
            {
                foreach (Transform p in frame.transform) {
                    p.GetComponent<MeshRenderer>().material.color = color;
                }
            }
        
        
        private void Update()
            {
//                return;

                float3 camPos = vufCamera.transform.position;
                Camera cam = vufCamera.GetComponent<Camera>();
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f , 0f));

           
                float center;
                if (_plane.Raycast(ray, out center )) {
                    Vector3 hitPoint = ray.GetPoint(center);

                    if (_points.Count >= smoothingPoints) {
                        _points.RemoveAt(0);
                    }

                    _points.Add(hitPoint);
                    _smoothPoint = new float3();

                    foreach (var p in _points) {
                        _smoothPoint += p;
                    }
                    
                    

                    _smoothPoint /= _points.Count;
                    
                    Debug.Log(_smoothPoint);
                    frame.transform.position = _smoothPoint;

                    float  distance =  math.distance(_smoothPoint, camPos) / scaleAdjust;
                    if (distance > Tolerance) {
                        frame.transform.localScale = new float3(
                            _scale.x * distance,
                            _scale.y,
                            _scale.z * distance
                        );
                    }
                }
            }

    }
}
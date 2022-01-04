using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace myScript {
    public class FrameWorld {

        public float3 position;
        public float3 size;

    }

    public class FrameController : MonoBehaviour {

        public GameObject framePrefab;
        public GameObject viewerPrefab;
        public GameObject container;

        public AirtableUIEvents uiEvents;
        public TileTracker tileTracker;

        public int pinX = 500;
        public int pinY = 500;

        public Material pinMaterial;
        public Mesh pinMesh;

        private bool _pinsActive;

        private float _gridSize = .25f;

        private Camera _trackerCamera;
        private PinHandler _visualizer;

        //stuff for debugging on desktop
        private Camera _deskCam;
        private float _deskCamSpeed = 0.5f;

        //stuff for frame transforms 
        private float3 _frameScale;
        private float _frameScaleAdjust = 2f;
        private float _activeFrameScale = 0.6f;
        private Plane _framePlane = new Plane();

        private readonly int _smoothingPointCount = 10;
        private readonly List<float3> _smoothingPoints = new  List<float3>();
        private readonly List<Vector2> _frameCorners = new List<Vector2>();
        
        public float pinAdjust = .75f;

        private const float FrameScaleTolerance = 0.01f;
        private const float MaxFrameScale = 0.25f;
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVW";
        private const int Rows = 19;

        private void Awake()
            {
                _trackerCamera = viewerPrefab.GetComponent<Camera>();

                _frameScale = framePrefab.transform.localScale;
                _framePlane.SetNormalAndPosition(Vector3.up, Vector3.zero);

                ToggleFrameMeshes(false);
                
                _visualizer = new PinHandler(pinMesh, pinMaterial, pinX, pinY);

                _visualizer.CreatePinEntity();
                //bottom left
                _frameCorners.Add(new Vector2(-0.5f, -0.5f));
                //bottom right 
                _frameCorners.Add(new Vector2(+0.5f, -0.5f));
                //top left 
                _frameCorners.Add(new Vector2(-0.5f, +0.5f));
                //top right 
                _frameCorners.Add(new Vector2(+0.5f, +0.5f));
            }

        public void TogglePinEntities(bool state)
            {
                if (state && !_pinsActive) {
                    Debug.Log("pins set to active");
                    PinHandler.activatePins = true;
                    _pinsActive = true;

                } else if(!state && _pinsActive){
                    //turn entities off
                    Debug.Log("pins set to inactive");
//                    PinHandler.deActivatePins = true;
                    _pinsActive = false;
                }
            }

        private void FixedUpdate()
            {
                MoveFrameWithVuforia();
            }


        public void ToggleFrameMeshes(bool state)
            {
                foreach (Transform p in framePrefab.transform) {
                    p.GetComponent<MeshRenderer>().enabled = state;
                }
            }
        
        private void MoveFrameWithVuforia()
            {
                float3 camPos = viewerPrefab.transform.position;
                Ray ray = _trackerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f , 0f));

                float center;
                if (_framePlane.Raycast(ray, out center )) {
                    Vector3 hitPoint = ray.GetPoint(center);

                    if (_smoothingPoints.Count >= _smoothingPointCount) {
                        _smoothingPoints.RemoveAt(0);
                    }

                    _smoothingPoints.Add(hitPoint);
                    float3 smoothPoint = new float3();

                    foreach (var p in _smoothingPoints) {
                        smoothPoint += p;
                    }

                    smoothPoint /= _smoothingPoints.Count;
                    framePrefab.transform.position = smoothPoint;

                    float distance = math.distance(smoothPoint, camPos) / _frameScaleAdjust;
                    //set the bounds of the frame 
                    distance = math.min(distance, MaxFrameScale);
                    if (distance > FrameScaleTolerance) {
                        framePrefab.transform.localScale = new float3(
                            _frameScale.x * distance,
                            _frameScale.y,
                            _frameScale.z * distance
                        );
                        _activeFrameScale = framePrefab.transform.localScale.x;
                        PinHandler.pinScale = _activeFrameScale * pinAdjust;

                        //update tile text
                        uiEvents.CurrentTile = GetCenterTileID();
                    }
                }
            }

        private string GetCenterTileID()
            {
                var position = framePrefab.transform.position - container.transform.position;
                
                int tileX = (int) math.round(position.x / _gridSize);
                int tileY = (int) math.round(position.z / _gridSize);
                
                int tileAddrY = (tileY * -1);
                
                return Letters[tileX] + "-" + tileAddrY;
            }

        public List<TileRect> GetTilesInFrame()
            {
                List<TileRect> tileRects = new List<TileRect>();

                for (var i = 0; i < _frameCorners.Count; i++) {
                    var point = _frameCorners[i];
                    var position = framePrefab.transform.position - container.transform.position;
                    //multiply by 4 to account for container scale 
                    int tileX = (int) math.round(point.x * 4f * _activeFrameScale + position.x / _gridSize);
                    int tileY = (int) math.round(point.y * 4f * _activeFrameScale + position.z / _gridSize);

                    int tileAddrY = (tileY * -1);

                    if (tileX < 0 || tileX >= Letters.Length) continue;
                    if (tileAddrY < 1 || tileAddrY > Rows) continue;

                    var tileId = Letters[tileX] + "-" + tileAddrY;
                    
                    Vector2 tileLocation = new Vector2((tileX - 0.5f) * _gridSize, (tileY - 0.5f) * _gridSize);
                    Vector2 frameCorner = new Vector2(point.x * _activeFrameScale + position.x, point.y * _activeFrameScale + position.z);

                    var tileRect = GetRect(i, tileId, tileLocation, frameCorner);

                    tileRects.Add(tileRect);
                }

                return tileRects;
            }

        private TileRect GetRect(int i, string id,  Vector2 tileLocation, Vector2 frameCorner)
            {
                switch (i) {
                    case 0:
                        return NormalizeTopLeft(id, ToWorldConvert(tileLocation), ToWorldConvert(frameCorner), Side.Bottom, Side.Left);

                    case 1:
                        return NormalizeTopLeft(id, ToWorldConvert(tileLocation), ToWorldConvert(frameCorner), Side.Bottom, Side.Right);

                    case 2:
                        return NormalizeTopLeft(id, ToWorldConvert(tileLocation), ToWorldConvert(frameCorner), Side.Top, Side.Left);

                    case 3:
                        return NormalizeTopLeft(id, ToWorldConvert(tileLocation), ToWorldConvert(frameCorner), Side.Top, Side.Right);
                    default:
                        throw new NotImplementedException("Unknown index or you cant spell");
                }
            }

        private TileRect NormalizeTopLeft(string id, Vector3 tileCorner, Vector3 frameCorner, Side v, Side h)
            {
                Vector3 tileBotLeft = new Vector3(tileCorner.x, tileCorner.y, tileCorner.z);

                var norm = (frameCorner - tileBotLeft) / _gridSize;

                Rect botLeftRect = new Rect(0, 0, norm.x, 1f - norm.z);

                return new TileRect {
                    id = id,
                    rect = botLeftRect,
                    horizontal = h,
                    vertical = v
                };
            }

        private Vector3 ToWorldConvert(Vector2 v2)
            {
                return new Vector3(v2.x, 0f, v2.y) + container.transform.position;
            }

        public FrameWorld GetWorldFrame()
            {
                float3 framePos = framePrefab.transform.position;

                var bL  =  new float3(-0.5f, 0, -0.5f) * _activeFrameScale + framePos;
                var tR  = new float3(+0.5f, 0, +0.5f) * _activeFrameScale + framePos;

                return new FrameWorld {position = bL, size = tR - bL};
            }

    }
}
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace myScript {
    public class PinController : MonoBehaviour {

        public int xAmount = 5;
        public int yAmount = 5;

        [Range(0.01f, 10f)]
        public float pinSpeed = 1f;

        public GameObject pinPrefab;

        private GameObject _prefab;
        private GameObject _container;
        private List<GameObject> _pins;

        private void Start()
            {
                CreateVisual();
            }

        private void CreateVisual()
            {
                if (_container != null) {
                    Destroy(_container.gameObject);
                }

                _container = new GameObject("container");
                _pins = new List<GameObject>();

                _prefab = Instantiate(pinPrefab);

                for (int x = 0; x < xAmount; x++) {
                    for (int z = 0; z < yAmount; z++) {
                        //create instance of visual
                        GameObject instance = Instantiate(_prefab, _container.transform, true);
                        //randomize its y pos
                        float height = Mathf.PerlinNoise(1f, 10f) * Random.Range(1, 5);
                        //get and store positions 
                        var visual = instance.GetComponent<PinVisual>();
                        visual.StartPosition = new float3(x, 0, z);
                        visual.EndPosition = new float3(x, height, z);
                        //set position of object
                        instance.transform.position = visual.StartPosition;
                        //set parent to container 
                        _pins.Add(instance);
                    }
                }

                Destroy(_prefab);
            }

        private void Update()
            {
                foreach (var p in _pins) {
                    MovePins(p);
                }
            }

        private void MovePins(GameObject p)
            {
                var visual = p.GetComponent<PinVisual>();

                Vector3 position = math.lerp(
                    visual.StartPosition,
                    visual.EndPosition,
                    math.abs(math.sin(Time.time + pinSpeed)));

                p.transform.localScale = math.lerp(
                    new float3(1f, 1f, 1f),
                    new float3(1f, visual.EndPosition.y, 1f), math.abs(math.sin(Time.time + pinSpeed)));

                var tPos = position;
                position = new Vector3(tPos.x, p.transform.localScale.y/2, tPos.z);
                p.transform.position = position;
            }

    }
}
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace myScript.Visualizer
{
    public struct Pin : IComponentData
    {
        public float3 postion;
    }

    public class RenderPoints : MonoBehaviour
    {
        public int pointCount = 100;
        private float3[] _savePoints;
        private static Material _lineMaterial;

        private static void CreateLineMaterial() {
            if (!_lineMaterial) {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader) {hideFlags = HideFlags.HideAndDontSave};
                _lineMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _lineMaterial.SetInt("_Cull", (int) UnityEngine.Rendering.CullMode.Off);
                _lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        private void OnRenderObject() {
            CreateLineMaterial();
            _lineMaterial.SetPass(0);

            float3[] points = MovePointObject(pointCount);
            

//            float[] points = new float[pointCount];
//            NativeArray<float> input = new NativeArray<float>(pointCount, Allocator.Persistent);
//            for (int i = 0; i < input.Length; i++) {
//                input[i] = 1.0f * i;
//                points[i] = input[i];
//            }
//            var job = new RenderJob {Input = input};
//            input.Dispose();
//            job.Schedule().Complete();

            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.TRIANGLES);

            for (int i = 0; i < points.Length; ++i) {
                float a = i / (float) pointCount;
                GL.Color(new Color(a, 1 - a, 0, 0.8F));
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(points[i].x, points[i].y, points[i].z);
            }

            GL.End();
            GL.PopMatrix();
        }

        private float3[] MovePointObject(int count) {
            float3[] points = new float3[count];
            for (int i = 0; i < points.Length; i++) {
                float x = Random.Range(0, 100);
                float z = Random.Range(0, 10);
                float3 p = new float3(x, 0, z);
                points[i] = p;
            }

            return points;
        }


        [BurstCompile(CompileSynchronously = true)]
        private struct MovePinPosition : IJobForEach<Pin, Translation>
        {
            public NativeArray<float> Pins;

            public void Execute(ref Pin pin, ref Translation pos) {
            }
        }


        [BurstCompile(CompileSynchronously = true)]
        private struct RenderJob : IJob
        {
            [ReadOnly] public NativeArray<float> Input;

            public void Execute() {
                float result = 0.0f;

                for (var i = 0; i < Input.Length; i++) {
                    var t = Input[i];
                    result += t;
                }
            }
        }
    }
}
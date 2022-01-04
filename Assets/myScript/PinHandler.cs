using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace myScript {
    public class PinHandler {

        public static float pinScale = 0.1f;
        public static float pinHeightScale = 4000f; // should be 4000

        private static Material _pinMaterial;
        private static Mesh _pinMesh;
        private static int _pinX;
        private static int _pinY;

        public static float pinPositionThreshold = 0.01f;
        public static float pinSpeed = 1.0f;
        public static float pinSmoothing = 1f;

        public static bool activatePins = false;
        public static bool deActivatePins = false;

        public PinHandler(Mesh pinMesh, Material pinMaterial, int pinX, int pinY)
            {
                _pinMesh = pinMesh;
                _pinMaterial = pinMaterial;
                _pinX = pinX;
                _pinY = pinY;
            }

        public void CreatePinEntity()
            {
                var entityManager = World.Active.EntityManager;
                var pinVisualType = entityManager.CreateArchetype(
                    typeof(Translation),
                    typeof(LocalToWorld),
                    typeof(NonUniformScale),
                    typeof(PinData),
                    typeof(RenderMesh));

                int pinCount = _pinX * _pinY;
                for (int x = 0; x < pinCount; x++) {
                    entityManager.CreateEntity(pinVisualType);
                }
            }

        private struct Pin : IComponentData { }

        private struct PinData : IComponentData {

            public float3 PositionEnd;
            public float3 Scale;

        }

        public class PinSystem : JobComponentSystem {

            private EntityCommandBufferSystem _commandBuffer;

            protected override void OnCreate()
                {
                    _commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
                }

            private struct SetPinDataJob : IJobForEachWithEntity<PinData> {

                public NativeArray<float3> PinPositions;
                public NativeArray<float3> PinScales;

                [BurstCompile]
                public void Execute(Entity entity, int index, ref PinData pinData)
                    {
                        pinData.PositionEnd = PinPositions[index];
                        pinData.Scale = PinScales[index];
                    }

            }

            private struct SetPinsToActive : IJobForEachWithEntity<PinData, Translation, NonUniformScale> {

                [WriteOnly] public EntityCommandBuffer.Concurrent CommandBuffer;

                public void Execute(Entity entity, int index, ref PinData pinData, ref Translation pos, ref NonUniformScale scale)
                    {
                        CommandBuffer.SetComponent(index, entity, new NonUniformScale {Value =  new float3(pinData.Scale.x, 0f, pinData.Scale.z)});
                        CommandBuffer.SetComponent(index, entity, new Translation {Value = pinData.PositionEnd});
                        CommandBuffer.AddComponent(index, entity, typeof(Pin));
                        CommandBuffer.SetSharedComponent(index, entity, new RenderMesh { mesh = _pinMesh, material = _pinMaterial});
                    }

            }

//            private struct SetPinsToInactive : IJobForEachWithEntity<Pin, PinData> {
//
//                public EntityCommandBuffer.Concurrent CommandBuffer;
//
//                public void Execute(Entity entity, int index, ref Pin pin, ref PinData pinData)
//                    {
//                        CommandBuffer.SetComponent(index, entity, new NonUniformScale {Value =  new float3(pinData.Scale.x, 0f, pinData.Scale.z)});
//                        CommandBuffer.RemoveComponent(index, entity, typeof(Pin));
//                        CommandBuffer.RemoveComponent(index, entity, typeof(RenderMesh));
//                    }
//
//            }

            //animate to new position
            [RequireComponentTag(typeof(Pin))]
            private struct MoveAndScalePinJob : IJobForEachWithEntity<PinData, Translation, NonUniformScale> {

                [ReadOnly] public float PinThreshold;
                [ReadOnly] public float PinSpeed;
                [ReadOnly] public float PinSmooth;

                [BurstCompile]
                public void Execute(Entity entity, int index, ref PinData pinData, ref Translation pos, ref NonUniformScale scale)
                    {
                        float3 curPos = pos.Value;
                        float3 curScale = scale.Value;

                        if (math.distance(scale.Value.y, pinData.Scale.y) > PinThreshold) {
                            scale.Value += (pinData.Scale - curScale) * PinSpeed / PinSmooth;
                        } else {
                            scale.Value = pinData.Scale;
                        }

                        if (math.distance(pos.Value, pinData.PositionEnd) > PinThreshold) {
                            pos.Value += (pinData.PositionEnd - curPos) * PinSpeed / PinSmooth;
                        } else {
                            pos.Value = pinData.PositionEnd;
                        }
                    }

            }

            protected override JobHandle OnUpdate(JobHandle inputDeps)
                {
                    if (HeightMapManager.Instance == null) {
                        return new JobHandle();
                    }

                    TileRegion tempRegion = HeightMapManager.Instance.CurrentRegion;

                    if (tempRegion?.worldFrame == null) {
                        return new JobHandle();
                    }

                    float[] values = GetValueOfRegion(tempRegion);

                    int pinCount = _pinX * _pinY;
                    NativeArray<float3> pinPositions = new NativeArray<float3>(pinCount, Allocator.TempJob);
                    NativeArray<float3> pinScales = new NativeArray<float3>(pinCount, Allocator.TempJob);

                    int i = 0;
                    for (int z = _pinY - 1; z >= 0; z--) {
                        for (int x = 0; x < _pinX; x++) {
                            float wx = tempRegion.worldFrame.position.x + x  * tempRegion.worldFrame.size.x / _pinX;
                            float wy = tempRegion.worldFrame.position.z + z * tempRegion.worldFrame.size.z / _pinY;

                            float height = values[i] / pinHeightScale;
                            pinScales[i] = new float3(pinScale / _pinX, height, pinScale / _pinX);
                            pinPositions[i] = new float3(wx, height / 2f, wy);
                            i++;
                        }
                    }

                    JobHandle updatePinJob = new SetPinDataJob {PinPositions = pinPositions, PinScales = pinScales}.Schedule(this, inputDeps);

                    updatePinJob.Complete();
                    pinPositions.Dispose();
                    pinScales.Dispose();

                    if (activatePins) {
                        var commandBuffer = _commandBuffer.CreateCommandBuffer().ToConcurrent();
                        JobHandle createPinJob = new SetPinsToActive { CommandBuffer = commandBuffer}.Schedule(this, inputDeps);
                        createPinJob.Complete();
                        activatePins = false;
                    }

//                    if (deActivatePins) {
//                        var commandBuffer = _commandBuffer.PostUpdateCommands.ToConcurrent();
//                        JobHandle deactivePins = new SetPinsToInactive {CommandBuffer = commandBuffer}.Schedule(this, inputDeps);
//                        deactivePins.Complete();
//                        deActivatePins = false;
//                    }

                    JobHandle animatePinJob = new MoveAndScalePinJob {
                        PinSmooth =  pinSmoothing, PinSpeed = pinSpeed, PinThreshold = pinPositionThreshold
                    }.Schedule(this, inputDeps);

                    animatePinJob.Complete();

                    return animatePinJob;
                }

        }

        private static float[] GetValueOfRegion(TileRegion region)
            {
                //temp storage for values 
                float[] tempValues = new float[_pinX * _pinY];

                if (region == null) {
                    return tempValues;
                }
                //density of viewer 
                float stepX = region.width / (float) _pinX;
                float stepY = region.height / (float) _pinY;

                //loop through each pin for X and Y
                for (int i = 0, x = 0; x < _pinX; x++) {
                    for (int y = 0; y < _pinY; y++, i++) {
                        //get the x and y key for accessing the pixel 
                        var pixelX = (int) math.floor(stepX * x);
                        var pixelY = (int) math.floor(stepY * y);

                        var index = region.height * pixelY + pixelX;
                        //store the accessed pixels value along with the pins position within the viewer
                        if (region.depths.Count > index) {
                            tempValues[i] =  region.depths[index];
                        } else {
                            tempValues[i] = 0;
                        }
                    }
                }
                //return array of values for pins 
                return tempValues;
            }

    }
}
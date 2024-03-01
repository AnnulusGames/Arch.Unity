using UnityEngine;
using Arch.Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Arch.Core;
using Arch.Core.Utils;
using Unity.Burst;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Unity.PerformanceTesting;

namespace Arch.Unity.Tests.Runtime
{
    public struct StressTestComponent
    {
        public int Value;
    }

    public class StressTest
    {
        World world;
        QueryDescription query;

        [SetUp]
        public void OneTimeSetUp()
        {
            world = World.Create();

            for (int i = 0; i < 2_500_000; i++)
            {
                world.Create(new StressTestComponent()
                {
                    Value = 0
                });
            }

            query = new QueryDescription().WithAll<StressTestComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            world.Dispose();
        }

        [Test, Performance]
        public void Test_Query()
        {
            Measure.Method(() =>
            {
                world.Query(query, (ref StressTestComponent test) =>
                {
                    test.Value++;
                });
            })
            .WarmupCount(10)
            .MeasurementCount(100)
            .Run();

            world.Query(query, (ref StressTestComponent test) =>
            {
                Assert.That(test.Value, Is.EqualTo(110));
            });
        }

        [Test, Performance]
        public void Test_Job()
        {
            Measure.Method(() =>
            {
                new StressTestJob()
                {
                    StressTestComponentId = Component<StressTestComponent>.ComponentType.Id
                }
                .ScheduleParallel(world, query)
                .Complete();
                JobArchChunkHandle.CheckHandles();
            })
            .WarmupCount(10)
            .MeasurementCount(100)
            .Run();

            world.Query(query, (ref StressTestComponent test) =>
            {
                Assert.That(test.Value, Is.EqualTo(110));
            });
        }
    }

    [BurstCompile]
    public struct StressTestJob : IJobArchChunk
    {
        public int StressTestComponentId;

        public unsafe void Execute(NativeChunk chunk)
        {
            var array = chunk.GetNativeArray<StressTestComponent>(StressTestComponentId);
            var ptr = (StressTestComponent*)array.GetUnsafePtr();
            for (int i = 0; i < array.Length; i++)
            {
                ptr[i].Value++;
            }
        }
    }
}
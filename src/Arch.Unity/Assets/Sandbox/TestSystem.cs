using Arch.Core;
using Arch.Core.Utils;
using Arch.Unity;
using Arch.Unity.Conversion;
using Arch.Unity.Jobs;
using Unity.Burst;
using UnityEngine;

public class TestSystem : MonoBehaviour
{
    World world;
    QueryDescription query;

    void Start()
    {
        world = EntityConversion.DefaultWorld;
        query = new QueryDescription().WithAll<Test>();
    }

    void Update()
    {
        var job = new TestJob()
        {
            ComponentId = Component<Test>.ComponentType.Id
        };
        job.ScheduleParallel(world, query).Complete();

        world.Query(query, (ref Test test) =>
        {
            Debug.Log(test.Name + ":" + test.Value);
        });
    }
}

[BurstCompile]
public struct TestJob : IJobArchChunk
{
    public int ComponentId;

    public void Execute(NativeChunk chunk)
    {
        var array = chunk.GetNativeArray<Test>(ComponentId);
        for (int i = 0; i < array.Length; i++)
        {
            var test = array[i];
            test.Value++;
            array[i] = test;
        }
    }
}

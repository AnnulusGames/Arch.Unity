using Arch.Core;
using Arch.Core.Utils;
using Arch.System;
using Arch.Unity.Jobs;
using Arch.Unity.Toolkit;
using R3;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

public static class TestBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        ArchApp.Create()
            .AddSystems(SystemRunner.FixedUpdate, systems =>
            {
                systems.Add<HelloSystem>();
            })
            .Run()
            .RegisterTo(Application.exitCancellationToken);
    }
}

public struct User
{
    public User(FixedString64Bytes name)
    {
        this.Name = name;
    }

    public FixedString64Bytes Name;
}

public struct Score
{
    public int Value;
}

[BurstCompile]
public struct IncrementScoreJob : IJobArchChunk
{
    public int Id;

    public void Execute(NativeChunk chunk)
    {
        var array = chunk.GetNativeArray<Score>(Id);
        for (int i = 0; i < array.Length; i++)
        {
            var id = array[i];
            id.Value++;
            array[i] = id;
        }
    }
}

public sealed partial class HelloSystem : BaseSystem<World, SystemState>
{
    public HelloSystem(World world) : base(world) { }

    public override void Initialize()
    {
        World.Create(new User("Alice"), new Score() { Value = 1 });
        World.Create(new User("Bob"), new Score() { Value = 2 });
    }

    public override void BeforeUpdate(in SystemState t)
    {
        var job = new IncrementScoreJob()
        {
            Id = Component<Score>.ComponentType.Id
        };
        job.Schedule(World, new QueryDescription().WithAll<Score>())
            .Complete();
    }

    [Query]
    public void Hello(ref User user)
    {
        Debug.Log($"Hello, {user.Name}!");
    }
}
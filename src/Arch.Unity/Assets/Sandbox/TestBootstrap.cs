using Arch.Core;
using Arch.System;
using Arch.Unity.Toolkit;
using R3;
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

public class User
{
    public User(string name)
    {
        this.Name = name;
    }

    public string Name;
}


public sealed partial class HelloSystem : BaseSystem<World, SystemState>
{
    public HelloSystem(World world) : base(world) { }

    public override void Initialize()
    {
        World.Create(new User("Alice"));
        World.Create(new User("Bob"));
    }

    [Query]
    public void Hello(ref User user)
    {
        Debug.Log($"Hello, {user.Name}!");
    }
}
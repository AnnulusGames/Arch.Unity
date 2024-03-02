using Arch.System;

namespace Arch.Unity.Toolkit
{
    public interface ISystemRunner
    {
        void Run();
        void Add(ISystem<SystemState> system);
        void Remove(ISystem<SystemState> system);
    }
}
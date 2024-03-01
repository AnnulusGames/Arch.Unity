using Arch.System;

namespace Arch.Unity.Toolkit
{
    public interface ISystemRunner
    {
        void Run();
        void Register(ISystem<SystemState> system);
        void Unregister(ISystem<SystemState> system);
    }
}
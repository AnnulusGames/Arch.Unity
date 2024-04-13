using Arch.System;

namespace Arch.Unity.Toolkit
{
    public interface ISystemRunner
    {
        void Run();
        void Add(UnitySystemBase system);
        void Remove(UnitySystemBase system);
    }
}
using Arch.Unity;

namespace Arch.Unity.Toolkit
{
    public interface ISystemRunner
    {
        PlayerLoopTiming Timing { get; }
        void Run();
        void Add(UnitySystemBase system);
        void Remove(UnitySystemBase system);
    }
}

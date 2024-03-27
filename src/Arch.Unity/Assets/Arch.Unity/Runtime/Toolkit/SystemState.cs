using System;

namespace Arch.Unity.Toolkit
{
    public struct SystemState : IEquatable<SystemState>
    {
        public float DeltaTime;
        public double Time;

        public readonly bool Equals(SystemState other)
        {
            return other.DeltaTime == DeltaTime && other.Time == Time;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is SystemState state) return Equals(state);
            return false;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(DeltaTime, Time);
        }
    }
}
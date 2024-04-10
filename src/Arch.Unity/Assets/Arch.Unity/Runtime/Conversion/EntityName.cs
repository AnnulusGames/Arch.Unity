using System;
using Unity.Collections;

namespace Arch.Unity
{
    public readonly struct EntityName : IEquatable<EntityName>
    {
        public EntityName(in FixedString64Bytes value)
        {
            Value = value;
        }

        public readonly FixedString64Bytes Value;

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is not EntityName entityName) return false;
            return entityName.Equals(this);
        }

        public bool Equals(EntityName other)
        {
            return other.Value.Equals(Value);
        }

        public override string ToString()
        {
            var str = Value;
            return str.ConvertToString();
        }

        public static implicit operator FixedString64Bytes(EntityName entityName)
        {
            return entityName.Value;
        }

        public static implicit operator string(EntityName entityName)
        {
            var str = entityName.Value;
            return str.ConvertToString();
        }

        public static implicit operator EntityName(FixedString64Bytes value)
        {
            return new(value);
        }

        public static implicit operator EntityName(string value)
        {
            return new(value);
        }
    }
}
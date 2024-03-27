using System;
using UnityEngine;

namespace Arch.Unity.Conversion
{
    public readonly struct GameObjectReference : IEquatable<GameObjectReference>
    {
        public GameObjectReference(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
        
        public readonly GameObject GameObject;

        public bool Equals(GameObjectReference other)
        {
            return other.GameObject == GameObject;
        }

        public override bool Equals(object obj)
        {
            if (obj is GameObjectReference reference) return Equals(reference);
            return false;
        }

        public override int GetHashCode()
        {
            return GameObject.GetHashCode();
        }
    }
}

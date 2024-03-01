using UnityEngine;

namespace Arch.Unity.Conversion
{
    public readonly struct GameObjectReference
    {
        public GameObjectReference(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
        
        public readonly GameObject GameObject;
    }
}

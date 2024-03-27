using UnityEngine;

namespace Arch.Unity.Conversion
{
    [AddComponentMenu("Arch/Entity Converter")]
    [DefaultExecutionOrder(-100)]
    public sealed class EntityConverter : MonoBehaviour
    {
        [SerializeField] EntityConversionOptions options;

        void Awake()
        {
            EntityConversion.Convert(gameObject, options);
        }
    }
}
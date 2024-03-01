using Arch.Unity;
using Arch.Unity.Conversion;
using Unity.Collections;
using UnityEngine;

public class TestAuthoring : MonoBehaviour, IComponentConverter
{
    public int value;

    public void Convert(IEntityConverter converter)
    {
        converter.AddComponent(new Test()
        {
            Name = name,
            Value = value
        });
    }
}

public struct Test
{
    public float Value;
    public FixedString128Bytes Name;
}
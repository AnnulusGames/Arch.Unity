using Arch.Unity.Conversion;
using UnityEngine;

public class ExampleConverter : MonoBehaviour, IComponentConverter
{
    public int value;

    public void Convert(IEntityConverter converter)
    {
        converter.AddComponent(new ExampleComponent()
        {
            Value = value
        });
    }
}

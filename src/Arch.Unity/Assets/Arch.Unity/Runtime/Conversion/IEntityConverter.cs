using Arch.Core;

namespace Arch.Unity.Conversion
{
    public interface IEntityConverter
    {
        void AddComponent<T>(T component);
        Entity Convert(World world);
    }
}

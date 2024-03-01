namespace Arch.Unity.Conversion
{
    public static class EntityConverterExtensions
    {
        public static void AddComponent<T>(this IEntityConverter converter) where T : unmanaged
        {
            converter.AddComponent<T>(default);
        }
    }
}

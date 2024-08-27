using System;
using Arch.Core;

namespace Arch.Unity.Queries
{
    public ref struct NativeQueryArchetypeEnumerator
    {
        private readonly NativeQuery _query;
        private Enumerator<Archetype> _archetypes;

        internal NativeQueryArchetypeEnumerator(NativeQuery query, Span<Archetype> archetypes)
        {
            _query = query;
            _archetypes = new Enumerator<Archetype>(archetypes);
        }

        public bool MoveNext()
        {
            while (_archetypes.MoveNext())
            {
                var archetype = _archetypes.Current;
                if (archetype.Entities > 0 && _query.Valid(archetype.BitSet))
                {
                    return true;
                }
            }

            return false;
        }

        public readonly Archetype Current => _archetypes.Current;
    }
}
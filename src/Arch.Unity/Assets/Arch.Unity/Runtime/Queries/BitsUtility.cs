using System;
using UnityEngine;

namespace Arch.Unity.Queries
{
    internal static class BitsUtility
    {
        public static bool Any(in Span<uint> bits, in Span<uint> otherBits)
        {
            var min = Mathf.Min(bits.Length, otherBits.Length);

            for (var i = 0; i < min; i++)
            {
                var bit = bits[i];
                if ((bit & otherBits[i]) > 0) return true;
            }

            return false;
        }

        public static bool All(in Span<uint> bits, in Span<uint> otherBits)
        {
            var min = Mathf.Min(bits.Length, otherBits.Length);

            for (var i = 0; i < min; i++)
            {
                var bit = bits[i];
                if ((bit & otherBits[i]) != bit) return false;
            }

            for (var i = min; i < bits.Length; i++)
            {
                if (bits[i] != 0) return false;
            }

            return true;
        }

        public static bool None(in Span<uint> bits, in Span<uint> otherBits)
        {
            var min = Mathf.Min(bits.Length, otherBits.Length);

            for (var i = 0; i < min; i++)
            {
                var bit = bits[i];
                if ((bit & otherBits[i]) > 0) return false;
            }

            return true;
        }

        public static bool Exclusive(in Span<uint> bits, in Span<uint> otherBits)
        {
            var min = Mathf.Min(bits.Length, otherBits.Length);

            for (var i = 0; i < min; i++)
            {
                var bit = bits[i];
                if ((bit ^ otherBits[i]) != 0) return false;
            }

            for (var i = min; i < bits.Length; i++)
            {
                if (bits[i] != 0) return false;
            }

            return true;
        }
    }
}
using System;
using System.Diagnostics;

namespace NonogramSolver.Backend
{

    // An immutable value that represents a set of colors
    public struct ColorSet
    {
        public ColorSet(uint val)
        {
            value = val;
        }

        private readonly uint value;

        public bool IsEmpty => (value == 0);

        public bool HasColor(uint color)
        {
            uint mask = ColorToMask(color);
            return (value & mask) != 0;
        }

        public bool IsSingle()
        {
            uint val = value;
            while (val != 0)
            {
                // If we ever hit a point where val is 1, then there must only be one bit
                if (val == 1)
                    return true;

                // Val is not 1 (implicit from above) but the lowest bit is 1 => more than one bit is 1
                if ((val & 1) != 0)
                    return false;

                val = val >> 1;
            }

            return false;
        }

        public static readonly ColorSet Empty = new ColorSet(0);

        public static ColorSet CreateFullColorSet(uint maxColor)
        {
            // Lets suppose there are 8 bits
            // Max = 1111,1111
            // Suppose the max color is 2
            // This means that the valid color indexes are:
            //      001, 010, 100
            // The number of bits that is used by colors is 3
            // And then the full color set is:
            // 0000,0111 = Max >> 5
            uint numbits = maxColor + 1;
            int shiftby = (int)(32u - numbits);
            uint mask = UInt32.MaxValue >> shiftby;
            return new ColorSet(mask);
        }

        public static ColorSet CreateSingle(uint color)
        {
            return new ColorSet(ColorToMask(color));
        }

        public ColorSet Union(ColorSet other)
        {
            return new ColorSet(value | other.value);
        }

        public ColorSet Intersect(ColorSet other)
        {
            return new ColorSet(value & other.value);
        }

        public ColorSet RemoveColor(uint color)
        {
            uint mask = ColorToMask(color);
            return new ColorSet(value & (~mask));
        }

        public uint GetSingleColor()
        {
            if (!IsSingle())
                throw new InvalidOperationException("Cannot call 'GetSingleColor' on color set that has more than one color");

            uint val = value;
            uint color = 0;
            while (val > 1)
            {
                color++;
                val = val >> 1;
            }
            return color;
        }

        public ColorSet AddColor(uint color)
        {
            uint mask = ColorToMask(color);
            return new ColorSet(value | mask);
        }

        public static bool operator ==(ColorSet first, ColorSet second)
        {
            return first.value == second.value;
        }

        public static bool operator !=(ColorSet first, ColorSet second)
        {
            return first.value != second.value;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        private static uint ColorToMask(uint color)
        {
            return 1U << (int)color;
        }
    }
}

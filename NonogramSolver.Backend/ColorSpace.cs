using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NonogramSolver.Backend
{
    public class InvalidColorException : Exception
    {
        public InvalidColorException() { }
        public InvalidColorException(string message) : base(message) { }
        public InvalidColorException(string message, Exception inner) : base(message, inner) { }
    }

    internal class ColorSpace
    {
        public ColorSpace(uint maxColor)
        {
            if (maxColor == 0)
                throw new ArgumentOutOfRangeException("maxColor must not be 0");

            MaxColor = maxColor;
        }

        public uint MaxColor { get; private set; }

        public const uint Empty = 0;

        public void ValidateColor(uint color)
        {
            if (color >= MaxColor)
                throw new InvalidColorException($"Invalid color number {color}");
        }
    }
}

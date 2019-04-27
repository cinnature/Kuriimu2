﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanvas.Interface;
using System.Drawing;

namespace Kanvas.Swizzle
{
    /// <summary>
    /// This is a simple linear swizzle.
    /// </summary>
    /// <remarks>It is a default implementation of left-to-right and top-to-bottom.</remarks>
    public class LinearSwizzle : IImageSwizzle
    {
        public MasterSwizzle Swizzle { get; }

        /// <inheritdoc cref="IImageSwizzle.Width"/>
        public int Width { get; }

        /// <inheritdoc cref="IImageSwizzle.Height"/>
        public int Height { get; }

        /// <summary>
        /// Creates a new instance of <see cref="LinearSwizzle"/>.
        /// </summary>
        /// <param name="widthStride"></param>
        /// <param name="heightStride"></param>
        public LinearSwizzle(int widthStride, int heightStride)
        {
            Width = widthStride;
            Height = heightStride;

            var bitField = new List<(int, int)>();
            for (int i = 1; i < widthStride; i *= 2)
                bitField.Add((i, 0));
            for (int i = 1; i < heightStride; i *= 2)
                bitField.Add((0, i));
            Swizzle = new MasterSwizzle(widthStride, new Point(0, 0), bitField.ToArray());
        }

        /// <inheritdoc cref="IImageSwizzle.Get(Point)"/>
        public Point Get(Point point) => Swizzle.Get(point.Y * Width + point.X);
    }
}

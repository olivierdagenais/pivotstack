using SoftwareNinjas.Core;

namespace PivotStack
{
    public static class Size
    {
        public static Size<TUnit> Create<TUnit> (TUnit width, TUnit height)
        {
            return new Size<TUnit> (width, height);
        }
    }

    public struct Size<T>
    {
        public T Width { get; set; }
        public T Height { get; set; }

        public Size(T width, T height) : this ()
        {
            Width = width;
            Height = height;
        }

        public override string ToString ()
        {
            return "({0}, {1})".FormatInvariant (Width, Height);
        }
    }
}
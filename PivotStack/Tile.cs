using System.Drawing;
using SoftwareNinjas.Core;

namespace PivotStack
{
    public class Tile
    {
        internal const string TileNameTemplate = "{0}_{1}";
        internal const string TileZeroZero = "0_0";

        private readonly Rectangle _rectangle;
        private readonly int _row;
        private readonly int _column;

        public Tile(Size size, int row, int column)
        {
            _rectangle = CreateRectangle (size);
            _row = row;
            _column = column;
        }

        public Tile(int left, int top, int right, int bottom, int row, int column)
        {
            _rectangle = CreateRectangle (new Point (left, top), new Point (right, bottom));
            _row = row;
            _column = column;
        }

        public Rectangle Rectangle
        {
            get
            {
                return _rectangle;
            }
        }

        public string TileName
        {
            get
            {
                return ComputeTileName (_row, _column);
            }
        }

        public override bool Equals (object obj)
        {
            var that = obj as Tile;
            if (null == that)
            {
                return false;
            }
            return this._rectangle == that._rectangle
                   && this._row == that._row
                   && this._column == that._column;
        }

        public override int GetHashCode ()
        {
            return _rectangle.GetHashCode () ^ _row.GetHashCode () ^ _column.GetHashCode ();
        }

        public override string ToString ()
        {
            return "{0} @ {1}".FormatInvariant (_rectangle, TileName);
        }

        internal static string ComputeTileName(int row, int column)
        {
            if (0 == row && 0 == column)
            {
                return TileZeroZero;
            }
            return TileNameTemplate.FormatInvariant (column, row);
        }

        internal static Rectangle CreateRectangle(Size size)
        {
            return new Rectangle (0, 0, size.Width, size.Height);
        }

        internal static Rectangle CreateRectangle(Point point1, Point point2)
        {
            return new Rectangle (point1.X, point1.Y, point2.X - point1.X + 1, point2.Y - point1.Y + 1);
        }
    }
}

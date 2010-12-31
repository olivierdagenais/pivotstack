using System.Collections.Generic;

using SoftwareNinjas.Core;

namespace PivotStack
{
    public class ImageCollectionTile
    {
        private const string ToStringTemplate = "{0} with {1} tile{2}, morton {3}-{4}";

        private readonly int _row;
        private readonly int _column;
        private readonly int _startingMortonNumber;
        private readonly string _tileName;
        private readonly IList<int> _ids;
        private readonly int _hashCode;
        private readonly string _toString;

        public ImageCollectionTile (int row, int column, int startingMortonNumber, IEnumerable<int> ids)
        {
            _row = row;
            _column = column;
            _startingMortonNumber = startingMortonNumber;
            _tileName = DeepZoomImage.TileName (row, column);
            _ids = new List<int> (ids).AsReadOnly ();

            var hashCode = _row ^ _column ^ _startingMortonNumber;
            foreach (var id in ids)
            {
                hashCode ^= id.GetHashCode ();
            }
            _hashCode = hashCode;

            var count = _ids.Count;
            var countPlural = 1 == count ? "" : "s";
            var firstMorton = startingMortonNumber;
            var lastMorton = firstMorton + count - 1;
            _toString = ToStringTemplate.FormatInvariant (_tileName, count, countPlural, firstMorton, lastMorton);
        }

        public int Row
        {
            get
            {
                return _row;
            }
        }

        public int Column
        {
            get
            {
                return _column;
            }
        }

        public int StartingMortonNumber
        {
            get
            {
                return _startingMortonNumber;
            }
        }

        public string TileName
        {
            get
            {
                return _tileName;
            }
        }

        public IEnumerable<int> Ids
        {
            get
            {
                return _ids;
            }
        }

        public override string ToString ()
        {
            return _toString;
        }

        public override bool Equals (object obj)
        {
            var that = obj as ImageCollectionTile;
            if (null == that)
            {
                return false;
            }

            var areEqual = this._row == that._row
                         && this._column == that._column
                         && this._startingMortonNumber == that._startingMortonNumber
                         && this._ids.Count == that._ids.Count;
            for (var c = 0; areEqual && c < this._ids.Count; c++)
            {
                areEqual = this._ids[c] == that._ids[c];
            }
            return areEqual;
        }

        public override int GetHashCode ()
        {
            return _hashCode;
        }
    }
}

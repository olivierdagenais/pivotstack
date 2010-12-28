using System.Collections.Generic;

using SoftwareNinjas.Core;

namespace PivotStack
{
    public class ImageCollectionTile
    {
        private const string ToStringTemplate = "{0} with {1} tile{2}, morton {3}-{4}";

        private readonly int _row;
        private readonly int _column;
        private readonly string _tileName;
        private readonly IList<Pair<int, int>> _idsToMortonNumbers;
        private readonly int _hashCode;
        private readonly string _toString;

        public ImageCollectionTile (int row, int column, IEnumerable<Pair<int, int>> idsToMortonNumbers)
        {
            _row = row;
            _column = column;
            _tileName = DeepZoomImage.TileName (row, column);
            _idsToMortonNumbers = new List<Pair<int, int>> (idsToMortonNumbers).AsReadOnly ();

            var hashCode = _row ^ _column;
            foreach (var pair in idsToMortonNumbers)
            {
                hashCode ^= pair.GetHashCode ();
            }
            _hashCode = hashCode;

            var count = _idsToMortonNumbers.Count;
            var countPlural = 1 == count ? "" : "s";
            var firstMorton = _idsToMortonNumbers[0].Second;
            var lastMorton = _idsToMortonNumbers[_idsToMortonNumbers.Count - 1].Second;
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

        public string TileName
        {
            get
            {
                return _tileName;
            }
        }

        public IList<Pair<int, int>> IdsToMortonNumbers
        {
            get
            {
                return _idsToMortonNumbers;
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
                         && this._idsToMortonNumbers.Count == that._idsToMortonNumbers.Count;
            for (var c = 0; areEqual && c < this._idsToMortonNumbers.Count; c++)
            {
                areEqual = Equals (this._idsToMortonNumbers[c], that._idsToMortonNumbers[c]);
            }
            return areEqual;
        }

        public override int GetHashCode ()
        {
            return _hashCode;
        }
    }
}

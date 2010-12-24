using System.Collections.Generic;

using SoftwareNinjas.Core;

namespace PivotStack
{
    public class ImageCollectionTile
    {
        private readonly int _row;
        private readonly int _column;
        private readonly string _tileName;
        private readonly IList<Pair<int, int>> _idsToMortonNumbers;

        public ImageCollectionTile (int row, int column, IEnumerable<Pair<int, int>> idsToMortonNumbers)
        {
            _row = row;
            _column = column;
            _tileName = DeepZoomImage.TileName (row, column);
            _idsToMortonNumbers = new List<Pair<int, int>> (idsToMortonNumbers).AsReadOnly ();
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
    }
}

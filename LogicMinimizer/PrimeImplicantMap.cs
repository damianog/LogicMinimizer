using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicMinimizer
{
    public sealed class PrimeImplicantMap
    {
        private List<List<byte>> _map;
        private int _maxRows;
        private int _maxCols;

        public PrimeImplicantMap(int maxRows, int maxCols)
        {
            _map = new List<List<byte>>(maxRows);
            for (int r = 0; r < maxRows; r++)
            {
                var row = new List<byte>();
                _map.Add(row);
                for (int c = 0; c < maxCols; c++)
                {
                    row.Add(0);
                }
            }
            _maxRows = maxRows;
            _maxCols = maxCols;
        }

        public byte this[int row, int col]
        {
            get { return _map[row][col]; }
            set { _map[row][col] = value; }
        }

        public int MaxColumns
        {
            get { return _maxCols; }
        }

        public int MaxRows
        {
            get { return _maxRows; }
        }


        public IEnumerable<int> RowIndexes(int row)
        {
            return Enumerable.Range(0, _maxCols).Where(i => _map[row][i] > 0).Select(i => i).ToArray();
        }


        /// <summary>
        /// Remove rows with only 0 on cells
        /// </summary>
        /// <returns>A descending ordered list of removed indexes</returns>
        public List<int> RemoveEmptyRows()
        {
            var idxs = Enumerable.Range(0, _maxRows)
                .Select(i => new { Idx = i, Sum = _map[i].Sum(t => t) })
                .Where(t => t.Sum == 0)
                .Select(t => t.Idx)
                .OrderByDescending(t => t)
                .ToList();
            idxs.ForEach(idx =>
            {
                _map.RemoveAt(idx);
                _maxRows--;
            });
            return idxs;
        }

        /// <summary>
        /// Find an essential row
        /// </summary>
        /// <returns>Index of the essential row or -1 if none</returns>
        public int FindEssentialRow()
        {
            for (int c = 0; c < _maxCols; c++)
            {
                int sum = 0;
                int idx = -1;
                for (int r = 0; r < _maxRows; r++)
                {
                    var value = _map[r][c];
                    if (value > 0)
                    {
                        idx = r;
                        sum += _map[r][c];
                    }
                    if (sum > 1) break;
                }
                if (sum == 1) return idx;
            }
            return -1;
        }

        public void RemoveEssentialRow(int row)
        {
            var cols = Enumerable.Range(0, _maxCols)
                .Select(c => new { Col = c, Marked = _map[row][c] > 0 })
                .Where(t => t.Marked)
                .Select(t => t.Col)
                .OrderByDescending(t => t)
                .ToList();

            // Remove columns marked by essential row
            for (int r = 0; r < _maxRows; r++)
            {
                cols.ForEach(c => _map[r].RemoveAt(c));
            }
            _maxCols -= cols.Count();

            // Remove the essential row
            _map.RemoveAt(row);
            _maxRows--;
        }

        /// <summary>
        /// Remove the domaninated row by onother
        /// </summary>
        /// <returns>Index of removed row</returns>
        public int RemoveDomaninatedRow()
        {
            int dominatedRow = -1;
            for (int r0 = 0; (r0 < _maxRows - 1) && (dominatedRow < 0); r0++)
            {
                for (int r1 = r0 + 1; (r1 < _maxRows) && (dominatedRow < 0); r1++)
                {
                    bool dominant0 = true;
                    bool dominant1 = true;
                    for (int c = 0; c < _maxCols; c++)
                    {
                        dominant0 &= (_map[r0][c] >= _map[r1][c]);
                        dominant1 &= (_map[r0][c] <= _map[r1][c]);
                    }
                    if (dominant0) dominatedRow = r1;
                    if (dominant1) dominatedRow = r0;
                }
            }
            if (dominatedRow > -1)
            {
                _map.RemoveAt(dominatedRow);
                _maxRows--;
            }
            return dominatedRow;
        }

        public int RemoveDominantColumn()
        {
            int dominantColumn = -1;
            for (int c0 = 0; (c0 < _maxCols - 1) && (dominantColumn < 0); c0++)
            {
                for (int c1 = c0 + 1; (c1 < _maxCols) && (dominantColumn < 0); c1++)
                {
                    bool dominant0 = true;
                    bool dominant1 = true;
                    for (int r = 0; r < _maxRows; r++)
                    {
                        dominant0 &= (_map[r][c0] >= _map[r][c1]);
                        dominant1 &= (_map[r][c0] <= _map[r][c1]);
                    }
                    if (dominant0)
                        dominantColumn = c0;
                    else
                        if (dominant1) dominantColumn = c1;
                }
            }
            if (dominantColumn > -1)
            {
                for (int r = 0; r < _maxRows; r++)
                {
                    _map[r].RemoveAt(dominantColumn);
                }
                _maxCols--;
            }
            return dominantColumn;
        }

        public bool IsSolution(int[] indexes)
        {
            var isSolution = true;
            for (int c = 0; c < _maxCols; c++)
            {
                int sum = 0;
                for (int i = 0; i < indexes.Length; i++)
                {
                    sum += _map[indexes[i]][c];
                }
                isSolution &= (sum > 0);
            }
            return isSolution;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicMinimizer
{
    public sealed class Solver
    {

        private int _variables;
        private int _maxCount;
        private int[] _mTerms;
        private int[] _dcTerms;

        public Solver(int nVar, int[] mTerms, int[] dontCareTerms)
        {
            _variables = nVar;
            _maxCount = (int)Math.Pow(2, nVar);
            _mTerms = mTerms;
            _dcTerms = dontCareTerms;
        }

        public string Solve()
        {
            // Create the list of minterms
            var minterms = _mTerms.Select(t => new Minterm(t, t.ToBinaryString(_variables))).ToList();
            // Create the list of dont care terms
            var dcterms = _dcTerms.Select(t => new Minterm(t, t.ToBinaryString(_variables))).ToList();

            // Merge the two list for the first step of Quine-McCluskey method
            var terms = new List<Minterm>();
            terms.AddRange(minterms);
            terms.AddRange(dcterms);

            // Find prime implicants
            var primeImplicants = FindPrimeImplicants(terms).OrderBy(t => t.NTerm.Count()).ThenBy(t => t.NTerm[0]).ToList();

            // Optimize the prime 
            var optimized = Optimize(primeImplicants, _mTerms);

            var function = "";
            if (optimized.Count() == 0)
            {
                function = "0";
            }
            else if ((optimized.Count() == 1) && (optimized[0].NTerm.Count() == _maxCount))
            {
                function = "1";
            }
            else
            {
                function = string.Join("+", optimized.Select(t => t.LiteralFunction).ToArray());
            }
            return function;
        }

        private List<Minterm> Optimize(List<Minterm> primeImplicants, int[] mterms)
        {
            var list = new List<Minterm>();
            var maxRows = primeImplicants.Count();
            var maxCols = mterms.Length;

            var map = new PrimeImplicantMap(maxRows, maxCols);

            // Chart initialization
            for (int r = 0; r < maxRows; r++)
            {
                for (int c = 0; c < maxCols; c++)
                {
                    if (primeImplicants[r].NTerm.Contains(mterms[c]))
                    {
                        map[r, c] = 1;
                    }
                }
            }

            // Remove rows with only 0
            //map.RemoveEmptyRows().ForEach(t => primeImplicants.RemoveAt(t));

            while (primeImplicants.Count() > 0)
            {
                bool dirty;

                // Step 1 
                // Find essential row 
                dirty = false;
                var er = map.FindEssentialRow();
                if (er > -1)
                {
                    list.Add(primeImplicants[er]);
                    primeImplicants.RemoveAt(er);
                    map.RemoveEssentialRow(er);
                    dirty = true;
                }

                // Step 2
                // Remove dominated rows
                int dr = -1;
                do
                {
                    dr = map.RemoveDomaninatedRow();
                    if (dr > -1)
                    {
                        primeImplicants.RemoveAt(dr);
                        dirty = true;
                    }
                } while (dr > -1);

                // Step 3
                // Remove dominat row
                int dc = -1;
                do
                {
                    dc = map.RemoveDominantColumn();
                    if (dc > -1) dirty = true;
                } while (dc > -1);

                if (!dirty)
                {
                    if (primeImplicants.Count() > 1)
                    {
                        var branchBounds = BranchBounds(primeImplicants);
                        var subSolutions = new List<List<Minterm>>();
                        foreach (var bound in branchBounds)
                        {
                            var boundIndexes = bound.Select(t => primeImplicants.IndexOf(t)).ToArray();
                            if (map.IsSolution(boundIndexes))
                            {
                                subSolutions.Add(bound);
                            }
                        }
                        list.AddRange(subSolutions.OrderBy(t => t.Count()).First());
                    }
                    else
                    {
                        list.AddRange(primeImplicants);
                    }
                    primeImplicants.Clear();
                }
            }
            return list;
        }

        private List<Minterm> FindPrimeImplicants(IList<Minterm> minterms)
        {
            var list = new List<Minterm>();

            var groupedMinterms = minterms.GroupBy(t => t.Group).Select(g => new { Group = g.Key, MinTerms = g.Select(t => t) }).ToArray();
            for (int i = 0; i < groupedMinterms.Count() - 1; i++)
            {
                if (groupedMinterms[i + 1].Group - groupedMinterms[i].Group == 1)
                {
                    foreach (var a in groupedMinterms[i].MinTerms)
                    {
                        foreach (var b in groupedMinterms[i + 1].MinTerms)
                        {
                            var minterm = "";
                            int prime = 0;
                            for (int k = 0; k < a.STerm.Length; k++)
                            {
                                var bit = a.STerm[k];
                                if (bit.Equals(b.STerm[k]))
                                    minterm = minterm + bit;
                                else
                                {
                                    prime++;
                                    minterm = minterm + "-";
                                }
                            }
                            if (prime == 1)
                            {
                                a.Prime = b.Prime = false;
                                if (!list.Exists(t => t.STerm == minterm))
                                {
                                    var nt = new List<int>(a.NTerm);
                                    nt.AddRange(b.NTerm);
                                    list.Add(new Minterm(nt, minterm));
                                }
                            }
                        }
                    }
                }
            }

            var primeImplicats = new List<Minterm>();
            if (list.Count() > 0)
            {
                primeImplicats.AddRange(FindPrimeImplicants(list));
            }
            primeImplicats.AddRange(minterms.Where(t => t.Prime));
            return primeImplicats;
        }

        public IEnumerable<List<Minterm>> BranchBounds(List<Minterm> list)
        {
            var sub = new List<Minterm>();
            var bounds = new List<List<Minterm>>();
            Branch(list, sub, -1, bounds);
            return bounds.Where(t => t.Count() > 1 && t.Count() < list.Count());
        }

        private void Branch(List<Minterm> list, List<Minterm> sublist, int idx, List<List<Minterm>> bounds)
        {
            for (int i = idx + 1; i < list.Count(); i++)
            {
                sublist.Add(list[i]);
                Branch(list, sublist, i, bounds);
                sublist.Remove(list[i]);
            }
            bounds.Add(new List<Minterm>(sublist));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicMinimizer
{
    public sealed class Minterm
    {
        private bool _prime;
        private int _group;
        private List<int> _nterms;
        private string _sterm;

        public bool Prime
        {
            get { return _prime; }
            set { _prime = value; }
        }

        public int Group
        {
            get { return _group; }
        }

        public Minterm(int nterm, string sterm)
        {
            _prime = true;
            _group = sterm.Count(t => t == '1');
            _nterms = new List<int>();
            _nterms.Add(nterm);
            _sterm = sterm;
        }

        public Minterm(IEnumerable<int> nterms, string sterm)
        {
            _prime = true;
            _group = sterm.Count(t => t == '1');
            _nterms = new List<int>(nterms);
            _sterm = sterm;
        }

        public IList<int> NTerm
        {
            get { return _nterms; }
        }

        public string STerm
        {
            get { return _sterm; }
            private set { _sterm = value; }
        }

        public string LiteralFunction
        {
            get { return _sterm.ToLiteralFunction(); }
        }

        public override string ToString()
        {
            return string.Format("m({0}) : {1} {2}", String.Join(",", _nterms.OrderBy(t => t)), _sterm, Prime ? "" : "x");
        }
    }
}

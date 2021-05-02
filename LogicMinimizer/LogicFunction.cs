using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogicMinimizer
{
    public class LogicFunction
    {
        public static readonly char[] LiteralVars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K' };
        public static readonly int MaxVariables = LiteralVars.Count();

        private int _variables;
        private int _maxValue;
        private IEnumerable<LogicFunction> _functions;
        private string _function;

        public LogicFunction(string function, int variables)
        {
            _variables = variables;
            _maxValue = (int)Math.Pow(2, variables);

            var funcs = function.Split('+');
            _function = function;

            if (funcs.Count() > 1)
                _functions = funcs.Select(f => new LogicFunction(f, variables));
        }

        public int MaxValue
        {
            get { return _maxValue; }
        }

        public bool Test(int value)
        {
            var bits = value.ToBinaryString(_variables).Select(t => t == '1').ToArray();
            return Test(bits);
        }

        private bool Test(bool[] bits)
        {
            if (_functions != null) return _functions.Select(f => f.Test(bits)).Any(t => t);

            var regex = new Regex(@"(?<bit>!?[A-Z])");
            var matches = regex.Matches(_function);

            var result = true;
            for (int i = 0; i < matches.Count; i++)
            {
                result &= matches[i].Value.StartsWith("!") ? !bits[(matches[i].Value[1] - 'A')] : bits[(matches[i].Value[0] - 'A')];
            }

            return result;
        }

        public static IEnumerable<bool> Execute(string function, int variables)
        {
            var fun = new LogicFunction(function, variables);
            var values = new List<bool>(fun.MaxValue);
            return Enumerable.Range(0, fun.MaxValue).Select(i => fun.Test(i));
        }
    }
}

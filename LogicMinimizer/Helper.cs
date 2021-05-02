using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicMinimizer
{
    public static class Helper
    {        
        public static string ToBinaryString(this int number, int width)
        {
            return Convert.ToString(number, 2).PadLeft(width, '0');
        }

        public static string ToLiteralFunction(this string minterm)
        {
            var f = new StringBuilder();
            for (int i = 0; i < minterm.Length; i++)
            {
                f.Append(((minterm[i] == '0') ? "!" + LogicFunction.LiteralVars[i] : (minterm[i] == '1') ? LogicFunction.LiteralVars[i].ToString() : ""));
            }
            return f.ToString();
        }

        public static int NumberOfSetBits(this int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPALCommander
{
    class IntComparer : IComparer<String>
    {
        public int Compare(string x, string y)
        {
            Int32 number1;
            Int32 number2;

            x = x.Replace(" ", "");
            y = y.Replace(" ", "");

            if (Int32.TryParse(x, out number1) && int.TryParse(y, out number2))
            {
                if (number1 >number2) return 1;
                if (number1 < number2) return -1;
                if (number1 == number2) return 0;
            }

            return string.Compare(x, y, true);
        }
    }
}

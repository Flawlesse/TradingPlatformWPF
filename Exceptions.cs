using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingPlatform
{
    class NoResultSelectedException : Exception
    {
        public NoResultSelectedException(string msg)
        : base(msg)
        {

        }
    }
}

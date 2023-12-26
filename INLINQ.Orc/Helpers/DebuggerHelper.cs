using INLINQ.Orc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INLINQ.Orc.Helpers
{
    public static class DebuggerHelper
    {
        public static string GetTimeString()
        {
            return DateTime.Now.ToString("T");
        }
    }
}

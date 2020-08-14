using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devhus.DownloadService.Wpf.Logic
{
    class Utilities
    {
        internal static string SecondsToString(long seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);

            if (t.Days > 0)
                return t.Days + " Days";

            if (t.Hours > 0)
                return t.Hours + " Hours";

            if (t.Minutes > 0)
                return t.Minutes + " Minutes";

            return t.Seconds + " Seconds";
        }
    }
}

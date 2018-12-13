using System;
using System.Collections.Generic;
using System.Linq;

namespace HueSyncClone.Core.Drawing
{
    public struct CieLabSpace : ISpace<CieLabColor>
    {
        public double GetDistance(CieLabColor x, CieLabColor y) 
            => Math.Sqrt(Math.Pow(x.L - y.L, 2) + Math.Pow(x.A - y.A, 2) + Math.Pow(x.B - y.B, 2));

        public CieLabColor GetCentroid(IReadOnlyCollection<CieLabColor> xs) 
            => new CieLabColor(xs.Average(x => x.L), xs.Average(x => x.A), xs.Average(x => x.B));
    }
}
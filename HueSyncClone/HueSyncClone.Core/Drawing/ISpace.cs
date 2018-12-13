using System.Collections.Generic;

namespace HueSyncClone.Core.Drawing
{
    public interface ISpace<T>
    {
        double GetDistance(T x, T y);
        T GetCentroid(IReadOnlyCollection<T> xs);
    }
}
using System.Collections.Generic;

namespace HueSyncClone.Drawing
{
    public interface ISpace<T>
    {
        double GetDistance(T x, T y);
        T GetCentroid(IReadOnlyCollection<T> xs);
    }
}
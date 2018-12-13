using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HueSyncClone.Drawing
{
    public class CieLabColorTest
    {
        [Theory]
        [InlineData(new[] { 0.9505, 1, 1.089 }, new[] { 100, 0.0053, -0.0104})]
        [InlineData(new[] { 0.3118, 0.2227, 0.0336 }, new[] { 54.3123, 41.7683, 58.4960})]
        [InlineData(new[] { 0.3107, 0.5565, 0.2443 }, new[] { 79.4142, -66.8355, 42.977})]
        [InlineData(new[] { 0.0, 0.0, 0.0 }, new[] { 0.0, 0.0, 0.0 })]
        public void XyzToLab(double[] xyz, double[] lab)
        {
            Assert.Equal(
                new CieLabColor(lab[0], lab[1], lab[2]),
                CieLabColor.FromXyz(new XyzColor(xyz[0], xyz[1], xyz[2]))
            );
        }

        [Theory]
        [InlineData(new[] { 0.9505, 1, 1.089 }, new[] { 100, 0.0053, -0.0104 })]
        [InlineData(new[] { 0.3118, 0.2227, 0.0336 }, new[] { 54.3123, 41.7683, 58.4960 })]
        [InlineData(new[] { 0.3107, 0.5565, 0.2443 }, new[] { 79.4142, -66.8355, 42.977 })]
        [InlineData(new[] { 0.0, 0.0, 0.0 }, new[] { 0.0, 0.0, 0.0 })]
        public void LabToXyz(double[] xyz, double[] lab)
        {
            Assert.Equal(
                new XyzColor(xyz[0], xyz[1], xyz[2]),
                new CieLabColor(lab[0], lab[1], lab[2]).ToXyzColor()
            );
        }
    }
}

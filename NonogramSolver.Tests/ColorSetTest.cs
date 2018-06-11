using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonogramSolver.Backend;

namespace NonogramSolver.Tests
{
    [TestClass]
    class ColorSetTest
    {
        [TestMethod]
        public void Basic()
        {
            ColorSet colorSet = new ColorSet();
            Assert.True(colorSet.IsEmpty);
            Assert.True(!colorSet.HasColor(0));
        }

        [TestMethod]
        public void HasColor()
        {
            ColorSet colorSet = new ColorSet(3);
            Assert.True(colorSet.HasColor(0));
            Assert.True(colorSet.HasColor(1));
            Assert.True(!colorSet.HasColor(2));
            Assert.True(!colorSet.HasColor(3));
        }

        [TestMethod]
        public void HasColor2()
        {
            ColorSet colorSet = new ColorSet(5);
            Assert.True(colorSet.HasColor(0));
            Assert.True(!colorSet.HasColor(1));
            Assert.True(colorSet.HasColor(2));
            Assert.True(!colorSet.HasColor(3));
        }

        [TestMethod]
        public void AddColor()
        {
            ColorSet colorSet = new ColorSet(6);
            Assert.True(!colorSet.HasColor(0));

            ColorSet newColorSet = colorSet.AddColor(0);
            Assert.True(newColorSet.HasColor(0));
            Assert.True(!colorSet.HasColor(0));
        }

        [TestMethod]
        public void AddColor2()
        {
            ColorSet colorSet = new ColorSet(4);
            ColorSet cs2 = colorSet.AddColor(0);
            ColorSet cs3 = cs2.AddColor(3);

            ColorSet final = new ColorSet(1 + 4 + 8);
            Assert.True(colorSet != final);
            Assert.True(cs2 != final);
            Assert.True(cs3 == final);
        }

        [TestMethod]
        public void CreateFull()
        {
            ColorSet full = ColorSet.CreateFullColorSet(2);
            ColorSet correct = new ColorSet(7);
            Assert.True(full == correct);
        }

        [TestMethod]
        public void CreateFull2()
        {
            ColorSet full = ColorSet.CreateFullColorSet(4);
            ColorSet correct = new ColorSet(31);
            Assert.True(full == correct);
        }

        [TestMethod]
        public void IsSingle()
        {
            ColorSet colorSet = new ColorSet(2);
            Assert.True(colorSet.IsSingle());
            Assert.True(colorSet.GetSingleColor() == 1);
        }

        [TestMethod]
        public void IsSingle2()
        {
            ColorSet colorSet = new ColorSet(5);
            Assert.True(!colorSet.IsSingle());

            ColorSet cs2 = colorSet.RemoveColor(2);
            Assert.True(cs2.IsSingle());
            Assert.True(cs2.GetSingleColor() == 0);
        }

        [TestMethod]
        public void IsSingle3()
        {
            ColorSet colorSet = new ColorSet();
            Assert.True(!colorSet.IsSingle());
        }

        [TestMethod]
        public void Union()
        {
            ColorSet cs1 = new ColorSet(5);
            ColorSet cs2 = new ColorSet(10);
            ColorSet final = ColorSet.CreateFullColorSet(3);

            ColorSet union = cs1.Union(cs2);
            Assert.True(union == final);
        }

        [TestMethod]
        public void Intersect()
        {
            ColorSet cs1 = new ColorSet(5);
            ColorSet cs2 = new ColorSet(10);
            ColorSet final = new ColorSet();

            ColorSet intersect = cs1.Intersect(cs2);
            Assert.True(intersect.IsEmpty);
            Assert.True(intersect == final);
        }

        [TestMethod]
        public void Intersect2()
        {
            ColorSet cs1 = new ColorSet(3);
            ColorSet cs2 = new ColorSet(6);
            ColorSet final = new ColorSet(2);

            ColorSet intersect = cs1.Intersect(cs2);
            Assert.True(intersect == final);
        }

    }
}

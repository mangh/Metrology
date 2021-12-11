/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/unitsofmeasurement


********************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Demo.UnitsOfMeasurement
{
    [TestClass]
    public class CatalogTest
    {
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void DuplicateProxyThrowsException()
        {
            // Catalog is (statically) populated with all compile-time units and scales at program startup.
            // Attempt to catalog a unit or a scale proxy that is already cataloged is redundant:
            Catalog.Add(Meter.Proxy);
            // The above would be fine after calling Catalog.Clear() method before.
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void DuplicateProxyFromAssemblyThrowsException()
        {
            // Catalog is (statically) populated with all compile-time units and scales at program startup.
            // Reloading from assembly is reduntant:
            Catalog.AppendFromAssembly(typeof(Meter).Assembly);
            // The above would be fine after calling Catalog.Clear() method before.
        }

        [TestMethod]
        public void Reload()
        {
            int uct = Catalog.AllUnits.Count();
            int sct = Catalog.AllScales.Count();

            Catalog.Reset();

            Assert.AreEqual(uct, Catalog.AllUnits.Count());
            Assert.AreEqual(sct, Catalog.AllScales.Count());
        }

        [TestMethod]
        public void Select()
        {
            foreach (Unit<double> u in Catalog.Units<double>())
            {
                Assert.AreEqual(u, Catalog.Unit<double>(u.Symbol.Default));
                Assert.IsTrue(Catalog.Units<double>(u.Family).Contains(u));
                Assert.IsTrue(Catalog.Units<double>(u.Sense).Contains(u));
            }
            foreach (Unit<decimal> u in Catalog.Units<decimal>())
            {
                Assert.AreEqual(u, Catalog.Unit<decimal>(u.Symbol.Default));
                Assert.IsTrue(Catalog.Units<decimal>(u.Family).Contains(u));
                Assert.IsTrue(Catalog.Units<decimal>(u.Sense).Contains(u));
            }
            foreach(Scale<double> s in Catalog.Scales<double>())
            {
                Assert.AreEqual(s, Catalog.Scale<double>(s.Family, s.Unit.Symbol.Default));
                Assert.IsTrue(Catalog.Scales<double>(s.Family).Contains(s));
                Assert.IsTrue(Catalog.Scales<double>(s.Unit.Sense).Contains(s));
                Assert.IsTrue(Catalog.Units(Catalog.Scales<double>(s.Family)).Contains(s.Unit));
            }
        }
    }
}

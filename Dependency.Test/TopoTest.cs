using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Dependency.Test
{
    class TestSortItem : IGraphItem
    {
        public TestSortItem(string name, List<string> dependencies)
        {
            Name = name;
            Dependencies = dependencies;
        }
        public string Name { get; }
        public List<string> Dependencies { get; }
    }

    [TestFixture]
    public class TopoTest
    {
        [Test]
        public void TopoSortTest1()
        {
            var data = new List<IGraphItem>
            {
                new TestSortItem("A", new List<string>{ "E" }),
                new TestSortItem("B", new List<string>{ "F" }),
                new TestSortItem("C", new List<string>{ "G", "A", "B" }),
                new TestSortItem("D", new List<string>()),
                new TestSortItem("E", new List<string>()),
                new TestSortItem("F", new List<string>{ "E" }),
                new TestSortItem("G", new List<string>())
            };

            var sorter = new TopologicalSorter();
            var sortedData = sorter.Do(data);

            var indexA = sortedData.TakeWhile(i => i.Name != "A").Count();
            var indexB = sortedData.TakeWhile(i => i.Name != "B").Count();
            var indexC = sortedData.TakeWhile(i => i.Name != "C").Count();
            var indexD = sortedData.TakeWhile(i => i.Name != "D").Count();
            var indexE = sortedData.TakeWhile(i => i.Name != "E").Count();
            var indexF = sortedData.TakeWhile(i => i.Name != "F").Count();
            var indexG = sortedData.TakeWhile(i => i.Name != "G").Count();

            Assert.Greater(indexA,indexE);
            Assert.Greater(indexB,indexF);
            Assert.Greater(indexC,indexG);
            Assert.Greater(indexC,indexA);
            Assert.Greater(indexC,indexB);
            Assert.Greater(indexF,indexE);

        }

        [Test]
        public void CyclicTopoSortTest1()
        {
            var data = new List<IGraphItem>
            {
                new TestSortItem("A", new List<string>{ "A" }),
            };

            var sorter = new TopologicalSorter();
            Assert.Throws<Exception>(() => sorter.Do(data));
        }

        [Test]
        public void CyclicTopoSortTest2()
        {
            var data = new List<IGraphItem>
            {
                new TestSortItem("A", new List<string>{ "B" }),
                new TestSortItem("B", new List<string>{ "C" }),
                new TestSortItem("C", new List<string>{ "D" }),
                new TestSortItem("D", new List<string>{ "E" }),
                new TestSortItem("E", new List<string>{ "A" }),
            };

            var sorter = new TopologicalSorter();
            Assert.Throws<Exception>(() => sorter.Do(data));
        }

        [Test]
        public void BadDependencyTopoSortTest()
        {
            var data = new List<IGraphItem>
            {
                new TestSortItem("A", new List<string>{ "E" }),
            };

            var sorter = new TopologicalSorter();
            Assert.Throws<Exception>(() => sorter.Do(data));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dependency
{
    public interface IGraphItem
    {
        string Name { get; }
        List<string> Dependencies { get; }
    }

    public class TopologicalSorter
    {
        private enum SortState
        {
            Unknown,
            Active,
            Finished
        }

        private sealed class ItemTag<T>
        {
            public SortState State { get; set; }
            public T Item { get; private set; }

            public ItemTag(T item)
            {
                Item = item;
                State = SortState.Unknown;
            }
        }

        public List<T> Do<T>(List<T> items) where T : IGraphItem
        {
            var allNodes = items.ToDictionary(item => item.Name, item => new ItemTag<T>(item));

            CheckForMissingDependencies(allNodes);

            var lastCyclicOrder = new Stack<string>();
            var sortedNames = new HashSet<string>();
            foreach (var tag in allNodes)
            {
                Visit(tag.Value, allNodes, lastCyclicOrder, sortedNames);
            }

            return sortedNames.Select(name => allNodes[name].Item).ToList();
        }

        private void CheckForMissingDependencies<T>(Dictionary<string, ItemTag<T>> allNodes) where T : IGraphItem
        {
            foreach (var node in allNodes)
            {
                foreach (var dep in node.Value.Item.Dependencies)
                {
                    if (!allNodes.ContainsKey(dep))
                    {
                        throw new Exception($"Missing Dependency: [{node.Value.Item.Name}] <- [{dep}].");
                    }
                }
            }
        }

        private void Visit<T>(ItemTag<T> tag, Dictionary<string, ItemTag<T>> allNodes, Stack<string> lastCyclicOrder, HashSet<string> sortedNames) where T : IGraphItem
        {
            if (tag.State == SortState.Active)
            {
                var cycle = "";
                cycle = lastCyclicOrder.Reverse().Aggregate(cycle, (current, item) => current + ("[" + item + "] <- "));
                throw new Exception("Cyclic Dependency: " + cycle + " [" + tag.Item.Name + "].");
            }
            else if (tag.State == SortState.Unknown)
            {
                tag.State = SortState.Active;
                lastCyclicOrder.Push(tag.Item.Name);

                foreach (var dep in tag.Item.Dependencies.Select(s => allNodes.First( t => s == t.Key )))
                {
                    Visit(dep.Value, allNodes, lastCyclicOrder, sortedNames);
                }

                lastCyclicOrder.Pop();
                tag.State = SortState.Finished;
                sortedNames.Add(tag.Item.Name);
            }
        }

    }
}
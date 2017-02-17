using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<string> LastCyclicOrder = new List<string>(); //used to see what caused the cycle

        sealed class ItemTag2
        {
            public enum SortTag
            {
                NotMarked,
                TempMarked,
                Marked
            }

            public string Name => Item.Name;
            public SortTag Tag { get; set; }
            public IGraphItem Item { get; private set; }

            public ItemTag2(IGraphItem item)
            {
                Item = item;
                Tag = SortTag.NotMarked;
            }
        }

        public
//            List<IGraphItem>
            List<string>
            Do( List<IGraphItem> items)
        {
            LastCyclicOrder.Clear();

            List<ItemTag2> allNodes = new List<ItemTag2>();
            HashSet<string> sorted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                if (!allNodes.Any(n => string.Equals(n.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    allNodes.Add(new ItemTag2(item)); //don't insert duplicates
                }
//                foreach (string dep in dependencies(item))
//                {
//                    if (allNodes.Any(n => string.Equals(n.Name, dep, StringComparison.OrdinalIgnoreCase))) continue; //throw new Exception("Dublicate");
//                    allNodes.Add(new ItemTag(dep));
//                }
            }

            foreach (ItemTag2 tag in allNodes)
            {
                Visit(tag, allNodes, sorted);
            }

            return sorted.ToList();
        }

        void Visit(ItemTag2 tag, List<ItemTag2> allNodes, HashSet<string> sorted)
        {
            if (tag.Tag == ItemTag2.SortTag.TempMarked)
            {
                throw new Exception("GraphIsCyclic");
            }
            else if (tag.Tag == ItemTag2.SortTag.NotMarked)
            {
                tag.Tag = ItemTag2.SortTag.TempMarked;
                LastCyclicOrder.Add(tag.Name);

                foreach (ItemTag2 dep in tag.Item.Dependencies.Select(s => allNodes.First( t => string.Equals(s, t.Name, StringComparison.OrdinalIgnoreCase)))) //get name tag which falls with used string
                {
                    Visit(dep, allNodes, sorted);
                }

                LastCyclicOrder.Remove(tag.Name);
                tag.Tag = ItemTag2.SortTag.Marked;
                sorted.Add(tag.Name);
            }
        }

    }
}
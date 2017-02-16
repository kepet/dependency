using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dependency
{
    /// <summary>
    /// Provides fast-algorithm and low-memory usage to sort objects based on their dependencies.
    /// </summary>
    /// <remarks>
    /// Definition: http://en.wikipedia.org/wiki/Topological_sorting
    /// Source code credited to: http://tawani.blogspot.com/2009/02/topological-sorting-and-cyclic.html
    /// Original Java source code: http://www.java2s.com/Code/Java/Collections-Data-Structure/Topologicalsorting.htm
    /// </remarks>
    /// <author>ThangTran</author>
    /// <history>
    /// 2012.03.21 - ThangTran: rewritten based on <see cref="TopologicalSorter"/>.
    /// </history>
    public class DependencySorter<T>
    {
        //**************************************************
        //
        // Private members
        //
        //**************************************************

        #region Private members

        /// <summary>
        /// Gets the dependency matrix used by this instance.
        /// </summary>
        private readonly Dictionary<T, Dictionary<T, object>> _matrix = new Dictionary<T, Dictionary<T, object>>();

        #endregion


        //**************************************************
        //
        // Public methods
        //
        //**************************************************

        #region Public methods

        /// <summary>
        /// Adds a list of objects that will be sorted.
        /// </summary>
        public void AddObjects(params T[] objects)
        {
            // --- Begin parameters checking code -----------------------------
            Debug.Assert(objects != null);
            Debug.Assert(objects.Length > 0);
            // --- End parameters checking code -------------------------------

            // add to matrix
            foreach (T obj in objects)
            {
                // add to dictionary
                _matrix.Add(obj, new Dictionary<T, object>());
            }
        }

        /// <summary>
        /// Sets dependencies of given object.
        /// This means <paramref name="obj"/> depends on these <paramref name="dependsOnObjects"/> to run.
        /// Please make sure objects given in the <paramref name="obj"/> and <paramref name="dependsOnObjects"/> are added first.
        /// </summary>
        public void SetDependencies(T obj, params T[] dependsOnObjects)
        {
            // --- Begin parameters checking code -----------------------------
            Debug.Assert(dependsOnObjects != null);
            // --- End parameters checking code -------------------------------

            // set dependencies
            Dictionary<T, object> dependencies = _matrix[obj];
            dependencies.Clear();

            // for each depended objects, add to dependencies
            foreach (T dependsOnObject in dependsOnObjects)
            {
                dependencies.Add(dependsOnObject, null);
            }
        }

        /// <summary>
        /// Sorts objects based on this dependencies.
        /// Note: because of the nature of algorithm and memory usage efficiency, this method can be used only one time.
        /// </summary>
        public T[] Sort()
        {
            // prepare result
            List<T> result = new List<T>(_matrix.Count);

            // while there are still object to get
            while (_matrix.Count > 0)
            {
                // get an independent object
                T independentObject;
                if (!this.GetIndependentObject(out independentObject))
                {
                    // circular dependency found
                    throw new CircularReferenceException();
                }

                // add to result
                result.Add(independentObject);

                // delete processed object
                this.DeleteObject(independentObject);
            }

            // return result
            return result.ToArray();
        }

        #endregion


        //**************************************************
        //
        // Private methods
        //
        //**************************************************

        #region Private methods

        /// <summary>
        /// Returns independent object or returns NULL if no independent object is found.
        /// </summary>
        private bool GetIndependentObject(out T result)
        {
            // for each object
            foreach (KeyValuePair<T, Dictionary<T, object>> pair in _matrix)
            {
                // if the object contains any dependency
                if (pair.Value.Count > 0)
                {
                    // has dependency, skip it
                    continue;
                }

                // found
                result = pair.Key;
                return true;
            }

            // not found
            result = default(T);
            return false;
        }

        /// <summary>
        /// Deletes given object from the matrix.
        /// </summary>
        private void DeleteObject(T obj)
        {
            // delete object from matrix
            _matrix.Remove(obj);

            // for each object, remove the dependency reference
            foreach (KeyValuePair<T, Dictionary<T, object>> pair in _matrix)
            {
                // if current object depends on deleting object
                pair.Value.Remove(obj);
            }
        }


        #endregion
    }

    /// <summary>
    /// Represents a circular reference exception when sorting dependency objects.
    /// </summary>
    public class CircularReferenceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircularReferenceException"/> class.
        /// </summary>
        public CircularReferenceException() : base("Circular reference found.")
        {
        }
    }







    public class TopologicalSorter
    {
        public List<string> LastCyclicOrder = new List<string>(); //used to see what caused the cycle

        sealed class ItemTag
        {
            public enum SortTag
            {
                NotMarked,
                TempMarked,
                Marked
            }

            public string Item { get; set; }
            public SortTag Tag { get; set; }

            public ItemTag(string item)
            {
                Item = item;
                Tag = SortTag.NotMarked;
            }
        }

        public IEnumerable<string> Do( IEnumerable<string> source, Func<string, IEnumerable<string>> dependencies)
        {
            LastCyclicOrder.Clear();

            List<ItemTag> allNodes = new List<ItemTag>();
            HashSet<string> sorted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string item in source)
            {
                if (!allNodes.Any(n => string.Equals(n.Item, item, StringComparison.OrdinalIgnoreCase)))
                {
                    allNodes.Add(new ItemTag(item)); //don't insert duplicates
                }
                foreach (string dep in dependencies(item))
                {
                    if (allNodes.Any(n => string.Equals(n.Item, dep, StringComparison.OrdinalIgnoreCase))) continue; //throw new Exception("Dublicate");
                    allNodes.Add(new ItemTag(dep));
                }
            }

            foreach (ItemTag tag in allNodes)
            {
                Visit(tag, allNodes, dependencies, sorted);
            }

            return sorted;
        }

        void Visit(ItemTag tag, List<ItemTag> allNodes, Func<string, IEnumerable<string>> dependencies, HashSet<string> sorted)
        {
            if (tag.Tag == ItemTag.SortTag.TempMarked)
            {
                throw new Exception("GraphIsCyclic");
            }
            else if (tag.Tag == ItemTag.SortTag.NotMarked)
            {
                tag.Tag = ItemTag.SortTag.TempMarked;
                LastCyclicOrder.Add(tag.Item);

                foreach (ItemTag dep in dependencies(tag.Item) .Select(s => allNodes.First( t => string.Equals(s, t.Item, StringComparison.OrdinalIgnoreCase)))) //get item tag which falls with used string
                {
                    Visit(dep, allNodes, dependencies, sorted);
                }

                LastCyclicOrder.Remove(tag.Item);
                tag.Tag = ItemTag.SortTag.Marked;
                sorted.Add(tag.Item);
            }
        }


    }
}
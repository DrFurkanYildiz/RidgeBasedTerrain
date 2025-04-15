using System;
using System.Collections.Generic;

/// <summary>
/// A disjoint-set (union-find) data structure implementation for generic types
/// </summary>
public class DisjointSet<T> where T : class
{
    private Dictionary<T, T> parent = new Dictionary<T, T>();
    private Dictionary<T, int> rank = new Dictionary<T, int>();
    
    /// <summary>
    /// Creates a new set containing the given object
    /// </summary>
    public void MakeSet(T item)
    {
        if (!parent.ContainsKey(item))
        {
            parent[item] = item;
            rank[item] = 0;
        }
    }
    
    /// <summary>
    /// Finds the representative of the set containing the given object
    /// </summary>
    public T Find(T item)
    {
        if (!parent.ContainsKey(item))
        {
            return null;
        }
        
        if (!parent[item].Equals(item))
        {
            parent[item] = Find(parent[item]); // Path compression
        }
        
        return parent[item];
    }
    
    /// <summary>
    /// Merges the sets containing the two objects
    /// </summary>
    public void Union(T item1, T item2)
    {
        T root1 = Find(item1);
        T root2 = Find(item2);
        
        if (root1 == null || root2 == null || root1.Equals(root2))
        {
            return;
        }
        
        // Union by rank
        if (rank[root1] < rank[root2])
        {
            parent[root1] = root2;
        }
        else if (rank[root1] > rank[root2])
        {
            parent[root2] = root1;
        }
        else
        {
            parent[root2] = root1;
            rank[root1]++;
        }
    }
    
    /// <summary>
    /// Gets a list of all groups in the disjoint set
    /// </summary>
    public List<List<T>> GetGroups()
    {
        Dictionary<T, List<T>> groups = new Dictionary<T, List<T>>();
        
        // Group items by their root
        foreach (T item in parent.Keys)
        {
            T root = Find(item);
            
            if (!groups.ContainsKey(root))
            {
                groups[root] = new List<T>();
            }
            
            groups[root].Add(item);
        }
        
        // Convert dictionary to list of lists
        List<List<T>> result = new List<List<T>>();
        foreach (var group in groups.Values)
        {
            result.Add(group);
        }
        
        return result;
    }
}
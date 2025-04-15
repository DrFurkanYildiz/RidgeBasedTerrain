using System.Collections.Generic;

public class DisjointSet<T> where T : class
{
    private Dictionary<T, T> parent = new Dictionary<T, T>();
    private Dictionary<T, int> rank = new Dictionary<T, int>();
    
    public void MakeSet(T item)
    {
        if (!parent.ContainsKey(item))
        {
            parent[item] = item;
            rank[item] = 0;
        }
    }
    
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
    
    public List<List<T>> GetGroups()
    {
        // Use a separate dictionary to track which root each item belongs to
        Dictionary<T, T> compressedRoots = new Dictionary<T, T>();
    
        // First, get all roots without modifying the parent dictionary during enumeration
        foreach (T item in new List<T>(parent.Keys))
        {
            // Find the root but store it separately to avoid modification during iteration
            T root = item;
            while (!parent[root].Equals(root))
            {
                root = parent[root];
            }
            compressedRoots[item] = root;
        }
    
        // Now group items by their root
        Dictionary<T, List<T>> groups = new Dictionary<T, List<T>>();
        foreach (var pair in compressedRoots)
        {
            T item = pair.Key;
            T root = pair.Value;
        
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
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents cube coordinates for hexagonal grid
/// </summary>
[System.Serializable]
public struct HexCoordinates : IEquatable<HexCoordinates>
{
    public int Q { get; private set; } // x axis
    public int R { get; private set; } // y axis
    public int S { get; private set; } // z axis (derived from Q+R+S=0)
    
    /// <summary>
    /// Creates a new hex coordinate from Q, R, S values
    /// </summary>
    public HexCoordinates(int q, int r, int s)
    {
        // Validate that Q + R + S = 0
        if (q + r + s != 0)
        {
            throw new ArgumentException("Hex coordinates must satisfy Q + R + S = 0");
        }
        
        Q = q;
        R = r;
        S = s;
    }
    
    /// <summary>
    /// Creates a new hex coordinate from Q and R values (S derived)
    /// </summary>
    public static HexCoordinates FromQR(int q, int r)
    {
        return new HexCoordinates(q, r, -q - r);
    }
    
    /// <summary>
    /// Pointy‑topped hex’leri dünya‑koordinata çevirir.
    /// </summary>
    public static Vector3 HexToWorld(HexCoordinates hex, float diameter)
    {
        float radius = diameter * 0.5f;
        // Amit Patel’in formülleri:
        float x = radius * (Mathf.Sqrt(3f) * hex.Q + Mathf.Sqrt(3f) / 2f * hex.R);
        float z = radius * (3f / 2f * hex.R);
        return new Vector3(x, 0f, z);
    }

    /// <summary>
    /// Creates cube coordinates from world position
    /// </summary>
    public static HexCoordinates FromPosition(Vector3 position, float diameter)
    {
        // radius = hex’in köşesine kadar uzaklığı
        float radius = diameter / 2f;

        // 1) Axial q ve r’yi gerçek konumdan hesapla
        //    q = (√3/3 * x  -  1/3 * z) / radius
        //    r = (2/3   * z)  / radius
        float q = ((Mathf.Sqrt(3f) / 3f) * position.x  -  (1f / 3f) * position.z) / radius;
        float r = (2f / 3f * position.z) / radius;

        // 2) s = -q - r
        // 3) En yakın tam küp koordinatına yuvarla
        return CubeRound(q, r, -q - r);
    }
    /*
    public static HexCoordinates FromPosition(Vector3 position, float diameter)
    {
        float radius = diameter / 2f;
        float x = position.x / (radius * 1.5f);
        float z = position.z / (radius * Mathf.Sqrt(3));
        
        // Convert to cube coordinates using axial conversion
        float q = x;
        float r = (-x / 2f) + ((Mathf.Sqrt(3)/2) * z);
        
        // Round to nearest hex
        return CubeRound(q, r, -q - r);
    }
    */
    
    /// <summary>
    /// Rounds floating point cube coordinates to the nearest hex
    /// </summary>
    private static HexCoordinates CubeRound(float q, float r, float s)
    {
        int roundQ = Mathf.RoundToInt(q);
        int roundR = Mathf.RoundToInt(r);
        int roundS = Mathf.RoundToInt(s);
        
        float qDiff = Mathf.Abs(roundQ - q);
        float rDiff = Mathf.Abs(roundR - r);
        float sDiff = Mathf.Abs(roundS - s);
        
        // If Q was rounded furthest from original, adjust it
        if (qDiff > rDiff && qDiff > sDiff)
        {
            roundQ = -roundR - roundS;
        }
        // If R was rounded furthest from original, adjust it
        else if (rDiff > sDiff)
        {
            roundR = -roundQ - roundS;
        }
        // Otherwise adjust S (or if all are equal)
        else
        {
            roundS = -roundQ - roundR;
        }
        
        return new HexCoordinates(roundQ, roundR, roundS);
    }
    
    /// <summary>
    /// Gets the six neighboring hex coordinates
    /// </summary>
    public List<HexCoordinates> GetNeighbors()
    {
        return new List<HexCoordinates>
        {
            // Clockwise from NE
            new HexCoordinates(Q + 1, R - 1, S),
            new HexCoordinates(Q + 1, R, S - 1),
            new HexCoordinates(Q, R + 1, S - 1),
            new HexCoordinates(Q - 1, R + 1, S),
            new HexCoordinates(Q - 1, R, S + 1),
            new HexCoordinates(Q, R - 1, S + 1)
        };
    }
    
    #region Equality and Comparison
    public bool Equals(HexCoordinates other)
    {
        return Q == other.Q && R == other.R && S == other.S;
    }
    
    public override bool Equals(object obj)
    {
        if (obj is HexCoordinates)
            return Equals((HexCoordinates)obj);
        return false;
    }
    
    public override int GetHashCode()
    {
        // Combining hash codes for the three coordinates
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Q.GetHashCode();
            hash = hash * 31 + R.GetHashCode();
            hash = hash * 31 + S.GetHashCode();
            return hash;
        }
    }
    
    public static bool operator ==(HexCoordinates a, HexCoordinates b)
    {
        return a.Equals(b);
    }
    
    public static bool operator !=(HexCoordinates a, HexCoordinates b)
    {
        return !a.Equals(b);
    }
    #endregion
    
    public override string ToString()
    {
        return $"Hex({Q}, {R}, {S})";
    }
}
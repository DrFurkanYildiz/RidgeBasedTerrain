using UnityEngine;
using System.Collections.Generic;

public class Hexagon
{
    public List<Vector3> Points { get; }
    public Vector3 Center { get; }
    public Vector3 Normal { get; }

    public Hexagon(Vector3 position, float diameter)
    {
        Center = position;
        Normal = Vector3.up;
        Points = CalculatePoints(position, diameter);
    }

    public Hexagon(float diameter)
    {
        Center = Vector3.zero;
        Normal = Vector3.up;
        Points = CalculatePoints(Center, diameter);
    }

    public Hexagon()
    {
        Center = Vector3.zero;
        Normal = Vector3.up;
        Points = CalculatePoints(Center, 1f);
    }

    private static List<Vector3> CalculatePoints(Vector3 center, float diameter)
    {
        List<Vector3> result = new List<Vector3>();
        float R = diameter / 2f; // radius fonksiyonu yerine direkt çap/2
        float a = -Mathf.PI / 6f; // -30 derece (hexagon başlangıç açısı)//Burası + olabilir!!!

        for (int i = 0; i < 6; ++i)
        {
            float angle = a + i * Mathf.PI / 3f; // 60 derece aralıklarla
            float x = Mathf.Cos(angle) * R;
            float z = Mathf.Sin(angle) * R;
            result.Add(center + new Vector3(x, 0, z));
        }
        return result;
    }
}
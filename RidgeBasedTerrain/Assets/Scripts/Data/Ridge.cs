using System.Collections.Generic;
using UnityEngine;

public class Ridge
{
    private Vector3 _start;
    private Vector3 _end;
    private List<Vector3> _points;

    public Ridge(Vector3 start, Vector3 end)
    {
        _start = start;
        _end = end;
    }

    public void SetPoints(List<Vector3> points)
    {
        _points = points;
    }

    public List<Vector3> GetPoints()
    {
        return _points;
    }
}
/// <summary>
/// Connection between two ridge vertices
/// </summary>
public class RidgeConnection
{
    private RidgeVertex _first;
    private RidgeVertex _second;
    
    public RidgeConnection(RidgeVertex first, RidgeVertex second)
    {
        _first = first;
        _second = second;
    }
    
    public (RidgeVertex first, RidgeVertex second) Get()
    {
        return (_first, _second);
    }
}
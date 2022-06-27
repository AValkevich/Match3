using UnityEngine;

public class CellController: MonoBehaviour
{
    private int[] _cellIndex; 
    
    public void SetCellIndex(int[] cellIndex)
    {
        _cellIndex = cellIndex;
    }

    public int[] GetCellIndex()
    {
        return _cellIndex;
    }
}

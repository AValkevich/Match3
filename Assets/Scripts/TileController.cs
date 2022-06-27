using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour,IDragHandler, IBeginDragHandler,IEndDragHandler
{
    public delegate void Swipe(int[] cellIndex, int tileIndex, Vector2 direction);
    public static event Swipe CheckSwipe;
    
    private bool _isCheckDirection, _isHorizontalMove;
    private float _distance;
    private int _tileIndex;
    
    public void OnDrag(PointerEventData eventData)
    {
        _distance = Vector3.Distance(Vector3.zero, transform.localPosition);
        
        if (_isCheckDirection && _distance > 15)
        {
            if (Mathf.Abs(transform.localPosition.x) > Mathf.Abs(transform.localPosition.y))
                _isHorizontalMove = true;
            else
                _isHorizontalMove = false;
           
            _isCheckDirection = false;
        }

        if (!_isCheckDirection && _distance < 100)
        {
            if (_isHorizontalMove)
            {
                transform.localPosition = new Vector3(transform.localPosition.x + eventData.delta.x , 0, 0);
            }
            else
            {
                transform.localPosition = new Vector3(0, transform.localPosition.y +  eventData.delta.y,0);
            }
                   
        } 
        else if(_isCheckDirection)
        {
            transform.localPosition += (Vector3) eventData.delta;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        eventData.pointerDrag.transform.parent.SetAsLastSibling();
        _isCheckDirection = true;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if(_distance < 60)
            transform.localPosition = Vector3.zero;
        else
        {
            Vector2 direction;
            if (Mathf.Abs(transform.localPosition.x) > Mathf.Abs(transform.localPosition.y))
            {
                if (transform.localPosition.x > 0)
                {
                   direction = Vector2.right;
                }
                else
                {
                    direction = Vector2.left;
                }
            }
            else
            {
                if (transform.localPosition.y> 0)
                {
                    direction = Vector2.up;
                }
                else
                {
                    direction = Vector2.down;
                }
            }

            int[] cellIndex = transform.parent.GetComponent<CellController>().GetCellIndex();
            CheckSwipe?.Invoke(cellIndex, _tileIndex, direction);
        }
    }
    
    public void SetTileIndex(int index)
    {
        _tileIndex = index;
    }
    
    public int GetTileIndex()
    {
        return _tileIndex;
    }

}

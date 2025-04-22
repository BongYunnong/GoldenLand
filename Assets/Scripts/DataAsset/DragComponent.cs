using UnityEngine;

public class DragComponent : MonoBehaviour
{
    public LayerMask itemLayer;
    public DraggableComponent pendingItem;
    
    public void SetPending(DraggableComponent item)
    {
        pendingItem = item;
    }
    
    private void Update()
    {
        if (pendingItem && pendingItem.gameObject.activeInHierarchy)
        {
            pendingItem.UpdateDraggingItem();
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
            {
                pendingItem.MouseUp();
                pendingItem = null;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit, 100000, itemLayer);
            if (hit.collider != null && hit.collider.transform.TryGetComponent(out DraggableComponent item))
            {
                if(item.MouseDown())
                {
                    SetPending(item);
                }
            }
        }
    }
}

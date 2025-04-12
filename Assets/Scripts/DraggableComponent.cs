using System;
using UnityEngine;

public class DraggableComponent : MonoBehaviour
{
    [SerializeField] private Transform innerBody;
    [SerializeField] private Vector3 dragOffset = new Vector3(0,2,0);

    private Rigidbody rb;
    private Vector3 dragStartPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void UpdateDraggingItem()
    {
        Vector3 mouseWorldPos = transform.position;
        PlayerController.Instance.GetMouseFloorRayHitPos(ref mouseWorldPos);
        transform.position = Vector3.Lerp(transform.position, (mouseWorldPos + dragOffset), Time.deltaTime * 5.0f);
        if (innerBody)
        {
            innerBody.localRotation = Quaternion.LookRotation(transform.position - (mouseWorldPos + Vector3.back * 20));
        }
    }

    public void MouseUp()
    {
        rb.useGravity = true;
    }
    
    public bool MouseDown()
    {
        if (CanDrag())
        {
            dragStartPos = transform.position;
            dragStartPos.z = -2f;
            rb.useGravity = false;
            return true;
        }
        return false;
    }

    private bool CanDrag()
    {
        return true;
    }
}

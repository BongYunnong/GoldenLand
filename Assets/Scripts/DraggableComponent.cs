using System;
using UnityEngine;

public class DraggableComponent : MonoBehaviour
{
    [SerializeField] private Transform innerBody;
    [SerializeField] private Vector3 dragOffset = new Vector3(0,2,0);

    private Rigidbody rb;
    private Vector3 targetPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void UpdateDraggingItem()
    {
        Vector3 mouseWorldPos = transform.position;
        // PlayerController.Instance.GetMouseFloorRayHitPos(ref mouseWorldPos);
        mouseWorldPos = PlayerController.Instance.GetMouseRayProjectOnPlane(transform.position.y);
        mouseWorldPos.y = 0;
        targetPos = mouseWorldPos + dragOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5.0f);
        if (innerBody)
        {
            innerBody.localRotation = Quaternion.LookRotation(transform.position - (mouseWorldPos + Vector3.back * 20));
        }
    }

    public void MouseUp()
    {
        rb.useGravity = true;
        if (innerBody.TryGetComponent(out Animator animator))
        {
            animator.SetBool("Grabbed", false);
        }
    }
    
    public bool MouseDown()
    {
        if (CanDrag())
        {
            rb.useGravity = false;
            if (innerBody.TryGetComponent(out Animator animator))
            {
                animator.SetBool("Grabbed", true);
            }
            return true;
        }
        return false;
    }

    private bool CanDrag()
    {
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platformer3D : PlatformerComponent
{
    private Rigidbody rb;
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    public override void Move(Vector3 velocity)
    {
        collisions.Reset();
        collisions.velocityOld = velocity;

        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        base.Move(velocity);

        collisions.below = true;
    }

    protected override void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        Vector3 rayOrigin = (directionY == -1) ? transform.position : transform.position;
        RaycastHit2D[] hit;

        if (directionY < 0)
            hit = Physics2D.RaycastAll(rayOrigin, Vector3.up * directionY, rayLength, collisionMask | upperCollisionMask);
        else
            hit = Physics2D.RaycastAll(rayOrigin, Vector3.up * directionY, rayLength, collisionMask);
        Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.cyan);

        int index = 0;
        if (hit.Length > 0)
        {
            for (int j = 0; j < hit.Length; j++)
            {
                if (hit[j].point.y <= hit[index].point.y)
                {
                    index = j;
                }
            }
            bool inMask = false;
            for (int k = 0; k < GroundMasks.Length; k++)
            {
                if (hit[index].transform.gameObject && GroundMasks[k].gameObject == hit[index].transform.gameObject)
                {
                    inMask = true;
                }
            }
            if (inMask)
            {
                return;
            }

            velocity.y = (hit[index].distance - skinWidth) * directionY;

            collisions.below = directionY == -1;
            collisions.above = directionY == 1;

            currGround = hit[index].transform.gameObject;

            float slopeAngle = currGround.transform.rotation.eulerAngles.z;
            if (slopeAngle >= 180)
            {
                slopeAngle = -360 + slopeAngle;
            }
            if (currGround && directionY < 0 && Mathf.Abs(slopeAngle) >= 10 && Mathf.Abs(slopeAngle) <= maxClimbAngle)
            {
                velocity.y = Mathf.Tan(Mathf.Abs(slopeAngle) * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
            }
        }
    }
}

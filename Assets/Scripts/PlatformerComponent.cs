using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerComponent : MonoBehaviour
{
    protected const float skinWidth = .015f;

    [System.Serializable]
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }


    public LayerMask collisionMask;
    public LayerMask upperCollisionMask;

    protected Character character;

    public GameObject currGround;
    public Transform originGround { get; protected set; }

    public Collider2D[] GroundMasks;

    public CollisionInfo collisions;

    protected bool useGroundAsParent = true;

    [SerializeField] protected float maxClimbAngle = 60;
    [SerializeField] protected float maxDescendAngle = 50;

    public bool prevAbove = false;
    public bool prevBelow = false;
    public bool prevRight = false;
    public bool prevLeft = false;

    protected virtual void Awake()
    {
        character = GetComponent<Character>();
    }

    public virtual void Move(Vector3 velocity)
    {
        prevBelow = collisions.below;
        prevAbove = collisions.above;
        prevRight = collisions.right;
        prevLeft = collisions.left;
    }

    public void SetGroundAsParent(bool useGroundAsParent)
    {
        this.useGroundAsParent = useGroundAsParent;
    }


    public virtual bool IsOnOnewayPlatform()
    {
        if (currGround == null) return false;
        return upperCollisionMask == (upperCollisionMask | (1 << currGround.layer));
    }

    protected virtual void VerticalCollisions(ref Vector3 velocity)
    {

    }
}

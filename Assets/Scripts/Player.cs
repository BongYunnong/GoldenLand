using UnityEngine;

public class Player : Character
{
    protected override void Update()
    {
        if (initialized == false) return;
        base.Update();

        EmotionFunction();
    }

    private void EmotionFunction()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                anim.SetInteger("EmotionIndex", i);
                anim.SetTrigger("EmotionTrigger");
            }
        }
    }
    
    protected void MoveInputFunction()
    {
        mInputs[(int)EInputAction.Left] = false;
        mInputs[(int)EInputAction.Right] = false;
        mInputs[(int)EInputAction.Up] = false;
        mInputs[(int)EInputAction.Down] = false;
        mInputs[(int)EInputAction.Dodge] = false;

        if (IsControllable() == false) return;

        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 moveDir = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized * moveInput.z + Camera.main.transform.right * moveInput.x;
        moveDir.y = 0;
        SetMoveInput(new Vector2(moveDir.x, moveDir.z).normalized);

        Vector3 diff = FindObjectOfType<PlayerController>().GetMousePos() - perceptionComponent.transform.position;
        if (diff.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(diff);
            perceptionComponent.transform.rotation = Quaternion.Lerp(perceptionComponent.transform.rotation, targetRot, Time.deltaTime * 10.0f);
        }
    }
}

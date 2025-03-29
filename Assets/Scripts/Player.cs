using UnityEngine;

public class Player : Character
{
    public override void InitializeCharacter()
    {
        base.InitializeCharacter();

        dashInput = false;
        f_Dodge = 0;
    }

    #region Update
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
    
    protected override void MoveInputFunction()
    {
        mInputs[(int)EKeyInput.Left] = false;
        mInputs[(int)EKeyInput.Right] = false;
        mInputs[(int)EKeyInput.Up] = false;
        mInputs[(int)EKeyInput.Down] = false;
        mInputs[(int)EKeyInput.Dodge] = false;

        if (IsControllable() == false) return;

        if (f_Dodge <= 0)
        {
            Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 moveDir = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized * moveInput.z + Camera.main.transform.right * moveInput.x;
            moveDir.y = 0;
            SetInput(new Vector2(moveDir.x, moveDir.z).normalized);
            dashInput = Input.GetKey(KeyCode.LeftShift);
        }
        else
        {
            SetInput(Vector2.zero);
        }

        Vector3 diff = FindObjectOfType<PlayerController>().GetMousePos() - perceptionComponent.transform.position;
        if (diff.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(diff);
            perceptionComponent.transform.rotation = Quaternion.Lerp(perceptionComponent.transform.rotation, targetRot, Time.deltaTime * 10.0f);
        }
    }
    #endregion
}

using UnityEngine;

public class SimpleController : VoBehavior
{
    public float MoveSpeed = 1.0f;
    private const float SQRT_2 = 1.41421f;

    void FixedUpdate()
    {
        bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        
        if (left && !right)
        {
            if (up && !down)
                this.transform.AddPosition(-this.MoveSpeed / SQRT_2, 0, this.MoveSpeed / SQRT_2);
            else if (down && !up)
                this.transform.AddPosition(-this.MoveSpeed / SQRT_2, 0, -this.MoveSpeed / SQRT_2);
            else
                this.transform.AddPosition(-this.MoveSpeed, 0, 0);
        }
        else if (right && !left)
        {
            if (up && !down)
                this.transform.AddPosition(this.MoveSpeed / SQRT_2, 0, this.MoveSpeed / SQRT_2);
            else if (down && !up)
                this.transform.AddPosition(this.MoveSpeed / SQRT_2, 0, -this.MoveSpeed / SQRT_2);
            else
                this.transform.AddPosition(this.MoveSpeed, 0, 0);
        }
        else if (up && !down)
        {
            this.transform.AddPosition(0, 0, this.MoveSpeed);
        }
        else if (down && !up)
        {
            this.transform.AddPosition(0, 0, -this.MoveSpeed);
        }
    }
}

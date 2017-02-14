using UnityEngine;

public class SpriteFader : VoBehavior
{
    public float FadeSpeed = 1.0f;

    void Update()
    {
        Color c = this.spriteRenderer.color;
        if (_fadingIn)
        {
            c.a = c.a + Time.deltaTime * this.FadeSpeed;
            if (c.a >= 1.0f)
                _fadingIn = false;
            c.a = Mathf.Min(1.0f, c.a);
        }
        else
        {
            c.a = c.a - Time.deltaTime * this.FadeSpeed;
            if (c.a <= 0.0f)
                _fadingIn = true;
            c.a = Mathf.Max(0.0f, c.a);
        }
        this.spriteRenderer.color = c;
    }

    private bool _fadingIn = false;
}

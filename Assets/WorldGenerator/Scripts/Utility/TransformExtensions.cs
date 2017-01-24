using UnityEngine;


public static class TransformExtensions
{
    public static void SetPosition(this Transform self, float x, float y, float z)
    {
        self.position = new Vector3(x, y, z);
    }

    public static void SetPosition2D(this Transform self, float x, float y)
    {
        self.position = new Vector3(x, y, self.position.z);
    }

    public static void SetLocalPosition(this Transform self, float x, float y, float z)
    {
        self.localPosition = new Vector3(x, y, z);
    }

    public static void SetLocalPosition2D(this Transform self, float x, float y)
    {
        self.localPosition = new Vector3(x, y, self.localPosition.z);
    }

    public static void SetX(this Transform self, float x)
    {
        self.position = new Vector3(x, self.position.y, self.position.z);
    }

    public static void SetY(this Transform self, float y)
    {
        self.position = new Vector3(self.position.x, y, self.position.z);
    }

    public static void SetZ(this Transform self, float z)
    {
        self.position = new Vector3(self.position.x, self.position.y, z);
    }

    public static void SetLocalX(this Transform self, float x)
    {
        self.localPosition = new Vector3(x, self.localPosition.y, self.localPosition.z);
    }

    public static void SetLocalY(this Transform self, float y)
    {
        self.localPosition = new Vector3(self.localPosition.x, y, self.localPosition.z);
    }

    public static void SetLocalZ(this Transform self, float z)
    {
        self.localPosition = new Vector3(self.localPosition.x, self.localPosition.y, z);
    }
}

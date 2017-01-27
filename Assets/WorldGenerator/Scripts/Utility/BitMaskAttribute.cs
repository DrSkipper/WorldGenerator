using UnityEngine;

// Credit: http://answers.unity3d.com/questions/393992/custom-inspector-multi-select-enum-dropdown.html
// Allows for multi-select enum dropdown by use of [BitMask(typeof(ENUM_NAME))] before enum declaration.
// See Also: EnumEditorExtension.cs
public class BitMaskAttribute : PropertyAttribute
{
    public System.Type propType;
    public BitMaskAttribute(System.Type aType)
    {
        propType = aType;
    }
}

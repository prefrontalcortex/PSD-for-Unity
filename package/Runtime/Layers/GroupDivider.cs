using UnityEngine;
using System;

[Serializable]
public class GroupDivider : PsLayer
{
    public static GroupDivider Create()
    {
        var result = ScriptableObject.CreateInstance<GroupDivider>();
        result.name = "</Layer group>";
        return result;
    }
}
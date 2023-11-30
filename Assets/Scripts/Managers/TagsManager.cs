using System;
using System.Collections.Generic;
using UnityEngine;

public class TagsManager
{
    [Flags]
    public enum Tag
    {
        Untagged = 0,
        Player = 1,
        SafeArea = 2,
        Banana = 4,
        Runner = 8,
        Catcher = 16,
        NuteralPlayer = 32,
    }

    public static Tag GetTagFromString(string tagName)
    {
        List<string> tagNames = new List<string>(Enum.GetNames(typeof(Tag)));
        int tagIndex = tagNames.IndexOf(tagName) - 1;
        return  tagIndex == -1 ? Tag.Untagged : (Tag)(int)Mathf.Pow(2, tagIndex);
    }

    public static bool IsTagOneOfMultipleTags(Tag tag, Tag multipleTags)
    {
        return (tag & multipleTags) != 0;
    }
}

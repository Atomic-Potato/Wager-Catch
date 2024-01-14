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
        None = 64,
        Guard = 128,
        Mouse = 256,
    }

    public static Tag TeamTags =>  Tag.NuteralPlayer | Tag.Catcher | Tag.Runner;

    public enum TeamTag
    {
        NuteralPlayer = 32,
        Runner = 8,
        Catcher = 16,
        None = 64,
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

    public static Tag ConvertTeamTagToTag(TeamTag tag)
    {
        switch (tag)
        {
            case TeamTag.Runner:
                return Tag.Runner;
            case TeamTag.Catcher:
                return Tag.Catcher;
            default:
                return Tag.NuteralPlayer;
        }
    }
}

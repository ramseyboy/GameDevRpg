using System;
using System.Collections.Generic;
using System.Xml;

namespace RolePlayingGameData;

/// <summary>
///     Extends NET framework classes with the methods missing on XBox
///     Implements only basic functionality of those methods
/// </summary>
public static class MissingMethods
{
    /// <summary>
    ///     Implements List.RemoveAll(Predicate match) method
    /// </summary>
    public static int RemoveAll<T>(this List<T> that, Predicate<T> match)
    {
        var count = that.Count;
        var res = new List<T>(that.Count);
        foreach (var item in that)
        {
            if (match(item) == false)
            {
                res.Add(item);
            }
        }

        that.Clear();
        that.AddRange(res);
        return that.Count - count;
    }

    /// <summary>
    ///     Implements List.Find(Predicate match) method
    /// </summary>
    public static T Find<T>(this List<T> that, Predicate<T> match)
    {
        foreach (var item in that)
        {
            if (match(item))
            {
                return item;
            }
        }

        return default;
    }

    /// <summary>
    ///     Implements List.Exists(Predicate match) method
    /// </summary>
    public static bool Exists<T>(this List<T> that, Predicate<T> match)
    {
        foreach (var item in that)
        {
            if (match(item))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Implements List.TrueForAll(Predicate match) method
    /// </summary>
    public static bool TrueForAll<T>(this List<T> that, Predicate<T> match)
    {
        foreach (var item in that)
        {
            if (match(item) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Implements List.FindIndex(Predicate match) method
    /// </summary>
    public static int FindIndex<T>(this List<T> that, Predicate<T> match)
    {
        for (var i = 0; i < that.Count; i++)
        {
            if (match(that[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Implements XmlReader.ReadElementString(string name) method
    /// </summary>
    public static string ReadElementString(this XmlReader that, string name)
    {
        return that.ReadElementContentAsString();
    }
}

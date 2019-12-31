using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumHelper
{
    public static int GetIndex<T>(T value) where T : Enum {
        return Array.IndexOf(Enum.GetValues(typeof(T)), value);
    }

    public static int GetNameValue<T>(T value) where T : Enum {
        return (int)Enum.Parse(typeof(T), value.ToString());
    }

    public static T GetValueName<T>(int value) where T : Enum {
        return (T)Enum.ToObject(typeof(T), value);
    }
    
    public static bool TryGetValueName<T>(int value, out T name) where T : Enum {
        name = (T)Enum.ToObject(typeof(T), value);
        return Enum.IsDefined(typeof(T), name);
    }

    public static int GetValue<T>(int index) where T : Enum {
        return (int)(Enum.GetValues(typeof(T))).GetValue(index);
    }

    public static T GetName<T>(int index) where T : Enum {
        return (T)(Enum.GetValues(typeof(T))).GetValue(index);
    }

    public static T GetNextName<T>(T value) where T : Enum {
        Array enumArray = Enum.GetValues(typeof(T));
        int index = Array.IndexOf(enumArray, value);
        index = (index + 1) % enumArray.Length;
        return GetName<T>(index);
    }
}

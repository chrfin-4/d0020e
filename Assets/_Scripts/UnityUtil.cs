using System;
using UnityEngine;

public class UnityUtil
{

    public static T GetNestedComponent<T>(params string[] names)
        => GetNestedComponent<T>(GameObject.Find(names[0]).transform, rest(names));

    public static T GetNestedComponent<T>(Transform root, params string[] names)
    {
        Transform tmp = root;
        foreach (string name in names)
            tmp = tmp.Find(name);
        return tmp.GetComponent<T>();
    }

    private static T[] rest<T>(T[] arr)
    {
        T[] result = new T[arr.Length-1];
        Array.Copy(arr, 1, result, 0, arr.Length-1);
        return result;
    }

}

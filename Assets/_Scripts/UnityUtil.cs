using System;
using UnityEngine;

public static class UnityUtil
{

    public static T NestedComponent<T>(params string[] names)
        => NestedComponent<T>(GameObject.Find(names[0]).transform, rest(names));

    public static T NestedComponent<T>(this GameObject root, params string[] names)
        => root.transform.NestedComponent<T>(names);

    public static T NestedComponent<T>(this Transform root, params string[] names)
    {
        Transform tmp = root;
        foreach (string name in names)
            tmp = tmp.Find(name);
        return tmp.GetComponent<T>();
    }

    public static GameObject NestedObject(params string[] names)
        => NestedObject(GameObject.Find(names[0]).transform, rest(names));

    public static GameObject NestedObject(this GameObject root, params string[] names)
        => NestedObject(root.transform, names);

    public static GameObject NestedObject(this Transform root, params string[] names)
    {
        Transform tmp = root;
        foreach (string name in names)
            tmp = tmp.Find(name);
        return tmp.gameObject;
    }

    private static T[] rest<T>(T[] arr)
    {
        T[] result = new T[arr.Length-1];
        Array.Copy(arr, 1, result, 0, arr.Length-1);
        return result;
    }

}

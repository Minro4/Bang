using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TransformExtensions
{
    public static List<GameObject> FindObjectsWithTag(this Transform parent, string tag)
    {
        List<GameObject> taggedGameObjects = new List<GameObject>();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == tag)
            {
                taggedGameObjects.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                taggedGameObjects.AddRange(FindObjectsWithTag(child, tag));
            }
        }
        return taggedGameObjects;
    }
    public static GameObject FindObjectWithTag(this Transform parent, string tag)
    {

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == tag)
            {
                return child.gameObject;
            }
            if (child.childCount > 0)
            {
               GameObject f = FindObjectWithTag(child, tag);
                if (f != null)
                {
                    return f;
                }
            }
        }
return null;
    }
}

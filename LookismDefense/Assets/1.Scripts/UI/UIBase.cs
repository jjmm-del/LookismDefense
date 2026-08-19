using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public abstract class UIBase : MonoBehaviour
{
    private readonly Dictionary<Type, UnityEngine.Object[]> objects = new();

    protected void Bind<T>(Type enumType) where T : UnityEngine.Object
    {
        string[]names = Enum.GetNames(enumType);
        UnityEngine.Object[] boundObjects = new UnityEngine.Object[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            T component = FindChild<T>(gameObject, names[i], true);
            if (component == null)
            {
                Debug.LogError(
                    $"[{GetType().Name}] {typeof(T).Name}바인딩 실패 : {names[i]}"
                    );
            }
            boundObjects[i] = component;
        }

        objects[typeof(T)] = boundObjects;
    }

    protected T Get<T>(int index) where T : UnityEngine.Object
    {
        if (!objects.TryGetValue(typeof(T), out UnityEngine.Object[] boundObjects))
        {
            return null;
        }

        return boundObjects[index] as T;
    }

    protected Button GetButton(int index)
    {
        return Get<Button>(index);
    }

    protected TMP_Text GetText(int index)
    {
        return Get<TMP_Text>(index);
    }

    private static T FindChild<T>(GameObject root, string objectName, bool recursive) where T : UnityEngine.Object
    {
        foreach (Transform child in root.transform)
        {
            if (child.name == objectName)
            {
                T component = child.GetComponent<T>();

                if (component != null)
                {
                    return component;
                }
            }

            if (recursive)
            {
                T result = FindChild<T>(child.gameObject, objectName, true);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }
    
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
    
}

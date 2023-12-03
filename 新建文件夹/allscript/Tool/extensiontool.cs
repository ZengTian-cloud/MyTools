using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameObject扩展
/// </summary>
public static class GameObjectExtension
{
    [Obsolete("无效方法 : Unity Object should not use null coalescing. 请使用 GetMissingComponent 方法")]
    public static T GetOrAddCompoonet<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
    }

    public static GameObject CloneSelf(this GameObject gameObject, Transform parent, string name = "", bool isInitBaseProp = true)
    {
        GameObject instObj = GameObject.Instantiate(gameObject, parent);
        instObj.transform.localPosition = Vector3.zero;
        instObj.transform.localScale = Vector3.one;
        instObj.transform.localRotation = Quaternion.identity;
        if (!string.IsNullOrEmpty(name))
        {
            instObj.name = name;
        }
        return instObj;
    }

    public static T AddMissingComponent<T>(this GameObject go) where T : Component
    {
        if (!go.TryGetComponent<T>(out var component))
        {
            component = go.AddComponent<T>();
        }
        return component;
    }
}

/// <summary>
/// Transform扩展
/// </summary>
public static class TransformExtension
{
    [Obsolete ("无效方法 : Unity Object should not use null coalescing. 请使用 GetMissingComponent 方法")]
    public static T GetOrAddCompoonet<T>(this Transform transform) where T : Component
    {
        return transform.GetComponent<T>() ?? transform.gameObject.AddComponent<T>();
    }

    public static T AddMissingComponent<T>(this Transform trans) where T : Component
    {
        if (!trans.TryGetComponent<T>(out var component))
        {
            component = trans.gameObject.AddComponent<T>();
        }
        return component;
    }


    public static Transform FindHideInChild(this Transform transform, string childName, bool isDepth = false)
    {
        if (!isDepth)
        {
            int childCount = transform.childCount;
            for (int i = 0; i <= childCount - 1; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }
        }
        else
        {
            string[] pathNodeArr = childName.Split(new char[] { '/' });
            Transform finder = null;
            FindDepth(transform, pathNodeArr, 0, (_finder) => {
                if (_finder != null)
                {
                    finder = _finder;
                }
            });
            return finder;
        }
        return null;
    }
    private static void FindDepth(Transform tran, string[] pathNodeArr, int index, Action<Transform> callback)
    {
        if (index >= pathNodeArr.Length)
        {
            callback?.Invoke(tran);
            return;
        }
        int childCount = tran.childCount;
        string findName = pathNodeArr[index];
        for (int i = 0; i <= childCount - 1; i++)
        {
            Transform child = tran.GetChild(i);
            if (child.name == findName)
            {
                FindDepth(child, pathNodeArr, ++index, callback);
            }
        }
    }
    public static void ClearChild(this Transform transform)
    {
        while(transform.childCount > 0)
        {
            UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}

/// <summary>
/// Button扩展
/// </summary>
public static class ButtonExtension
{
    public static T GetOrAddCompoonet<T>(this Button btn) where T : Component
    {
        return btn.transform.GetComponent<T>() ?? btn.transform.gameObject.AddComponent<T>();
    }

    public static void SetText(this Button btn, string tx, string childTextName = "")
    {
        if (!string.IsNullOrEmpty(childTextName))
        {
            Transform childTextTran = btn.transform.FindHideInChild(childTextName);
            if (childTextTran != null)
            {
                Text c_text = childTextTran.GetComponentInChildren<Text>();
                if (c_text != null)
                {
                    c_text.SetTextExt(tx);
                    return;
                }

                TMP_Text c_tmptext = childTextTran.GetComponentInChildren<TMP_Text>();
                if (c_tmptext != null)
                {
                    c_tmptext.SetTextExt(tx);
                }
            }
            return;
        }


        Text text = btn.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.SetTextExt(tx);
            return;
        }

        TMP_Text tmptext = btn.GetComponentInChildren<TMP_Text>();
        if (tmptext != null)
        {
            tmptext.SetTextExt(tx);
        }
    }

    public static void AddListenerBeforeClear(this Button btn, UnityEngine.Events.UnityAction unityAction)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            unityAction?.Invoke();
        });
    }
}

/// <summary>
/// Text扩展
/// </summary>
public static class TextExtension
{
    public static T GetOrAddCompoonet<T>(this Text tx) where T : Component
    {
        return tx.transform.GetComponent<T>() ?? tx.transform.gameObject.AddComponent<T>();
    }

    public static void SetTextExt(this Text text, string content)
    {
        if (text == null)
        {
            return;
        }

        // TODO: limit count and block word library

        text.text = content;
    }
}

/// <summary>
/// TMP_Text扩展
/// </summary>
public static class TMP_TextExtension
{
    public static T GetOrAddCompoonet<T>(this TMP_Text tx) where T : Component
    {
        return tx.transform.GetComponent<T>() ?? tx.transform.gameObject.AddComponent<T>();
    }

    public static void SetTextExt(this TMP_Text text, string content)
    {
        if (text == null)
        {
            return;
        }

        // TODO: limit count and block word library
        text.text = content;
    }

    public static void SetTextWithEllipsis(this TMP_Text text, string content, int characterVisibleCount)
    {
        if (text == null)
        {
            return;
        }

#if UNITY_2019_1_OR_NEWER
        var updatedText = content;

        // 判断是否需要过长显示省略号
        if (content.Length > characterVisibleCount)
        {
            updatedText = content.Substring(0, characterVisibleCount - 1);
            updatedText += "…";
        }
#else
        // create generator with content and current Rect
        var generator = new TextGenerator();
        var rectTransform = textComponent.GetComponent<RectTransform>();
        var settings = textComponent.GetGenerationSettings(rectTransform.rect.size);
        generator.Populate(content, settings);
 
        // truncate visible content and add ellipsis
        var characterCountVisible = generator.characterCountVisible;
        var updatedText = content;
        if (content.Length > characterCountVisible)
        {
            updatedText = content.Substring(0, characterCountVisible - 1);
            updatedText += "…";
        }
#endif
        text.SetTextExt(updatedText);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditorHeroPopList : MonoBehaviour
{
    public GameObject root;
    public Button btnTitle1;
    public Button btnTitle2;

    public Button btnClose;

    public ScrollRect btnsr;
    public GameObject content;
    public GameObject btnroi;

    private EditorHero editorHero;
    private Action<string> action;

    private List<GameObject> objs = new List<GameObject>();

    public void SetActive(bool b, string sType, EditorHero editorHero, Action<string> action)
    {
        Clear();
        if (!b)
        {
            root.SetActive(false);
            return;
        }
        this.editorHero = editorHero;
        this.action = action;
        if (sType == "animlist")
        {
            CreateAnimList();
            btnTitle2.gameObject.SetActive(false);
        }
        else if (sType == "effectpoint")
        {
            CreateEffectPointList();
            btnTitle2.gameObject.SetActive(false);
        }
        else if (sType == "effectres")
        {
            CreateEffectResList();
            btnTitle2.gameObject.SetActive(false);
        }
        else if (sType == "jumpplane")
        {
            CreateCustomAnimList();
            btnTitle2.gameObject.SetActive(false);
        }

        btnClose.onClick.AddListener(() =>
        {
            action?.Invoke("");
            SetActive(false, "", null, null);
        });

        btnTitle1.onClick.AddListener(() =>
        {
            
        });

        btnTitle2.onClick.AddListener(() =>
        {
            
        });
        btnroi.SetActive(false);
        root.SetActive(true);
    }

    public void CreateAnimList()
    {
        AnimationClip[] animationClips = editorHero.curEHD.animator.runtimeAnimatorController.animationClips;
        foreach (var c in animationClips)
        {
            GameObject o = GameObject.Instantiate(btnroi);
            o.transform.SetParent(btnroi.transform.parent);
            o.transform.localScale = Vector3.one;
            o.GetComponentInChildren<TMP_Text>().text = c.name;
            o.GetComponentInChildren<Button>().onClick.AddListener(()=>
            {
                action?.Invoke(c.name);
                SetActive(false, "", null, null);
            });
            o.SetActive(true);
            objs.Add(o);
        }
    }

    public void CreateCustomAnimList()
    {
        List<string> extAct = new List<string>() {
            "entrance", "idle", "idle_2", "move", "move_2", "stun", "death",
            "attack_1", "attack_2", "skill1_1", "skill1_2", "skill2_1", "skill2_2", "skill3_1", "skill3_2","showidle","showloopidle"};
        // AnimationClip[] animationClips = editorHero.curEHD.animator.runtimeAnimatorController.animationClips;
        foreach (var c in extAct)
        {
            GameObject o = GameObject.Instantiate(btnroi);
            o.transform.SetParent(btnroi.transform.parent);
            o.transform.localScale = Vector3.one;
            o.GetComponentInChildren<TMP_Text>().text = c;
            o.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                action?.Invoke(c);
                SetActive(false, "", null, null);
            });
            o.SetActive(true);
            objs.Add(o);
        }
    }


    public void CreateEffectResList()
    {
        List<string> ps = EditorHeroHelper.GetEffects(editorHero.curEHD.id);
        if (ps != null)
        {
            foreach (var p in ps)
            {
                GameObject o = GameObject.Instantiate(btnroi);
                o.transform.SetParent(btnroi.transform.parent);
                o.transform.localScale = Vector3.one;
                o.GetComponentInChildren<TMP_Text>().text = p;
                o.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    action?.Invoke(p);
                    SetActive(false, "", null, null);
                });
                o.SetActive(true);
                objs.Add(o);
            }
        }
    }

    public void CreateEffectPointList()
    {
        EditotHeroPointData editotHeroPointData = editorHero.curEHD.pointData;
        if (editotHeroPointData != null)
        {
            foreach (var p in editotHeroPointData.paths)
            {
                GameObject o = GameObject.Instantiate(btnroi);
                o.transform.SetParent(btnroi.transform.parent);
                o.transform.localScale = Vector3.one;
                o.GetComponentInChildren<TMP_Text>().text = p.Key;
                o.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    action?.Invoke(p.Key);
                    SetActive(false, "", null, null);
                });
                o.SetActive(true);
                objs.Add(o);
            }
        }
    }

    public void Clear()
    {
        foreach (var item in objs)
        {
            GameObject.Destroy(item);
        }
        objs = new List<GameObject>();
    }
}

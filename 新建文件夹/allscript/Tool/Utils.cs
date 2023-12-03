using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class Utils
{
    public static T Find<T>(Transform root, string path) where T : Component
    {
        Transform transform = root.Find(path);

        if (transform!=null)

            return transform.GetComponent<T>();

        return null;
    }

    public static GameObject AddChiled(Transform parent, GameObject go)
    {
        GameObject obj = GameObject.Instantiate(go, parent);

        obj.transform.SetParent(parent, false);

        return obj;
    }

    public static T AddChiled<T>(Transform parent , GameObject go) where T :Component
    {
        GameObject obj = GameObject.Instantiate(go, parent);

        obj.transform.SetParent(parent, false);

        return obj.AddMissingComponent<T>();
    }


}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BaseWindow
{
    public abstract string Prefab { get; }
    public abstract string uiAtlasName { get; }
    public abstract void initUI(BaseWin win, GameObject parent);

}


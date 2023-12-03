using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class PrefabUIBase
{
	public GameObject _Root;
	public abstract string PrefabType {get;}
	public List<Button> btns;

	public abstract void InitRect();
	public abstract void DoAnim();
	public abstract void RegisterButton();
	public abstract void UnRegisterButton();
}

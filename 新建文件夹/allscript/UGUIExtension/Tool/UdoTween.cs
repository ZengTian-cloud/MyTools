using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class UcsdoTween
{
	public static UdoTween playgameobject(GameObject temobj, int temidx)
	{
#if !UNITY_EDITOR
		if (temobj == null)
		{
			temobj = new GameObject ("[Invalid UdoTween]");
			UnityEngine.Object.Destroy (temobj, 1f);
		}
#endif
		UdoTween temtween = null;
		if (temobj != null)
		{
			temtween = temobj.GetComponent<UdoTween>();
			if (temtween == null)
			{
				temtween = temobj.AddComponent<UdoTween>();
			}
			temtween.Play(temidx);
		}
		return temtween;
	}

	public static void stopid(GameObject temobj, int temidx)
	{
		if (temobj != null)
		{
			var temtween = temobj.GetComponent<UdoTween>();
			if (temtween != null)
			{
				temtween.Stop(temidx);
			}
		}
	}

	public static void stopself(GameObject temobj)
	{
		if (temobj != null)
		{
			var temtween = temobj.GetComponent<UdoTween>();
			if (temtween != null)
			{
				temtween.StopAll();
			}
		}
	}
}

public class UdoTween : MonoBehaviour
{
	// ---------------------------------------- 静态调用(start) ----------------------------------------
	// 清除dotween缓存
	public static void clearall(bool bdestroy)
	{
		DOTween.Clear(bdestroy);
	}

	// 播放一个动画
	public static UdoTween playone(int temid, int temidx)
	{
		return UcsdoTween.playgameobject(GameNode.GetGameObject(temid), temidx);
	}

	// 停止一个动画
	public static void stopone(int temid, int temidx)
	{
		UcsdoTween.stopid(GameNode.GetGameObject(temid), temidx);
	}

	// 停止所有动画
	public static void stopall(int temid)
	{
		UcsdoTween.stopself(GameNode.GetGameObject(temid));
	}
	// ---------------------------------------- 静态调用(end) ----------------------------------------

	// ---------------------------------------- 调用动画实例(start) ----------------------------------------
	// 设置循环
	// count: -1.无限次数  >0.指定次数
	// ttype: 循环类型 0.不断从起点重新开始 1.起点->终点->起点循环 2.不断将终点重置为起点重新开始
	public UdoTween setloops(int count, int ttype)
	{
		if (curSeq != null)
		{
			curSeq.SetLoops(count, (LoopType)ttype);
		}
		return this;
	}

	public UdoTween setspeedbase()
	{
		if (curSeq != null)
		{
			curSeq.SetSpeedBased();
		}
		return this;
	}

	// 设置忽略时间缩放
	// bupdate: 是否忽略时间缩放
	public UdoTween setupdate(bool bupdate)
	{
		if (curSeq != null)
		{
			curSeq.SetUpdate(bupdate);
		}
		return this;
	}

	// 设置缓冲类型
	// ease: 缓冲标记 配置定义在ease类  详见链接.https://blog.csdn.net/u013762848/article/details/82256276
	public UdoTween setease(int ease)
	{
		if (curSeq != null)
		{
			curSeq.SetEase((Ease)ease);
		}
		return this;
	}

	// 设置相对变化, 即目标值会转化为变化值 最终目标值 = 初始值 + 变化值
	public UdoTween setrelative()
	{
		if (curSeq != null)
		{
			curSeq.SetRelative();
		}
		return this;
	}

	// 设置自动销毁
	// bauto: 是否自动销毁
	public UdoTween setautokill(bool bauto)
	{
		if (curSeq != null)
		{
			curSeq.SetAutoKill(bauto);
		}
		return this;
	}

	// 设置延迟
	// time: 延迟时间, 为动画开始前的延迟, 不能用于动画之间的间隔时间
	public UdoTween setdelay(float time)
	{
		if (curSeq != null)
		{
			curSeq.SetDelay(time);
		}
		return this;
	}

	// 直接完成
	public UdoTween docomplete()
	{
		transform.DOComplete();
		return this;
	}

	// 每完成一个动画步骤回掉
	// cb: 回调函数

	Dictionary<Sequence, Action> stepcalls;
	public Dictionary<Sequence, Action> Stepcalls
	{
		get
		{
			if (stepcalls == null)
			{
				stepcalls = new Dictionary<Sequence, Action>();
			}
			return stepcalls;
		}
	}
	public UdoTween onstepcomplete(Action cb)
	{
		if (curSeq != null)
		{
			Action temp = Stepcalls[curSeq] = cb;
			curSeq.OnStepComplete(delegate ()
			{
				temp?.Invoke();
			});
		}
		return this;
	}

	// 动画播放完成后调用
	// cb: 回调函数
	Dictionary<Sequence, Action> completecalls;
	public Dictionary<Sequence, Action> Completecalls
	{
		get
		{
			if (completecalls == null)
			{
				completecalls = new Dictionary<Sequence, Action>();
			}
			return completecalls;
		}
	}
	public UdoTween oncomplete(Action cb)
	{
		if (curSeq != null)
		{
			Action temp = Completecalls[curSeq] = cb;
			curSeq.OnComplete(delegate ()
			{
				temp?.Invoke();
			});
		}
		return this;
	}

	// 每帧回调
	// cb: 回调函数
	Dictionary<Sequence, Action> updatecalls;
	public Dictionary<Sequence, Action> Updatecalls
	{
		get
		{
			if (updatecalls == null)
			{
				updatecalls = new Dictionary<Sequence, Action>();
			}
			return updatecalls;
		}
	}
	public UdoTween onupdate(Action cb)
	{
		if (curSeq != null)
		{
			Action temp = Updatecalls[curSeq] = cb;
			curSeq.OnUpdate(delegate ()
			{
				temp?.Invoke();
			});
		}
		return this;
	}

	// x轴全局移动动画
	public UdoTween domovex(float x, float time, bool b)
	{
		SetTween(transform.DOMoveX(x, time, b));
		return this;
	}

	// y轴全局移动动画
	public UdoTween domovey(float y, float time, bool b)
	{
		SetTween(transform.DOMoveY(y, time, b));
		return this;
	}

	// z轴全局移动动画
	public UdoTween domovez(float z, float time, bool b)
	{
		SetTween(transform.DOMoveZ(z, time, b));
		return this;
	}

	// xyz轴全局移动动画
	public UdoTween domove(float posx, float posy, float posz, float time, bool b)
	{
		SetTween(transform.DOMove(new Vector3(posx, posy, posz), time, b));
		return this;
	}


	// x轴local移动动画
	public UdoTween dolocalmovex(float x, float time, bool b)
	{
		SetTween(transform.DOLocalMoveX(x, time, b));
		return this;
	}

	// y轴local移动动画
	public UdoTween dolocalmovey(float y, float time, bool b)
	{
		SetTween(transform.DOLocalMoveY(y, time, b));
		return this;
	}

	// z轴local移动动画
	public UdoTween dolocalmovez(float z, float time, bool b)
	{
		SetTween(transform.DOLocalMoveZ(z, time, b));
		return this;
	}

	// xyz轴local移动动画
	public UdoTween dolocalmove(float posx, float posy, float posz, float time, bool b)
	{
		SetTween(transform.DOLocalMove(new Vector3(posx, posy, posz), time, b));
		return this;
	}
	// xyz轴联合移动动画
	public UdoTween doblendablemoveby(float posx, float posy, float posz, float time, bool b)
	{
		SetTween(transform.DOBlendableMoveBy(new Vector3(posx, posy, posz), time, b));
		return this;
	}

	// xyz轴联合local移动动画
	public UdoTween doblendablelocalmoveby(float posx, float posy, float posz, float time, bool b)
	{
		SetTween(transform.DOBlendableLocalMoveBy(new Vector3(posx, posy, posz), time, b));
		return this;
	}
	// x轴全局缩放动画
	public UdoTween doscalex(float x, float time)
	{
		SetTween(transform.DOScaleX(x, time));
		return this;
	}

	// y轴全局缩放动画
	public UdoTween doscaley(float y, float time)
	{
		SetTween(transform.DOScaleY(y, time));
		return this;
	}

	// z轴全局缩放动画
	public UdoTween doscalez(float z, float time)
	{
		SetTween(transform.DOScaleZ(z, time));
		return this;
	}

	// xyz轴全局缩放动画
	public UdoTween doscale(float x, float y, float z, float time)
	{
		SetTween(transform.DOScale(new Vector3(x, y, z), time));
		return this;
	}

	// 全局旋转动画
	public UdoTween dorotate(float posx, float posy, float posz, float time, int mode)
	{
		SetTween(transform.DORotate(new Vector3(posx, posy, posz), time, (RotateMode)mode));
		return this;
	}

	// local旋转动画
	public UdoTween dolocalrotate(float posx, float posy, float posz, float time, int mode)
	{
		SetTween(transform.DOLocalRotate(new Vector3(posx, posy, posz), time, (RotateMode)mode));
		return this;
	}

	public UdoTween dolocalrotatebyquaternion(float posx, float posy, float posz, float time)
	{
		SetTween(transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(posx, posy, posz)), time));
		return this;
	}

	// 看相某点动画
	public UdoTween dolookat(float sx, float sy, float sz, float tx, float ty, float tz, float time, int flags)
	{
		SetTween(transform.DOLookAt(new Vector3(sx, sy, sz), time, (AxisConstraint)flags, new Vector3(tx, ty, tz)));
		return this;
	}

	// 全局跳跃动画
	public UdoTween dojump(float posx, float posy, float posz, float time, bool b, float jumpPower, int jumps)
	{
		SetSequence(transform.DOJump(new Vector3(posx, posy, posz), jumpPower, jumps, time, b));
		return this;
	}

	// 本地跳跃动画
	public UdoTween dolocaljump(float posx, float posy, float posz, float time, bool b, float jumpPower, int jumps)
	{
		SetSequence(transform.DOLocalJump(new Vector3(posx, posy, posz), jumpPower, jumps, time, b));
		return this;
	}

	// 锚点x移动动画
	public UdoTween doanchorposx(float posx, float time, bool b)
	{
		var temcom = GetComponent<RectTransform>();
		SetTween(temcom.DOAnchorPosX(posx, time, b));
		return this;
	}

	// 锚点y轴移动动画
	public UdoTween doanchorposy(float posy, float time, bool b)
	{
		var temcom = GetComponent<RectTransform>();
		SetTween(temcom.DOAnchorPosY(posy, time, b));
		return this;
	}

	// 锚点xy轴移动动画
	public UdoTween doanchorpos2d(float posx, float posy, float time, bool b)
	{
		var temcom = GetComponent<RectTransform>();
		SetTween(temcom.DOAnchorPos(new Vector2(posx, posy), time, b));
		return this;
	}

	// sizedelta修改动画
	public UdoTween dosizedelta(float sizex, float sizey, float time)
	{
		var temcom = gameObject.GetComponent<RectTransform>();
		SetTween(temcom.DOSizeDelta(new Vector2(sizex, sizey), time));
		return this;
	}

	// image填充动画
	public UdoTween doimagefillamount(float a, float time)
	{
		Image temcom = gameObject.GetComponent<Image>();
		if (temcom != null)
		{
			SetTween(temcom.DOFillAmount(a, time));
		}
		return this;
	}

	// image渐隐渐显动画
	public UdoTween doimagefade(float a, float time)
	{
		Image temcom = gameObject.GetComponent<Image>();
		if (temcom != null)
		{
			SetTween(temcom.DOFade(a / 255, time));
		}
		return this;
	}

	// rawimage渐隐渐显动画
	public UdoTween dorawimagefade(float a, float time)
	{
		RawImage temcom = gameObject.GetComponent<RawImage>();
		if (temcom != null)
		{
			SetTween(temcom.DOFade(a / 255, time));
		}
		return this;
	}

	// RawImage改变值动画
	public UdoTween dorawimagefloat(float val, string key, float time)
	{
		var temcom = GetComponent<RawImage>();
		if (temcom != null && temcom.material != null)
		{
			SetTween(temcom.material.DOFloat(val, key, time));
		}
		return this;
	}

	// spriterenderer渐隐渐显动画
	public UdoTween dospriterendererfade(float a, float time)
	{
		SpriteRenderer temcom = gameObject.GetComponent<SpriteRenderer>();
		if (temcom != null)
		{
			SetTween(temcom.DOFade(a / 255, time));
		}
		return this;
	}

	// 文字打字动画
	public UdoTween dotextchange(string endtext, float time)
	{
		Text temcom = gameObject.GetComponent<Text>();
		if (temcom != null)
		{
			SetTween(temcom.DOText(endtext, time));
		}
		return this;
	}

	// 文字渐隐渐显动画
	public UdoTween dotextfade(float a, float time)
	{
		Text temcom = gameObject.GetComponent<Text>();
		if (temcom != null)
		{
			SetTween(temcom.DOFade(a / 255, time));
		}
		return this;
	}

	// canvasgroup渐隐渐显动画
	public UdoTween docanvasgroupfade(float a, float time)
	{
		CanvasGroup temcom = gameObject.GetComponent<CanvasGroup>();
		if (temcom == null)
		{
			temcom = gameObject.AddComponent<CanvasGroup>();
		}
		SetTween(temcom.DOFade(a / 255, time));
		return this;
	}

	// mesh渐隐渐显动画
	public UdoTween domeshrendererfade(float a, string key, float time)
	{
		var temcom = GetComponent<MeshRenderer>();
		if (temcom != null && temcom.material != null)
		{
			SetTween(temcom.material.DOFade(a / 255, key, time));
		}
		return this;
	}

	// renderer改变值动画
	public UdoTween dorendererfloat(float val, string key, float time)
	{
		var temcom = GetComponent<Renderer>();
		if (temcom != null && temcom.material != null)
		{
			SetTween(temcom.material.DOFloat(val, key, time));
		}
		return this;
	}

	// 滑动条动画
	public UdoTween doslidermove(float over, float time)
	{
		Slider temcom = gameObject.GetComponent<Slider>();
		if (temcom != null)
		{
			SetTween(temcom.DOValue(over, time));
		}
		return this;
	}

	// 滑动列表滑动动画
	public UdoTween doscrollrectmove(float posx, float posy, float time)
	{
		var temcom = GetComponent<ScrollRect>();
		if (temcom != null)
		{
			SetTween(DOTween.To(
				() => temcom.normalizedPosition,
				x => temcom.normalizedPosition = x,
				new Vector2(posx, posy),
				time
			));
		}
		return this;
	}

	// 数字修改动画
	public UdoTween playrisenum(int initnum, int endnum, float time, Action<int> cb)
	{
		int number = initnum;
		bool over = false;
		SetTween(DOTween.To(
			() => number,
			x =>
			{
				number = x;
				if (!over)
				{
					cb?.Invoke(number);
				}
				if (number == endnum)
				{
					over = true;
					cb?.Invoke(endnum);
				}
			},
			endnum,
			time
		));
		return this;
	}

	//屏幕（摄像机）震动效果，time为时间，strongth为强度（0～1）
	public UdoTween doshakeposition(float time, float strongth)
	{
		SetTween(transform.DOShakePosition(time, strongth));
		return this;
	}
	// ---------------------------------------- 调用动画实例(end) ----------------------------------------

	private Dictionary<int, Sequence> seqDict = new Dictionary<int, Sequence>();
	private Sequence curSeq;

	static UdoTween()
	{
		DOTween.SetTweensCapacity(200, 200);
	}

	internal UdoTween Play(int key)
	{
		if (seqDict.ContainsKey(key))
		{
			curSeq = seqDict[key];
			if (curSeq.IsPlaying())
			{
				curSeq.Kill();
			}
			curSeq = DOTween.Sequence();
			seqDict[key] = curSeq;
		}
		else
		{
			curSeq = DOTween.Sequence();
			seqDict.Add(key, curSeq);
		}
		return this;
	}

	internal void Stop(int key)
	{
		if (seqDict.ContainsKey(key))
		{
			Sequence temSeq = seqDict[key];
			clearoneseqcall(temSeq);
			if (temSeq.IsPlaying())
			{
				temSeq.Kill();
			}
		}
	}

	internal void StopAll()
	{
		foreach (var item in seqDict)
		{
			Sequence temSeq = item.Value;
			clearoneseqcall(temSeq);
			if (temSeq != null && temSeq.IsPlaying())
			{
				temSeq.Kill();
			}
		}
		seqDict.Clear();
	}

	private void clearoneseqcall(Sequence seq)
	{
		if (Updatecalls != null)
			if (Updatecalls.ContainsKey(seq))
			{
				Updatecalls[seq] = null;
			}
		if (Stepcalls != null)
			if (Stepcalls.ContainsKey(seq))
			{
				Stepcalls[seq] = null;
			}
		if (Completecalls != null)
			if (Completecalls.ContainsKey(seq))
			{
				Completecalls[seq] = null;
			}
	}

	private void OnDestroy()
	{
		StopAll();
	}

	private void OnDisable()
	{
		// 清除所有的回调
		foreach (var item in seqDict)
		{
			Sequence temSeq = item.Value;
			if (temSeq != null)
				clearoneseqcall(temSeq);
		}
	}

	private void SetTween(Tweener temp)
	{
		if (curSeq != null)
		{
			curSeq.Append(temp);
		}
	}

	private void SetSequence(Sequence temp)
	{
		if (curSeq != null)
		{
			curSeq.Append(temp);
		}
	}
}
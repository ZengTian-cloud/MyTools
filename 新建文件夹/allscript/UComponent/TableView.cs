using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// 无限滑动列表
[ExecuteInEditMode]
public class TableView : MonoBehaviour
{
	public enum AttachDirDef
	{
		//
		AttachTop, // 上对齐, 从下往上滑动
		AttachBottom, // 下对齐, 从上往下滑动
		AttachLeft, // 左对齐, 从右往左滑动
		AttachRight // 右对齐, 从左往右滑动
	}

	[SerializeField]
	private bool _interactable = true;
	[SerializeField]
	private bool _ControllerChildWidth = false;
	public class LoadCfg
	{
		public int mIndex = 0;
		public int newHeadIdx = 0;
		public int curIdx = 0;
		public bool force = false;
	}
	#region public attribute
	public ScrollRect mScrollComponent;
	public RectTransform maskView;
	public RectTransform moveContainer;
	public TableItem minItem;
	public RectTransform rectItem;
	public GameObject emptyTipObject;
	public UnityAction<GameObject,int> onItemRender;//单个item渲染回调
	public UnityAction onItemDispose;//单个item释放回调

	public bool buseloadani = true;
	public AttachDirDef mAttachDir = AttachDirDef.AttachTop;
	public float mleft = 0;
	public float mright = 0;
	public float mtop = 0;
	public float mbottom = 0;
	public Vector2 padding = Vector2.zero;
	public bool adaptColumn = false;
	[Range(1, 20)]
	public int columnCount = 1;

	public int recordCount { get; private set; }
	public int curHeadIndex { get; private set; }
	public int currFirstIndex { get; private set; }
	public Rect minRect { get; private set; }

	public UnityAction onReCalculate;
	#endregion

	#region private attribute
	private int datacount = 0;
	private float minScroll = 0.3f;
	private float maxScroll = 0.6f;
	private int showItemCount = 5;
	private float renderPerFrames = 1;
	private int lastHeadIndex = 0;
	private int cloumIndex = 0;
	private Vector3 dtmove;
	private Vector3 beginPosition;
	private Vector3 currPosition;
	private float beginWidth;
	private float beginHeight;
	private List<TableItem> repositionTileList = new List<TableItem>();
	private List<int> repositionTileIndexList = new List<int>();
	private Queue<TableItem> preRenderList = new Queue<TableItem>();
	private Queue<int> preRepositionIntList = new Queue<int>();
	private List<float> itemRealDistance = new List<float>();
	private List<Vector2> itemRealPosList = new List<Vector2>();
	private Queue<LoadCfg> preloadList = new Queue<LoadCfg>();
	// 是否在编辑器增删过item
	private bool _editOperate = false;
	public bool bEditOperate
	{
		get { return _editOperate; }
		set
		{
			if (value)
			{
				preRepositionIntList.Clear();
			}
			_editOperate = value;
		}
	}

	// 开启/禁用玩家主动滑动操作
	public bool m_Interactable
	{
		get => _interactable;
		set
		{
			_interactable = value;
			ChangeScrollRectScrollDir();
		}
	}

	// editor模式面板值变化监听
	private bool bChangedVal = false;
	// 刷新列表延迟标记
	private int changeTag = int.MaxValue;
	// 临时变量
	private bool _tmpcreatetag = false;
	private bool _tmpcreateall = false;
	#endregion

	//设置列表数据
	#region public method
	public void SetDatas(int datacount, bool breset)
	{
#if UNITY_EDITOR
		if (bEditOperate)
		{
			return;
		}
#endif
		//数量
		var curlen = datacount;
		this.datacount = datacount;

        if (breset)
		{
			currFirstIndex = 0;
			moveContainer.localPosition = beginPosition;
		}
		else if (currFirstIndex + showItemCount > curlen)
		{
			currFirstIndex = Math.Max(0, curlen - showItemCount);
		}

		ReCalculate();
		ClearAll();
		if (!CheckShowEmpty())
		{
			DoScroll(currFirstIndex, true);
		}
		
	}

	public int GetIndex(TableItem item)
	{
		int index = repositionTileList.IndexOf(item);
		if (index >= 0)
		{
			index = repositionTileIndexList[index];
		}
		return index;
	}

	//刷新数据
	public void RefreshDatas()
	{
		DoScroll(currFirstIndex, true);
	}

	public void RefreshAllItemSize()
	{
		ReCalculate();
		foreach (var item in repositionTileList)
		{
			item.rectTransform.sizeDelta = new Vector2(minRect.width, item.rectTransform.sizeDelta.y);
		}
		foreach (var item in preRenderList)
		{
			item.rectTransform.sizeDelta = new Vector2(minRect.width, item.rectTransform.sizeDelta.y);
		}
		RefreshDatas();
	}

	public void ClearItemCreateFlag()
	{
		for (int i = 0; i < repositionTileList.Count; i++)
		{
			repositionTileList[i].Createtag = true;
		}
	}

	//滑动到某个节点
	public void ScrollToIndex(int newHead, float timeratio, float posratio)
	{
		int realcount = itemRealPosList.Count;
		if (moveContainer != null && realcount > 0)
		{
			newHead = newHead < 0 ? 0 : (newHead >= realcount ? realcount - 1 : newHead);
			var endx = moveContainer.sizeDelta.x - maskView.rect.width;
			var endy = moveContainer.sizeDelta.y - maskView.rect.height;
			if (endy > 0 && mAttachDir == AttachDirDef.AttachTop)
			{
				float tary = -itemRealPosList[newHead].y - maskView.rect.height * posratio;
				tary = tary >= endy ? endy : (tary <= 0 ? 0 : tary);
				var cury = moveContainer.localPosition.y;
				var tartime = Mathf.Abs(cury - tary) * 0.001f;
				moveContainer.DOLocalMoveY(tary, (tartime > maxScroll ? maxScroll : (tartime < minScroll ? minScroll : tartime)) * timeratio, true);
			}
			else if (endy > 0 && mAttachDir == AttachDirDef.AttachBottom)
			{
				float tary = -itemRealPosList[newHead].y + maskView.rect.height * posratio;
				tary = tary + endy < 0 ? -endy : (tary > 0 ? 0 : tary);
				var cury = moveContainer.localPosition.y;
				var tartime = Mathf.Abs(cury - tary) * 0.001f;
				moveContainer.DOLocalMoveY(tary, (tartime > maxScroll ? maxScroll : (tartime < minScroll ? minScroll : tartime)) * timeratio, true);
			}
			else if (endx > 0 && mAttachDir == AttachDirDef.AttachLeft)
			{
				float tarx = -itemRealPosList[newHead].x + maskView.rect.width * posratio;
				tarx = tarx + endx < 0 ? -endx : (tarx > 0 ? 0 : tarx);
				var curx = moveContainer.localPosition.x;
				var tartime = Mathf.Abs(curx - tarx) * 0.001f;
				moveContainer.DOLocalMoveX(tarx, (tartime > maxScroll ? maxScroll : (tartime < minScroll ? minScroll : tartime)) * timeratio, true);
			}
			else if (endx > 0 && mAttachDir == AttachDirDef.AttachRight)
			{
				float tarx = -itemRealPosList[newHead].x - maskView.rect.width * posratio;
				tarx = tarx >= endx ? endx : (tarx <= 0 ? 0 : tarx);
				var curx = moveContainer.localPosition.x;
				var tartime = Mathf.Abs(curx - tarx) * 0.001f;
				moveContainer.DOLocalMoveX(tarx, (tartime > maxScroll ? maxScroll : (tartime < minScroll ? minScroll : tartime)) * timeratio, true);
			}
		}
	}
	#endregion

	#region private method
	private void CalcBounds()
	{
		var trect = rectItem != null ? rectItem : minItem?.rectTransform;
		if (trect)
		{
			float twidth = trect.sizeDelta.x;
			float theight = trect.sizeDelta.y;
			if (_ControllerChildWidth)
			{
				twidth = (maskView.rect.width - padding.x - mleft - mright) / columnCount;
			}
			minRect = new Rect(0, 0, twidth, theight);
		}
	}

	private void CalcPage(int totalItemCount)
	{
		if (moveContainer != null)
		{
			recordCount = totalItemCount;

			bool tHorizontal = mAttachDir == AttachDirDef.AttachLeft || mAttachDir == AttachDirDef.AttachRight;
			itemRealDistance.Clear();
			for (int i = 0; i < recordCount; i++)
			{
				//var tdata = GetOneData(i);
				var tvalue = tHorizontal ? minRect.width : minRect.height;
				itemRealDistance.Add(tvalue);
			}

			float lastPosTag = 0;
			float lastDistance = 0;
			float curDistance = 0;
			float itemsDistance = 0;
			Vector2 itemPos = Vector2.zero;
			itemRealPosList.Clear();
			for (int rowidx = 0; rowidx < recordCount; rowidx += columnCount)
			{
				for (int itemidx = rowidx; itemidx < recordCount && itemidx < rowidx + columnCount; itemidx++)
				{
					curDistance = Mathf.Max(curDistance, itemRealDistance[itemidx]);
				}
				for (int itemidx = rowidx; itemidx < recordCount && itemidx < rowidx + columnCount; itemidx++)
				{
					itemPos = ReCalcItem(itemidx, lastPosTag, lastDistance, curDistance);
					itemRealPosList.Add(itemPos);
				}
				lastPosTag = tHorizontal ? itemPos.x : itemPos.y;
				lastDistance = curDistance;
				itemsDistance += curDistance;
				curDistance = 0;
			}

			int temcount = (int)Mathf.Ceil((float)recordCount / columnCount);
			if (tHorizontal)
			{
				float totalwidth = itemsDistance + padding.x * temcount + mleft + mright;
				moveContainer.sizeDelta = new Vector2(totalwidth, moveContainer.sizeDelta.y);
			}
			else
			{
				float totalheight = itemsDistance + padding.y * temcount + mtop + mbottom;
				moveContainer.sizeDelta = new Vector2(moveContainer.sizeDelta.x, totalheight);
			}
		}
	}

	private Vector2 ReCalcItem(int curIndex, float lastPosTag, float lastDistance, float curDistance)
	{
		Vector2 pos = new Vector2(0, 0);
		int temidx1 = curIndex % columnCount;
		int temidx2 = curIndex / columnCount;
		if (mAttachDir == AttachDirDef.AttachTop)
		{
			pos.x = (minRect.width + padding.x) * temidx1 + padding.x + minRect.width * 0.5f + mleft - mright;
			pos.y = lastPosTag - padding.y - lastDistance * 0.5f - curDistance * 0.5f;
			if (temidx2 == 0) pos.y -= mtop;
		}
		else if (mAttachDir == AttachDirDef.AttachBottom)
		{
			pos.x = (minRect.width + padding.x) * temidx1 + padding.x + minRect.width * 0.5f + mleft - mright;
			pos.y = lastPosTag + padding.y + lastDistance * 0.5f + curDistance * 0.5f;
			if (temidx2 == 0) pos.y += mbottom;
		}
		else if (mAttachDir == AttachDirDef.AttachLeft)
		{
			pos.y = -(minRect.height + padding.y) * temidx1 - padding.y - minRect.height * 0.5f + mbottom - mtop;
			pos.x = lastPosTag + padding.x + lastDistance * 0.5f + curDistance * 0.5f;
			if (temidx2 == 0) pos.x += mleft;
		}
		else
		{
			pos.y = -(minRect.height + padding.y) * temidx1 - padding.y - minRect.height * 0.5f + mbottom - mtop;
			pos.x = lastPosTag - padding.x - lastDistance * 0.5f - curDistance * 0.5f;
			if (temidx2 == 0) pos.x -= mright;
		}
		return pos;
	}

	private bool PreRefresh(int index, int newHeadIdx, int curIdx, bool force)
	{
		var createidx = index - newHeadIdx;
		_tmpcreatetag = createidx >= repositionTileList.Count && repositionTileList.Count < recordCount;
		if (_tmpcreatetag)
		{
			var tobj = minItem.gameObject;
			var cloneobj = (GameObject)Instantiate(tobj);
			var cloneRefer = cloneobj.GetComponent<TableItem>();
            Rect itemRect = cloneobj.GetComponent<RectTransform>().rect;
			cloneobj.GetComponent<RectTransform>().sizeDelta = new Vector2(minRect.width, itemRect.height);
			repositionTileList.Add(cloneRefer);
			repositionTileIndexList.Add(-1);
			cloneobj.transform.SetParent(transform, false);
			cloneobj.SetActive(true);
		}
		if (!(index >= curIdx && index < curIdx + showItemCount && !force))
		{
			int moveIdx = index;
			if (curIdx != newHeadIdx)
			{
				moveIdx = curIdx + showItemCount - (index - newHeadIdx) - 1;
			}
			var tileItem = GetItemAndSetPreRender(moveIdx, index);
			if (tileItem != null)
			{
				DoPreRender(tileItem, index);
			}
		}
		return _tmpcreatetag;
	}

	private void DoPreRender(TableItem temitem, int index)
	{
		if (temitem == null) return;
		temitem.name = "peritem";
		var tempos = temitem.transform.localPosition;
		tempos.x = -10000;
		tempos.y = (index + 1) * 1000;
		temitem.transform.localPosition = tempos;
	}

	private void RenderItem()
	{
		for (int i = 0; i < renderPerFrames; i++)
		{
			DoRender();
		}
	}

	private void DoRender()
	{
		if (preRepositionIntList.Count > 0)
		{
			var temitem = preRenderList.Dequeue();
			int curRenderIdx = preRepositionIntList.Dequeue();
			if (curRenderIdx + 1 <= recordCount)
			{
				SetItemPosition(temitem.rectTransform, curRenderIdx);
				
				if (onItemRender != null)
				{
					onItemRender(temitem.gameObject, curRenderIdx + 1);

                }
				temitem.Createtag = false;
				
			}
		}
	}

	private TableItem GetItemAndSetPreRender(int curIdx, int newIdx)
	{
		int index = repositionTileIndexList.IndexOf(curIdx);
		if (index == -1)
		{
			index = repositionTileIndexList.IndexOf(-1);
		}
		TableItem temitem = null;
		if (index >= 0 && repositionTileList.Count > index && repositionTileIndexList.Count > index)
		{
			repositionTileIndexList[index] = newIdx;
			temitem = repositionTileList[index];
			preRenderList.Enqueue(temitem);
			preRepositionIntList.Enqueue(newIdx);
		}
		return temitem;
	}

	private bool CheckShowEmpty()
	{
		bool bempty = recordCount <= 0;
		if (emptyTipObject != null)
		{
			emptyTipObject.SetActive(bempty);
		}
		return bempty;
	}

	private void DoScroll(int newHead, bool force)
	{
		preloadList.Clear();
		newHead = newHead < 0 ? 0 : (newHead > recordCount ? recordCount : newHead);
		if (newHead != currFirstIndex || force)
		{
			int curIdx = currFirstIndex;
			_tmpcreateall = false;
			for (int i = newHead; i < newHead + showItemCount; i++)
			{
				if (_tmpcreateall)
				{
					LoadCfg temcfg = new LoadCfg();
					temcfg.mIndex = i;
					temcfg.newHeadIdx = newHead;
					temcfg.curIdx = curIdx;
					temcfg.force = force;
					preloadList.Enqueue(temcfg);
				}
				else
				{
					_tmpcreateall = PreRefresh(i, newHead, curIdx, force) && (buseloadani && i + 1 >= columnCount);
				}
			}
			currFirstIndex = newHead;
		}
	}

	private void SetItemPosition(RectTransform temTrans, int index)
	{
		if (temTrans.parent != transform)
		{
			temTrans.SetParent(transform, false);
		}
		var pos = itemRealPosList[index];
		temTrans.localPosition = pos;
	}

	public void ClearAll()
	{
		for (int i = 0; i < repositionTileList.Count; i++)
		{
			DoPreRender(repositionTileList[i], i);
		}
		for (int i = 0; i < repositionTileIndexList.Count; i++)
		{
			repositionTileIndexList[i] = -1;
		}
		lastHeadIndex = 0;
		preRepositionIntList.Clear();
		preRenderList.Clear();
		repositionTileList.Clear();
    }

	private void CalcColumn()
	{
		if (adaptColumn && minRect != null)
		{
			if (mAttachDir == AttachDirDef.AttachLeft || mAttachDir == AttachDirDef.AttachRight)
			{
				columnCount = (int)((moveContainer.rect.height - mtop - mbottom) / (minRect.height + padding.y));
			}
			else
			{
				columnCount = (int)((moveContainer.rect.width - mleft - mright) / (minRect.width + padding.x));
			}
			columnCount = columnCount <= 1 ? 1 : (columnCount >= 20 ? 20 : columnCount);
		}
	}

	private void CalcShowItem()
	{
		if (mAttachDir == AttachDirDef.AttachLeft || mAttachDir == AttachDirDef.AttachRight)
		{
			showItemCount = ((int)Math.Ceiling(maskView.rect.width / (minRect.width + padding.x)) + 2) * columnCount;
		}
		else
		{
			showItemCount = ((int)Math.Ceiling(maskView.rect.height / (minRect.height + padding.y)) + 2) * columnCount;
		}
		renderPerFrames = showItemCount;
	}

	private void Awake()
	{
		bChangedVal = false;
		if (minItem != null)
		{
			minItem.gameObject.SetActive(false);
		}
		if (moveContainer != null)
		{
			Vector3 tempos = moveContainer.localPosition;
			beginPosition = new Vector3(tempos.x, tempos.y, tempos.z);
		}
	}

	private void Start()
	{
		if (maskView != null)
		{
			beginWidth = maskView.rect.width;
			beginHeight = maskView.rect.height;
		}
		ReCalculate();
		DoScroll(0, true);
	}

	private void Update()
	{
		if (preloadList.Count > 0)
		{
			var totalcnt = columnCount;
			while (preloadList.Count > 0 && --totalcnt >= 0)
			{
				var oneload = preloadList.Dequeue();
				PreRefresh(oneload.mIndex, oneload.newHeadIdx, oneload.curIdx, oneload.force);
			}
			return;
		}

		if (bChangedVal || Math.Abs(beginWidth - maskView.rect.width) > 1 || Math.Abs(beginHeight - maskView.rect.height) > 1)
		{
			bChangedVal = false;
			beginWidth = maskView.rect.width;
			beginHeight = maskView.rect.height;
			changeTag = Math.Min(changeTag, Application.isPlaying ? 10 : 1);
		}
		--changeTag;
		if (changeTag <= 0)
		{
			changeTag = int.MaxValue;
			ReCalculate();
			if (!bEditOperate && Application.isPlaying)
			{
				DoScroll(currFirstIndex, true);
			}
		}

		//if (_luadata != null)
		//{
			currPosition = moveContainer.localPosition;
			dtmove = beginPosition - currPosition;

			cloumIndex = 0;
			for (int i = 0; i < itemRealPosList.Count; i += columnCount)
			{
				var tpos = itemRealPosList[i];
				if ((mAttachDir == AttachDirDef.AttachTop && dtmove.y <= tpos.y) ||
					(mAttachDir == AttachDirDef.AttachBottom && dtmove.y >= tpos.y) ||
					(mAttachDir == AttachDirDef.AttachLeft && dtmove.x >= tpos.x) ||
					(mAttachDir == AttachDirDef.AttachRight && dtmove.x <= tpos.x))
				{
					cloumIndex = i;
				}
			}

			curHeadIndex = -cloumIndex;
			if (curHeadIndex != lastHeadIndex && curHeadIndex <= 0)
			{
				DoScroll(Mathf.Abs(curHeadIndex), false);
			}
			lastHeadIndex = curHeadIndex;
		//}
	}

	private void LateUpdate()
	{
		if (preRepositionIntList.Count > 0)
		{
			RenderItem();
		}
	}

	private void OnDestroy()
	{
        //onItemRender?.Dispose();
        //onItemDispose?.Dispose();
        onItemRender = null;
		onItemDispose = null;
		//_luadata = null;
	}
	#endregion

	private void ChangeScrollDir()
	{
		if (moveContainer != null)
		{
			moveContainer.sizeDelta = Vector2.zero;
			if (mAttachDir == AttachDirDef.AttachTop)
			{
				maskView.pivot = new Vector2(0, 1);
				moveContainer.anchorMin = new Vector2(0, 1);
				moveContainer.anchorMax = new Vector2(1, 1);
				moveContainer.pivot = new Vector2(0, 1);
			}
			else if (mAttachDir == AttachDirDef.AttachBottom)
			{
				maskView.pivot = new Vector2(0, 0);
				moveContainer.anchorMin = new Vector2(0, 0);
				moveContainer.anchorMax = new Vector2(1, 0);
				moveContainer.pivot = new Vector2(0, 0);
			}
			else if (mAttachDir == AttachDirDef.AttachLeft)
			{
				maskView.pivot = new Vector2(0, 1);
				moveContainer.anchorMin = new Vector2(0, 0);
				moveContainer.anchorMax = new Vector2(0, 1);
				moveContainer.pivot = new Vector2(0, 1);
			}
			else
			{
				maskView.pivot = new Vector2(1, 1);
				moveContainer.anchorMin = new Vector2(1, 0);
				moveContainer.anchorMax = new Vector2(1, 1);
				moveContainer.pivot = new Vector2(1, 1);
			}
		}
		ChangeScrollRectScrollDir();
	}

	// 设置ScrollRect的滑动方向
	private void ChangeScrollRectScrollDir()
	{
		if (mScrollComponent != null)
		{
			bool tHorizontal = mAttachDir == AttachDirDef.AttachLeft || mAttachDir == AttachDirDef.AttachRight;
			mScrollComponent.horizontal = m_Interactable && tHorizontal;
			mScrollComponent.vertical = m_Interactable && !tHorizontal;
		}
	}

	public void ReCalculate()
	{
		ChangeScrollDir();
		CalcBounds();
		CalcColumn();
		CalcShowItem();
#if UNITY_EDITOR
		if (bEditOperate || !Application.isPlaying)
		{
			var childrens = new List<TableItem>(GetComponentsInChildren<TableItem>());
			CalcPage(childrens.Count);
			for (int i = 0; i < childrens.Count; i++)
			{
				RectTransform itemRect = childrens[i].GetComponent<RectTransform>();
				itemRect.sizeDelta = new Vector2(minRect.width, itemRect.rect.height);
				SetItemPosition(itemRect, i);
			}
			return;
		}
#endif
		//if (_datalist != null)
		
		CalcPage(datacount);
		onReCalculate?.Invoke();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (enabled)
		{
			bChangedVal = true;
		}
	}
#endif
}
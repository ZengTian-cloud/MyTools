using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class CommonPopWin
{
    public override UILayerType uiLayerType => UILayerType.Pop;
    public override string uiAtlasName => "pop";

    protected override void OnInit()
    {
    }

    protected override void OnOpen()
    {
        base.OnOpen();
      
    }


    protected override void OnClose()
    {
        base.OnClose();
   
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnRegister(bool register)
    {
        base.OnRegister(register);
    }

    partial void btnOK_Click()
    {
        this.Close();
        leftCB?.Invoke();
    }

    partial void btnNO_Click()
    {
        this.Close();
        rightCB?.Invoke();
    }

    private Action leftCB = null;
    private Action rightCB = null;
    public void RegisterClickListen(string title,string content, string left, Action leftCB,string right,Action rightCB)
    {
        txTitle.text = title;
        txContent.text = content;
        btn_left.GetComponentInChildren<TextMeshProUGUI>().text = left;
        btn_right.GetComponentInChildren<TextMeshProUGUI>().text = right;
        btn_left.AddListenerBeforeClear(() => {
            this.Close();
            leftCB?.Invoke(); });
        btn_right.AddListenerBeforeClear(() => {
            this.Close();
            rightCB?.Invoke(); });
        this.leftCB = leftCB;
        this.rightCB = rightCB;
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Managers;
//using System;
//using LitJson;
//using System.Collections.Concurrent;

//public class AbHelp
//{
    
//    public ArrayList abCommonList = new ArrayList();
//    private static volatile AbHelp inst;
//    private static object syncLock = new object();
//    private AbHelp() { }

//    public static AbHelp Inst {
//        get {
//            if (inst == null)
//            {
//                lock(syncLock)
//                {
//                    inst = new AbHelp();
//                    inst.abCommonList.AddRange(GetPreUpdateAb());
//                }
               
//            }
//            return inst;
//        }
       
//    }

//    public ConcurrentDictionary<string ,Boolean> tsingleregister = new ConcurrentDictionary<string , Boolean>();

//    /// <summary>
//    /// ��ȡ������Ҫ���صĹ�����ԴAB��
//    /// </summary>
//    /// <returns></returns>
//    public static ArrayList GetPreUpdateAb() {
//        return new ArrayList(){
//               "zbasefont",
//               "zlaunchui"
//        };
//    }

//    static AbHelp(){

        

//    }

//    public void enqueueload() {
        
//    }

//    /// <summary>
//    /// ���AB��
//    /// </summary>
//    /// <param name="abList"></param>
//    public void checkab(ref ArrayList abList) {

//        if (abList != null && abList.Count > 0) {
//            for (int i = 0; i < abList.Count; i++) {
//                abList[i] = zxsuffix.delassetbundle((string)abList[i]);
//            }
//        }
//    }

//    /// <summary>
//    /// ���AB��
//    /// </summary>
//    /// <param name="temabname"></param>
//    /// <returns></returns>
//    public string checkab(string temabname)
//    {
//        return zxsuffix.delassetbundle(temabname);
//    }

//    /// <summary>
//    /// ��ȡAB��������
//    /// </summary>
//    /// <param name="temabname"></param>
//    /// <returns></returns>
//    public string[] checkdepends(string temabname) {
//        string tname = checkab(temabname);
//        return GameCenter.mIns.m_ResMgr.CheckDependencies(tname, true);
//    }

//    /// <summary>
//    /// ͬ������AB��
//    /// </summary>
//    /// <param name="abList">AB�������б�</param>
//    public void  AddSyncAb(ArrayList abList) {
//        GameCenter.mIns.m_ResMgr.LoadSyncAssetBundle(abList);
//    }


//    /// <summary>
//    /// �첽����AB������Ҫ����δ�ȸ���Ϳ�ʼ����AB��(�ݲ�ʵ��)
//    /// </summary>
//    public void AddASyncAb(Action<string> tcallback, ArrayList abList) {


//        //GameCenter.mIns.m_ResMgr.LoadAsyncAssetBundle(tcallback,abList);

//        GameCenter.mIns.m_ResMgr.LoadIntelligentAssetBundle(tcallback, abList);
        
//    }



//    public void UnloadAb(string[] abList) {
//        GameCenter.mIns.m_ResMgr.UnLoadAssetBundles(false, abList);
//    }

//}

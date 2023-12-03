using System;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;
using Cysharp.Threading.Tasks;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using GameTiJi;

public class HotUpdate : MonoBehaviour
{
    private GameObject parent = null;
    private Action onUpdateComplate;

    private TextMeshProUGUI txtStatus;
    private TextMeshProUGUI txtPercent;
    private Slider sliderStatus;
    public HotUpdate(GameObject obj,Action onUpdateComplate)
    {
        parent = obj;
        this.onUpdateComplate = onUpdateComplate;
        txtStatus = parent.transform.Find("txtbg/txtStatus").GetComponent<TextMeshProUGUI>();
        txtPercent = parent.transform.Find("txtbg/txtpercent").GetComponent<TextMeshProUGUI>();
        sliderStatus = parent.transform.Find("expSlider").GetComponent<Slider>();
    }

    private Dictionary<string,ResourceDownloaderOperation> downloadList = new Dictionary<string,ResourceDownloaderOperation>();
    private int totalPackageCount = 0;//�и��µİ�����
    private int totalDownloadCount = 0;//�����ļ�����
    private long totalDownloadBytes = 0;//�����ļ��ܴ�С
    private int downloadPackageCount = 0;//�����ذ�������
    private long downloadBytes = 0;//�������ܴ�С������
    private int downloadCount = 0;//�������ļ���������

    private void setStatus(int filecount,long downbytes) {

        long tbytes = downloadBytes + downbytes;
        int tcount = downloadCount + filecount;
        txtStatus.text = string.Format("����������Դ��{0}/{1}��", ResourceUnitDisplayFormat(tbytes), ResourceUnitDisplayFormat(totalDownloadBytes));
        float scale = float.Parse(tbytes + "") / float.Parse(totalDownloadBytes + "");
        float percent = (float)Math.Round(scale, 4);
        percent = percent * 100;
        txtPercent.text = percent.ToString()+"%";
        scale = (float)Math.Round(scale, 2);
        sliderStatus.DOValue(scale,0.2f);
    }
    public async UniTask startUpdate()
    {
        Debug.Log("GIANTGAME ��ʼ����ȸ� startUpdate");
        txtStatus.text = "���ڼ�����...";
        //await hotUpdate.checkUpdate();
        totalDownloadCount = 0;
        totalDownloadBytes = 0;
        downloadPackageCount = 0;
        downloadBytes = 0;
        downloadCount = 0;
        downloadList.Clear();
        foreach (var packname in ResourcesManager.Instance.Packages)
        {
            Debug.Log($"GIANTGAME ������ package:{packname}" );
            ResourcePackage resourcePackage = await ResourcesManager.Instance.getPackage(packname);
            await UniTask.Delay(200);

            //await resourcePackage.ClearAllCacheFilesAsync();

            UpdatePackageVersionOperation operation = resourcePackage.UpdatePackageVersionAsync();
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                //����ʧ��
                Debug.LogError("GIANTGAME ����ʧ��" + operation.Error);
                //TODO
                return;
            }

            string m_PackageVersion = operation.PackageVersion;
            Debug.Log($"GIANTGAME ������ ��ȡ��Զ�˰汾�� package:{packname} ��m_PackageVersion��{m_PackageVersion}");
            var operation2 = resourcePackage.UpdatePackageManifestAsync(m_PackageVersion);
            await operation2.ToUniTask();
            Debug.Log($"GIANTGAME ������ ��ȡԶ�˰汾�Ž�� package:{packname} ��m_PackageVersion��{m_PackageVersion} ��EOperationStatus.Succeed��{EOperationStatus.Succeed}");
            if (operation2.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"GIANTGAME ��ȡ�����嵥ʧ�� operation2.Error��{operation2.Error}");
                return;
            }
            Download(packname);

        }

        if (totalPackageCount > 0 && downloadList.Count > 0)
        {
            //�и���
            Debug.Log($"�����½��� �и��� totalPackageCount:{totalPackageCount} downloadList.Count��{downloadList.Count}");
            string title = GameCenter.mIns.m_LanMgr.GetLan("ui_login_update_yes_title");
            string text = string.Format(GameCenter.mIns.m_LanMgr.GetLan("ui_login_update_yes_msg"), ResourceUnitDisplayFormat(totalDownloadBytes));
            GameCenter.mIns.m_UIMgr.PopFullScreenPrefab(new PopFullScreenStyle(title, text, 2, (confirm) =>{
                if (confirm == 1)
                {
                    //����
                    OnDownloadStart();
                }
                else { 
                    //������ ���˳���Ϸ
                    GameCenter.mIns.ExitGame();
                }
            }));
        }
        else
        {
            Debug.Log($"�����½��� ~~~~~~û�и��� totalPackageCount:{totalPackageCount} downloadList.Count��{downloadList.Count} ��ʼ������Ϸ");
            onUpdateComplate();
        }
        
    }

    private async void OnDownloadStart()
    {
        txtStatus.alignment = TextAlignmentOptions.Left;
        setStatus(0, 0);
        txtPercent.gameObject.SetActive(true);

        bool isLoadGame = false;
        foreach (var key in downloadList.Keys)
        {
            if (key.Equals("DefaultPackage"))
                isLoadGame = true;
            Debug.Log($"��ʼ���� package:{key}");
            var downloader = downloadList.GetValueOrDefault(key);
            await UniTask.DelayFrame(1);
            await BeginDownload(downloader);
        }
        if (isLoadGame)
        {
            Debug.Log($"GIANTGAME ������� ���������� ������Ϸ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            GameCenter.mIns.restartGame();
        }
        else
        {
            Debug.Log($"GIANTGAME ������� ����û�и��� ֱ�ӽ���������-------------------------------------------------");
            onUpdateComplate();
        }
    }
    private async void Download(string packagename)
    {
        int downloadingMaxNum = 3;
        int failedTryAgain = 3;
        int timeout = 60;
        var package  = YooAssets.GetPackage(packagename);
        Debug.Log($"GIANTGAME ������� package:{package.PackageName}");
        ResourceDownloaderOperation downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
        //û����Ҫ���ص���Դ
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.LogWarning("GIANTGAME û����Ҫ���ص��ļ�!");
            return;
        }
        //��Ҫ���ص��ļ��������ܴ�С
        totalDownloadCount += downloader.TotalDownloadCount;
        totalDownloadBytes += downloader.TotalDownloadBytes;
        totalPackageCount += 1;
        totalPackageCount++;
        //string size = ResourceUnitDisplayFormat(totalDownloadBytes);
        Debug.Log($"GIANTGAME �и��� package:{package.PackageName} TotalDownloadCount:{downloader.TotalDownloadCount} TotalDownloadBytes:{downloader.TotalDownloadBytes}");
        downloadList.TryAdd(package.PackageName, downloader);
        await UniTask.DelayFrame(1);
    }

    /// <summary>
    /// ��ʼִ������
    /// </summary>
    /// <param name="downloader"></param>
    /// <param name="loadedCallback"></param>
    /// <returns></returns>
    private async UniTask BeginDownload(ResourceDownloaderOperation downloader)
    {
        //ע��ص�����
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        //��������
        downloader.BeginDownload();
        await downloader.ToUniTask();

        //������ؽ��
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //���سɹ�
            Debug.LogError("GIANTGAME �������!");

            downloadBytes = downloader.TotalDownloadBytes;
            downloadCount += downloader.TotalDownloadCount;
            // await StartGame();
        }
        else
        {
            //����ʧ��
            Debug.LogError("GIANTGAME ����ʧ�ܣ�");
        }
    }

    /// <summary>
    /// ��ʼ����
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log($"GIANTGAME ��ʼ���أ��ļ�����{fileName}, �ļ���С��{sizeBytes}");

    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="isSucceed"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("GIANTGAME ����" + (isSucceed ? "�ɹ�" : "ʧ��"));
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        //Debug.Log(string.Format("GIANTGAME �ļ�������{0}, �������ļ�����{1}, �����ܴ�С��{2}, �����ش�С��{3}", totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes));
        //string loadedSize = ResourceUnitDisplayFormat(currentDownloadBytes);
        //string totalSize = ResourceUnitDisplayFormat(totalDownloadBytes);
        setStatus(currentDownloadCount, currentDownloadBytes);
    }

    /// <summary>
    /// ���س���
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.LogError(string.Format("GIANTGAME ���س����ļ�����{0}, ������Ϣ��{1}", fileName, error));
    }

    /// <summary>
    /// ��Դ��С��λ��ʾ����
    /// </summary>
    /// <param name="resBytes"></param>
    /// <returns></returns>
    private string ResourceUnitDisplayFormat(float resBytes)
    {
        float G = resBytes / 1024 / 1024 / 1024;
        float MB = resBytes / 1024 / 1024;
        float KB = resBytes / 1024;
        if (G >= 1)
            return $"{string.Format("{0:F}", G)}G";
        else if (MB >= 0)
            return $"{string.Format("{0:F}", MB)}MB";
        else if (KB >= 0)
            return $"{string.Format("{0:F}", KB)}KB";
        else
            return $"{string.Format("{0:F}", resBytes)}B";
    }
}

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
    private int totalPackageCount = 0;//有更新的包数量
    private int totalDownloadCount = 0;//下载文件总数
    private long totalDownloadBytes = 0;//下载文件总大小
    private int downloadPackageCount = 0;//已下载包的数量
    private long downloadBytes = 0;//已下载总大小，因多包
    private int downloadCount = 0;//已下载文件数，因多包

    private void setStatus(int filecount,long downbytes) {

        long tbytes = downloadBytes + downbytes;
        int tcount = downloadCount + filecount;
        txtStatus.text = string.Format("正在下载资源（{0}/{1}）", ResourceUnitDisplayFormat(tbytes), ResourceUnitDisplayFormat(totalDownloadBytes));
        float scale = float.Parse(tbytes + "") / float.Parse(totalDownloadBytes + "");
        float percent = (float)Math.Round(scale, 4);
        percent = percent * 100;
        txtPercent.text = percent.ToString()+"%";
        scale = (float)Math.Round(scale, 2);
        sliderStatus.DOValue(scale,0.2f);
    }
    public async UniTask startUpdate()
    {
        Debug.Log("GIANTGAME 开始检测热更 startUpdate");
        txtStatus.text = "正在检查更新...";
        //await hotUpdate.checkUpdate();
        totalDownloadCount = 0;
        totalDownloadBytes = 0;
        downloadPackageCount = 0;
        downloadBytes = 0;
        downloadCount = 0;
        downloadList.Clear();
        foreach (var packname in ResourcesManager.Instance.Packages)
        {
            Debug.Log($"GIANTGAME 检测更新 package:{packname}" );
            ResourcePackage resourcePackage = await ResourcesManager.Instance.getPackage(packname);
            await UniTask.Delay(200);

            //await resourcePackage.ClearAllCacheFilesAsync();

            UpdatePackageVersionOperation operation = resourcePackage.UpdatePackageVersionAsync();
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Debug.LogError("GIANTGAME 更新失败" + operation.Error);
                //TODO
                return;
            }

            string m_PackageVersion = operation.PackageVersion;
            Debug.Log($"GIANTGAME 检测更新 获取到远端版本号 package:{packname} ，m_PackageVersion：{m_PackageVersion}");
            var operation2 = resourcePackage.UpdatePackageManifestAsync(m_PackageVersion);
            await operation2.ToUniTask();
            Debug.Log($"GIANTGAME 检测更新 获取远端版本号结果 package:{packname} ，m_PackageVersion：{m_PackageVersion} ，EOperationStatus.Succeed：{EOperationStatus.Succeed}");
            if (operation2.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"GIANTGAME 获取更新清单失败 operation2.Error：{operation2.Error}");
                return;
            }
            Download(packname);

        }

        if (totalPackageCount > 0 && downloadList.Count > 0)
        {
            //有更新
            Debug.Log($"检查更新结束 有更新 totalPackageCount:{totalPackageCount} downloadList.Count：{downloadList.Count}");
            string title = GameCenter.mIns.m_LanMgr.GetLan("ui_login_update_yes_title");
            string text = string.Format(GameCenter.mIns.m_LanMgr.GetLan("ui_login_update_yes_msg"), ResourceUnitDisplayFormat(totalDownloadBytes));
            GameCenter.mIns.m_UIMgr.PopFullScreenPrefab(new PopFullScreenStyle(title, text, 2, (confirm) =>{
                if (confirm == 1)
                {
                    //更新
                    OnDownloadStart();
                }
                else { 
                    //不更新 ，退出游戏
                    GameCenter.mIns.ExitGame();
                }
            }));
        }
        else
        {
            Debug.Log($"检查更新结束 ~~~~~~没有更新 totalPackageCount:{totalPackageCount} downloadList.Count：{downloadList.Count} 开始进入游戏");
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
            Debug.Log($"开始下载 package:{key}");
            var downloader = downloadList.GetValueOrDefault(key);
            await UniTask.DelayFrame(1);
            await BeginDownload(downloader);
        }
        if (isLoadGame)
        {
            Debug.Log($"GIANTGAME 更新完成 主包更新了 重启游戏~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            GameCenter.mIns.restartGame();
        }
        else
        {
            Debug.Log($"GIANTGAME 更新完成 主包没有更新 直接进入主界面-------------------------------------------------");
            onUpdateComplate();
        }
    }
    private async void Download(string packagename)
    {
        int downloadingMaxNum = 3;
        int failedTryAgain = 3;
        int timeout = 60;
        var package  = YooAssets.GetPackage(packagename);
        Debug.Log($"GIANTGAME 检测下载 package:{package.PackageName}");
        ResourceDownloaderOperation downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.LogWarning("GIANTGAME 没有需要下载的文件!");
            return;
        }
        //需要下载的文件总数和总大小
        totalDownloadCount += downloader.TotalDownloadCount;
        totalDownloadBytes += downloader.TotalDownloadBytes;
        totalPackageCount += 1;
        totalPackageCount++;
        //string size = ResourceUnitDisplayFormat(totalDownloadBytes);
        Debug.Log($"GIANTGAME 有更新 package:{package.PackageName} TotalDownloadCount:{downloader.TotalDownloadCount} TotalDownloadBytes:{downloader.TotalDownloadBytes}");
        downloadList.TryAdd(package.PackageName, downloader);
        await UniTask.DelayFrame(1);
    }

    /// <summary>
    /// 开始执行下载
    /// </summary>
    /// <param name="downloader"></param>
    /// <param name="loadedCallback"></param>
    /// <returns></returns>
    private async UniTask BeginDownload(ResourceDownloaderOperation downloader)
    {
        //注册回调方法
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        //开启下载
        downloader.BeginDownload();
        await downloader.ToUniTask();

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //下载成功
            Debug.LogError("GIANTGAME 更新完成!");

            downloadBytes = downloader.TotalDownloadBytes;
            downloadCount += downloader.TotalDownloadCount;
            // await StartGame();
        }
        else
        {
            //下载失败
            Debug.LogError("GIANTGAME 更新失败！");
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log($"GIANTGAME 开始下载：文件名：{fileName}, 文件大小：{sizeBytes}");

    }

    /// <summary>
    /// 下载完成
    /// </summary>
    /// <param name="isSucceed"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("GIANTGAME 下载" + (isSucceed ? "成功" : "失败"));
    }

    /// <summary>
    /// 更新中
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        //Debug.Log(string.Format("GIANTGAME 文件总数：{0}, 已下载文件数：{1}, 下载总大小：{2}, 已下载大小：{3}", totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes));
        //string loadedSize = ResourceUnitDisplayFormat(currentDownloadBytes);
        //string totalSize = ResourceUnitDisplayFormat(totalDownloadBytes);
        setStatus(currentDownloadCount, currentDownloadBytes);
    }

    /// <summary>
    /// 下载出错
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.LogError(string.Format("GIANTGAME 下载出错：文件名：{0}, 错误信息：{1}", fileName, error));
    }

    /// <summary>
    /// 资源大小单位显示工具
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

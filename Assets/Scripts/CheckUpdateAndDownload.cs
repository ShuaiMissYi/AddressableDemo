using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CheckUpdateAndDownload : MonoBehaviour
{

    private string key = "Assets/Textures/kenan.png";

    public RawImage img;


    /// <summary>
    /// 显示下载状态和进度
    /// </summary>
    public Text UpdateText;

    //重试按钮
    public Button RetryBtn;

    private void Start()
    {
        RetryBtn.gameObject.SetActive(false);
        RetryBtn.onClick.AddListener(()=>
        {
            StartCoroutine(IE_DoUpdateAddressadble());
        });
        //默认更新检测一次
        StartCoroutine(IE_DoUpdateAddressadble());
    }

    IEnumerator IE_DoUpdateAddressadble()
    {
        AsyncOperationHandle<IResourceLocator> initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        List<string> catalogsToUpdate = new List<string>();
        //检查是否有新版本
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates();
        checkHandle.Completed += op =>
        {
            Debug.Log("checkHandle.Completed  ");
            catalogsToUpdate.AddRange(op.Result);
        };
        yield return checkHandle;
        Debug.Log($"catalogsToUpdate.Count  {catalogsToUpdate.Count}");
        if (catalogsToUpdate.Count > 0)
        {
            //更新目录
            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
            yield return updateHandle;
            //
            List<IResourceLocator> locators = updateHandle.Result;
            Debug.Log($"locators.count   {locators.Count}");
            foreach (IResourceLocator locator in locators)
            {
                List<object> keys = new();
                keys.AddRange(locator.Keys);
                //获取待下载的文件总大小
                AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(keys.GetEnumerator());
                yield return sizeHandle;
                if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogErrorFormat("GetDownloadSizeAsync Error\n" + sizeHandle.OperationException.ToString());
                    yield break;
                }
                long totalDownloadSize = (long)sizeHandle.Result;
                UpdateText.text = UpdateText.text + "\n DownloadSize: " + totalDownloadSize;
                Debug.Log("DownloadSize :  "+ totalDownloadSize);
                if (totalDownloadSize>0)
                {
                    //下载
                    AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(keys,true);
                    while (!downloadHandle.IsDone)
                    {
                        if (downloadHandle.Status == AsyncOperationStatus.Failed)
                        {
                            Debug.LogErrorFormat("DownloadDependenciesAsync Error\n" + downloadHandle.OperationException.ToString());
                            yield break;
                        }
                        //下载进度
                        float percentage = downloadHandle.PercentComplete;
                        Debug.Log($"已下载： {percentage}");
                        UpdateText.text += $"\n 已下载：{percentage}";
                        yield return null;
                    }
                    if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log("下载完毕！");
                        UpdateText.text += "\n 下载完毕";
                    }
                }
            }
        }
        else
        {
            UpdateText.text += "\n 没有检测到更新";
        }
        //进入游戏
        EnterGame();
    }

    private void EnterGame()
    {
        Debug.Log("进入游戏");

        Addressables.LoadAssetAsync<Texture2D>(key).Completed += (handle) =>
        {
            Texture2D tex = handle.Result;
            img.texture = tex;
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(tex.width, tex.height);
        };

    }










}

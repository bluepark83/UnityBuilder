using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : MonoBehaviour
{
    public static Action<float> ProgressEvent;
    public static Action<bool> CompletionEvent;
    public static AsyncOperationHandle downloadHandle;
    
    async void Start()
    {
        try
        {
            var handle = Addressables.InitializeAsync();
            await handle.Task;

            var downloadSizeAsync = Addressables.GetDownloadSizeAsync("scene");
            await downloadSizeAsync.Task;
            var downloadSize = downloadSizeAsync.Result;

            if (downloadSize > 0)
            {
                StartCoroutine(nameof(DownloadAssets));
            }
            else
            {
                CompletionEvent.Invoke(true);
            }

            Run();
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
    
    IEnumerator DownloadAssets()
    {
        downloadHandle = Addressables.DownloadDependenciesAsync("scene", false);
        float progress = 0;

        while (downloadHandle.Status == AsyncOperationStatus.None)
        {
            var percentageComplete = downloadHandle.GetDownloadStatus().Percent;
            if (percentageComplete > progress * 1.1) // Report at most every 10% or so
            {
                progress = percentageComplete; // More accurate %
                ProgressEvent.Invoke(progress);
            }

            yield return null;
        }

        CompletionEvent.Invoke(downloadHandle.Status == AsyncOperationStatus.Succeeded);
        Addressables.Release(downloadHandle); //Release the operation handle
    }

    void Run()
    {
        Addressables.LoadSceneAsync("dun_cron");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

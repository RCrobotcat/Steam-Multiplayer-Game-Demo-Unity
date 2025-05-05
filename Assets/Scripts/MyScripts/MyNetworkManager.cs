using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public partial class MyNetworkManager : NetworkManager
{
    public FadeInOutScreen fadeinOutScreen;
    public string firstSceneToLoad;
    private string[] scenesToLoad;
    private bool subScenesLoaded;
    private bool isInTransition;
    private bool firstSceneLoaded;

    void Start()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings - 2;
        scenesToLoad = new string[sceneCount];
        for (int i = 0; i < sceneCount; i++)
            scenesToLoad[i] = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i + 2));
    }

    void Update()
    {
        var loadedScenes = SceneManager.GetAllScenes();

        // 如果场景中有 firstSceneToLoad，并且加载了多个 firstSceneToLoad，则卸载最后一个加载的 firstSceneToLoad
        if (loadedScenes.Length > 0)
        {
            int firstSceneCount = 0;
            Scene lastLoadedFirstScene = default;

            foreach (var loadedScene in loadedScenes)
            {
                if (loadedScene.name == firstSceneToLoad)
                {
                    firstSceneCount++;
                    lastLoadedFirstScene = loadedScene;
                }
            }

            if (firstSceneCount > 1)
            {
                UnloadAdditiveScene(lastLoadedFirstScene);
            }
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        fadeinOutScreen.ShowScreenNoDelay();
        if (sceneName == onlineScene)
            StartCoroutine(ServerLoadSubScenes());
    }

    IEnumerator ServerLoadSubScenes()
    {
        foreach (var additiveScene in scenesToLoad)
            yield return SceneManager.LoadSceneAsync(additiveScene,
                new LoadSceneParameters
                {
                    loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D
                });
        subScenesLoaded = true;
    }

    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(sceneName, sceneOperation, customHandling);
        if (sceneOperation == SceneOperation.UnloadAdditive)
            StartCoroutine(UnloadAdditive(sceneName));
        else if (sceneOperation == SceneOperation.LoadAdditive)
            StartCoroutine(LoadAdditiveScene(sceneName));
    }

    IEnumerator LoadAdditiveScene(string sceneName)
    {
        isInTransition = true;
        yield return fadeinOutScreen.FadeIn();
        if (mode == NetworkManagerMode.ClientOnly)
        {
            loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
                yield return null;
        }

        NetworkClient.isLoadingScene = false;
        isInTransition = false;
        OnClientSceneChanged();
        if (!firstSceneLoaded)
        {
            firstSceneLoaded = true;
            yield return new WaitForSeconds(0.6f);
        }
        else
        {
            firstSceneLoaded = false;
            yield return new WaitForSeconds(0.5f);
        }

        yield return fadeinOutScreen.FadeOut();
    }

    IEnumerator UnloadAdditive(string sceneName)
    {
        isInTransition = true;
        yield return fadeinOutScreen.FadeIn();
        if (mode == NetworkManagerMode.ClientOnly)
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
            yield return Resources.UnloadUnusedAssets();
        }

        NetworkClient.isLoadingScene = false;
        isInTransition = false;
        OnClientSceneChanged();
    }

    public void UnloadAdditiveScene(Scene sceneToUnload)
    {
        SceneManager.UnloadSceneAsync(sceneToUnload);
    }
}
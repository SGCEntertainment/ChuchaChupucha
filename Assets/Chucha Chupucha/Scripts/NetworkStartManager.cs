using System;
using Fusion;
using Fusion.Sockets;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkStartManager : MonoBehaviour
{
    [NonSerialized] NetworkRunner _networkRunner;
    [SerializeField] NetworkRunner runnerPrefab;

    [Space(10)]
    [SerializeField] GameObject ui;

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {

        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if (sceneManager == null)
        {
            Debug.Log($"NetworkRunner does not have any component implementing {nameof(INetworkSceneManager)} interface, adding {nameof(NetworkSceneManagerDefault)}.", runner);
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = Guid.NewGuid().ToString(),
            Initialized = initialized,
            SceneManager = sceneManager
        });
    }

    public void ConnectToGame()
    {
        if (TryGetSceneRef(out var sceneRef))
        {
            StartCoroutine(RunnerStartProcess(GameMode.AutoHostOrClient, sceneRef));
        }
    }

    protected bool TryGetSceneRef(out SceneRef sceneRef)
    {
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex < 0 || activeScene.buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            sceneRef = default;
            return false;
        }
        else
        {
            sceneRef = activeScene.buildIndex;
            return true;
        }
    }

    protected IEnumerator RunnerStartProcess(GameMode serverMode, SceneRef sceneRef)
    {
        ui.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _networkRunner = Instantiate(runnerPrefab);
        _networkRunner.name = serverMode.ToString();

        var serverTask = InitializeNetworkRunner(_networkRunner, serverMode, NetAddress.Any(27015), sceneRef, null);

        while (serverTask.IsCompleted == false)
        {
            yield return new WaitForSeconds(1f);
        }

        if (serverTask.IsFaulted)
        {
            Log.Debug($"Unable to start server: {serverTask.Exception}");
            yield break;
        }
    }
}

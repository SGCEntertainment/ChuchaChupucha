using UnityEngine;
using Unity.Netcode;

public class Manager : MonoBehaviour
{
    [SerializeField] GameObject ui;

    void DestroyUI()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ui.SetActive(false);
    }

    public void StartHost()
    {
        if(NetworkManager.Singleton.StartHost())
        {
            DestroyUI();
        }
    }

    public void StartClient()
    {
        if(NetworkManager.Singleton.StartClient())
        {
            DestroyUI();
        }
    }
}

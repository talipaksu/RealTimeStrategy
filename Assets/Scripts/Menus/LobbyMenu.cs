using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;

    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    public void LeaveLobby()
    {
        //sunucu isem
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();

        }//client isem
        else
        {
            NetworkManager.singleton.StopClient();

            //Ana menüye dön
            SceneManager.LoadScene(0);
        }
    }
}

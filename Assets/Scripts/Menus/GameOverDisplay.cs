using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    //GameOverDisplay prefabi oyun sceneında koyuldu
    //Oyun başladığında GameOverHandlerdaki eventi dinleyerek oyun sonunda kazanan ismi gösterecek

    
    [SerializeField] private GameObject gameOverDisplayParent = null;
    //Önyüzde kazanan oyuncuyu gösterecek text mesh pro text nesnesine referans veriyoruz
    [SerializeField] private TMP_Text winnerNameText = null;
    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    //oyun bittiğinde ayrılmak için ihtiyacımız olan button...
    //Unityde GameOverDisplay Prefabinde gerekli OnClick() eventi ataması yapıldı
    public void LeaveGame()
    {
        //sunucu ya da client olduğumuzda farklı 
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            //stop hosting
            //bizi networkmanagerda set ettiğimiz offline scene a yönlendirir
            //tüm clientlar kicklenir
            NetworkManager.singleton.StopHost();
        }
        else
        {
            //stop client
            //bizi networkmanagerda set ettiğimiz offline scene a yönlendirir
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = $"{winner} Has Won!";
        gameOverDisplayParent.SetActive(true);
    }
}

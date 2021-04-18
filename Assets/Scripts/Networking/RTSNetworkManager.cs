using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class RTSNetworkManager : NetworkManager
{

    [SerializeField] private GameObject unitBasePrefab = null;
    //oyunun bitip bitmediği takibini yapacak nesneyi spawn etmek için refere ediyoruz
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    //tüm katılımcıların diğer oyuncuları bilmesi için tüm oyuncuların tutulduğu bir liste oluşturuyoruz
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    //oyun oynanırken başka oyuncuların katılmamasından emin olmak için...
    private bool isGameInProgress = false;

    #region Server

    //oyun başladıysa yeni connection kabul etme, disconnect et
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!isGameInProgress) { return; }

        conn.Disconnect();
    }

    //sunucu bir kullancıyı attığında...
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Remove(player);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
    }

    public void StartGame()
    {
        if (Players.Count < 2) { return; }

        isGameInProgress = true;

        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        //player bağlandığında ona random bir renk ataması yapıyoruz
        player.SetTeamColor(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));

        //eğer partide 1 kullanıcı varsa onu party owner yapıyoruz
        player.SetIsPartyOwner(Players.Count == 1);


    }

    //sahne değiştiği andan hemen sonra tetiklenir.
    //oyun sahnemize geçtikten sonra tetiklenmesini istiyoruz
    public override void OnServerSceneChanged(string sceneName)
    {
        //oyun sahnemizden birinin açıldığını kontrol ediyoruz
        //SceneManager Unitynin built in classlarından birisi
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            //sunucuda GameOverHandler tipinden bir obje spawn ediyoruz
            //herhangi bir connection ile ilişkilendirmiyoruz ki herkes buna erişsin
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach (RTSPlayer player in Players)
            {
                //sunucuda bir UnitBase örneği oluştur
                GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);

                //sunucuda oluşan UnitBase örneğini player ile ilişkilendir connection üzerinden
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }

        }
    }
    #endregion

    #region Client

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion





}

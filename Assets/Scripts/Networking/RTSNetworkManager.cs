using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitSpawnerPrefab = null;
    //oyunun bitip bitmediği takibini yapacak nesneyi spawn etmek için refere ediyoruz
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        //sunucuda bir UnitSpawner örneği oluştur
        GameObject unitSpawnerInstance = Instantiate(unitSpawnerPrefab, conn.identity.transform.position, conn.identity.transform.rotation);

        //sunucuda oluşan UnitSpawner örneğini player ile ilişkilendir connection üzerinden
        NetworkServer.Spawn(unitSpawnerInstance, conn);
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
        }
    }
}

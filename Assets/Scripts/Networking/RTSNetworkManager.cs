using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitSpawnerPrefab = null;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        //sunucuda bir UnitSpawner örneği oluştur
        GameObject unitSpawnerInstance = Instantiate(unitSpawnerPrefab, conn.identity.transform.position, conn.identity.transform.rotation);

        //sunucuda oluşan UnitSpawner örneğini player ile ilişkilendir connection üzerinden
        NetworkServer.Spawn(unitSpawnerInstance, conn);
    }
}

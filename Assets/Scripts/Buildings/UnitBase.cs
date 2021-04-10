using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health = null;

    //playerin öldüğünü anlamak için. sunucu tarafında fırlatılacak bir event tanımlıyoruz. bu event parametre olarak client id alıyor.
    public static event Action<int> ServerOnPlayerDie;

    //UnitBase in create/destroy edildiğinde fırlatılacak eventler. Ki diğer classlardan yakalanabilsin
    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;

        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBaseDespawned?.Invoke(this);

        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);
        NetworkServer.Destroy(this.gameObject);
    }

    #endregion

    #region Client

    #endregion
}

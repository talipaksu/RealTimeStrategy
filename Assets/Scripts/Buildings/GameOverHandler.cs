using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    //Oyunun bittiğini anlamak için UnitBase'leri takip etmesi gerek
    //Ayrıca networkde spawnlanması gerek. Unityde bir GameObject'e bağladık
    //Herhangi bir playerdan bağımsız olduğu için, bağımsız bir GameObject e bağlanmalı
    //spawn işini RTSNetworkManager'da yapıyoruz

    private List<UnitBase> bases = new List<UnitBase>();

    #region Server
    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        //hala birden fazla oyuncu varsa fonksiyondan çık
        if (bases.Count != 1) { return; }

        Debug.Log("Game Over");
    }

    #endregion

    #region Client

    #endregion
}

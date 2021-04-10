using System;
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

    //Oyun bitince kazanan oyuncuyu göstermek adına client tarafında fırlatılacak bir event tanımlıyoruz.
    //kazanan oyuncunun adını parametre alacak
    public static event Action<String> ClientOnGameOver;


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

        //Sunucunun clientlara oyunun bittiğini bildirmesi gerek
        int playerId = bases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerId}");
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        //bu eventi UI yakalayarak ekrana kimin kazandığını bastıracak.
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}

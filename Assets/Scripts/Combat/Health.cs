using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    //currentHealth bu obje için tüm clientlarda senkron olmalı
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currenthHealth;

    public event Action ServerOnDie;

    //health değişikliklerinde fırlatılacak event, ilk parametre current health ikinci parametre max health
    //clientta fırlatılacak
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    //obje oluşturulduğunda sunucuda healthı max olmalı
    public override void OnStartServer()
    {
        currenthHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        //destroy edilen base in bize ait olup olmadığını kontrol ediyoruz
        if (connectionToClient.connectionId != connectionId) { return; }

        DealDamage(currenthHealth);
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currenthHealth == 0) { return; }

        //her hasarda damage azalt. 0 dan küçükse healthı 0 yap
        currenthHealth = Mathf.Max(currenthHealth - damageAmount, 0);

        if (currenthHealth != 0) { return; }

        //canın tükendiğini gerekli yerlere bildirmek için event fırlat
        ServerOnDie?.Invoke();
    }

    #endregion

    #region Client

    //health çalıştığında tetiklenecek method
    //syncvar değişkenimize hook olarak verdik
    //event tetikleyecek ve HealthDisplay scriptimizden yakalayacağız
    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}

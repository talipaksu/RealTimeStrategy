using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    //currentHealth bu obje için tüm clientlarda senkron olmalı
    [SyncVar]
    private int currenthHealth;

    public event Action ServerOnDie;

    #region Server

    //obje oluşturulduğunda sunucuda healthı max olmalı
    public override void OnStartServer()
    {
        currenthHealth = maxHealth;
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

        Debug.Log("We died!");
    }

    #endregion

    #region Client

    #endregion
}

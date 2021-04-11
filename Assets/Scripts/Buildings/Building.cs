using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Building : NetworkBehaviour
{
    //building iconlarına referans
    [SerializeField] private Sprite icon = null;
    //buildingleri networkta göndermek amaçlı unique id tutacağız 1 2 3 4 gibi. -1 invalid değerdir.
    //böylece sunucuya clientın hangi tip bir building inşa etmek istediğini söyleyeceğiz
    [SerializeField] private int id = -1;
    //build inşa etmek için gerekli tutar
    [SerializeField] private int price = 100;

    //sunucu tarafında Building örneğinin oluşturulduğu zaman fırlatılacak event
    public static event Action<Building> ServerOnBuildingSpawned;
    //sunucu tarafında Building örneğinin kaldırıldığı zaman fırlatılacak event
    public static event Action<Building> ServerOnBuildingDespawned;

    //client tarafında Building örneğinin oluşturulduğu zaman fırlatılacak event
    public static event Action<Building> AuthorityOnBuildingSpawned;
    //client tarafında Building örneğinin yokedildiği zaman fırlatılacak event
    public static event Action<Building> AuthorityOnBuildingDespawned;

    //başka claslardan erişim ihtiyacı doğacak. getterlarımızı oluşturuyoruz
    public Sprite GetIcon()
    {
        return this.icon;
    }

    public int GetId()
    {
        return this.id;
    }

    public int GetPrice()
    {
        return this.price;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);
    }

    #endregion

    #region Client

    //clientta çalışan Start fonksiyonu (yetkimiz varsa)
    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    //clientta çalışan Stop fonksiyonu
    public override void OnStopClient()
    {
        if (!hasAuthority) { return; }
        AuthorityOnBuildingDespawned?.Invoke(this);
    }

    #endregion
}

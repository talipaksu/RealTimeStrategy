using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System;

public class Unit : NetworkBehaviour
{
    [SerializeField] private int resourceCost = 10;
    //nesnenin yokedildiğinde olacakları yönetmek için healthtan bir referans oluşturuyoruz
    [SerializeField] private Health health = null;
    //UnitCommandGiver sınıfından Unit nesnesi üzerinden UnitMovement'taki fonksiyonlara erişmek için referans oluştur
    [SerializeField] private UnitMovement unitMovement = null;
    //UnitCommandGiver sınıfından Unit nesnesi üzerinden Targeter'daki fonksiyonlara erişmek için referans oluştur
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;
    //sunucu tarafında Unit örneğinin oluşturulduğu zaman fırlatılacak event
    public static event Action<Unit> ServerOnUnitSpawned;
    //sunucu tarafında Unit örneğinin kaldırıldığı zaman fırlatılacak event
    public static event Action<Unit> ServerOnUnitDespawned;
    //client tarafında Unit örneğinin oluşturulduğu zaman fırlatılacak event
    public static event Action<Unit> AuthorityOnUnitSpawned;
    //client tarafında Unit örneğinin kaldırıldığı zaman fırlatılacak event
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public int GetResourceCost()
    {
        return this.resourceCost;
    }

    public UnitMovement GetUnitMovement()
    {
        return this.unitMovement;
    }

    public Targeter GetTargeter()
    {
        return this.targeter;
    }

    #region Server
    //sunucuda çalışan Start fonksiyonu
    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
        //healtta tetiklenen serverOnDie ı dinlemeye başla
        health.ServerOnDie += ServerHandleDie;
    }
    //sunucuda çalışan Stop fonksiyonu
    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        ServerOnUnitDespawned?.Invoke(this);
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    #endregion

    #region Client
    //clientta çalışan Start fonksiyonu (yetkimiz varsa)
    //OnStartClient yapamayız çünkü Client + Host da olabiliriz. bu nedenle sadece authority check yapıyoruz
    public override void OnStartAuthority()
    {
        //!isClientOnly && !hasAuthority yi yukarıdaki Client + Host olma ihtimaline karşı kaldırdık. OnStartAuthority bu kontrolü bizim yerimize yapıyor
        AuthorityOnUnitSpawned?.Invoke(this);
    }
    //clientta çalışan Stop fonksiyonu
    public override void OnStopClient()
    {
        //!isClientOnly yi kaldırdık çünkü host + client iken düzgün çalışmayacaktı
        if (!hasAuthority) { return; }
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        //client tarafında olduğu için authority check yap
        if (!hasAuthority) { return; }
        //onSelected UnityEventini tetikle. Unityde highlight ile ilişkilendirmiştik. bu highlightı enable yap
        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        //client tarafında olduğu için authority check yap
        if (!hasAuthority) { return; }
        //onDeselected UnityEventini tetikle. Unityde highlight ile ilişkilendirmiştik. bu highlightı disable yap
        onDeselected?.Invoke();
    }

    #endregion 
}

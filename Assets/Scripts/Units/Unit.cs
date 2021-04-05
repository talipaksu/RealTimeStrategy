using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System;

public class Unit : NetworkBehaviour
{
    //UnitCommandGiver sınıfından Unit nesnesi üzerinden UnitMovement'taki fonksiyonlara erişmek için referans oluştur
    [SerializeField] private UnitMovement unitMovement = null;
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
    public UnitMovement GetUnitMovement()
    {
        return this.unitMovement;
    }

    #region Server
    //sunucuda çalışan Start fonksiyonu
    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
    }
    //sunucuda çalışan Stop fonksiyonu
    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
    }

    #endregion

    #region Client
    //clientta çalışan Start fonksiyonu
    public override void OnStartClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }
        AuthorityOnUnitSpawned?.Invoke(this);
    }
    //clientta çalışan Stop fonksiyonu
    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }
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

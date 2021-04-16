using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    //nesnenin yokedildiğinde olacakları yönetmek için healthtan bir referans oluşturuyoruz
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;
    //unit spawner üstündeki timer halkası ve kuyrukta spawn edilmeyi bekleyen unit sayısı için referanslarımızı oluşturuyoruz
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxUnitQueue = 5;
    //spawn olan unitlerin üstüste binmemesi için bir range değişkeni oluşturuyoruz
    [SerializeField] private float spawnMoveRange = 7f;
    //spawn etme süresi
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdate))]
    private int queuedUnits;

    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        //healtta tetiklenen serverOnDie ı dinlemeye başla
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) { return; }

        //sunucuda bir Unit örneği oluştur
        GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);
        //sunucuda oluşturmuş olduğum Unit örneğini, NetworkBehaviour un sağladığı connectionToClient connection bilgisi ile kullanıcı ile ilişkilendirerek
        //tüm clientlarda oluştur.
        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;

        spawnOffset.y = unitSpawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();

        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        queuedUnits--;

        unitTimer = 0f;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(this.gameObject);
    }


    [Command]
    private void CmdSpawnUnit()
    {
        if (queuedUnits == maxUnitQueue) { return; }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.GetResources() < unitPrefab.GetResourceCost()) { return; }

        queuedUnits++;

        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());

    }


    #endregion

    #region Client

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //eğer left mouse button ile UnitSpawner a basıldı ise gerekli aksiyonları al
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        //clientta çalıştığı için başka playerların nesnesine müdahale etmemek adına Authority kontrolü yap
        if (!hasAuthority) { return; }

        //Client, sunucuya "benim için unit oluştur" diyor
        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdate(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    #endregion

}

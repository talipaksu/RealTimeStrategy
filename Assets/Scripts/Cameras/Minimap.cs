using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6f;

    private Transform playerCameraTransform;

    private void Update()
    {
        if (playerCameraTransform != null) { return; }

        if (NetworkClient.connection.identity == null) { return; }

        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        //tıkladığımız nokta, mousePos, Screen Point'tir
        //tıkladığımız nokta minimap in içinde mi?
        //ScreenPointToLocalPointInReactangle; mesela rectangle 50x50 ise fonksiyon bize 1 ile 50 arası olarak tıkladığımız noktayı verir
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, mousePos, null, out Vector2 localPoint)) { return; }

        //bize minimap içinde tıkladığımız nokta için bir oran verir. böylece minimapın boyutu pixel cinsinden ne olursa olsun etkilenmez
        Vector2 lerp = new Vector2((localPoint.x - minimapRect.rect.x) / minimapRect.rect.width, (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        //Mathf.Lerp, 3. parametredeki değeri, 1. ve 2. parametre arasında oranlar
        //böylece minimap içinde tıkladığımız noktayı oyun sahnesinde oranlı bir şekilde elde edebiliriz.
        Vector3 newCameraPos = new Vector3(Mathf.Lerp(-mapScale, mapScale, lerp.x), playerCameraTransform.position.y, Mathf.Lerp(-mapScale, mapScale, lerp.y));

        //offset değeri bırakıyoruz. tıkladığımız yer bam diye ortalanmasın diye
        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);

    }


}

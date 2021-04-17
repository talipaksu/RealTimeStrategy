using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

//tüm logic Client taraflı
public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    //kameramızı mouse ile hareket ettirmek istediğimizde kullanacağız bu değişkeni
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 previousInput;

    //yeni unity input system ı kullanıyoruz
    //gerekli maplemeleri unityde yaptık ve bize auto generated bir Controls scripti üretti
    private Controls controls;

    public override void OnStartAuthority()
    {
        //unityde disable olan Player prefabi altındaki Camera nesnemizi aktif ediyoruz
        //authority içinde yaptık çünkü sadece bizim playerımızın kamerasının aktif olmasını istiyoruz.
        playerCameraTransform.gameObject.SetActive(true);

        controls = new Controls();

        //unityde tanımladığımız MoveCamera actionuna erişiyoruz. wasd ve arrow keyler tanımlandı içerisinde
        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;
        //yeni input system ile eventlara unsubscribe olma ihtiyacı yok

        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        //diğer playerların kamerasını kontrol etmemek adına...
        //ve oyuna focuslu değilsek kontrol etmemek adına...
        if (!hasAuthority || !Application.isFocused) { return; }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        //keyboard input yoksa
        if (previousInput == Vector2.zero)
        {
            //mouse değerlerini oku
            Vector3 cursorMovement = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            //dikey hareket

            //mouse screen sınırları içerisinde mi kontrol et
            if (cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                cursorMovement.z += 1;
            }
            else if (cursorPosition.y <= screenBorderThickness)
            {
                cursorMovement.z -= 1;
            }

            //yatay hareket
            if (cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                cursorMovement.x += 1;
            }
            else if (cursorPosition.x <= screenBorderThickness)
            {
                cursorMovement.x -= 1;
            }

            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        //keyboard input varsa
        else
        {
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        //gameplay ekranı sınırları içerisinde olduğundan emin ol
        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenXLimits.y);

        playerCameraTransform.position = pos;
    }

    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }
}

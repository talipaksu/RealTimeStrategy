using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

//tüm logic Client taraflı
public class CameraController : NetworkBehaviour
{
    //Player prefabına eklediğimiz cinemachine virtual camera componentine referans veriyoruz
    [SerializeField] private Transform playerCameraTransform = null;
    //kameranın hareket hızı
    [SerializeField] private float speed = 20f;
    //kameramızı mouse ile hareket ettirmek istediğimizde kullanacağız bu değişkeni
    //mouseumuzu ekranın köşelerine getireceğimiz mesafeyi temsil eder, köşelere screenBorderThickness kala kamera hareket etmeye başlar
    [SerializeField] private float screenBorderThickness = 10f;

    //ekran limitlerini belirliyoruz. inspector alanından set edilecek
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    //inputumuz değiştiğinde onu Vector2 değişkeni şeklinde saklayacağız
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

        //unityde tanımladığımız Player Map i altındaki MoveCamera actionuna erişiyoruz(yeni input system) wasd ve arrow keyler tanımlandı içerisinde
        //inputları her framede okumayacağız. eventları dinleyeceğiz
        //performed butona tıklandığında, cancelled butondan çekildiğinde tetiklenir
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
            //mouse ekranın üst kenarına screenBorderThickness kadar yakın mı?
            if (cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                //yukarı hareket et
                cursorMovement.z += 1;
            }
            //ekranın en altı zaten sıfırdır. mouse screenBorderThickness tan küçükse ekranın alt kenarındadır
            else if (cursorPosition.y <= screenBorderThickness)
            {
                //aşağı hareket et
                cursorMovement.z -= 1;
            }

            //yatay hareket
            //mouse ekranın sağ kenarına screenBorderThisness kadar yakın mı?
            if (cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                //sağa hareket et
                cursorMovement.x += 1;
            }
            //mouse ekranın sol kenarına screenBorderThisness kadar yakın mı?
            else if (cursorPosition.x <= screenBorderThickness)
            {
                //sola hareket et
                cursorMovement.x -= 1;
            }

            //frame sayısı bağımsız bir şekilde kameranın pozisyonunu hareket ettiriyoruz
            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        //keyboard input varsa
        else
        {
            //y si zaten sıfır olacak.
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        //gameplay ekranı sınırları içerisinde olduğundan emin ol
        //kameranın sonsuz uzaklığa gitmemesi için;
        //Mathf.Clamp ; eğer 1. parametre, 2. ve 3. parametrenin arasında ise 1. parametre değerini döndürür
        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = pos;
    }

    //input değerimizi previousInput değişkenine atıyoruz
    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }
}

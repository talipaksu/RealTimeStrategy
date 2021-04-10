using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private Image healthBarImage = null;

    private void Awake()
    {
        //nesne oluştuğunda ClientOnHealthUpdated eventini dinlemeye başla
        health.ClientOnHealthUpdated += HandleHealthUpdated;
    }

    private void OnDestroy()
    {
        //nesne yok edildiğinde ClientOnHealthUpdated eventini dinlemeyi bırak
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    //mouse objenin üstüne geldiğinde healthbarı görmek için...
    private void OnMouseEnter()
    {
        healthBarParent.SetActive(true);
    }

    //mouse objenin üstünden çekilince healthbarı gizlemek için...
    private void OnMouseExit()
    {
        healthBarParent.SetActive(false);
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        //fillAmount değeri 0 ve 1 arasında bir değer alır.
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
    }
}

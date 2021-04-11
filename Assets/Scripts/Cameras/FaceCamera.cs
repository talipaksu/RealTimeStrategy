using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//attach edildiği nesneyi her daim kameraya döndürür. 
//örneğin unitlerin üstündeki HealthDisplay UI ları
public class FaceCamera : MonoBehaviour
{
    private Transform mainCameraTransform;


    private void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    //every frame after Update()
    private void LateUpdate()
    {
        //her daim rotation kameraya dönük olur
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward, mainCameraTransform.rotation * Vector3.up);
    }
}

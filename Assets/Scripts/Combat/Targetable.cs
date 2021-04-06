using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


//Target olabilecek objelere attach edeceğiz
public class Targetable : NetworkBehaviour
{
    //inspectordan vereceğimiz bir point olacak. atılacak mermilerin gideceği konumu göstermek adına
    [SerializeField] private Transform aimAtPoint = null;

    public Transform GetAimAtPoint()
    {
        return this.aimAtPoint;
    }
}

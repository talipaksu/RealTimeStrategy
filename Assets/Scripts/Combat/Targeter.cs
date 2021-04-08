using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//Tüm ateş edecek Unitlere attach edilecek
public class Targeter : NetworkBehaviour
{
    //hedef objemizi tutmak için referans yaratıyoruz
    private Targetable target;

    //UnitMovement classından erişip targetı takip etmek için kullanacağız getterı

    public Targetable GetTarget()
    {
        return this.target;
    }

    #region Server

    //Client, sunucuya hedef almak istediği targetı bildirir
    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        //targetGameObject'in Targetable bir obje olduğundan emin oluyoruz, eğer Targetable ise onu newTarget ismine refere ediyoruz
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        target = newTarget;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    #endregion

    #region Client

    #endregion

}

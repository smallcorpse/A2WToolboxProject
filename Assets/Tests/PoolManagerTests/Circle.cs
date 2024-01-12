using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using A2W;

public class Circle : MonoBehaviour
{
    private void OnEnable()
    {
        Invoke("ReleaseSelf", 5f);
    }

    public void ReleaseSelf()
    {
        PoolManager.ReleaseObject(this.gameObject);
    }
}

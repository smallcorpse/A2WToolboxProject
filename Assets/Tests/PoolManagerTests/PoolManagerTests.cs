using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using A2W;

public class PoolManagerTests : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] AssetReference reference;
    [SerializeField] string address;

    private void Awake()
    {
        PoolManager.WarmPool(prefab, 100);
        PoolManager.WarmPool(reference, 100);
        PoolManager.WarmPool(address, 100);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            PoolManager.SpawnObject(prefab, GetRandomPositionInScreen(), Quaternion.identity);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            PoolManager.SpawnObject(reference, GetRandomPositionInScreen(), Quaternion.identity);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            PoolManager.SpawnObject(address, GetRandomPositionInScreen(), Quaternion.identity);
        }
    }

    private Vector3 GetRandomPositionInScreen()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 10));
    }

    private Vector3 GetRandomPositionOutScreen(int w)
    {
        float rx = Random.Range(-2 * w, Screen.width + 2 * w);
        float ry = Random.Range(-2 * w, Screen.height + 2 * w);
        float x = 0;
        float y = 0;
        if (Random.value > 0.5f)
        {
            x = rx < Screen.width * 0.5f ? Mathf.Clamp(rx, -w, 0) : Mathf.Clamp(rx, Screen.width, Screen.width + w);
            y = ry;
        }
        else
        {
            x = rx;
            y = ry < Screen.height * 0.5f ? Mathf.Clamp(ry, -w, 0) : Mathf.Clamp(ry, Screen.height, Screen.height + w);
        }
        return Camera.main.ScreenToWorldPoint(new Vector3(x, y, 0));
    }
}

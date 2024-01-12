using System;
using System.Collections.Generic;
using MonsterLove.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace A2W
{
	public class PoolManager : SceneSingleton<PoolManager>
	{
		public bool logStatus;
		public Transform root;

		private Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup;
		private Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup;

		private Dictionary<object, GameObject> addressMap;

		private bool dirty = false;

		private void init()
		{
			prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
			instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();

			addressMap = new Dictionary<object, GameObject>();
		}

		private void logUpdate()
		{
			if (logStatus && dirty)
			{
				printStatus();
				dirty = false;
			}
		}

		private async void warmPool(object key, int size)
        {
			if (prefabLookup is null)
			{
				init();
			}

			if (addressMap.ContainsKey(key) is false)
            {
				addressMap.Add(key, await Addressables.LoadAssetAsync<GameObject>(key).Task);
			}
			warmPool(addressMap[key], size);
        }

		private void warmPool(GameObject prefab, int size)
		{
            if (prefabLookup is null)
            {
                init();
            }

            if (prefabLookup.ContainsKey(prefab))
			{
				throw new Exception("Pool for prefab " + prefab.name + " has already been created");
			}
			var pool = new ObjectPool<GameObject>(() => { return instantiatePrefab(prefab); }, size);
			prefabLookup[prefab] = pool;

			dirty = true;
		}

		private GameObject spawnObject(object key)
        {
			return spawnObject(key, Vector3.zero, Quaternion.identity);
		}

		private GameObject spawnObject(GameObject prefab)
		{
			return spawnObject(prefab, Vector3.zero, Quaternion.identity);
		}

        private GameObject spawnObject(object key, Vector3 position, Quaternion rotation)
        {
			if (addressMap.ContainsKey(key) is false)
            {
				throw new Exception($"Can not find {key} in pool, Try warm pool.");
            }
			return spawnObject(addressMap[key], position, rotation);
		}

        private GameObject spawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			if (!prefabLookup.ContainsKey(prefab))
			{
				WarmPool(prefab, 1);
			}

			var pool = prefabLookup[prefab];

			var clone = pool.GetItem();
			clone.transform.SetPositionAndRotation(position, rotation);
			clone.SetActive(true);

			instanceLookup.Add(clone, pool);
			dirty = true;
			return clone;
		}

		private void releaseObject(GameObject clone)
		{
			clone.SetActive(false);

			if (instanceLookup.ContainsKey(clone))
			{
				instanceLookup[clone].ReleaseItem(clone);
				instanceLookup.Remove(clone);
				dirty = true;
			}
			else
			{
				Debug.LogWarning("No pool contains the object: " + clone.name);
			}
		}


		private GameObject instantiatePrefab(GameObject prefab)
		{
			var go = Instantiate(prefab) as GameObject;
			//if (root != null) go.transform.parent = root;
			if (root != null) go.transform.SetParent(root, false);
			go.SetActive(false);
			return go;
		}

		private void printStatus()
		{
			foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyVal in prefabLookup)
			{
				Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key.name, keyVal.Value.CountUsedItems, keyVal.Value.Count));
			}
		}

		#region Static API

		public static void Init()
        {
			instance.init();
        }

		public static void WarmPool(GameObject prefab, int size)
		{
			instance.warmPool(prefab, size);
		}

		public static void WarmPool(object key, int size)
        {
			instance.warmPool(key, size);
        }

		public static GameObject SpawnObject(GameObject prefab)
		{
			return instance.spawnObject(prefab);
		}

		public static GameObject SpawnObject(object key)
        {
			return instance.spawnObject(key);
        }

		public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			return instance.spawnObject(prefab, position, rotation);
		}

		public static GameObject SpawnObject(object key, Vector3 position, Quaternion rotation)
        {
			return instance.spawnObject(key, position, rotation);
        }

		public static void ReleaseObject(GameObject clone)
		{
			instance.releaseObject(clone);
		}

		#endregion
	}
}





using System;
using System.Collections.Generic;
using MonsterLove.Collections;
using UnityEngine;

namespace A2W
{
	public class PoolManager : MonoBehaviour
	{
		public bool logStatus;
		public Transform root;

		private Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup;
		private Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup;

		private bool dirty = false;

		public void Init()
		{
			prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
			instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
		}

		public void WarmPool(GameObject prefab, int size)
		{
            if (prefabLookup is null)
            {
                Init();
            }

            if (prefabLookup.ContainsKey(prefab))
			{
				throw new Exception("Pool for prefab " + prefab.name + " has already been created");
			}
			var pool = new ObjectPool<GameObject>(() => { return instantiatePrefab(prefab); }, size);
			prefabLookup[prefab] = pool;

			dirty = true;
		}

		public GameObject SpawnObject(GameObject prefab)
		{
			return SpawnObject(prefab, Vector3.zero, Quaternion.identity);
		}

        public GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
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

		public void ReleaseObject(GameObject clone)
		{
			clone.SetActive(false);
			if (root != null) clone.transform.SetParent(root, false);

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

		private void logUpdate()
		{
			if (logStatus && dirty)
			{
				printStatus();
				dirty = false;
			}
		}

		private void printStatus()
		{
			foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyVal in prefabLookup)
			{
				Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key.name, keyVal.Value.CountUsedItems, keyVal.Value.Count));
			}
		}

	}
}





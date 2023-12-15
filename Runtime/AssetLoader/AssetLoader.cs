using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace A2W
{
    public class AssetLoader : Singleton<AssetLoader>
    {
        public async Task<T> InstantiateAsync<T>(AssetReference assetReference)  where T : Component
        {
            GameObject p = await Addressables.LoadAssetAsync<GameObject>(assetReference).Task;
            GameObject go = Instantiate(p);
            return go.GetComponent<T>();
        }

        public async Task<T> Load<T>(AssetReference assetReference)
        {
            return await Addressables.LoadAssetAsync<T>(assetReference).Task;
        }
    }
}



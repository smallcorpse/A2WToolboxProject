using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using System.Threading.Tasks;

namespace A2W
{
    public class AssetsLoader : Singleton<AssetsLoader>
    {
        public string PackageVersion { get; private set; }
        public bool PackageLoaded { get; private set; }

        public async UniTask Init()
        {
            YooAssets.Initialize();

            // 创建默认的资源包
            var package = YooAssets.CreatePackage("DefaultPackage");

            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);

            await InitPackage(package);
            await RequestPackageVersionAsync(package);
            await UpdatePackageManifest(package);

            PackageLoaded = true;
        }

        /// <summary>
        /// 加载游戏对象资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> LoadPrefab<T>(string assetName) where T : MonoBehaviour
        {
            //Logger.Info("Load Resource type {0} : {1}", typeof(T).Name, assetName);
            AssetHandle handle = YooAssets.LoadAssetAsync<GameObject>(assetName);
            await handle.Task;
            T component = handle.InstantiateSync().GetComponent<T>();
            if (component == null)
            {
                throw new System.Exception("Prefab resource " + assetName + " not has component " + typeof(T).Name);
            }

            return component;
        }

        /// <summary>
        /// 加载游戏对象资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> LoadAsset<T>(string assetName, bool isInstantiate = true) where T : Object
        {
            //Logger.Info("Load Resource type {0} : {1}", typeof(T).Name, assetName);
            if (typeof(T) == typeof(GameObject) && isInstantiate)
            {
                AssetHandle handle = YooAssets.LoadAssetAsync<GameObject>(assetName);
                await handle.Task;
                return handle.InstantiateSync() as T;
            }
            else
            {
                AssetHandle handle = YooAssets.LoadAssetAsync<T>(assetName);
                await handle.Task;
                return handle.AssetObject as T;
            }
        }

        private async UniTask RequestPackageVersionAsync(ResourcePackage package)
        {
            var operation = package.RequestPackageVersionAsync();
            await operation.ToUniTask();

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                PackageVersion = operation.PackageVersion;
                Debug.Log($"Request package Version : {PackageVersion}");
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }

        private async UniTask UpdatePackageManifest(ResourcePackage package)
        {
            var operation = package.UpdatePackageManifestAsync(PackageVersion);
            await operation.ToUniTask();

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }

#if UNITY_EDITOR
        private async UniTask InitPackage(ResourcePackage package)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = editorFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
            await initOperation.ToUniTask();

            if (initOperation.Status == EOperationStatus.Succeed)
                Debug.Log("资源包初始化成功！");
            else
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }

#elif UNITY_STANDALONE_WIN
        private async UniTask InitPackage(ResourcePackage package)
        {
            // 尝试使用离线模式初始化
            var initParameters = new OfflinePlayModeParameters();
            
            // 即使没有内置文件也尝试初始化
            var initOperation = package.InitializeAsync(initParameters);
            await initOperation.ToUniTask();

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
            }
            else
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
                
                // 如果离线模式失败，尝试使用联机模式
                Debug.Log("尝试使用联机模式初始化...");
                var hostPlayModeParameters = new HostPlayModeParameters();
                hostPlayModeParameters.QueryServices = new GameQueryServices();
                hostPlayModeParameters.DefaultHostServer = GetHostServerURL();
                hostPlayModeParameters.FallbackHostServer = GetHostServerURL();
                
                var onlineInitOperation = package.InitializeAsync(hostPlayModeParameters);
                await onlineInitOperation.ToUniTask();
                
                if (onlineInitOperation.Status == EOperationStatus.Succeed)
                {
                    Debug.Log("联机模式资源包初始化成功！");
                }
                else
                {
                    Debug.LogError($"联机模式资源包初始化失败：{onlineInitOperation.Error}");
                }
            }
        }

        private string GetHostServerURL()
        {
            // 这里返回你的资源服务器地址
            // 可以根据需要从配置文件中读取或使用硬编码地址
            return "http://your-resource-server.com/"; // 替换为你的实际服务器地址
        }

        private class GameQueryServices : IQueryServices
        {
            public bool QueryStreamingAssets(string fileName)
            {
                // 这里可以自定义逻辑判断文件是否在StreamingAssets中
                // 如果选择了None选项，可以返回false让系统从服务器下载
                return false;
            }
        }
#endif
    }
}
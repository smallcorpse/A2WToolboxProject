using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using A2W;

public class SceneLoaderTest : MonoBehaviour
{
    [SerializeField] string sceneName;
    [SerializeField] Button button;
    [SerializeField] AnimatorTransition transitionPrefab;

    private void Awake()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            var transition = Instantiate<AnimatorTransition>(transitionPrefab);
            SceneLoader.instance.LoadScene(sceneName, transition, false, 1f);
        });
    }
}

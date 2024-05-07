using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using A2W;

public class SceneLoaderTest : MonoBehaviour
{
    [SerializeField] string sceneName;
    [SerializeField] Button button;
    [SerializeField] Transition transition;

    private void Awake()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            SceneLoader.instance.LoadScene(sceneName, transition, false, 1f);
        });
    }
}

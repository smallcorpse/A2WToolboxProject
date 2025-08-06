using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class UIPanel : MonoBehaviour
{
    public abstract void Init();

    public abstract UniTask Show();

    public abstract UniTask Hide();
}



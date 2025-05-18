using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A2W
{
    public interface ITransition
    {
        public void Init(Transform parent);

        public void Begin();

        public void Finish();

        public bool IsDone();
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class CullingState : MonoBehaviour
{
    public Renderer target { get; set; }

    void OnPreCull()
    {
        target.enabled = true;
    }

    void OnPostRender()
    {
        target.enabled = false;
    }
}

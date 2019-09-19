using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PortalRendering
{
    public class PortalDebugRenderOverlay : MonoBehaviour
    {
        internal RenderTexture renderTarget;
        public RawImage fullscreenOverlay;

        public static PortalDebugRenderOverlay Instance { get; private set; }

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                throw new System.Exception("PortalDebugRenderOverlay is already assigned! is the script assigned twice in the scene?");
            }
        }

        void Update()
        {

        }
    }
}

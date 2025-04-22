using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Edgar.Unity
{
    public class FogOfWarURPFeature : ScriptableRendererFeature
    {
        internal class BlitPass : ScriptableRenderPass
        {
            // ... (other members: blitMaterial, filterMode, settings, identifiers, profilerTag) ...
            public Material blitMaterial = null;
            public FilterMode filterMode { get; set; }
            private BlitSettings settings;
            RTHandle m_TemporaryColorTexture;
            RTHandle m_DestinationTexture; // Used only if dstType is TextureID
            private RenderTargetIdentifier sourceIdentifier { get; set; }
            private RenderTargetIdentifier destinationIdentifier { get; set; }
            string m_ProfilerTag;


            public BlitPass(RenderPassEvent renderPassEvent, BlitSettings settings, string tag)
            {
                this.renderPassEvent = renderPassEvent;
                this.settings = settings;
                blitMaterial = settings.blitMaterial;
                m_ProfilerTag = tag;
            }

            public void Setup(RenderTargetIdentifier source, RenderTargetIdentifier destination)
            {
                this.sourceIdentifier = source;
                this.destinationIdentifier = destination;
                // Reset handles - allocation happens in OnCameraSetup
                m_DestinationTexture = null;
                m_TemporaryColorTexture = null;
            }

            // Prepare resources (Allocate RTHandles)
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;

                // --- CORRECTED RTHandles.Alloc calls ---
                // Use the overload: Alloc(in RenderTextureDescriptor descriptor, FilterMode filterMode, TextureWrapMode wrapMode, ..., string name = "")
                TextureWrapMode wrapMode = TextureWrapMode.Clamp; // Default wrap mode

                // Allocate the destination texture RTHandle if needed
                if (settings.dstType == Target.TextureID && !string.IsNullOrEmpty(settings.dstTextureId))
                {
                    // Pass descriptor first, then filterMode, wrapMode, and the optional name
                    m_DestinationTexture = RTHandles.Alloc(descriptor, filterMode: filterMode, wrapMode: wrapMode, name: settings.dstTextureId);
                }

                // Allocate the temporary texture handle needed for read/write conflicts
                m_TemporaryColorTexture = RTHandles.Alloc(descriptor, filterMode: filterMode, wrapMode: wrapMode, name: "_TemporaryColorTexture");
                // --- End Correction ---

            }


            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
                using (new ProfilingScope(cmd, new ProfilingSampler(m_ProfilerTag)))
                {
                    var cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

                    RTHandle sourceHandle;
                    bool releaseSourceHandle = false;
                    if (settings.srcType == Target.CameraColor) {
                         sourceHandle = cameraColorTargetHandle;
                    } else {
                        // Wrap existing identifier (this Alloc overload is correct)
                        sourceHandle = RTHandles.Alloc(sourceIdentifier);
                        releaseSourceHandle = true;
                    }

                    RTHandle destinationHandle;
                    bool releaseDestinationHandle = false;
                    if (settings.dstType == Target.CameraColor) {
                        destinationHandle = cameraColorTargetHandle;
                    } else if (settings.dstType == Target.TextureID) {
                        destinationHandle = m_DestinationTexture; // Use handle allocated in OnCameraSetup
                         if (destinationHandle == null) { // Safety check
                            Debug.LogError($"FogOfWarURPFeature: Destination RTHandle '{settings.dstTextureId}' was null during Execute. Check allocation.", FogOfWarGrid2D.Instance);
                            if(releaseSourceHandle) RTHandles.Release(sourceHandle);
                            // Ensure temporary allocated in OnCameraSetup is released if we exit early
                            RTHandles.Release(m_TemporaryColorTexture);
                            CommandBufferPool.Release(cmd);
                            return;
                        }
                    } else { // Target.RenderTextureObject
                        // Wrap existing identifier (this Alloc overload is correct)
                        destinationHandle = RTHandles.Alloc(destinationIdentifier);
                        releaseDestinationHandle = true;
                    }

                    if (settings.setInverseViewMatrix) {
                        Shader.SetGlobalMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);
                    }

                    // Use original identifier check for intermediate texture logic, as handles might be wrappers
                    bool useIntermediateTexture = sourceIdentifier == destinationIdentifier ||
                                                 (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor);

                    if (useIntermediateTexture) {
                         // Check if temporary texture was allocated successfully
                        if (m_TemporaryColorTexture == null) {
                             Debug.LogError("FogOfWarURPFeature: Temporary Blit Texture was null during Execute. Check allocation.", FogOfWarGrid2D.Instance);
                             if(releaseSourceHandle) RTHandles.Release(sourceHandle);
                             if(releaseDestinationHandle) RTHandles.Release(destinationHandle);
                             RTHandles.Release(m_DestinationTexture); // Release other potentially allocated handle
                             CommandBufferPool.Release(cmd);
                             return;
                        }
                        Blitter.BlitCameraTexture(cmd, sourceHandle, m_TemporaryColorTexture, blitMaterial, settings.blitMaterialPassIndex);
                        Blitter.BlitCameraTexture(cmd, m_TemporaryColorTexture, destinationHandle);
                    } else {
                        Blitter.BlitCameraTexture(cmd, sourceHandle, destinationHandle, blitMaterial, settings.blitMaterialPassIndex);
                    }

                    // Release temporary wrapper handles allocated within Execute
                    if (releaseSourceHandle) RTHandles.Release(sourceHandle);
                    if (releaseDestinationHandle) RTHandles.Release(destinationHandle);
                } // End ProfilingScope

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                // Release RTHandles allocated in OnCameraSetup
                RTHandles.Release(m_TemporaryColorTexture);
                RTHandles.Release(m_DestinationTexture);
                m_TemporaryColorTexture = null;
                m_DestinationTexture = null;
            }
        }

        // ... (Rest of the FogOfWarURPFeature class: BlitSettings, Target enum, fields, Create, UpdateIdentifier, AddRenderPasses) ...
         [System.Serializable]
        internal class BlitSettings
        {

            public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;

            [HideInInspector]
            public Material blitMaterial = null;

            [HideInInspector]
            public int blitMaterialPassIndex = 0;

            [HideInInspector]
            public bool setInverseViewMatrix = false;

            [HideInInspector]
            public Target srcType = Target.CameraColor;

            [HideInInspector]
            public string srcTextureId = "_CameraColorTexture";

            #pragma warning disable 0649

            [HideInInspector]
            public RenderTexture srcTextureObject;

            #pragma warning restore 0649

            [HideInInspector]
            public Target dstType = Target.CameraColor;

            [HideInInspector]
            public string dstTextureId = "_BlitPassTexture";

            #pragma warning disable 0649

            [HideInInspector]
            public RenderTexture dstTextureObject;

            #pragma warning restore 0649
        }

        internal enum Target
        {
            CameraColor,
            TextureID,
            RenderTextureObject
        }

        [SerializeField]
        internal BlitSettings settings = new BlitSettings();

        BlitPass blitPass;

        private RenderTargetIdentifier currentSrcIdentifier, currentDstIdentifier;

        public override void Create()
        {
            var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 0;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
            blitPass = new BlitPass(settings.Event, settings, name);
        }


         private RenderTargetIdentifier UpdateIdentifier(Target type, string s, RenderTexture obj)
        {
            if (type == Target.RenderTextureObject)
            {
                return obj != null ? new RenderTargetIdentifier(obj) : BuiltinRenderTextureType.CameraTarget;
            }
            else if (type == Target.TextureID && !string.IsNullOrEmpty(s))
            {
                return new RenderTargetIdentifier(s);
            }
            return BuiltinRenderTextureType.CameraTarget;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!Application.isPlaying || FogOfWarGrid2D.Instance == null || !FogOfWarGrid2D.Instance.enabled)
            {
                return;
            }
            var currentCamera = renderingData.cameraData.camera;
            if (currentCamera.GetComponent<FogOfWarGrid2D>() == null && currentCamera.GetComponent<FogOfWarAdditionalCameraGrid2D>() == null)
            {
                return;
            }
            var material = FogOfWarGrid2D.Instance.GetMaterial(currentCamera);
            if (material == null)
            {
                return;
            }

            blitPass.blitMaterial = material;

            bool useAfterPostProcess = settings.Event == RenderPassEvent.AfterRendering && renderingData.cameraData.postProcessEnabled;
            string afterPostProcessTextureName = "_AfterPostProcessTexture";

            Target currentSrcType = settings.srcType;
            string currentSrcTextureId = settings.srcTextureId;
            if (useAfterPostProcess && settings.srcType == Target.CameraColor)
            {
                currentSrcType = Target.TextureID;
                currentSrcTextureId = afterPostProcessTextureName;
            }
            else if (!useAfterPostProcess && settings.srcType == Target.TextureID && settings.srcTextureId == afterPostProcessTextureName)
            {
                currentSrcType = Target.CameraColor;
                currentSrcTextureId = "";
            }

            Target currentDstType = settings.dstType;
            string currentDstTextureId = settings.dstTextureId;
             if (useAfterPostProcess && settings.dstType == Target.CameraColor)
             {
                 currentDstType = Target.TextureID;
                 currentDstTextureId = afterPostProcessTextureName;
             }
             else if (!useAfterPostProcess && settings.dstType == Target.TextureID && settings.dstTextureId == afterPostProcessTextureName)
             {
                 currentDstType = Target.CameraColor;
                 currentDstTextureId = "";
             }

            currentSrcIdentifier = UpdateIdentifier(currentSrcType, currentSrcTextureId, settings.srcTextureObject);
            currentDstIdentifier = UpdateIdentifier(currentDstType, currentDstTextureId, settings.dstTextureObject);


            blitPass.Setup(currentSrcIdentifier, currentDstIdentifier);

            renderer.EnqueuePass(blitPass);
        }
    }
} // End namespace Edgar.Unity
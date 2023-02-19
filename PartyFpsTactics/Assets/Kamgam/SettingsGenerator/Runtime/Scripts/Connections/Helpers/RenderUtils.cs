using UnityEngine.Rendering;

namespace Kamgam.SettingsGenerator
{
    public static class RenderUtils
    {
        public enum RenderPipe { BuiltIn, URP, HDRP }

        public static RenderPipe GetCurrentRenderPipeline()
        {
            RenderPipelineAsset rpa = GraphicsSettings.renderPipelineAsset;
            if(rpa != null)
            {
                switch(rpa.GetType().Name)
                {
                    case "UniversalRenderPipelineAsset": return RenderPipe.URP;
                    case "HDRenderPipelineAsset": return RenderPipe.HDRP;
                }
            }

            return RenderPipe.BuiltIn;
        }
    }
}

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class ServerWebGLBuild
{
    public static void Build()
    {
        var oldCompression = PlayerSettings.WebGL.compressionFormat;
        var oldFallback = PlayerSettings.WebGL.decompressionFallback;
        try
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Delivery/RoadHome-WebGL/site",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("WebGL build failed: " + report.summary.result);
        }
        finally
        {
            PlayerSettings.WebGL.compressionFormat = oldCompression;
            PlayerSettings.WebGL.decompressionFallback = oldFallback;
            AssetDatabase.SaveAssets();
        }
    }
}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kamgam.SettingsGenerator
{
    public static class PackageImporter
    {
        public enum RenderPiplelineType
        {
            URP, HDRP, BuiltIn
        }

        private class Package
        {
            public RenderPiplelineType RenderPipeline;
            public string PackagePath;

            public Package(RenderPiplelineType renderPipeline, string packagePath)
            {
                RenderPipeline = renderPipeline;
                PackagePath = packagePath;
            }
        }

        static List<Package> Packages = new List<Package>()
        {
			new Package( RenderPiplelineType.URP,  "Assets/Kamgam/SettingsGenerator/Packages/SettingsGeneratorURP.unitypackage" ),
            new Package( RenderPiplelineType.HDRP, "Assets/Kamgam/SettingsGenerator/Packages/SettingsGeneratorHDRP.unitypackage" )
        };

        static Package getPackageFor(RenderPiplelineType renderPipeline)
        {
            foreach (var pkg in Packages)
            {
                if (pkg.RenderPipeline == renderPipeline)
                    return pkg;
            }

            return null;
        }

        static System.Action _onComplete;

        #region Start Import Delayed
        static double startPackageImportAt;

        public static void ImportDelayed(System.Action onComplete)
        {
            // Materials may not be loaded at this time. Thus we wait for them to be imported.
            _onComplete = onComplete;
            EditorApplication.update -= onEditorUpdate;
            EditorApplication.update += onEditorUpdate;
            startPackageImportAt = EditorApplication.timeSinceStartup + 3; // wait N seconds
        }

        static void onEditorUpdate()
        {
            // wait for the time to reach startPackageImportAt
            if (startPackageImportAt - EditorApplication.timeSinceStartup < 0)
            {
                EditorApplication.update -= onEditorUpdate;
                ImportFixes();
                return;
            }
        }
        #endregion

        static int _crossCompileCallbackID = -1;

        [MenuItem("Tools/Settings Generator/Debug/Import Fixes", priority = 200)]
        public static void ImportFixes()
        {
            // Don't import during play mode.
            if (EditorApplication.isPlaying)
                return;

            Debug.Log("PackageImporter: Importing..");

            var createdForRenderPipleline = RenderPiplelineType.BuiltIn;
            var currentRenderPipline = GetCurrentRenderPiplelineType();

            var package = getPackageFor(currentRenderPipline);
            if (package == null)
            {
                Debug.Log("PackageImporter: All good (no changes needed).");
                _onComplete?.Invoke();
            }
            else
            {
                Debug.Log("PackageImporter: Upgrading from '" + createdForRenderPipleline.ToString() + "' to '" + currentRenderPipline.ToString() + "'.");

                // AssetDatabase.importPackageCompleted callbacks are lost after a recompile.
                // Therefore, if the package includes any scripts then these will not be called.
                // See: https://forum.unity.com/threads/assetdatabase-importpackage-callbacks-dont-work.544031/#post-3716791

                // We use CrossCompileCallbacks to register a callback for after compilation.
                _crossCompileCallbackID = CrossCompileCallbacks.RegisterCallback(onPackageImportedAfterRecompile);
                // We also have to store the external callback (if there is one)
                CrossCompileCallbacks.StoreAction(typeof(PackageImporter).FullName + ".importedCallack", _onComplete);
                // Delay to avoid "Calling ... from assembly reloading callbacks are not supported." errors.
                CrossCompileCallbacks.DelayExecutionAfterCompilation = true;

                // If the package does not contain any scripts the we can still use the normal callbacks.
                AssetDatabase.importPackageCompleted -= onPackageImported;
                AssetDatabase.importPackageCompleted += onPackageImported;

                // import package
                Debug.Log("PackageImporter: Importing '" + package.PackagePath + "'.");
                AssetDatabase.ImportPackage(package.PackagePath, interactive: false);
                AssetDatabase.SaveAssets();
            }
        }

        // This is only execute if the package did not contain any script files.
        static void onPackageImported(string packageName)
        {
            Debug.Log("PackageImporter: Package '" + packageName + "' imported.");

            // There was no recompile. Thus we clear the registered callback.
            CrossCompileCallbacks.ReleaseIndex(_crossCompileCallbackID);

            // Check if it is one of our packages.
            // Abort if not.
            bool isFixerPackage = false;
            foreach (var pkg in Packages)
            {
                if (pkg.PackagePath.Contains(packageName))
                    isFixerPackage = true;
            }
            if (!isFixerPackage)
                return;

            AssetDatabase.importPackageCompleted -= onPackageImported;

            onPackageImportDone(_onComplete);
            _onComplete = null;
        }

        static void onPackageImportedAfterRecompile()
        {
            Debug.Log("PackageImporter: Recompile detected. Assuming package import is done.");

            // The registered callback is already cleared by now.
            // Now we let's retrieve that stored extenal callback and hand it over.
            var onComplete = CrossCompileCallbacks.GetStoredAction(typeof(PackageImporter).FullName + ".importedCallack");
            onPackageImportDone(onComplete);
        }

        static void onPackageImportDone(System.Action onComplete)
        {
            AssetDatabase.SaveAssets();
            onComplete?.Invoke();

            Debug.Log("PackageImporter: Done");
        }

        public static RenderPiplelineType GetCurrentRenderPiplelineType()
        {
            var currentRP = GraphicsSettings.currentRenderPipeline;

            // null if built-in
            if (currentRP != null)
            {
                if (currentRP.GetType().Name.Contains("Universal"))
                {
                    return RenderPiplelineType.URP;
                }
                else
                {
                    return RenderPiplelineType.HDRP;
                }
            }

            return RenderPiplelineType.BuiltIn;
        }
    }
}
#endif
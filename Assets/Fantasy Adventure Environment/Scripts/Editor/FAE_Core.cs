// Fantasy Adventure Environment
// Staggart Creations
// http://staggart.xyz

using UnityEngine;
using System.IO;

//Make this entire class is editor-only without requiring it to be in an "Editor" folder
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace FAE
{
    public class FAE_Core : Editor
    {
        public const string ASSET_NAME = "Fantasy Adventure Environment";
        public const string ASSET_ABRV = "FAE";
        public const string ASSET_ID = "70354";

        public const string PACKAGE_VERSION = "20174";
        public static string INSTALLED_VERSION = "1.5.1";
        public const string MIN_UNITY_VERSION = "2017.4";

        public static string DOC_URL = "http://staggart.xyz/unity/fantasy-adventure-environment/fae-documentation/";
        public static string FORUM_URL = "https://forum.unity3d.com/threads/486102";

        private const string UniversalShaderPackageGUID = "7c884420a5dfbaa4db9afe42d366b843";

        public static void OpenStorePage()
        {
            Application.OpenURL("com.unity3d.kharma:content/" + ASSET_ID);
        }

        public static string PACKAGE_ROOT_FOLDER
        {
            get { return SessionState.GetString(ASSET_ABRV + "_BASE_FOLDER", string.Empty); }
            set { SessionState.SetString(ASSET_ABRV + "_BASE_FOLDER", value); }
        }

        public static string GetRootFolder()
        {
            //Get script path
            string[] scriptGUID = AssetDatabase.FindAssets("FAE_Core t:script");
            string scriptFilePath = AssetDatabase.GUIDToAssetPath(scriptGUID[0]);

            //Truncate to get relative path
            PACKAGE_ROOT_FOLDER = scriptFilePath.Replace("/Scripts/Editor/FAE_Core.cs", string.Empty);

#if FAE_DEV
            Debug.Log("<b>Package root</b> " + PACKAGE_ROOT_FOLDER);
#endif

            return PACKAGE_ROOT_FOLDER;
        }

        public enum ShaderInstallation
        {
            BuiltIn,
            UniversalRP
        }

#if UNITY_2019_3_OR_NEWER
        public class RunOnImport : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string str in importedAssets)
                {
                    if (str.Contains("FAE_Core.cs"))
                    {
                        GetRootFolder();

                        string urpFolder = FAE_Core.PACKAGE_ROOT_FOLDER + "/Shaders/URP/";

                        var info = new DirectoryInfo(urpFolder);
                        FileInfo[] fileInfo = info.GetFiles();

                        //Only one file in the folder, shaders not yet unpacked
                        if (fileInfo.Length <= 2 && UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
                        {
                            if (EditorUtility.DisplayDialog("Fantasy Adventure Environment", "The Universal Render Pipeline is in use.\n\nURP compatible shaders can be unpacked and materials upgraded through the \"Help\" window after importing has finished\n\nErrors about _GrabTexture can safely be ignored.", "OK"))
                            {

                            }
                        }
                    }
                }
            }
        }
#endif

        private const string urpName = "Universal Render Pipeline";

        //Look up table to finding pipeline shader variants
        private static Dictionary<string, string> ShaderRelations = new Dictionary<string, string>
        {
            //Peartickles
            { "Legacy Shaders/Particles/Alpha Blended", urpName + "/Particles/Simple Lit" },
            { "Particles/Alpha Blended", urpName + "/Particles/Simple Lit" },
            { "Mobile/Particles/Alpha Blended", urpName + "/Particles/Simple Lit" },

            { "Standard", urpName + "/Lit" },
            { "Skybox/Cubemap","Skybox/Cubemap" },
            { "Nature/Terrain/Standard", urpName + "/Terrain/Lit" },

            { "FAE/Fog sheet", urpName + "/FAE/FAE_FogSheet" },
            { "FAE/Sunshaft", urpName + "/FAE/FAE_Sunshaft" },
            //{ "FAE/Sunshaft particle", urpName + "/FAE/FAE_SunshaftParticle" },

            { "FAE/Cliff", urpName + "/FAE/FAE_Cliff" },
            { "FAE/Cliff coverage", urpName + "/FAE/FAE_Cliff_Coverage" },

            { "FAE/Water", urpName + "/FAE/FAE_Water" },
            { "FAE/Waterfall", urpName + "/FAE/FAE_Waterfall" },
            { "FAE/Waterfall foam", urpName + "/FAE/FAE_WaterfallFoam" },

            { "FAE/Foliage", urpName+ "/FAE/FAE_Foliage" },
            { "FAE/Tree Branch", urpName+ "/FAE/FAE_TreeBranch" },
            { "FAE/Tree Trunk", urpName+ "/FAE/FAE_TreeTrunk" },
            { "FAE/Tree Billboard", urpName+ "/FAE/FAE_TreeBillboard" }
        };

        public static void InstallShaders(ShaderInstallation config)
        {
            string guid = UniversalShaderPackageGUID;
            string packagePath = AssetDatabase.GUIDToAssetPath(guid);

            GetRootFolder();

            //TODO: Package up current shaders
            if (config == ShaderInstallation.BuiltIn)
            {
                //AssetDatabase.ExportPackage(PACKAGE_ROOT_FOLDER + "/Shaders/URP", packagePath, ExportPackageOptions.Default | ExportPackageOptions.Recurse);

                UpgradeMaterials(config);
            }
            else
            {
                if (packagePath == string.Empty)
                {
                    Debug.LogError("URP Shader/material package with the GUID: " + guid + ". Could not be found in the project, was it changed or not imported? It should be located in <i>" + PACKAGE_ROOT_FOLDER + "/Shaders/URP</i>");
                    return;
                }
                AssetDatabase.ImportPackage(packagePath, false);
                AssetDatabase.importPackageCompleted += new AssetDatabase.ImportPackageCallback(ImportURPCallback);
            }

#if UNITY_2019_3_OR_NEWER && FAE_DEV
            SwitchRenderPipeline.SetPipeline(config);
#endif

        }

        static void ImportURPCallback(string packageName)
        {
            AssetDatabase.Refresh();

            UpgradeMaterials(ShaderInstallation.UniversalRP);

            AssetDatabase.importPackageCompleted -= ImportURPCallback;
        }

        public static void UpgradeMaterials(ShaderInstallation config)
        {
            string[] GUIDs = AssetDatabase.FindAssets("t: material", new string[] { PACKAGE_ROOT_FOLDER });

            int count = 0;
            if (GUIDs.Length > 0)
            {
                Material[] mats = new Material[GUIDs.Length];

                for (int i = 0; i < mats.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Material configuration", "Converting FAE materials for " + config, (float)i / mats.Length);
                    string path = AssetDatabase.GUIDToAssetPath(GUIDs[i]);

                    mats[i] = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));

                    string dest = string.Empty;
                    string source = mats[i].shader.name;
                    bool matched = ShaderRelations.TryGetValue(source, out dest);

                    if (config == ShaderInstallation.BuiltIn)
                    {
                        //Get key by value (inverse lookup)
                        dest = ShaderRelations.FirstOrDefault(x => x.Value == source).Key;

                        matched = dest != null;
                    }

                    if (config == ShaderInstallation.UniversalRP)
                    {
                        //Set grass to foliage shader
                        if (source == "FAE/Grass")
                        {
                            dest = urpName + "/FAE/FAE_Foliage";
                            matched = true;
                        }
                    }
                    if (config == ShaderInstallation.BuiltIn)
                    {
                        //Set foliage to grass shader
                        if (mats[i].name.Contains("Grass"))
                        {
                            dest = "FAE/Grass";
                            matched = true;
                        }
                    }

                    if (source == null && dest == null) continue;
                    if (string.Equals(dest, source)) continue;

                    if (matched)
                    {

                        if (config == ShaderInstallation.UniversalRP)
                        {
                            if ((source.Contains("WindStreak") || source.Contains("Fogsheets")))
                            {
                                //mats[i].EnableKeyword("_ALPHATEST_ON");
                                mats[i].EnableKeyword("_COLORADDSUBDIFF_ON");
                                mats[i].SetFloat("_ColorMode", 1);
                            }

                            if (source.Contains("Particle") && !source.Contains("Leaf"))
                            {
                                //Set material to transparent
                                mats[i].SetFloat("_Surface", 1);
                                mats[i].EnableKeyword("_COLORADDSUBDIFF_ON");
                                //Additive blending
                                mats[i].SetFloat("_ColorMode", 1);
                            }

                            if (mats[i].HasProperty("_Color")) mats[i].SetColor("_BaseColor", mats[i].GetColor("_Color"));
                            if (mats[i].HasProperty("_TintColor")) mats[i].SetColor("_BaseColor", mats[i].GetColor("_TintColor"));

                            //Grass to foliage switch
                            if (mats[i].HasProperty("_ColorTop")) mats[i].SetColor("_Color", mats[i].GetColor("_ColorTop"));

                            if (mats[i].HasProperty("_MainTex"))
                            {
                                mats[i].SetTexture("_BaseMap", mats[i].GetTexture("_MainTex"));
                            }

                            if (mats[i].name.Contains("Grass"))
                            {
                                mats[i].SetFloat("_MaxWindStrength", 0.2f);
                            }
                        }

                        if (mats[i].HasProperty("_TransmissionAmount"))
                        {
                            mats[i].SetFloat("_TransmissionAmount", Mathf.Clamp(mats[i].GetFloat("_TransmissionAmount"), 0, 10));
                        }

                        //Debug.Log("src: " + source + " dst:" + dest);
                        mats[i].shader = Shader.Find(dest);

                        EditorUtility.SetDirty(mats[i]);
                        count++;
                    }
                    else
                    {
#if FAE_DEV
                        Debug.LogError("No matching " + config + " shader could be found for " + mats[i].shader.name);
#endif
                    }
                }
                EditorUtility.ClearProgressBar();


                Debug.Log(count + " materials were configured for the " + config + " render pipeline");

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                //If any controllers are present in the open scene, these need to be nudged to apply the correct shaders
                CliffAppearance[] cliffConstrollers = GameObject.FindObjectsOfType<CliffAppearance>();
                for (int i = 0; i < cliffConstrollers.Length; i++)
                {
                    cliffConstrollers[i].OnEnable();
                }
            }
        }
    }
}//namespace
#endif //If Unity Editor
using System;
using BepInEx;
using UnityEngine;
using R2API.Utils;
using R2API.Networking;

namespace MoreArtifacts {

    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod)]
    [R2APISubmoduleDependency(nameof(NetworkingAPI))]
    public class MoreArtifacts : BaseUnityPlugin {
        public const string ModGUID = "com.ugff.moreartifacts";
        public const string ModName = "More Artifacts";
        public const string ModVersion = "1.1.0";


        public static new BepInEx.Logging.ManualLogSource Logger { get; private set; }

        public static CongregateArtifact congregateArtifact;
        public static ConglomerateArtifact wholeArtifact;

        public void Awake() {
            Logger = base.Logger;

            NetworkingAPI.RegisterMessageType<ScaleMessage>();

            CongregateArtifactManager.Init();
            congregateArtifact = new CongregateArtifact();

            ConglomerateArtifactManager.Init();
            wholeArtifact = new ConglomerateArtifact();
        }

        // Thanks to the R2Wiki for the help with this
        public static Sprite CreateSprite(byte[] resourceBytes, Color fallbackColor) {
            // Check to make sure that the byte array supplied is not null, and throw an appropriate exception if they are.
            if(resourceBytes == null)
                throw new ArgumentNullException(nameof(resourceBytes));

            // Create a temporary texture, then load the texture onto it.
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            try {
                tex.LoadImage(resourceBytes, false);
            } catch(Exception e) {
                Logger.LogError(e.ToString());
                Logger.LogInfo("Using fallback color.");

                var pixels = tex.GetPixels();
                for(var i = 0; i < pixels.Length; ++i) {
                    pixels[i] = fallbackColor;
                }

                tex.SetPixels(pixels);
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(31, 31));
        }
    }
}

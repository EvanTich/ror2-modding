using System;
using RoR2;
using UnityEngine;

namespace MoreArtifacts {

    public abstract class NewArtifact<T> where T : NewArtifact<T> {

        public static T Instance { get; private set; }

        // TODO LATER:
        // RoR2.ArtifactTrialMissionController
        // EntityStates.Missions.ArtifactWorld.TrialController.*
        // UnlockableDef / UnlockableCatalog

        /// <summary>
        /// In-game name of the artifact.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Name token of the artifact; backend value.
        /// </summary>
        public abstract string NameToken { get; }

        /// <summary>
        /// In-game description of the artifact.
        /// </summary>
        public abstract string Description { get; }

        public virtual UnlockableDef UnlockableDef { get; private set; } = null;

        /// <summary>
        /// In-game icon of the artifact when it is enabled.
        /// </summary>
        /// <see cref="CreateSprite(byte[], Color)"/>
        public abstract Sprite IconSelectedSprite { get; }

        /// <summary>
        /// In-game icon of the artifact when it is disabled.
        /// </summary>
        /// <see cref="CreateSprite(byte[], Color)"/>
        public abstract Sprite IconDeselectedSprite { get; }

        public virtual GameObject PickupModelPrefab { get; } = null;

        /// <summary>
        /// The resulting artifact definition from this class.
        /// </summary>
        public ArtifactDef ArtifactDef { get; protected set; }

        public bool ArtifactEnabled => RunArtifactManager.instance.IsArtifactEnabled(ArtifactDef);

        public NewArtifact() {
            if(Instance != null) {
                throw new InvalidOperationException("Same artifact cannot be created more than once. Use the already existing Instance.");
            }

            Instance = (T) this;
            InitManager();
            InitArtifact();
        }

        protected abstract void InitManager();
        //public abstract void InitMission(); TODO

        /// <summary>
        /// Initializes the artifact def and adds it to the game's list of artifacts.
        /// </summary>
        protected void InitArtifact() {
            R2API.LanguageAPI.Add($"ARTIFACT_{NameToken}_NAME", Name);
            R2API.LanguageAPI.Add($"ARTIFACT_{NameToken}_DESCRIPTION", Description);

            ArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            ArtifactDef.cachedName = $"ARTIFACT_{NameToken}";
            ArtifactDef.nameToken = $"ARTIFACT_{NameToken}_NAME";
            ArtifactDef.descriptionToken = $"ARTIFACT_{NameToken}_DESCRIPTION";

            // FIXME: research how unlockable defs work
            //if(!UnlockableDef) {
            //    UnlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            //    UnlockableDef.cachedName = Name;
            //    UnlockableDef.nameToken = $"{NameToken} Unlock";
            //    UnlockableDef.hidden = false;
            //}
            if(UnlockableDef) {
                ArtifactDef.unlockableDef = UnlockableDef;
            }
            
            ArtifactDef.smallIconSelectedSprite = IconSelectedSprite;
            ArtifactDef.smallIconDeselectedSprite = IconDeselectedSprite;
            if(PickupModelPrefab) {
                ArtifactDef.pickupModelPrefab = PickupModelPrefab;
            }

            R2API.ArtifactAPI.Add(ArtifactDef);
            MoreArtifacts.Logger.LogInfo($"Initialized Artifact: {Name}");
        }

        /// <summary>
        /// An easy way to create a sprite from embedded resources.
        /// </summary>
        /// <see cref="https://github.com/risk-of-thunder/R2Wiki/wiki/Embedding-and-loading-resources-(The-sane-way)"/>
        /// <param name="resourceBytes">Ensure your resource is a byte array before using this method.</param>
        /// <param name="fallbackColor">Uses a color to fill the texture when there is a problem loading an image from the resource.</param>
        /// <returns>A sprite to use with the artifact.</returns>
        public static Sprite CreateSprite(byte[] resourceBytes, Color fallbackColor) {
            // Create a temporary texture, then load the texture onto it.
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);

            tex.filterMode = FilterMode.Bilinear; // ah, right.

            try {
                if(resourceBytes == null) {
                    FillTexture(tex, fallbackColor);
                } else {
                    tex.LoadImage(resourceBytes, false);
                    tex.Apply();
                }
            } catch(Exception e) {
                MoreArtifacts.Logger.LogError(e.ToString());
                MoreArtifacts.Logger.LogInfo("Using fallback color.");
                FillTexture(tex, fallbackColor);
            }

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(31, 31));
        }

        public static Texture2D FillTexture(Texture2D tex, Color color) {
            var pixels = tex.GetPixels();
            for(var i = 0; i < pixels.Length; ++i) {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return tex;
        }
    }
}

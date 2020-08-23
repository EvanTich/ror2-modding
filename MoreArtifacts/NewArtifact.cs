using System;
using RoR2;
using UnityEngine;

namespace MoreArtifacts {

    public abstract class NewArtifact<T> where T : NewArtifact<T> {

        public static T Instance { get; private set; }

        // LATER:
        // RoR2.ArtifactTrialMissionController
        // EntityStates.Missions.ArtifactWorld.TrialController.*
        // UnlockableDef / UnlockableCatalog

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract Sprite IconSelectedSprite { get; }
        public abstract Sprite IconDeselectedSprite { get; }

        public ArtifactDef ArtifactDef { get; protected set; }

        public NewArtifact() {
            Instance = (T) this;
            InitArtifact();
        }

        protected void InitArtifact() {
            ArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            ArtifactDef.nameToken = Name;
            ArtifactDef.descriptionToken = Description;
            ArtifactDef.smallIconSelectedSprite = IconSelectedSprite;
            ArtifactDef.smallIconDeselectedSprite = IconDeselectedSprite;

            ArtifactCatalog.getAdditionalEntries += list => list.Add(ArtifactDef);
            MoreArtifacts.Logger.LogInfo($"Initialized Artifact: {Name}");
        }


        //public abstract void InitMission();

        public static Sprite CreateSprite(byte[] resourceBytes, Color fallbackColor) {
            // Create a temporary texture, then load the texture onto it.
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
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

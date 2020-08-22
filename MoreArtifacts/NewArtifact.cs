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
    }
}

using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreArtifacts {

    public class ConfusionArtifact : NewArtifact<ConfusionArtifact> {

        public override string Name => "Artifact of Confusion";
        public override string NameToken => "CONFUSION";
        public override string Description => "All damage is randomized from 10% to 200%.";
        public override Sprite IconSelectedSprite => CreateSprite(Properties.Resources.confusion_selected, Color.magenta);
        public override Sprite IconDeselectedSprite => CreateSprite(Properties.Resources.confusion_deselected, Color.gray);

        protected override void InitManager() {
            ConfusionArtifactManager.Init();
        }
    }

    public static class ConfusionArtifactManager {
        private static ArtifactDef myArtifact {
            get { return ConfusionArtifact.Instance.ArtifactDef; }
        }

        private static float MinRandom;
        private static float MaxRandom;

        internal static Xoroshiro128Plus random;

        public static void Init() {
            MinRandom = MoreArtifactsConfig.LowerRandomizeEntry.Value;
            MaxRandom = MoreArtifactsConfig.UpperRandomizeEntry.Value;

            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        private static void OnArtifactEnabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(!NetworkServer.active || artifactDef != myArtifact) {
                return;
            }

            // do things
            Run.onRunStartGlobal += SetRandom;
            On.RoR2.HealthComponent.TakeDamage += RandomizeDamage;
        }

        private static void OnArtifactDisabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(artifactDef != myArtifact) {
                return;
            }

            // undo things
            Run.onRunStartGlobal -= SetRandom;
            On.RoR2.HealthComponent.TakeDamage -= RandomizeDamage;
        }

        private static void RandomizeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            damageInfo.damage *= random.RangeFloat(MinRandom, MaxRandom);
            orig(self, damageInfo);
        }

        private static void SetRandom(Run run) {
            random = new Xoroshiro128Plus(run.runRNG.nextUlong);
        }
    }
}

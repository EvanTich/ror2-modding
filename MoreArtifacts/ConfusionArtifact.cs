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

        private static List<ConfusionWeight> Weights;

        internal static Xoroshiro128Plus random;

        public static void Init() {
            Weights = MoreArtifactsConfig.ConfusionRangesList;

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
            float rand = RandomFloat();
            if(rand < 0) {
                // heal instead
                self.Heal(-rand * damageInfo.damage, default);
            } else {
                damageInfo.damage = (float) Math.Ceiling(damageInfo.damage * rand);
                orig(self, damageInfo);
            }
        }

        private static float RandomFloat() {
            float p = random.nextNormalizedFloat;
            foreach(var w in Weights) {
                if(p > w.end)
                    continue;
                return random.RangeFloat(w.min, w.max);
            }
            return 1f;
        }

        private static void SetRandom(Run run) {
            random = new Xoroshiro128Plus(run.runRNG.nextUlong);
        }
    }

    public struct ConfusionWeight {
        public float min; // minimum number
        public float max; // maximum number
        public float end; // end of the range [0,1]
    }
}

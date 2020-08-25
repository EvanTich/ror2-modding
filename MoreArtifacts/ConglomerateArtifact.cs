using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreArtifacts {

    /// <summary>
    /// The actual artifact definition. Pretty simple (for now...).
    /// </summary>
    public class ConglomerateArtifact : NewArtifact<ConglomerateArtifact> {

        public override string Name => "Artifact of the Conglomerate";
        public override string Description => "A random character on the same team will take damage instead of the one that is damaged.";
        public override Sprite IconSelectedSprite => CreateSprite(Properties.Resources.conglomerate_selected, Color.magenta);
        public override Sprite IconDeselectedSprite => CreateSprite(Properties.Resources.conglomerate_deselected, Color.gray);

        protected override void InitManager() {
            ConglomerateArtifactManager.Init();
        }
    }

    /// <summary>
    /// Overarching Manager for this artifact. Handles hooking and unhooking actions.
    /// </summary>
    public static class ConglomerateArtifactManager {
        private static ArtifactDef myArtifact {
            get { return ConglomerateArtifact.Instance.ArtifactDef; }
        }

        //private static string[] ignoreList; // maybe? :)

        private static System.Collections.ObjectModel.ReadOnlyCollection<TeamComponent>[] teamsList;
        internal static Xoroshiro128Plus random;
        internal static List<DamageInfo> seen;

        public static void Init() {
            seen = new List<DamageInfo>();

            // loooong line
            // may as well get the actual array instead of using TeamComponent.GetTeamMembers 3 times
            teamsList = R2API.Utils.Reflection
                .GetFieldValue<System.Collections.ObjectModel.ReadOnlyCollection<TeamComponent>[]>(typeof(TeamComponent), "readonlyTeamsList");

            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        private static void OnArtifactEnabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(!NetworkServer.active || artifactDef != myArtifact) {
                return;
            }

            // do things
            Run.onRunStartGlobal += SetRandom;
            Stage.onServerStageBegin += EmptyList;

            // MMHook :)
            On.RoR2.HealthComponent.TakeDamage += JumbleDamage;
        }

        private static void OnArtifactDisabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(artifactDef != myArtifact) {
                return;
            }

            // undo things
            Run.onRunStartGlobal -= SetRandom;
            Stage.onServerStageBegin -= EmptyList;

            On.RoR2.HealthComponent.TakeDamage -= JumbleDamage;
        }

        private static void JumbleDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(!IsValid(self.body.teamComponent.teamIndex)) {
                orig(self, damageInfo);
            } else
            //if(Array.IndexOf(ignoreList, self.body.baseNameToken) != -1) {
            //    orig(self, damageInfo);
            //} else 
            if(seen.Contains(damageInfo)) {
                seen.Remove(damageInfo);
                orig(self, damageInfo);
            } else {
                // make sure the next does not jumble the damage
                seen.Add(damageInfo);
                // redirect victim
                var list = teamsList[(int) self.body.teamComponent.teamIndex];
                if(list.Count > 0) {
                    // one would assume that there would obviously be an element in the list but sometimes there isnt 
                    //  (which Xoroshiro does not like)
                    random.NextElementUniform(list).body.healthComponent.TakeDamage(damageInfo);
                }
            }
        }

        private static bool IsValid(TeamIndex index) {
            return index >= TeamIndex.Neutral && index < TeamIndex.Count;
        }

        private static void EmptyList(Stage _) {
            seen.Clear();
        }

        private static void SetRandom(Run run) {
            random = new Xoroshiro128Plus(run.runRNG.nextUlong);
        }
    }
}

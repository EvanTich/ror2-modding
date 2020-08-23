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
        public override string Description => "A random monster on the stage will take damage instead of the one that is damaged.";
        public override Sprite IconSelectedSprite => CreateSprite(null, Color.magenta);
        public override Sprite IconDeselectedSprite => CreateSprite(null, Color.gray);

    }

    /// <summary>
    /// Overarching Manager for this artifact. Handles hooking and unhooking actions.
    /// </summary>
    public static class ConglomerateArtifactManager {
        private static ArtifactDef myArtifact {
            get { return ConglomerateArtifact.Instance.ArtifactDef; }
        }

        private static Dictionary<TeamIndex, List<CharacterBody>> bodies;
        internal static Xoroshiro128Plus random;

        internal static List<DamageInfo> seen;

        public static void Init() {
            bodies = new Dictionary<TeamIndex, List<CharacterBody>>();
            foreach(TeamIndex index in Enum.GetValues(typeof(TeamIndex))) {
                bodies.Add(index, new List<CharacterBody>());
            }
            seen = new List<DamageInfo>();

            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        private static void OnArtifactEnabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(!NetworkServer.active || artifactDef != myArtifact) {
                return;
            }

            // do things
            Run.onRunStartGlobal += SetRandom;
            CharacterBody.onBodyStartGlobal += AddToList;
            Stage.onServerStageBegin += EmptyLists;
            GlobalEventManager.onCharacterDeathGlobal += RemoveFromList;
            // MMHook :)
            On.RoR2.HealthComponent.TakeDamage += JumbleDamage;
        }

        private static void OnArtifactDisabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(artifactDef != myArtifact) {
                return;
            }

            // undo things
            Run.onRunStartGlobal -= SetRandom;
            CharacterBody.onBodyStartGlobal -= AddToList;
            Stage.onServerStageBegin -= EmptyLists;
            GlobalEventManager.onCharacterDeathGlobal -= RemoveFromList;

            On.RoR2.HealthComponent.TakeDamage -= JumbleDamage;
        }

        private static void JumbleDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(seen.Contains(damageInfo)) {
                seen.Remove(damageInfo);
                orig(self, damageInfo);
            } else {
                // make sure the next does not jumble the damage
                seen.Add(damageInfo);
                // redirect victim
                var list = bodies[self.body?.teamComponent.teamIndex ?? TeamIndex.None];
                if(list.Count > 0) {
                    // one would assume that there would obviously be an element in the list but sometimes there isnt 
                    //  (which Xoroshiro does not like)
                    random.NextElementUniform(list).healthComponent.TakeDamage(damageInfo);
                }
            }
        }

        private static void SetRandom(Run run) {
            random = new Xoroshiro128Plus(run.runRNG.nextUlong);
        }

        private static void AddToList(CharacterBody body) {
            bodies[body.teamComponent.teamIndex].Add(body);
        }

        private static void EmptyLists(Stage _) {
            foreach(var list in bodies.Values) {
                list.Clear();
            }

            seen.Clear();
        }

        private static void RemoveFromList(DamageReport report) {
            bodies[report.victimTeamIndex].Remove(report.victimBody);
        }
    }
}

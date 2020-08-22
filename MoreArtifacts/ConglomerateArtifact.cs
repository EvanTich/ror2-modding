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
        public override Sprite IconSelectedSprite => MoreArtifacts.CreateSprite(Properties.Resources.congregate_selected, Color.magenta);
        public override Sprite IconDeselectedSprite => MoreArtifacts.CreateSprite(Properties.Resources.congregate_deselected, Color.gray);

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

        public static void Init() {
            bodies = new Dictionary<TeamIndex, List<CharacterBody>>();
            foreach(TeamIndex index in Enum.GetValues(typeof(TeamIndex))) {
                bodies.Add(index, new List<CharacterBody>());
            }

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
            Stage.onServerStageComplete += EmptyLists;
            GlobalEventManager.onCharacterDeathGlobal += RemoveFromList;
            GlobalEventManager.onServerDamageDealt += JumbleDamage; // USE MMHOOK if nothing else works
        }

        private static void OnArtifactDisabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(artifactDef != myArtifact) {
                return;
            }

            // undo things
            Run.onRunStartGlobal -= SetRandom;
            CharacterBody.onBodyStartGlobal -= AddToList;
            Stage.onServerStageComplete -= EmptyLists;
            GlobalEventManager.onCharacterDeathGlobal -= RemoveFromList;
            GlobalEventManager.onServerDamageDealt -= JumbleDamage;
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
        }

        private static void RemoveFromList(DamageReport report) {
            bodies[report.victimTeamIndex].Remove(report.victimBody);
        }

        private static void JumbleDamage(DamageReport report) {
            // heal damage from victim unless they are dead
            if(!report.victim.alive) {
                return;
            }

            // heal original victim
            report.victim.Heal(report.damageDealt, default);

            // deal damage to another random thing on the same team
            random.NextElementUniform(bodies[report.victimTeamIndex]).healthComponent.TakeDamage(report.damageInfo);
        }
    }
}

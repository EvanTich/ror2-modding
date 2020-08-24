using System;
using System.Collections.Generic;
using RoR2;
using R2API.Networking.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreArtifacts {

    /// <summary>
    /// The actual artifact definition. Pretty simple (for now...).
    /// </summary>
    public class CongregateArtifact : NewArtifact<CongregateArtifact> {

        public override string Name => "Artifact of the Congregate";
        public override string Description => "Monsters can combine to form larger, stronger monsters.";
        public override Sprite IconSelectedSprite => CreateSprite(Properties.Resources.congregate_selected, Color.magenta);
        public override Sprite IconDeselectedSprite => CreateSprite(Properties.Resources.congregate_deselected, Color.gray);

    }

    /// <summary>
    /// Overarching Manager for this artifact. Handles hooking and unhooking actions.
    /// </summary>
    public static class CongregateArtifactManager {
        private static ArtifactDef myArtifact {
            get { return CongregateArtifact.Instance.ArtifactDef; }
        }

        public static void Init() {
            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        private static void OnArtifactEnabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(!NetworkServer.active || artifactDef != myArtifact) {
                return;
            }

            // do things
            // add a MonoBehavior to check if two monsters of the same type are touching (body.mainHurtBox.collider.bounds)
            CharacterBody.onBodyStartGlobal += OnBodyStartGlobal;
        }

        private static void OnArtifactDisabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(artifactDef != myArtifact) {
                return;
            }

            // undo things
            CharacterBody.onBodyStartGlobal -= OnBodyStartGlobal;
        }

        private static void OnBodyStartGlobal(CharacterBody body) {
            if(body.teamComponent.teamIndex == TeamIndex.Monster) {
                body.gameObject.AddComponent<CongregateController>().Init(body);
            }
        }

    }

    /// <summary>
    /// Added onto all enemies when the artifact is active.
    /// Combines enemies when they get close enough.
    /// </summary>
    public class CongregateController : NetworkBehaviour {

        public static float CombineScale = 1.1f;
        public static int PearlsPerCount = 5;

        private static Dictionary<string, List<CongregateController>> dict = new Dictionary<string, List<CongregateController>>();

        private CharacterBody body;
        private DeathRewards rewards;
        private Vector3 initialScale;
        private int count; // number of enemies currently combined into this controller

        private bool isCombining;

        public CongregateController() {}

        public void Init(CharacterBody body) {
            this.body = body;
            rewards = body.gameObject.GetComponent<DeathRewards>();
            initialScale = body.modelLocator.modelTransform.localScale;
            count = 1;

            if(!dict.ContainsKey(body.baseNameToken)) {
                dict.Add(body.baseNameToken, new List<CongregateController>());
            }

            dict[body.baseNameToken].Add(this);
        }

        public void OnDisable() {
            if(dict.ContainsKey(body.baseNameToken)) {
                dict[body.baseNameToken].Remove(this);
            }
        }

        public void FixedUpdate() {
            if(body == null || isCombining || !dict.ContainsKey(body.baseNameToken)) return;

            var list = dict[body.baseNameToken];
            for(int i = 0; i < list.Count; i++) {
                if(list[i] == this || list[i].isCombining) continue;

                // TODO: check speed, ensure quality
                if(body.mainHurtBox.collider.bounds.Intersects(list[i].body.mainHurtBox.collider.bounds)) {
                    isCombining = true;
                    list[i].isCombining = true;
                    Combine(list[i]);
                    i--;
                }
            }
        }

        public void Combine(CongregateController other) {
            count += other.count;
            float scale = (CombineScale - 1) * count + 1;

            SendScale(initialScale * scale);

            if(body.inventory != null) {
                // Irradiant Pearls are nice because they give 10% more of everything
                body.inventory.ResetItem(ItemIndex.ShinyPearl);
                body.inventory.GiveItem(ItemIndex.ShinyPearl, count * PearlsPerCount - PearlsPerCount);
                body.RecalculateStats();

                // kill experience, gold gain, and health/shields
                body.healthComponent.health =
                    Mathf.Clamp(body.healthComponent.health + other.body.healthComponent.health, 0, body.maxHealth);
                body.healthComponent.shield =
                    Mathf.Clamp(body.healthComponent.shield + other.body.healthComponent.shield, 0, body.maxShield);

                rewards.expReward += other.rewards.expReward;
                rewards.goldReward += other.rewards.goldReward;
            }

            // combine elite types
            foreach(var index in BuffCatalog.eliteBuffIndices) {
                if(other.body.HasBuff(index) && !body.HasBuff(index)) {
                    body.AddBuff(index);
                }
            }

            dict[other.body.baseNameToken].Remove(other);
            Destroy(other.gameObject); // FIXME? find a better way of doing this
            isCombining = false;
        }

        void SendScale(Vector3 scale) {
            // only send from the server
            if(!NetworkServer.active) {
                return;
            }

            new ScaleMessage { body = body, scale = scale }.Send(R2API.Networking.NetworkDestination.Clients);
        }
    }

    internal struct ScaleMessage : INetMessage {
        internal CharacterBody body;
        internal Vector3 scale;

        public void OnReceived() {
            if(body != null && scale != null)
                body.modelLocator.modelTransform.localScale = scale;
        }

        public void Serialize(NetworkWriter writer) {
            writer.Write(body.gameObject);
            writer.Write(scale);
        }

        public void Deserialize(NetworkReader reader) {
            body = reader.ReadGameObject()?.GetComponent<CharacterBody>();
            scale = reader.ReadVector3();
        }
    }
}

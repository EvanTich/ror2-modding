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
        public override string NameToken => "CONGREGATE";
        public override string Description => "Monsters can combine to form larger, stronger monsters.";
        public override Sprite IconSelectedSprite => CreateSprite(Properties.Resources.congregate_selected, Color.magenta);
        public override Sprite IconDeselectedSprite => CreateSprite(Properties.Resources.congregate_deselected, Color.gray);

        protected override void InitManager() {
            CongregateArtifactManager.Init();
        }
    }

    /// <summary>
    /// Overarching Manager for this artifact. Handles hooking and unhooking actions.
    /// </summary>
    public static class CongregateArtifactManager {
        private static ArtifactDef myArtifact {
            get { return CongregateArtifact.Instance.ArtifactDef; }
        }

        public static void Init() {
            // ease of use values
            CongregateController.CombineScale           = MoreArtifactsConfig.CombineScaleEntry.Value;
            CongregateController.HealthMultiplier       = 1 + MoreArtifactsConfig.HealthMultiplierEntry.Value;
            CongregateController.CritMultiplier         = 1 + MoreArtifactsConfig.CritMultiplierEntry.Value;
            CongregateController.AttackSpeedMultiplier  = 1 + MoreArtifactsConfig.AttackSpeedMultiplierEntry.Value;
            CongregateController.MoveSpeedMultiplier    = 1 + MoreArtifactsConfig.MoveSpeedMultiplierEntry.Value;
            CongregateController.ArmorMultiplier        = 1 + MoreArtifactsConfig.ArmorMultiplierEntry.Value;
            CongregateController.DamageMultiplier       = 1 + MoreArtifactsConfig.DamageMultiplierEntry.Value;
            CongregateController.RegenMultiplier        = 1 + MoreArtifactsConfig.RegenMultiplierEntry.Value;
            CongregateController.BossMultiplier         = 1 + MoreArtifactsConfig.BossMultiplierEntry.Value;

            // actual init
            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        private static void OnArtifactEnabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(!NetworkServer.active || artifactDef != myArtifact) {
                return;
            }

            // do things
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

        public static float CombineScale;
        public static float HealthMultiplier;
        public static float CritMultiplier;
        public static float AttackSpeedMultiplier;
        public static float MoveSpeedMultiplier;
        public static float ArmorMultiplier;
        public static float DamageMultiplier;
        public static float RegenMultiplier;
        public static float BossMultiplier;

        private static readonly List<CombatSquad> squads = InstanceTracker.GetInstancesList<CombatSquad>();

        private static readonly Dictionary<string, List<CongregateController>> dict = new Dictionary<string, List<CongregateController>>();
        private static readonly Dictionary<string, Stats> baseStatDict = new Dictionary<string, Stats>();

        private CharacterBody body;
        private DeathRewards rewards;
        private Vector3 initialScale;
        private int count; // number of enemies currently combined into this controller
        private Stats baseStats;

        private bool isCombining;

        public CongregateController() {}

        public void Init(CharacterBody body) {
            this.body = body;
            rewards = body.gameObject.GetComponent<DeathRewards>();
            initialScale = body.modelLocator.modelTransform.localScale;
            count = 1;

            if(!dict.ContainsKey(body.baseNameToken)) {
                baseStats = new Stats {
                    baseMaxHealth = body.baseMaxHealth,
                    baseRegen = body.baseRegen,
                    baseMoveSpeed = body.baseMoveSpeed,
                    baseDamage = body.baseDamage,
                    baseAttackSpeed = body.baseAttackSpeed,
                    baseCrit = body.baseCrit,
                    baseArmor = body.baseArmor
                };

                baseStatDict.Add(body.baseNameToken, baseStats);
                dict.Add(body.baseNameToken, new List<CongregateController>());
            } else {
                baseStats = baseStatDict[body.baseNameToken]; // get a reference to another base stats class to prevent a large amount of memory use
            }

            dict[body.baseNameToken].Add(this);
        }

        public void OnDisable() {
            if(dict.ContainsKey(body.baseNameToken)) {
                dict[body.baseNameToken].Remove(this);
                Destroy(this);
            }
        }

        public void FixedUpdate() {
            if(body == null || isCombining || !dict.ContainsKey(body.baseNameToken)) return;

            var list = dict[body.baseNameToken];
            if(body.masterObject == null || body.gameObject == null) {
                MoreArtifacts.Logger.LogWarning("Body does not have master object or body does not have game object");
                RemoveFrom(list);
                return;
            }

            for(int i = 0; i < list.Count; i++) {
                var other = list[i];
                if(other == this || other.isCombining) continue;

                // TODO: check speed, ensure quality
                try {
                    if(body.mainHurtBox.collider.bounds.Intersects(other.body.mainHurtBox.collider.bounds)) {
                        Combine(other);
                    }
                } catch(NullReferenceException) {
                    // remove the one that threw the exception
                    if(body?.mainHurtBox?.collider?.bounds == null) {
                        MoreArtifacts.Logger.LogWarning("Null reference: Removing this offending body");
                        RemoveFrom(list);
                        return; // pls die
                    }

                    if(other?.body?.mainHurtBox?.collider?.bounds == null) {
                        MoreArtifacts.Logger.LogWarning("Null reference: Removing other offending body");
                        other.RemoveFrom(list); // how to prevent an infinite loop: remove the element you actually wanted to
                        i--;
                    }
                }

            }
        }

        public void Combine(CongregateController other) {
            isCombining = true;
            other.isCombining = true;

            count += other.count;
            float scale = (CombineScale - 1) * count + 1;

            // set stats, recalculate
            body.baseMaxHealth = baseStats.baseMaxHealth * count * HealthMultiplier;
            body.baseRegen = baseStats.baseRegen * count * RegenMultiplier;
            body.baseMoveSpeed = baseStats.baseMoveSpeed * count * MoveSpeedMultiplier;
            body.baseDamage = baseStats.baseDamage * count * DamageMultiplier;
            body.baseAttackSpeed = baseStats.baseAttackSpeed * count * AttackSpeedMultiplier;
            body.baseCrit = baseStats.baseCrit * count * CritMultiplier;
            body.baseArmor = baseStats.baseArmor * count * ArmorMultiplier;

            // if is a boss or champion
            if(body.isBoss || body.isChampion) {
                body.baseMaxHealth *= BossMultiplier;
                body.baseRegen *= BossMultiplier;
                body.baseMoveSpeed *= BossMultiplier;
                body.baseDamage *= BossMultiplier;
                body.baseAttackSpeed *= BossMultiplier;
                body.baseCrit *= BossMultiplier;
                body.baseArmor *= BossMultiplier;
            }

            body.RecalculateStats();

            // kill experience, gold gain, and health/shields
            body.healthComponent.health =
                Mathf.Clamp(body.healthComponent.health + other.body.healthComponent.health, 0, body.maxHealth);
            body.healthComponent.shield =
                Mathf.Clamp(body.healthComponent.shield + other.body.healthComponent.shield, 0, body.maxShield);

            rewards.expReward += other.rewards.expReward;
            rewards.goldReward += other.rewards.goldReward;

            SendCombine(
                initialScale * scale,
                body.baseMaxHealth,
                body.baseRegen,
                body.baseMoveSpeed,
                body.baseDamage,
                body.baseAttackSpeed,
                body.baseCrit,
                body.baseArmor,
                body.healthComponent.health,
                body.healthComponent.shield
            );

            // combine elite types
            foreach(var index in BuffCatalog.eliteBuffIndices) {
                if(other.body.HasBuff(index) && !body.HasBuff(index)) {
                    body.AddBuff(index);
                }
            }

            other.TrueDestroy();
            isCombining = false;
        }

        void SendCombine(
            Vector3 scale, 
            float baseMaxHealth,
            float baseRegen,
            float baseMoveSpeed,
            float baseDamage,
            float baseAttackSpeed,
            float baseCrit,
            float baseArmor,
            float health,
            float shield
        ) {
            // only send from the server
            if(!NetworkServer.active) {
                return;
            }

            // don't need to send over changed exp or gold because thats handled server-side anyways
            new CombineMessage {
                body = body,
                scale = scale,
                baseMaxHealth = baseMaxHealth,
                baseRegen = baseRegen,
                baseMoveSpeed = baseMoveSpeed,
                baseDamage = baseDamage,
                baseAttackSpeed = baseAttackSpeed,
                baseCrit = baseCrit,
                baseArmor = baseArmor,
                health = health,
                shield = shield
            }.Send(R2API.Networking.NetworkDestination.Clients);
        }

        private void TrueDestroy() {
            // make sure this monster is not part of a CombatSquad and remove it if it is
            foreach(var squad in squads) {
                if(squad.ContainsMember(body.master)) {
                    squad.RemoveMember(body.master);
                    break;
                }
            }

            body.master.DestroyBody(); // who woulda thunk it, it actually exists
            RemoveFrom(dict[body.baseNameToken]);
        }

        private void RemoveFrom(List<CongregateController> list) {
            this.enabled = false;
            list.Remove(this);
            Destroy(this);
        }
    }

    internal class Stats {
        internal float baseMaxHealth;
        internal float baseRegen;
        internal float baseMoveSpeed;
        internal float baseDamage;
        internal float baseAttackSpeed;
        internal float baseCrit;
        internal float baseArmor;
    }

    internal struct CombineMessage : INetMessage {
        internal CharacterBody body;
        internal Vector3 scale;
        internal float baseMaxHealth;
        internal float baseRegen;
        internal float baseMoveSpeed;
        internal float baseDamage;
        internal float baseAttackSpeed;
        internal float baseCrit;
        internal float baseArmor;
        internal float health;
        internal float shield;

        public void OnReceived() {
            if(body != null && scale != null)
                body.modelLocator.modelTransform.localScale = scale;

            body.RecalculateStats();
            body.baseMaxHealth   = baseMaxHealth;
            body.baseRegen       = baseRegen;
            body.baseMoveSpeed   = baseMoveSpeed;
            body.baseDamage      = baseDamage;
            body.baseAttackSpeed = baseAttackSpeed;
            body.baseCrit        = baseCrit;
            body.baseArmor       = baseArmor;

            body.healthComponent.health = health;
            body.healthComponent.shield = shield;
        }

        public void Serialize(NetworkWriter writer) {
            writer.Write(body.gameObject);
            writer.Write(scale);
            writer.Write(baseMaxHealth);
            writer.Write(baseRegen);
            writer.Write(baseMoveSpeed);
            writer.Write(baseDamage);
            writer.Write(baseAttackSpeed);
            writer.Write(baseCrit);
            writer.Write(baseArmor);
            writer.Write(health);
            writer.Write(shield);
        }

        public void Deserialize(NetworkReader reader) {
            body = reader.ReadGameObject()?.GetComponent<CharacterBody>();
            scale = reader.ReadVector3();
            baseMaxHealth = reader.ReadSingle();
            baseRegen = reader.ReadSingle();
            baseMoveSpeed = reader.ReadSingle();
            baseDamage = reader.ReadSingle();
            baseAttackSpeed = reader.ReadSingle();
            baseCrit = reader.ReadSingle();
            baseArmor = reader.ReadSingle();
            health = reader.ReadSingle();
            shield = reader.ReadSingle();
        }
    }
}

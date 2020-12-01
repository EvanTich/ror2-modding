using BepInEx.Configuration;
using RoR2;
using System;
using System.Linq;

namespace MoreArtifacts {

    public static class MoreArtifactsConfig {

        // Conglomerate Artifact
        public static ConfigEntry<string> IgnoreListEntry { get; set; }

        public static string[] IgnoreListArray {
            get {
                if(IgnoreListEntry == null) return null;

                return IgnoreListEntry.Value
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Distinct().ToArray();
            }
        }

        // Congregate Artifact
        public static ConfigEntry<float> HealthMultiplierEntry { get; set; }
        public static ConfigEntry<float> CritMultiplierEntry { get; set; }
        public static ConfigEntry<float> AttackSpeedMultiplierEntry { get; set; }
        public static ConfigEntry<float> MoveSpeedMultiplierEntry { get; set; }
        public static ConfigEntry<float> ArmorMultiplierEntry { get; set; }
        public static ConfigEntry<float> DamageMultiplierEntry { get; set; }
        public static ConfigEntry<float> RegenMultiplierEntry { get; set; }

        public static ConfigEntry<float> BossMultiplierEntry { get; set; }

        public static void Init(ConfigFile config) {

            // Conglomerate Artifact
            IgnoreListEntry = config.Bind(
                "ConglomerateArtifact", "IgnoreList", "",
                "The list of entity \"base name tokens\" to ignore when jumbling damage. Not used for now."
            );

            // Congregate Artifact
            HealthMultiplierEntry = config.Bind(
                "CongregateArtifact", "HealthMultiplier", 0.20f,
                "The percent to increase health by per number of monster, a value of 0.20 will increase health by 20% per monster."
            );

            CritMultiplierEntry = config.Bind(
                "CongregateArtifact", "CritMultiplier", 0.20f,
                "The percent to increase crit chance by per number of monster."
            );

            AttackSpeedMultiplierEntry = config.Bind(
                "CongregateArtifact", "AttackSpeedMultiplier", 0.20f,
                "The percent to increase attack speed by per number of monster."
            );

            MoveSpeedMultiplierEntry = config.Bind(
                "CongregateArtifact", "MoveSpeedMultiplier", 0.20f,
                "The percent to increase move speed by per number of monster."
            );

            ArmorMultiplierEntry = config.Bind(
                "CongregateArtifact", "ArmorMultiplier", 0.20f,
                "The percent to increase armor by per number of monster."
            );

            DamageMultiplierEntry = config.Bind(
                "CongregateArtifact", "DamageMultiplier", 0.20f,
                "The percent to increase damage by per number of monster."
            );

            RegenMultiplierEntry = config.Bind(
                "CongregateArtifact", "RegenMultiplier", 0.20f,
                "The percent to increase regen by per number of monster."
            );

            BossMultiplierEntry = config.Bind(
                "CongregateArtifact", "BossMultiplier", 2f,
                "Stat multiplier for when bosses or \"champions\" merge."
            );
        }
    }
}

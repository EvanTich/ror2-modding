using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoreArtifacts {

    public static class MoreArtifactsConfig {

        // Conglomerate Artifact
        public static ConfigEntry<string> IgnoreListEntry { get; set; }

        private static List<string> _ignoreList;
        public static List<string> IgnoreList {
            get {
                if(IgnoreListEntry == null) return null;
                if(_ignoreList == null) {
                    _ignoreList = IgnoreListEntry.Value
                        .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Distinct().ToList();
                }
                return _ignoreList;
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
        public static ConfigEntry<float> CombineScaleEntry { get; set; }

        // Confusion Artifact
        public static ConfigEntry<string> ConfusionRangesEntry { get; set; }

        private static List<ConfusionWeight> _confusionRangesList;
        public static List<ConfusionWeight> ConfusionRangesList {
            get {
                if(ConfusionRangesEntry == null) return null;
                if(_confusionRangesList == null) {
                    _confusionRangesList = ConfusionRangesEntry.Value
                        .Split(';')
                        .Select(x => {
                            var g = x.Trim().Split(':');

                            var range = g[0].Split('-')
                                .Select(y => float.Parse(y.Trim().Replace('~', '-')))
                                .ToArray();
                            var perc = float.Parse(g[1].Trim());

                            ConfusionWeight weight = new ConfusionWeight {
                                min = range[0],
                                max = range[1],
                                end = perc
                            };

                            return weight;
                        }).OrderBy(x => x.end).ToList();
                }
                return _confusionRangesList;
            }
        }

        public static void Init(ConfigFile config) {

            // Conglomerate Artifact
            IgnoreListEntry = config.Bind(
                "ConglomerateArtifact", "IgnoreList", "",
                "A comma separated list of entity \"base name tokens\" to ignore when jumbling damage."
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
                "CongregateArtifact", "BossMultiplier", 0.20f,
                "Percent multiplier for when bosses or \"champions\" merge."
            );

            CombineScaleEntry = config.Bind(
                "CongregateArtifact", "CombineScale", 1.1f,
                "Scale multiplier for when monsters merge."
            );

            // Confusion Artifact
            // thanks, bell curve :)
            ConfusionRangesEntry = config.Bind(
                "ConfusionArtifact", "ConfusionRanges",
                "~10-~4:0.03; ~2-0:0.25; 0-2:0.97; 4-10:1",
                "Ranges for the randomization, first group (before colon) states the min/max randomization and the second group is the percentage upper bound that the specified randomization is given."
            );

            MoreArtifacts.Logger.LogInfo("Loaded Config");
        }
    }
}

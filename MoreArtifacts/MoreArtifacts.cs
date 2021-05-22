using System;
using BepInEx;
using UnityEngine;
using R2API;
using R2API.Utils;
using R2API.Networking;

namespace MoreArtifacts {

    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod)]
    [R2APISubmoduleDependency(nameof(NetworkingAPI), nameof(ArtifactAPI), nameof(LanguageAPI))]
    public class MoreArtifacts : BaseUnityPlugin {
        public const string ModGUID = "com.ugff.moreartifacts";
        public const string ModName = "More Artifacts";
        public const string ModVersion = "1.2.5";


        internal static new BepInEx.Logging.ManualLogSource Logger { get; private set; }

        public static CongregateArtifact congregateArtifact;
        public static ConglomerateArtifact wholeArtifact;
        public static ConfusionArtifact confusionArtifact;

        public void Awake() {
            Logger = base.Logger;

            MoreArtifactsConfig.Init(Config);

            NetworkingAPI.RegisterMessageType<CombineMessage>();

            congregateArtifact = new CongregateArtifact();
            wholeArtifact = new ConglomerateArtifact();
            confusionArtifact = new ConfusionArtifact();
        }
    }
}

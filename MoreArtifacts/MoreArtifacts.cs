using System;
using BepInEx;
using UnityEngine;
using R2API.Utils;
using R2API.Networking;

namespace MoreArtifacts {

    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod)]
    [R2APISubmoduleDependency(nameof(NetworkingAPI))]
    public class MoreArtifacts : BaseUnityPlugin {
        public const string ModGUID = "com.ugff.moreartifacts";
        public const string ModName = "More Artifacts";
        public const string ModVersion = "1.2.0";


        public static new BepInEx.Logging.ManualLogSource Logger { get; private set; }

        public static CongregateArtifact congregateArtifact;
        public static ConglomerateArtifact wholeArtifact;

        public void Awake() {
            Logger = base.Logger;

            NetworkingAPI.RegisterMessageType<ScaleMessage>();

            CongregateArtifactManager.Init();
            congregateArtifact = new CongregateArtifact();

            ConglomerateArtifactManager.Init();
            wholeArtifact = new ConglomerateArtifact();
        }
    }
}

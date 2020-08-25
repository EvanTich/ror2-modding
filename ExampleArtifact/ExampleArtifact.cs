using BepInEx;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using MoreArtifacts;

namespace ExampleArtifact {

    /// <summary>
    /// Your artifact mod.
    /// </summary>
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(MoreArtifacts.MoreArtifacts.ModGUID, MoreArtifacts.MoreArtifacts.ModVersion)]
    public class ExampleArtifactMod : BaseUnityPlugin {
        public const string ModGUID = "com.ugff.exampleartifact";
        public const string ModName = "Example Artifact";
        public const string ModVersion = "1.0.0";


        internal static new BepInEx.Logging.ManualLogSource Logger { get; private set; }

        public static ExampleArtifact exampleArtifact;

        public void Awake() {
            Logger = base.Logger;

            // initialize artifacts and other things here
            exampleArtifact = new ExampleArtifact();
        }
    }

    /// <summary>
    /// The actual artifact definition.
    /// </summary>
    public class ExampleArtifact : NewArtifact<ExampleArtifact> {

        public override string Name => "Artifact of Examples";
        public override string Description => "In-game description of the artifact.";
        public override Sprite IconSelectedSprite => CreateSprite(Properties.Resources.example_selected, Color.magenta);
        public override Sprite IconDeselectedSprite => CreateSprite(Properties.Resources.example_deselected, Color.gray);

        protected override void InitManager() {
            ExampleArtifactManager.Init();
        }
    }

    /// <summary>
    /// Overarching Manager for this artifact. Handles hooking and unhooking actions.
    /// </summary>
    public static class ExampleArtifactManager {
        private static ArtifactDef myArtifact {
            get { return ExampleArtifact.Instance.ArtifactDef; }
        }

        private static int counter;

        public static void Init() {
            // initialize stuff here, like fields, properties, or things that should run only one time
            counter = 0;

            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        private static void OnArtifactEnabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(!NetworkServer.active || artifactDef != myArtifact) {
                return;
            }

            // hook things
            Run.onRunStartGlobal += Something;
        }

        private static void OnArtifactDisabled(RunArtifactManager man, ArtifactDef artifactDef) {
            if(artifactDef != myArtifact) {
                return;
            }

            // unhook things
            Run.onRunStartGlobal -= Something;
        }

        private static void Something(Run run) {
            counter++;
            ExampleArtifactMod.Logger.LogInfo($"Run started! Something done! counter = {counter}");
        }
    }
}

using System;
using RoR2;
using BepInEx;
using HarmonyLib;
using R2API.Utils;

namespace CommandChanges {

    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    public class CommandChanges : BaseUnityPlugin {
        public const string ModGUID = "com.ugff.commandchanges";
        public const string ModName = "Command Changes";
        public const string ModVersion = "1.1.0";

        public void Awake() {
            Harmony.CreateAndPatchAll(typeof(CommandChanges));
        }

        [HarmonyPatch(typeof(RoR2.Artifacts.CommandArtifactManager), "OnGenerateInteractableCardSelection", new Type[] { typeof(SceneDirector), typeof(DirectorCardCategorySelection) })]
        [HarmonyPrefix]
        static bool OnGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs) {
            dccs.RemoveCardsThatFailFilter(card => {
                UnityEngine.GameObject obj = card.spawnCard.prefab;
                return !(
                    obj.GetComponent<ShopTerminalBehavior>() && obj.name != "ShrineCleanse"
                    || obj.GetComponent<MultiShopController>() 
                    || obj.GetComponent<ScrapperController>()
                );
            });

            return false;
        }
    }
}

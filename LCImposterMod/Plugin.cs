using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCImposterMod.Patches;
using UnityEngine;
using TerminalApi;
using TerminalApi.Classes;
using static TerminalApi.TerminalApi;
using System;
using System.Collections;
using DunGen;
using UnityEngine.Windows;

namespace LCImposterMod
{
    [BepInDependency("LethalNetworkAPI")]
    [BepInPlugin(modGUID, modName, modVersion)]
    public class ImposterModBase: BaseUnityPlugin
    {
        private const string modGUID = "NecronWasTaken.LCImposterMod";
        private const string modName = "LC Imposter Mod";
        private const string modVersion = "0.0.1.0";
        private const string modAuthor = "NecronWasTaken";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static ImposterModBase Instance;

        internal static ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            // mls.LogInfo("The Imposter Mod has awaken :D");

            harmony.PatchAll(typeof(ImposterModBase));
            harmony.PatchAll(typeof(MainPatch));

            TerminalKeyword keyword = CreateTerminalKeyword("imposter", true);
            TerminalKeyword nounKeywordChance = CreateTerminalKeyword("chance");
            TerminalKeyword nounKeywordNickname = CreateTerminalKeyword("nickname");
            TerminalKeyword nounKeywordStatus = CreateTerminalKeyword("status");
            TerminalKeyword nounKeywordEnable = CreateTerminalKeyword("enable");
            TerminalKeyword nounKeywordDisable = CreateTerminalKeyword("disable");
            TerminalKeyword nounKeywordReset = CreateTerminalKeyword("reset");

            TerminalNode triggerNode = CreateTerminalNode("Something went wrong, I suppose...", true);

            keyword = keyword.AddCompatibleNoun(nounKeywordChance, triggerNode);
            keyword = keyword.AddCompatibleNoun(nounKeywordNickname, triggerNode);
            keyword = keyword.AddCompatibleNoun(nounKeywordStatus, triggerNode);
            keyword = keyword.AddCompatibleNoun(nounKeywordEnable, triggerNode);
            keyword = keyword.AddCompatibleNoun(nounKeywordDisable, triggerNode);
            keyword = keyword.AddCompatibleNoun(nounKeywordReset, triggerNode);

            AddTerminalKeyword(keyword);
            AddTerminalKeyword(nounKeywordChance);
            AddTerminalKeyword(nounKeywordNickname);
            AddTerminalKeyword(nounKeywordEnable);
            AddTerminalKeyword(nounKeywordDisable);
            AddTerminalKeyword(nounKeywordReset);
            AddTerminalKeyword(nounKeywordStatus, new CommandInfo
            {
                TriggerNode = triggerNode,
                Category = "other",
                Description = "Set an imposter settings for the next round.",
                DisplayTextSupplier = MainPatch.SetImposterSettings
            });
        }
    }
}

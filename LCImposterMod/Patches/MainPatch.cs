using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using TerminalApi;
using TerminalApi.Classes;
using static TerminalApi.TerminalApi;
using System;

namespace LCImposterMod.Patches
{
    [HarmonyPatch]
    public class MainPatch : NetworkBehaviour
    {
        private static string nickname = "imposter";
        private static int spawnChance = 30;

        private static bool isRoundStarted = false;

        private static TextMeshProUGUI _roleText;
        private static readonly LethalServerMessage<string> customServerMessage = new LethalServerMessage<string>(identifier: "role", ReceiveByServer);
        private static readonly LethalClientMessage<string> customClientMessage = new LethalClientMessage<string>(identifier: "role", ReceiveFromServer);

        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPostfix]
        static void StartHudPatch(ref HUDManager __instance)
        {
            if (!_roleText)
                AddRoleToHUD(__instance);
        }

        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
        [HarmonyPostfix]
        static void StartGamePatch(ref StartOfRound __instance)
        { 
            int playerCount = __instance.ClientPlayerList.Count;
            ulong[] playersIds = __instance.ClientPlayerList.Keys.ToArray();
            
            /*ImposterModBase.mls.LogInfo("Start game! Is server? :" + __instance.IsServer);
            ImposterModBase.mls.LogInfo("Player list:");
            foreach (var client in __instance.ClientPlayerList)
            {
                ImposterModBase.mls.LogInfo(client.Key + " " + client.Value);
            }*/

            int randomNumber = UnityEngine.Random.Range(0, 100);
            // ImposterModBase.mls.LogInfo("Random number (spawn rate): " + randomNumber);
            if (randomNumber < spawnChance)
            {
                randomNumber = UnityEngine.Random.Range(0, playerCount);
                customServerMessage.SendClient(nickname, (ulong)randomNumber);
                // ImposterModBase.mls.LogInfo("Random number (who it is): " + randomNumber + " Player id: " + playersIds[randomNumber]);
            }
            isRoundStarted = true;
        }

        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        [HarmonyPostfix]
        static void EndOfGamePatch(ref StartOfRound __instance)
        {
            _roleText.text = "crewmate";
            _roleText.color = Color.white;
            isRoundStarted = false;
        }

        private static void AddRoleToHUD(HUDManager __instance)
        {
            GameObject val = new GameObject("RoleHUDDisplay");
            val.AddComponent<RectTransform>();

            TextMeshProUGUI obj = val.AddComponent<TextMeshProUGUI>();
            RectTransform rectTransform = ((TMP_Text)obj).rectTransform;
            ((Transform)rectTransform).SetParent(((Component)__instance.PTTIcon).transform, false);
            rectTransform.anchoredPosition = new Vector2(0f, -5f);
            ((TMP_Text)obj).font = ((TMP_Text)__instance.controlTipLines[0]).font;
            ((TMP_Text)obj).fontSize = 12f;
            ((TMP_Text)obj).text = "crewmate";
            ((TMP_Text)obj).overflowMode = (TextOverflowModes)0;
            ((Behaviour)obj).enabled = true;

            _roleText = obj;
        }

        private static void ReceiveByServer(string data, ulong id)
        {
            // ImposterModBase.mls.LogInfo("Message by server recieved!" + data);
            _roleText.text = data;
            _roleText.color = Color.red;
        }
        private static void ReceiveFromServer(string data)
        {
            // ImposterModBase.mls.LogInfo("Message from server recieved!" + data);
            _roleText.text = data;
            _roleText.color = Color.red;
        }
        
        private static void ResetSettings()
        {   
            nickname = "imposter";
            spawnChance = 30;
            _roleText.enabled = true;
        }
        public static string SetImposterSettings()
        {
            if (!StartOfRound.Instance.IsServer)
            {
                return $"Only lobby owner can use these commands.\n";
            }
            string input = GetTerminalInput();

            string[] array = input.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);

            if (array[1] == "status") return $"Enabled: {_roleText.enabled}\nNickname: {nickname}\nSpawn chance: {spawnChance}%\n";

            if (isRoundStarted) return $"You can't change mod settings during the round.\n";
            try
            {
                switch (array[1])
                {
                    case "enable":
                        _roleText.enabled = true;
                        return $"Imposter mod enabled.\n";

                    case "disable":
                        _roleText.enabled = false;
                        return $"Imposter mod disabled.\n";

                    case "reset":
                        ResetSettings();
                        return $"Imposter mod settings are reset to default.\n";

                    case "nickname":
                        nickname = array[2];
                        return $"Imposter nickname are set to: {nickname}\n";

                    case "chance":
                        int chance = Int32.Parse(array[2]);
                        spawnChance = chance;
                        return $"Imposter spawn chance are set to: {array[2]}%\n";

                    default:
                        return "Something went wrong?\n";
                }
            }
            catch (FormatException)
            {
                return $"Invalid input format.\n";
            }
        }
    }
}

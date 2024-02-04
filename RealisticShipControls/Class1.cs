using HarmonyLib;
using PulsarModLoader;
using PulsarModLoader.Keybinds;
using PulsarModLoader.MPModChecks;
using PulsarModLoader.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace RealisticShipControls
{
    [HarmonyPatch(typeof(PLShipInfoBase), "Update")]
    class Test
    {
        public enum FA_MODE { None, Angular, Linear };
        public static FA_MODE faMode = FA_MODE.None;
        public static bool faToggle = true;

        public static string GetMode()
        {
            if (Test.faToggle)
                return "FlightAssistFull";
            switch (Test.faMode)
            {
                case Test.FA_MODE.None:
                    return "FlightAssistOff";
                case Test.FA_MODE.Angular:
                    return "FlightAssistAngular";
                case Test.FA_MODE.Linear:
                    return "FlightAssistLinear";
            }

            return null;
        }

        public static void UpdateFAMode(PLShipInfoBase currentShip, string buttonPressed)
        {
            float AngDragMultiplier = Traverse.Create(currentShip).Field("AngDragMultiplier").GetValue<float>();
            float DragMultiplier = Traverse.Create(currentShip).Field("DragMultiplier").GetValue<float>();

            switch (buttonPressed)
            {
                case "FlightAssistFull":
                    AngDragMultiplier = 1.3f;
                    DragMultiplier = 1f;
                    break;
                case "FlightAssistAngular":
                    AngDragMultiplier = 1.3f;
                    DragMultiplier = 0f;
                    Test.faMode = FA_MODE.Angular;
                    break;
                case "FlightAssistLinear":
                    AngDragMultiplier = 0f;
                    DragMultiplier = 1f;
                    Test.faMode = FA_MODE.Linear;
                    break;
                case "FlightAssistOff":
                    AngDragMultiplier = 0f;
                    DragMultiplier = 0f;
                    Test.faMode = FA_MODE.None;
                    break;
            }

            Traverse.Create(currentShip).Field("AngDragMultiplier").SetValue(AngDragMultiplier);
            Traverse.Create(currentShip).Field("DragMultiplier").SetValue(DragMultiplier);
        }
        
        static void Postfix(PLShipInfoBase __instance, ref float ___AngDragMultiplier, ref float ___DragMultiplier)
        {
            if (__instance.ExteriorRigidbody != null && !__instance.InWarp && !__instance.FlightAIEnabled && ___DragMultiplier == 0f)
                __instance.ExteriorRigidbody.drag = 0;

            if (__instance.InWarp || __instance.FlightAIEnabled)
            {
                ___AngDragMultiplier = 1.3f;
                ___DragMultiplier = 1f;
            }
        }
    }

    [HarmonyPatch(typeof(PLUIPilotingScreen), "SetupUI")]
    class PilotingScreen_SetupUI
    {
        static void Postfix(PLUIPilotingScreen __instance, UISprite ___MyPanel)
        {
            Traverse.Create(__instance).Method("CreateLabel", "Flight Assist", new Vector3(300f, -50f), 15, Color.white, ___MyPanel.transform, UIWidget.Pivot.TopLeft);
            UISprite b1 = Traverse.Create(__instance).Method("CreateButton", "FlightAssistFull", "Full assist", new Vector3(280f, -90f), new Vector2(220f, 40f),
                Color.white, ___MyPanel.transform, UIWidget.Pivot.TopLeft).GetValue() as UISprite;
            UISprite b2 = Traverse.Create(__instance).Method("CreateButton", "FlightAssistAngular", "Angular only", new Vector3(280f, -140f), new Vector2(220f, 40f),
                Color.white, ___MyPanel.transform, UIWidget.Pivot.TopLeft).GetValue() as UISprite;
            UISprite b3 = Traverse.Create(__instance).Method("CreateButton", "FlightAssistLinear", "Linear only", new Vector3(280f, -190f), new Vector2(220f, 40f),
                Color.white, ___MyPanel.transform, UIWidget.Pivot.TopLeft).GetValue() as UISprite;
            UISprite b4 = Traverse.Create(__instance).Method("CreateButton", "FlightAssistOff", "Disable", new Vector3(280f, -240f), new Vector2(220f, 40f),
                Color.white, ___MyPanel.transform, UIWidget.Pivot.TopLeft).GetValue() as UISprite;

            __instance.OnButtonMouseAway(b1);
            __instance.OnButtonMouseAway(b2);
            __instance.OnButtonMouseAway(b3);
            __instance.OnButtonMouseAway(b4);
        }
    }

    [HarmonyPatch(typeof(PLUIPilotingScreen), "OnButtonClick")]
    class PilotingScreen_OnButtonClick
    {
        static void Postfix(PLUIPilotingScreen __instance, UIWidget inButton)
        {
            if (inButton.name == "FlightAssistFull" || inButton.name == "FlightAssistAngular" ||
                inButton.name == "FlightAssistLinear" || inButton.name == "FlightAssistOff")
            {
                ModMessage.SendRPC("DanX100.RealisticShipControls", "RealisticShipControls.ToggleControls", PhotonTargets.MasterClient, new object[] {
                    inButton.name,
                    __instance.MyScreenHubBase.OptionalShipInfo.ShipID
                });

                if (!PhotonNetwork.isMasterClient)
                {
                    Test.UpdateFAMode(__instance.MyScreenHubBase.OptionalShipInfo, inButton.name);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PLUIPilotingScreen), "Update")]
    class PilotingScreen_Update
    {
        static void Postfix(PLUIPilotingScreen __instance, List<UIWidget> ___AllButtons)
        {
            foreach (UIWidget widget in ___AllButtons)
            {
                bool on = Test.GetMode() == widget.name;

                switch (widget.name)
                {
                    case "FlightAssistFull":
                        if (on)
                        {
                            widget.alpha = 1f;
                            widget.color = Color.white;
                        } else
                        {
                            widget.color = Color.gray * 0.5f;
                        }
                        break;
                    case "FlightAssistAngular":
                        if (on)
                        {
                            widget.alpha = 1f;
                            widget.color = Color.white;
                        }
                        else
                        {
                            widget.color = Color.gray * 0.5f;
                        }
                        break;
                    case "FlightAssistLinear":
                        if (on)
                        {
                            widget.alpha = 1f;
                            widget.color = Color.white;
                        }
                        else
                        {
                            widget.color = Color.gray * 0.5f;
                        }
                        break;
                    case "FlightAssistOff":
                        if (on)
                        {
                            widget.alpha = 1f;
                            widget.color = Color.white;
                        }
                        else
                        {
                            widget.color = Color.gray * 0.5f;
                        }
                        break;
                }
            }
        }
    }

    class ToggleControls : PulsarModLoader.ModMessage
    {
        public override void HandleRPC(object[] arguments, PhotonMessageInfo sender)
        {
            
            string buttonPressed = (string)arguments[0];
            PLShipInfoBase shipFromID = PLEncounterManager.Instance.GetShipFromID((int)arguments[1]);
            if (shipFromID != null)
            {
                Test.UpdateFAMode(shipFromID, buttonPressed);

                string msg = "[PL] has set the Flight Assist to ";
                switch (buttonPressed)
                {
                    case "FlightAssistFull":
                        msg += "full";
                        break;
                    case "FlightAssistAngular":
                        msg += "angular";
                        break;
                    case "FlightAssistLinear":
                        msg += "linear";
                        break;
                    case "FlightAssistOff":
                        msg += "disable";
                        break;
                }

                PLPlayer playerForPhotonPlayer = PLServer.GetPlayerForPhotonPlayer(sender.sender);
                if (playerForPhotonPlayer != null && playerForPhotonPlayer.TeamID == 0 && !playerForPhotonPlayer.IsBot)
                {
                    PLPlayer cachedFriendlyPlayerOfClass = PLServer.Instance.GetCachedFriendlyPlayerOfClass(1);
                    if (cachedFriendlyPlayerOfClass != null && playerForPhotonPlayer != cachedFriendlyPlayerOfClass)
                    {
                        PLServer.Instance.photonView.RPC("AddNotificationLocalize", cachedFriendlyPlayerOfClass.GetPhotonPlayer(), new object[]
                        {
                            msg,
                            playerForPhotonPlayer.GetPlayerID(),
                            PLServer.Instance.GetEstimatedServerMs() + 6000,
                            true
                        });
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PLInGameUI), "Update")]
    class HandleKeybinds
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            //If piloting
            if (PLNetworkManager.Instance.MyLocalPawn.CurrentShip.GetCurrentShipControllerPlayerID() == PLNetworkManager.Instance.LocalPlayerID)
            {
                PLShipInfo currentShip = PLNetworkManager.Instance.MyLocalPawn.CurrentShip;

                if (KeybindManager.Instance.GetButtonDown("fatoggle"))
                {
                    Test.faToggle = !Test.faToggle;
                    Test.UpdateFAMode(currentShip, Test.GetMode());

                    Messaging.Notification($"FA Toggled {(Test.faToggle ? "On" : "Off")}");
                }
            }
        }
    }
}

class Mod : PulsarMod, IKeybind
{
    public Mod() : base()
    {
        // Do additional setup here
    }

    public override string HarmonyIdentifier()
    {
        return "DanX100.RealisticShipControls";
    }

    public void RegisterBinds(KeybindManager manager)
    {
        manager.NewBind("FAToggle", "fatoggle", "Piloting", "o");
    }

    public override string Version => "1.0.4";
    public override string Author => "DanX100";
    public override string ShortDescription => "Adds realistic controls to your ship!";
    public override string LongDescription => "The mod allows you to disable the inertia compensation of the ship, which will significantly complicate the control of the ship."+
        "\nThe mod adds 4 control modes for the pilot:"+
        "\n 1) Full assist. In this mode, the ship is controlled as before, slowing down and stopping rotation when the engines are turned off."+
        "\n 2) Angular only. In this mode, the ship does not slow down, but slows down the rotation."+
        "\n 3) Linear only. In this mode, the ship does not stop rotating, but the movement slows down."+
        "\n 4) Disable. In this mode, all stabilizations are disabled and the ship behaves like a cosmic body."+
        "\n\nUsage:"+
        "\n 1) select the desired Flight Assist mode on the piloting panel"+
        "\n 2) take control of the ship"+
        "\n 3) Enjoy!"+
        "\n\nThanks to beta testers UnluckySlava, TipanLogic and FLUR."+
        "\nSpecial thanks to pokegustavo and EngBot for help in creating this mod.";
    public override int MPRequirements => (int)MPRequirement.All;
}
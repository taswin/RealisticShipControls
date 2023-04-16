using HarmonyLib;
using PulsarModLoader;
using PulsarModLoader.MPModChecks;
using UnityEngine;

namespace RealisticShipControls
{
    [HarmonyPatch(typeof(PLShipInfoBase), "Update")]
    class Test
    {
        static void Postfix(PLShipInfoBase __instance, ref float ___AngDragMultiplier, ref float ___DragMultiplier)
        {
            if (__instance.ExteriorRigidbody != null && !__instance.InWarp && !__instance.FlightAIEnabled && __instance.DragMultiplier == 0f) 
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
        static void Postfix(PLUIPilotingScreen __instance)
        {
            __instance.CreateLabel("Flight Assist", new Vector3(300f, -50f), 15, Color.white, __instance.MyPanel.transform, UIWidget.Pivot.TopLeft);
            UISprite b1 = __instance.CreateButton("FlightAssistFull", "Full assist", new Vector3(280f, -90f), new Vector2(220f, 40f),
                Color.white, __instance.MyPanel.transform, UIWidget.Pivot.TopLeft);
            UISprite b2 = __instance.CreateButton("FlightAssistAngular", "Angular only", new Vector3(280f, -140f), new Vector2(220f, 40f),
                Color.white, __instance.MyPanel.transform, UIWidget.Pivot.TopLeft);
            UISprite b3 = __instance.CreateButton("FlightAssistLinear", "Linear only", new Vector3(280f, -190f), new Vector2(220f, 40f),
                Color.white, __instance.MyPanel.transform, UIWidget.Pivot.TopLeft);
            UISprite b4 = __instance.CreateButton("FlightAssistOff", "Disable", new Vector3(280f, -240f), new Vector2(220f, 40f),
                Color.white, __instance.MyPanel.transform, UIWidget.Pivot.TopLeft);

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
                    switch (inButton.name)
                    {
                        case "FlightAssistFull":
                            __instance.MyScreenHubBase.OptionalShipInfo.AngDragMultiplier = 1.3f;
                            __instance.MyScreenHubBase.OptionalShipInfo.DragMultiplier = 1f;
                            break;
                        case "FlightAssistAngular":
                            __instance.MyScreenHubBase.OptionalShipInfo.AngDragMultiplier = 1.3f;
                            __instance.MyScreenHubBase.OptionalShipInfo.DragMultiplier = 0f;
                            break;
                        case "FlightAssistLinear":
                            __instance.MyScreenHubBase.OptionalShipInfo.AngDragMultiplier = 0f;
                            __instance.MyScreenHubBase.OptionalShipInfo.DragMultiplier = 1f;
                            break;
                        case "FlightAssistOff":
                            __instance.MyScreenHubBase.OptionalShipInfo.AngDragMultiplier = 0f;
                            __instance.MyScreenHubBase.OptionalShipInfo.DragMultiplier = 0f;
                            break;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PLUIPilotingScreen), "Update")]
    class PilotingScreen_Update
    {
        static void Postfix(PLUIPilotingScreen __instance)
        {
            foreach (UIWidget widget in __instance.AllButtons)
            {
                switch (widget.name)
                {
                    case "FlightAssistFull":
                        if (__instance.MyScreenHubBase.OptionalShipInfo.AngDragMultiplier == 1.3f &&
                            __instance.MyScreenHubBase.OptionalShipInfo.DragMultiplier == 1f)
                        {
                            widget.alpha = 1f;
                            widget.color = Color.white;
                        } else
                        {
                            widget.color = Color.gray * 0.5f;
                        }
                        break;
                    case "FlightAssistAngular":
                        if (__instance.MyScreenHubBase.OptionalShipInfo.AngDragMultiplier == 1.3f &&
                            __instance.MyScreenHubBase.OptionalShipInfo.DragMultiplier == 0f)
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
                        if (__instance.MyScreenHubBase.OptionalShipInfo.AngDragMultiplier == 0f &&
                            __instance.MyScreenHubBase.OptionalShipInfo.DragMultiplier == 1f)
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
                        if (__instance.MyScreenHubBase.OptionalShipInfo.AngDragMultiplier == 0f &&
                            __instance.MyScreenHubBase.OptionalShipInfo.DragMultiplier == 0f)
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
                string msg = "[PL] has set the Flight Assist to ";
                switch (buttonPressed)
                {
                    case "FlightAssistFull":
                        shipFromID.AngDragMultiplier = 1.3f;
                        shipFromID.DragMultiplier = 1f;
                        msg += "full";
                        break;
                    case "FlightAssistAngular":
                        shipFromID.AngDragMultiplier = 1.3f;
                        shipFromID.DragMultiplier = 0f;
                        msg += "angular";
                        break;
                    case "FlightAssistLinear":
                        shipFromID.AngDragMultiplier = 0f;
                        shipFromID.DragMultiplier = 1f;
                        msg += "linear";
                        break;
                    case "FlightAssistOff":
                        shipFromID.AngDragMultiplier = 0f;
                        shipFromID.DragMultiplier = 0f;
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
}

class Mod : PulsarMod
{
    public Mod() : base()
    {
        // Do additional setup here
    }

    public override string HarmonyIdentifier()
    {
        return "DanX100.RealisticShipControls";
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
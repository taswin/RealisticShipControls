using System.Collections.Generic;
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
                    float AngDragMultiplier = Traverse.Create(__instance.MyScreenHubBase.OptionalShipInfo).Field("AngDragMultiplier").GetValue<float>();
                    float DragMultiplier = Traverse.Create(__instance.MyScreenHubBase.OptionalShipInfo).Field("DragMultiplier").GetValue<float>();

                    switch (inButton.name)
                    {
                        case "FlightAssistFull":
                            AngDragMultiplier = 1.3f;
                            DragMultiplier = 1f;
                            break;
                        case "FlightAssistAngular":
                            AngDragMultiplier = 1.3f;
                            DragMultiplier = 0f;
                            break;
                        case "FlightAssistLinear":
                            AngDragMultiplier = 0f;
                            DragMultiplier = 1f;
                            break;
                        case "FlightAssistOff":
                            AngDragMultiplier = 0f;
                            DragMultiplier = 0f;
                            break;
                    }

                    Traverse.Create(__instance.MyScreenHubBase.OptionalShipInfo).Field("AngDragMultiplier").SetValue(AngDragMultiplier);
                    Traverse.Create(__instance.MyScreenHubBase.OptionalShipInfo).Field("DragMultiplier").SetValue(DragMultiplier);
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
                float AngDragMultiplier = Traverse.Create(__instance.MyScreenHubBase.OptionalShipInfo).Field("AngDragMultiplier").GetValue<float>();
                float DragMultiplier = Traverse.Create(__instance.MyScreenHubBase.OptionalShipInfo).Field("DragMultiplier").GetValue<float>();
                switch (widget.name)
                {
                    case "FlightAssistFull":
                        if (AngDragMultiplier == 1.3f &&
                            DragMultiplier == 1f)
                        {
                            widget.alpha = 1f;
                            widget.color = Color.white;
                        } else
                        {
                            widget.color = Color.gray * 0.5f;
                        }
                        break;
                    case "FlightAssistAngular":
                        if (AngDragMultiplier == 1.3f &&
                            DragMultiplier == 0f)
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
                        if (AngDragMultiplier == 0f &&
                            DragMultiplier == 1f)
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
                        if (AngDragMultiplier == 0f &&
                            DragMultiplier == 0f)
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
                float AngDragMultiplier = Traverse.Create(shipFromID).Field("AngDragMultiplier").GetValue<float>();
                float DragMultiplier = Traverse.Create(shipFromID).Field("DragMultiplier").GetValue<float>();

                string msg = "[PL] has set the Flight Assist to ";
                switch (buttonPressed)
                {
                    case "FlightAssistFull":
                        AngDragMultiplier = 1.3f;
                        DragMultiplier = 1f;
                        msg += "full";
                        break;
                    case "FlightAssistAngular":
                        AngDragMultiplier = 1.3f;
                        DragMultiplier = 0f;
                        msg += "angular";
                        break;
                    case "FlightAssistLinear":
                        AngDragMultiplier = 0f;
                        DragMultiplier = 1f;
                        msg += "linear";
                        break;
                    case "FlightAssistOff":
                        AngDragMultiplier = 0f;
                        DragMultiplier = 0f;
                        msg += "disable";
                        break;
                }

                Traverse.Create(shipFromID).Field("AngDragMultiplier").SetValue(AngDragMultiplier);
                Traverse.Create(shipFromID).Field("DragMultiplier").SetValue(DragMultiplier);

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
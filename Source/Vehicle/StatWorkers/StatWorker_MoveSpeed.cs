﻿using System.Collections.Generic;
using System.Text;
#if CR
using Combat_Realism;
#endif
using RimWorld;
using ToolsForHaul.Components;
using ToolsForHaul.Utilities;
using UnityEngine;
using Verse;

namespace ToolsForHaul.StatWorkers
{
    internal class StatWorker_MoveSpeed : StatWorker
    {
        public override string GetExplanation(StatRequest req, ToStringNumberSense numberSense)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetExplanation(req, numberSense));
            if (req.HasThing)
            {
                Pawn thisPawn = req.Thing as Pawn;

                if (thisPawn != null)
                {
                    if (thisPawn.RaceProps.intelligence >= Intelligence.ToolUser)
                    {
                        if (MapComponent_ToolsForHaul.currentVehicle.ContainsKey(thisPawn))
                        {
                            Vehicle_Cart vehicle_Cart = MapComponent_ToolsForHaul.currentVehicle[thisPawn] as Vehicle_Cart;
                            if (vehicle_Cart != null)
                                if (vehicle_Cart.MountableComp.IsMounted && vehicle_Cart.MountableComp.Driver == thisPawn)
                                {
                                    stringBuilder.AppendLine();
                                    stringBuilder.AppendLine("VehicleSpeed".Translate() + ": x" + vehicle_Cart.VehicleComp.VehicleSpeed);
                                    return stringBuilder.ToString();
                                }

                            Vehicle_Turret vehicle_Turret = MapComponent_ToolsForHaul.currentVehicle[req.Thing as Pawn] as Vehicle_Turret;
                            if (vehicle_Turret != null)
                                if (vehicle_Turret.mountableComp.IsMounted && vehicle_Turret.mountableComp.Driver == thisPawn)
                                {
                                    stringBuilder.AppendLine();
                                    stringBuilder.AppendLine("VehicleSpeed".Translate() + ": x" + vehicle_Turret.vehicleComp.VehicleSpeed);
                                    return stringBuilder.ToString();
                                }
                        }

#if CR
                        CompInventory compInventory = ThingCompUtility.TryGetComp<CompInventory>(req.Thing);
                        if (compInventory != null)
                        {
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine(Translator.Translate("CR_CarriedWeight") + ": x" + GenText.ToStringPercent(compInventory.moveSpeedFactor));
                            if (compInventory.encumberPenalty > 0f)
                            {
                                stringBuilder.AppendLine(Translator.Translate("CR_Encumbered") + ": -" + GenText.ToStringPercent(compInventory.encumberPenalty));
                                stringBuilder.AppendLine(Translator.Translate("CR_FinalModifier") + ": x" + GenText.ToStringPercent(this.GetStatFactor(thisPawn)));
                            }
                        }
#endif
                        CompSlotsBackpack compSlotsBackpack = ToolsForHaulUtility.TryGetBackpack(thisPawn).TryGetComp<CompSlotsBackpack>();
                        if (compSlotsBackpack != null)
                        {

                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine("CR_CarriedWeightBackpack".Translate() + ": x" + compSlotsBackpack.moveSpeedFactor.ToStringPercent());
                            if (compSlotsBackpack.encumberPenalty > 0f)
                            {
                                stringBuilder.AppendLine("CR_EncumberedBackpack".Translate() + ": -" + compSlotsBackpack.encumberPenalty.ToStringPercent());
                                stringBuilder.AppendLine("CR_FinalModifierBackpack".Translate() + ": x" + this.GetStatFactor(thisPawn).ToStringPercent());
                            }
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {

            float num = base.GetValueUnfinalized(req, applyPostProcess);

            if (req.HasThing && req.Thing is Pawn)
            {
                Pawn pawn = req.Thing as Pawn;

                if (pawn.RaceProps.intelligence >= Intelligence.ToolUser)

                    if (this.GetStatFactor(pawn) > 1.01f)
                    {
                        num = this.GetStatFactor(pawn);
                    }
                    else
                    {
                        num *= this.GetStatFactor(pawn);
                    }
            }

            return num;

        }

        private float GetStatFactor(Pawn thisPawn)
        {
            float result = 1f;

            if (MapComponent_ToolsForHaul.currentVehicle.ContainsKey(thisPawn))
            {
                Vehicle_Cart vehicleCart = MapComponent_ToolsForHaul.currentVehicle[thisPawn] as Vehicle_Cart;
                if (vehicleCart != null)
                {
                    if (vehicleCart.MountableComp.IsMounted && !vehicleCart.MountableComp.Driver.RaceProps.Animal && vehicleCart.MountableComp.Driver == thisPawn)
                    {
                        if (vehicleCart.VehicleComp.IsCurrentlyMotorized())
                        {
                            result = Mathf.Clamp(vehicleCart.VehicleComp.VehicleSpeed, 2f, 100f);
                        }
                        else
                        {
                            result = Mathf.Clamp(vehicleCart.VehicleComp.VehicleSpeed, 0.5f, 1f);
                        }

                        return result;
                    }
                }

                Vehicle_Turret vehicleTank = MapComponent_ToolsForHaul.currentVehicle[thisPawn] as Vehicle_Turret;
                if (vehicleTank != null)
                {
                    if (vehicleTank.mountableComp.IsMounted && !vehicleTank.mountableComp.Driver.RaceProps.Animal && vehicleTank.mountableComp.Driver == thisPawn)
                    {
                        if (vehicleTank.vehicleComp.IsCurrentlyMotorized())
                        {
                            result = Mathf.Clamp(vehicleTank.vehicleComp.VehicleSpeed, 2f, 100f);
                        }
                        else
                        {
                            result = Mathf.Clamp(vehicleTank.vehicleComp.VehicleSpeed, 0.5f, 1f);
                        }

                        return result;
                    }
                }
            }

            Apparel_Backpack apparelBackpack = ToolsForHaulUtility.TryGetBackpack(thisPawn);
            CompSlotsBackpack compSlotsBackpack = apparelBackpack?.slotsComp;
            if (compSlotsBackpack != null)
            {
                result = Mathf.Clamp(compSlotsBackpack.moveSpeedFactor - compSlotsBackpack.encumberPenalty, 0.5f, 1f);
            }

#if CR
            CompInventory compInventory = thisPawn.TryGetComp<CompInventory>();
            if (compInventory != null)
            {
                result = Mathf.Clamp(compInventory.moveSpeedFactor - compInventory.encumberPenalty, 0.1f, 1f);
                return result;
            }
#endif
            return result;
        }
    }
}
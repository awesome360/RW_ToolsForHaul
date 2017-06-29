﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompMountable.cs" company="">
// </copyright>
// <summary>
//   Defines the CompMountable type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ToolsForHaul.Components
{
    using System;
    using System.Collections.Generic;

    using RimWorld;

    using ToolsForHaul.Defs;
    using ToolsForHaul.Designators;
    using ToolsForHaul.Utilities;
    using ToolsForHaul.Vehicles;

    using UnityEngine;

    using Verse;
    using Verse.AI;
    using Verse.Sound;

    public class CompMountable : ThingComp
    {
        private Pawn driver;

        public float lastDrawAsAngle;

        private Building_Door lastPassedDoor;

        private Sustainer sustainerAmbient;

        private int tickCheck = Find.TickManager.TicksGame;

        private int tickCooldown = 60;

        private int tickLastDoorCheck = Find.TickManager.TicksGame;

        public bool IsPrisonBreaking;

        public Vector3 InteractionOffset => this.parent.def.interactionCellOffset.ToVector3().RotatedBy(this.lastDrawAsAngle);

        public bool IsMounted => this.Driver != null;

        public Vector3 Position
        {
            get
            {
                Vector3 position;

                position = this.Driver.DrawPos - this.InteractionOffset * 1.3f;

                // No Driver
                if (this.Driver == null) return this.parent.DrawPos;

                // Out of bound or Preventing cart from stucking door
                if (!position.InBounds(this.parent.Map)) return this.Driver.DrawPos;

                if (!position.ToIntVec3().Walkable(this.parent.Map)) return this.Driver.DrawPos;

                return position;
            }
        }

        public Sustainer SustainerAmbient
        {
            get
            {
                return this.sustainerAmbient;
            }

            set
            {
                this.sustainerAmbient = value;
            }
        }

        public Pawn Driver
        {
            get
            {
                return this.driver;
            }

            set
            {
                this.driver = value;
            }
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo compCom in base.CompGetGizmosExtra())
            {
                yield return compCom;
            }

            if (this.parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }


        }

        public override void CompTick()
        {
            if (this.driver == null)
            {
                return;
            }

            base.CompTick();
            if (!this.IsMounted)
            {
                return;
            }

            if (!this.Driver.Spawned)
            {
                this.parent.DeSpawn();
                return;
            }

            if (this.Driver.Dead || this.Driver.Downed || this.Driver.health.InPainShock || this.Driver.InMentalState
                || (this.parent.IsForbidden(Faction.OfPlayer) && this.Driver.Faction == Faction.OfPlayer))
            {
                if (!this.Driver.Position.InBounds(this.parent.Map))
                {
                    this.DismountAt(this.Driver.Position);
                    return;
                }

                this.DismountAt(
                    this.Driver.Position - this.parent.def.interactionCellOffset.RotatedBy(this.Driver.Rotation));
                this.Driver.Position = this.Driver.Position.RandomAdjacentCell8Way();
                return;
            }

            if (Find.TickManager.TicksGame - this.tickCheck >= this.tickCooldown)
            {
                if (this.Driver.Faction == Faction.OfPlayer && this.Driver.CurJob != null)
                {
                    if (this.Driver.CurJob.def.playerInterruptible
                        && (this.Driver.CurJob.def == JobDefOf.GotoWander || this.Driver.CurJob.def == JobDefOf.Open
                            || this.Driver.CurJob.def == JobDefOf.ManTurret
                            || this.Driver.CurJob.def == JobDefOf.EnterCryptosleepCasket
                            || this.Driver.CurJob.def == JobDefOf.UseNeurotrainer
                            || this.Driver.CurJob.def == JobDefOf.UseArtifact
                            || this.Driver.CurJob.def == JobDefOf.DoBill || this.Driver.CurJob.def == JobDefOf.Research

                            // || this.Driver.CurJob.def == JobDefOf.OperateDeepDrill
                            || this.Driver.CurJob.def == JobDefOf.Repair
                            || this.Driver.CurJob.def == JobDefOf.FixBrokenDownBuilding
                            || this.Driver.CurJob.def == JobDefOf.UseCommsConsole
                            || this.Driver.CurJob.def == JobDefOf.BuryCorpse
                            || this.Driver.CurJob.def == JobDefOf.TradeWithPawn
                            || this.Driver.CurJob.def == JobDefOf.Lovin
                            || this.Driver.CurJob.def == JobDefOf.SocialFight
                            || this.Driver.CurJob.def == JobDefOf.Maintain
                            || this.Driver.CurJob.def == JobDefOf.MarryAdjacentPawn
                            || this.Driver.CurJob.def == JobDefOf.SpectateCeremony
                            || this.Driver.CurJob.def == JobDefOf.StandAndBeSociallyActive
                            || this.Driver.CurJob.def == JobDefOf.LayDown || this.Driver.CurJob.def == JobDefOf.Ingest
                            || this.Driver.CurJob.def == JobDefOf.SocialRelax
                            || this.Driver.CurJob.def == JobDefOf.Refuel

                            // || this.Driver.CurJob.def == JobDefOf.FillFermentingBarrel
                            // || this.Driver.CurJob.def == JobDefOf.TakeBeerOutOfFermentingBarrel
                            || this.Driver.CurJob.def == JobDefOf.TakeWoundedPrisonerToBed
                            || this.Driver.CurJob.def == JobDefOf.TakeToBedToOperate
                            || this.Driver.CurJob.def == JobDefOf.EscortPrisonerToBed
                            || this.Driver.CurJob.def == JobDefOf.CarryToCryptosleepCasket
                            || this.Driver.CurJob.def == JobDefOf.ReleasePrisoner
                            || this.Driver.CurJob.def == JobDefOf.PrisonerAttemptRecruit
                            || this.Driver.CurJob.def == JobDefOf.PrisonerFriendlyChat
                            || this.Driver.CurJob.def == JobDefOf.PrisonerExecution
                            || this.Driver.CurJob.def == JobDefOf.FeedPatient
                            || this.Driver.CurJob.def == JobDefOf.TendPatient
                            || this.Driver.CurJob.def == JobDefOf.VisitSickPawn
                            || this.Driver.CurJob.def == JobDefOf.Slaughter || this.Driver.CurJob.def == JobDefOf.Milk
                            || this.Driver.CurJob.def == JobDefOf.Shear || this.Driver.CurJob.def == JobDefOf.Train
                            || this.Driver.CurJob.def == JobDefOf.Mate || this.Driver.health.HasHediffsNeedingTend())
                        && this.Driver.Position.Roofed(this.Driver.Map))
                    {
                        this.parent.Position = this.Position.ToIntVec3();
                        this.parent.Rotation = this.Driver.Rotation;
                        if (!this.Driver.Position.InBounds(this.parent.Map))
                        {
                            this.DismountAt(this.Driver.Position);
                            return;
                        }

                        this.DismountAt(this.Driver.Position - this.InteractionOffset.ToIntVec3());
                        this.Driver.Position = this.Driver.Position.RandomAdjacentCell8Way();
                        return;
                    }
                }

                this.tickCheck = Find.TickManager.TicksGame;
                this.tickCooldown = Rand.RangeInclusive(60, 180);

                CompVehicle vehicleComp = this.parent.TryGetComp<CompVehicle>();

                // bring vehicles home
                if (vehicleComp != null && !vehicleComp.MotorizedWithoutFuel())
                {
                    CompRefuelable refuelableComp = this.parent.TryGetComp<CompRefuelable>();
                    Job jobNew = TFH_Utility.DismountAtParkingLot(
                        this.Driver,
                        GameComponentToolsForHaul.CurrentDrivers[this.Driver]);
                    float hitPointsPercent = this.parent.HitPoints / this.parent.MaxHitPoints;

                    if (this.Driver.Faction == Faction.OfPlayer)
                    {
                        if (!GenAI.EnemyIsNear(this.Driver, 40f))
                        {
                            if (hitPointsPercent < 0.65f
                                || (this.Driver.CurJob != null && this.Driver.jobs.curDriver.asleep)
                                || ((this.parent as Vehicle_Cart) != null
                                    && (this.parent as Vehicle_Cart).VehicleComp.tankLeaking)
                                || !refuelableComp.HasFuel)
                            {
                                this.Driver.jobs.StartJob(jobNew, JobCondition.InterruptForced);
                            }
                        }
                    }
                }
            }

            if (Find.TickManager.TicksGame - this.tickLastDoorCheck >= 96
                && (this.Driver.Position.GetEdifice(this.parent.Map) is Building_Door
                    || this.parent.Position.GetEdifice(this.parent.Map) is Building_Door))
            {
                this.lastPassedDoor = (this.Driver.Position.GetEdifice(this.parent.Map) is Building_Door
                                           ? this.Driver.Position.GetEdifice(this.parent.Map)
                                           : this.parent.Position.GetEdifice(this.parent.Map)) as Building_Door;
                this.lastPassedDoor?.StartManualOpenBy(this.Driver);
                this.tickLastDoorCheck = Find.TickManager.TicksGame;
            }
            else if (Find.TickManager.TicksGame - this.tickLastDoorCheck >= 96)
            {
                if (this.lastPassedDoor != null)
                {
                    this.lastPassedDoor.StartManualCloseBy(this.Driver);
                    this.lastPassedDoor = null;
                }
            }

            if (this.Driver.pather.Moving)
            {
                if (this.Driver.Position != this.Driver.pather.Destination.Cell)
                {
                    this.lastDrawAsAngle = this.Driver.Rotation.AsAngle;
                    this.parent.Position = this.Position.ToIntVec3();
                    this.parent.Rotation = this.Driver.Rotation;
                }
            }
        }

        public void Dismount()
        {
            var cart = this.parent as Vehicle_Cart;
            this.Driver.AllComps?.Remove(cart.DriverComp);
            cart.DriverComp.Vehicle = null;
            cart.DriverComp.parent = null;

            GameComponentToolsForHaul.CurrentDrivers.Remove(this.Driver);

            this.Driver.RaceProps.makesFootprints = true;

            // if (Find.Reservations.IsReserved(parent, Driver.Faction))
            this.parent.Map.reservationManager.ReleaseAllForTarget(this.parent);
            if (this.Driver.Faction != Faction.OfPlayer)
            {
                this.parent.SetForbidden(true);
                this.parent.SetFaction(null);
            }

            this.Driver = null;

            this.SustainerAmbient?.End();

            // Find.ListerBuildings.Add(parent as Building);
        }

        public void DismountAt(IntVec3 dismountPos)
        {
            // if (Driver.Position.IsAdjacentTo8WayOrInside(dismountPos, Driver.Rotation, new IntVec2(1,1)))
            if (dismountPos != IntVec3.Invalid)
            {
                this.Dismount();
                this.parent.Position = dismountPos;
                return;
            }

            Log.Warning("Tried dismount at " + dismountPos);
        }

        public void MountOn(Pawn pawn)
        {
            if (this.Driver != null)
            {
                return;
            }

            // Check to make pawns not mount two vehicles at once
            if (TFH_Utility.IsDriver(pawn))
            {
                if (TFH_Utility.GetCartByDriver(pawn) != null)
                {
                    TFH_Utility.GetCartByDriver(pawn).MountableComp.Dismount();
                }

            }

            this.Driver = pawn;
            Vehicle_Cart vehicleCart = this.parent as Vehicle_Cart;


            if (this.Driver.RaceProps.Humanlike)
            {
                GameComponentToolsForHaul.CurrentDrivers.Add(pawn, this.parent as Vehicle_Cart);
                this.Driver.RaceProps.makesFootprints = false;
            }

            vehicleCart.DriverComp = new CompDriver
            {
                Vehicle = this.parent
            };
            this.Driver?.AllComps?.Add(vehicleCart.DriverComp);
            vehicleCart.DriverComp.parent = this.Driver;

            if (vehicleCart != null)
            {
                // Set faction of vehicle to whoever mounts it
                if (vehicleCart.Faction != this.driver.Faction)
                {
                    vehicleCart.SetFaction(this.driver.Faction);
                }

                if (vehicleCart.VehicleComp.IsCurrentlyMotorized())
                {
                    SoundInfo info = SoundInfo.InMap(this.parent);
                    this.SustainerAmbient = vehicleCart.VehicleComp.compProps.soundAmbient.TrySpawnSustainer(info);
                }

                this.IsPrisonBreaking = PrisonBreakUtility.IsPrisonBreaking(pawn);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            if (this.IsMounted)
            {
                if (GameComponentToolsForHaul.CurrentDrivers.ContainsKey(this.Driver))
                {
                    GameComponentToolsForHaul.CurrentDrivers.Remove(this.Driver);
                }
            }

            this.SustainerAmbient?.End();

            base.PostDeSpawn(map);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref this.driver, "Driver");
            Scribe_References.Look(ref this.lastPassedDoor, "lastPassedDoor");
            Scribe_Values.Look(ref this.lastDrawAsAngle, "lastDrawAsAngle");
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            if (this.IsMounted)
            {
                if (this.parent.TryGetComp<CompExplosive>() != null
                    && this.parent.TryGetComp<CompExplosive>().wickStarted)
                {
                    if (Rand.Value >= 0.1f)
                    {
                        this.DismountAt(
                            (this.Driver.Position - this.InteractionOffset.ToIntVec3()).RandomAdjacentCell8Way());
                    }
                }

                // if (tankLeaking)
                // {
                // if (hitpointsPercent < 0.2f * Rand.Range(0.5f, 1f))
                // {
                // if (!mountableComp.Driver.Position.InBounds())
                // {
                // mountableComp.DismountAt(mountableComp.Driver.Position);
                // FireUtility.TryStartFireIn(Position, 0.1f);
                // return;
                // }
                // mountableComp.DismountAt(mountableComp.Driver.Position - mountableComp.InteractionOffset.ToIntVec3());
                // mountableComp.Driver.Position = mountableComp.Driver.Position.RandomAdjacentCell8Way();
                // FireUtility.TryStartFireIn(Position, 0.1f);
                // return;
                // }
                // }
            }
        }

        /// <summary>
        /// The post spawn setup.
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            var cart = this.parent as Vehicle_Cart;

            if (this.Driver == null)
            {
                return;
            }

            if (cart.VehicleComp.IsCurrentlyMotorized())
            {
                LongEventHandler.ExecuteWhenFinished(
                    delegate
                        {
                            SoundInfo info = SoundInfo.InMap(this.parent);
                            this.SustainerAmbient = cart.VehicleComp.compProps.soundAmbient.TrySpawnSustainer(info);
                        });
            }

            //      if (this.Driver.RaceProps.Humanlike)
            {
                if (!GameComponentToolsForHaul.CurrentDrivers.ContainsKey(this.Driver))
                {
                    GameComponentToolsForHaul.CurrentDrivers.Add(this.Driver, this.parent as Vehicle_Cart);
                }
                if (this.Driver.RaceProps.Humanlike)
                {
                    this.Driver.RaceProps.makesFootprints = false;

                }
            //  cart.DriverComp = new CompDriver { Vehicle = this.parent };
            // Driver.AllComps?.Add(cart.DriverComp);
            //  cart.DriverComp.parent = this.Driver;
            }

        }
    }
}
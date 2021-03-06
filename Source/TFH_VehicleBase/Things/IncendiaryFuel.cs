﻿namespace ToolsForHaul.Things
{
    using System.Collections.Generic;

    using RimWorld;

    using Verse;

    public class IncendiaryFuel : Filth
    {
        private const float maxFireSize = 1.25f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.spawnTick = Find.TickManager.TicksGame;

            List<Thing> list = new List<Thing>(this.Position.GetThingList(map));
            foreach (Thing thing in list)
            {
                if (thing.HasAttachment(ThingDefOf.Fire))
                {
                    Fire fire = (Fire)thing.GetAttachment(ThingDefOf.Fire);
                    if (fire != null) fire.fireSize = maxFireSize;
                }

                // else
                // {
                // if (thing.HitPoints < 5)
                // {
                // thing.TryAttachFire(maxFireSize);
                // }
                // }
            }
        }

        private int spawnTick;
        private int fireTick = -5000;

        public override void Tick()
        {
            if (this.Position.GetThingList(this.Map).Any(x => x.def == ThingDefOf.FilthFireFoam))
            {
                if (!this.Destroyed) this.Destroy();
            }
            else
            {
                if (this.HasAttachment(ThingDefOf.Fire) && Find.TickManager.TicksGame > this.fireTick)
                {
                    Fire fire = (Fire)this.GetAttachment(ThingDefOf.Fire);
                    if (fire != null)
                    {
                        fire.fireSize = maxFireSize;
                    }

                    this.fireTick = Find.TickManager.TicksGame + 200;
                }
            }

            // if (spawnTick + 15000 < Find.TickManager.TicksGame)
            // {
            // Destroy(DestroyMode.Vanish);
            // }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace FM
{
    class JobDriver_ManagingAtManagingStation : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A).FailOnDespawnedOrForbiddenPlacedTargets();
            yield return
                Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
                    .FailOnDespawnedOrForbiddenPlacedTargets();
            yield return Manage(TargetIndex.A).FailOnDespawnedOrForbiddenPlacedTargets();
            yield return Toils_Reserve.Release(TargetIndex.A);
            yield break;
        }

        private Toil Manage(TargetIndex targetIndex)
        {
            Building_ManagerStation station = CurJob.GetTarget(targetIndex).Thing as Building_ManagerStation;
            if (station == null)
            {
                Log.Error("Target of manager job was not a manager station. This should never happen.");
                return null;
            }
            Comp_ManagerStation comp = station.GetComp<Comp_ManagerStation>();
            if (comp == null)
            {
                Log.Error("Target of manager job does not have manager station comp. This should never happen.");
                return null;
            }
            Toil toil = new Toil();
            toil.defaultDuration = (int) (comp.Props.Speed * (pawn.GetStatValue(StatDef.Named("ManagingSpeed")) + .5));
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.tickAction = delegate
            {
                toil.actor.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Managing")).Learn(0.11f);
            };
            List<Action> finishers = new List<Action>();
            finishers.Add(delegate
            {
                Manager.Get.DoWork();
            });
            toil.finishActions = finishers;
            return toil;
        }
    }
}
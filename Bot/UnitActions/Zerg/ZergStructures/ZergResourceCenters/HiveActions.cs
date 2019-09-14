﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures.ZergResourceCenters
{
    class HiveActions : ZergRescourceCenterActions
    {
        public HiveActions(ZergController controller, QueenToResourceCenterManager queenToResourceCenterManager) : base(controller, queenToResourceCenterManager)
        {
            unitType = Units.HIVE;
        }

        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformIntelligentActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            // Set the rally points.
            SetUnitRally(unit);
            SetWorkerRally(unit);

            if (!doNotUseResources)
            {
                // If there is no queen near by create one.
                if (Random.Next(100) < chanceOfExtraQueens || GetAssignedQueen(unit) == null)
                {
                    var queenResult = BirthQueen(unit);
                    if (saveFor && queenResult == BirthQueenResult.CanNotConstruct)
                    {
                        saveUnit = queen;
                    }
                }
                else if (Random.Next(100) < researchBurrowChance)
                {
                    var burrowResult = ResearchBurrow(unit);
                    if (saveFor && burrowResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchBurrow;
                    }
                }
                else if (Random.Next(100) < researchPneumatizedCarapaceChance)
                {
                    var pneumatizedCarapaceResult = ResearchPneumatizedCarapace(unit);
                    if (saveFor && pneumatizedCarapaceResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchPneumatizedCarapace;
                    }
                }
                else
                {
                    /*
                    var lairResult = UpgradeToLair(unit);
                    if (saveFor && lairResult == LairResult.CanNotConstruct)
                    {
                        saveUnit = lair;
                    }
                    */
                }
            }
        }

        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll,
            bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformRandomActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            var randomAction = Random.Next(6);

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;
                    /*
                    var laiResult = UpgradeToLair(unit);
                    if (saveFor && laiResult == LairResult.CanNotConstruct)
                    {
                        saveUnit = lair;
                    }*/
                    break;
                case 1:
                    if (doNotUseResources) return;

                    var burrowResult = ResearchBurrow(unit);
                    if (saveFor && burrowResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchBurrow;
                    }
                    break;
                case 2:
                    if (doNotUseResources) return;

                    var queenResult = BirthQueen(unit);
                    if (saveFor && queenResult == BirthQueenResult.CanNotConstruct)
                    {
                        saveUnit = queen;
                    }
                    break;
                case 3:
                    if (doNotUseResources) return;

                    var pneumatizedCarapaceResult = ResearchPneumatizedCarapace(unit);
                    if (saveFor && pneumatizedCarapaceResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchPneumatizedCarapace;
                    }
                    break;
                case 4:
                    SetUnitRally(unit);
                    break;
                case 5:
                    SetWorkerRally(unit);
                    break;
            }
        }

    }
}

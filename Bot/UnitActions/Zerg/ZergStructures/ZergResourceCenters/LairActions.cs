using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures.ZergResourceCenters
{
    class LairActions : ZergRescourceCenterActions
    {
        private readonly uint hive = Units.HIVE;

        public enum HiveResult { Success, NotUnitType, UnitBusy, CanNotConstruct };

        public LairActions(ZergController controller, QueenToResourceCenterManager queenToResourceCenterManager) : base(controller, queenToResourceCenterManager)
        {
            unitType = Units.LAIR;
        }

        // ********************************************************************************
        /// <summary>
        /// Preform an Intelligent actions for the unit.
        /// </summary>
        /// <param name="unit">The spawning pool unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
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
                    var hiveResult = UpgradeToHive(unit);
                    if (saveFor && hiveResult == HiveResult.CanNotConstruct)
                    {
                        saveUnit = hive;
                    }
                }
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Preform a random action for the passed unit.
        /// </summary>
        /// <param name="unit">The spawning pool unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll,
            bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformRandomActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            var randomAction = Random.Next(6);

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;

                    var laiResult = UpgradeToHive(unit);
                    if (saveFor && laiResult == HiveResult.CanNotConstruct)
                    {
                        saveUnit = hive;
                    }
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

        // ********************************************************************************
        /// <summary>
        /// Upgrade the unit to a hive.
        /// </summary>
        /// <param name="unit">The unit to upgrade.</param>
        /// <returns>The Hive Result.</returns>
        // ********************************************************************************
        public HiveResult UpgradeToHive(Unit unit)
        {
            if (!IsUnitType(unit)) return HiveResult.NotUnitType;

            if (IsBusy(unit)) return HiveResult.UnitBusy;

            if (!controller.CanConstruct(hive)) return HiveResult.CanNotConstruct;

            unit.Train(hive);
            Logger.Info("Upgrade to Hive @ {0} / {1}", unit.position.X, unit.position.Y);

            return HiveResult.Success;
        }
    }
}

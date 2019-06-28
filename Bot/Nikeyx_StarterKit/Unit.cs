using Google.Protobuf.Collections;
using SC2APIProtocol;
using System.Collections.Generic;
using System.Numerics;

// ReSharper disable MemberCanBePrivate.Global

namespace Bot
{
    public class Unit
    {
        private SC2APIProtocol.Unit original;
        private UnitTypeData unitTypeData;

        public string name;
        public uint unitType;
        public float integrity;
        public Vector3 position;
        public ulong tag;
        public float buildProgress;
        public UnitOrder order;
        public RepeatedField<UnitOrder> orders;
        public int supply;
        public bool isVisible;
        public int idealWorkers;
        public int assignedWorkers;
        public int vespene;
        public int minerals;
        public float sight;
        public ulong target;
        public bool isBurrowed;
        public bool isSelected;
        public RepeatedField<PassengerUnit> passangers;
        public int cargoMax;
        public int cargoUsed;
        public float energyCurrent;
        public float energyMax;
        public float weaponCooldown;

        public Unit(SC2APIProtocol.Unit unit)
        {
            this.original = unit;
            this.unitTypeData = ControllerDefault.gameData.Units[(int)unit.UnitType];

            this.name = unitTypeData.Name;
            this.tag = unit.Tag;
            this.unitType = unit.UnitType;
            this.position = new Vector3(unit.Pos.X, unit.Pos.Y, unit.Pos.Z);
            this.integrity = (unit.Health + unit.Shield) / (unit.HealthMax + unit.ShieldMax);
            this.buildProgress = unit.BuildProgress;
            this.idealWorkers = unit.IdealHarvesters;
            this.assignedWorkers = unit.AssignedHarvesters;

            this.order = unit.Orders.Count > 0 ? unit.Orders[0] : new UnitOrder();
            this.orders = unit.Orders;
            this.isVisible = (unit.DisplayType == DisplayType.Visible);
            this.isBurrowed = unit.IsBurrowed;
            this.isSelected = unit.IsSelected;

            this.supply = (int)unitTypeData.FoodRequired;

            this.vespene = unit.VespeneContents;
            this.minerals = unit.MineralContents;
            this.sight = this.unitTypeData.SightRange;
            this.target = unit.EngagedTargetTag;

            this.passangers = unit.Passengers;
            this.cargoMax = unit.CargoSpaceMax;
            this.cargoUsed = unit.CargoSpaceTaken;

            this.energyCurrent = unit.Energy;
            this.energyMax = unit.EnergyMax;

            this.weaponCooldown = unit.WeaponCooldown;
        }

        // ********************************************************************************
        /// <summary>
        /// Return the distance between two units (X,Y,Z).
        /// </summary>
        /// <param name="otherUnit">The unit to check distance against.</param>
        /// <returns>The distance between the two units.</returns>
        // ********************************************************************************
        public double GetDistance(Unit otherUnit)
        {
            return Vector3.Distance(position, otherUnit.position);
        }

        // ********************************************************************************
        /// <summary>
        /// Return the distance between the unit and a location (X,Y,Z).
        /// </summary>
        /// <param name="location">The location to check distance against.</param>
        /// <returns>The distance between the unit and location.</returns>
        // ********************************************************************************
        public double GetDistance(Vector3 location)
        {
            return Vector3.Distance(position, location);
        }

        // ********************************************************************************
        /// <summary>
        /// Train a unit.
        /// </summary>
        /// <param name="unitType">The unit type to train.</param>
        /// <param name="queue">If true it will queue the training.</param>
        // ********************************************************************************
        public void Train(uint unitType, bool queue = false)
        {
            if (!queue && orders.Count > 0)
                return;

            var abilityID = Abilities.GetID(unitType);
            var action = ControllerDefault.CreateRawUnitCommand(abilityID);
            action.ActionRaw.UnitCommand.UnitTags.Add(tag);
            ControllerDefault.AddAction(action);
            
            var targetName = ControllerDefault.GetUnitName(unitType);
            Logger.Info("Started training: {0}", targetName);
        }

        // ********************************************************************************
        /// <summary>
        /// Use an ability.
        /// </summary>
        /// <param name="abilityID">The ability to use.</param>
        /// <param name="toggleAutoCast">If true it will tonggle the auto cast.</param>
        /// <param name="targetUnit">If passed it will target this unit with the ability.</param>
        /// <param name="targetPosition">If passed it will target this location with the ability.</param>
        // ********************************************************************************
        public void UseAbility(int abilityID, bool toggleAutoCast = false, Unit targetUnit = null, Vector3 targetPosition = new Vector3())
        {
            if (orders.Count > 0) return;
            Action action = null;

            if (toggleAutoCast)
            {
                action = ControllerDefault.CreateToggleAutoCast(abilityID);
                action.ActionRaw.ToggleAutocast.UnitTags.Add(tag);
            }
            else
            {
                action = ControllerDefault.CreateRawUnitCommand(abilityID);
                action.ActionRaw.UnitCommand.UnitTags.Add(tag);
            }

            if (targetUnit != null)
            {
                action.ActionRaw.UnitCommand.TargetUnitTag = targetUnit.tag;
            }

            if (targetPosition != Vector3.Zero)
            {
                action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
                action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = targetPosition.X;
                action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = targetPosition.Y;
            }

            ControllerDefault.AddAction(action);
        }

        // ********************************************************************************
        /// <summary>
        /// Research ability.
        /// </summary>
        /// <param name="abilityID">The ability to research.</param>
        // ********************************************************************************
        public void Research(int abilityID)
        {
            if (orders.Count > 0) return;

            var action = ControllerDefault.CreateRawUnitCommand(abilityID);
            action.ActionRaw.UnitCommand.UnitTags.Add(tag);
            ControllerDefault.AddAction(action);

            var researchName = ControllerDefault.GetAbilityName(abilityID);
            Logger.Info("Started researching: {0}", researchName);
        }

        // ********************************************************************************
        /// <summary>
        /// Set the camera on a unit.
        /// </summary>
        // ********************************************************************************
        private void FocusCamera()
        {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.CameraMove = new ActionRawCameraMove();
            action.ActionRaw.CameraMove.CenterWorldSpace = new Point();
            action.ActionRaw.CameraMove.CenterWorldSpace.X = position.X;
            action.ActionRaw.CameraMove.CenterWorldSpace.Y = position.Y;
            action.ActionRaw.CameraMove.CenterWorldSpace.Z = position.Z;
            ControllerDefault.AddAction(action);
        }


        // ********************************************************************************
        /// <summary>
        /// Move a unit to a location.
        /// </summary>
        /// <param name="target">The location to move to.</param>
        // ********************************************************************************
        public void Move(Vector3 target)
        {
            var action = ControllerDefault.CreateRawUnitCommand(Abilities.MOVE);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            action.ActionRaw.UnitCommand.UnitTags.Add(tag);
            ControllerDefault.AddAction(action);
        }

        // ********************************************************************************
        /// <summary>
        /// Call the units smart ability. <parm/>
        /// I believe this is smart casting so units will check if other units are already doing the ability, but i am not sure.
        /// </summary>
        /// <param name="unit">The unit to preform the smart command on.</param>
        // ********************************************************************************
        public void Smart(Unit unit)
        {
            var action = ControllerDefault.CreateRawUnitCommand(Abilities.SMART);
            action.ActionRaw.UnitCommand.TargetUnitTag = unit.tag;
            action.ActionRaw.UnitCommand.UnitTags.Add(tag);
            ControllerDefault.AddAction(action);
        }

        // ********************************************************************************
        /// <summary>
        /// Attack a unit and location.
        /// </summary>
        /// <param name="unit">The unit to attack with.</param>
        /// <param name="target">The target location.</param>
        // ********************************************************************************
        public void Attack(Unit unit, Vector3 target)
        {
            var action = ControllerDefault.CreateRawUnitCommand(Abilities.ATTACK);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            action.ActionRaw.UnitCommand.UnitTags.Add(unit.tag);
            ControllerDefault.AddAction(action);
        }

        // ********************************************************************************
        /// <summary>
        /// Get a list of available abilities for the unit. <para/>
        /// It looks like abilities on a timer or ones that have used up there number of times to do will not show on the available ability list.
        /// </summary>
        /// <returns>The list of available abilities.</returns>
        // ********************************************************************************
        public List<AvailableAbility> GetAvailableAbilities()
        {
            RequestQueryAvailableAbilities requestQueryAvailableAbilities = new RequestQueryAvailableAbilities();
            requestQueryAvailableAbilities.UnitTag = tag;

            Request requestQuery = new Request();
            requestQuery.Query = new RequestQuery();
            requestQuery.Query.Abilities.Add(requestQueryAvailableAbilities);

            var result = Program.gc.SendQuery(requestQuery.Query);

            var availableAbilities = new List<AvailableAbility>();

            foreach(var availableAbility in result.Result.Abilities[0].Abilities)
            {
                availableAbilities.Add(availableAbility);
            }

            return availableAbilities;
        }
    }
}
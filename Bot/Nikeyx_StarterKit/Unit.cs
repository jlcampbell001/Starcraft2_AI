using System.Collections.Generic;
using System.Numerics;
using Google.Protobuf.Collections;
using SC2APIProtocol;

// ReSharper disable MemberCanBePrivate.Global

namespace Bot {
    public class Unit {
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

        public Unit(SC2APIProtocol.Unit unit) {
            this.original = unit;
            this.unitTypeData = ControllerDefault.gameData.Units[(int) unit.UnitType];

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

            this.supply = (int) unitTypeData.FoodRequired;

            this.vespene = unit.VespeneContents;
            this.minerals = unit.MineralContents;
            this.sight = this.unitTypeData.SightRange;
            this.target = unit.EngagedTargetTag;
        }                        
        
        // Return the distance between two units (X,Y,Z).
        public double GetDistance(Unit otherUnit) {
            return Vector3.Distance(position, otherUnit.position);
        }

        // Return the distance between the unit and a location (X,Y,Z).
        public double GetDistance(Vector3 location) {
            return Vector3.Distance(position, location);
        }
        
        // Train a unit.
        public void Train(uint unitType, bool queue=false) {            
            if (!queue && orders.Count > 0)
                return;            

            var abilityID = Abilities.GetID(unitType);            
            var action = ControllerDefault.CreateRawUnitCommand(abilityID);
            action.ActionRaw.UnitCommand.UnitTags.Add(tag);
            ControllerDefault.AddAction(action);

            var targetName = ControllerDefault.GetUnitName(unitType);
            Logger.Info("Started training: {0}", targetName);
        }
        
        // Set the camera on a unit.
        private void FocusCamera() {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.CameraMove = new ActionRawCameraMove();
            action.ActionRaw.CameraMove.CenterWorldSpace = new Point();
            action.ActionRaw.CameraMove.CenterWorldSpace.X = position.X;
            action.ActionRaw.CameraMove.CenterWorldSpace.Y = position.Y;
            action.ActionRaw.CameraMove.CenterWorldSpace.Z = position.Z;            
            ControllerDefault.AddAction(action);
        }
        
        
        // Move a unit to a location.
        public void Move(Vector3 target) {
            var action = ControllerDefault.CreateRawUnitCommand(Abilities.MOVE);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            action.ActionRaw.UnitCommand.UnitTags.Add(tag);
            ControllerDefault.AddAction(action);
        }
        
        // Call the units smart ability.
        // I belive this is smart casting so units will check if other units are already doing the ability, but i am not sure.
        public void Smart(Unit unit) {
            var action = ControllerDefault.CreateRawUnitCommand(Abilities.SMART);
            action.ActionRaw.UnitCommand.TargetUnitTag = unit.tag;
            action.ActionRaw.UnitCommand.UnitTags.Add(tag);
            ControllerDefault.AddAction(action);
        }
        
    }
}
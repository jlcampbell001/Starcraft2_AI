using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions
{
    class UnitActionsList
    {
        public List<UnitActionListItem> unitActionListItems = new List<UnitActionListItem>();

        public void addUnitAction(UnitActions unitAction, uint unitType)
        {
            UnitActionListItem unitActionListItem = new UnitActionListItem();
            unitActionListItem.unitAction = unitAction;
            unitActionListItem.unitType = unitType;

            unitActionListItems.Add(unitActionListItem);
        }

        public UnitActions GetUnitAction(uint unitType)
        {
            foreach(var unitActionListItem in unitActionListItems)
            {
                if (unitActionListItem.unitType == unitType)
                {
                    return unitActionListItem.unitAction;
                }
            }

            return null;
        }
    }
}

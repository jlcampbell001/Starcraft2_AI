using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// A list of unit actions that will link a unit type to a unit actions that can be searched later on.
    /// </summary>
    // --------------------------------------------------------------------------------
    class UnitActionsList
    {
        public List<UnitActionListItem> unitActionListItems = new List<UnitActionListItem>();

        // ********************************************************************************
        /// <summary>
        /// Adds a unit action to the list.
        /// </summary>
        /// <param name="unitAction">The unit action object.</param>
        /// <param name="unitType">The unit type to link to the unit action.</param>
        // ********************************************************************************
        public void AddUnitAction(UnitActions unitAction, uint unitType)
        {
            var unitActionListItem = new UnitActionListItem();
            unitActionListItem.UnitAction = unitAction;
            unitActionListItem.UnitType = unitType;

            unitActionListItems.Add(unitActionListItem);
        }

        // ********************************************************************************
        /// <summary>
        /// Scan through the list and get the unit action for the passed unit type.
        /// </summary>
        /// <param name="unitType">The unit type to search for.</param>
        /// <returns>The unit actions found or null.</returns>
        // ********************************************************************************
        public UnitActions GetUnitAction(uint unitType)
        {
            foreach(var unitActionListItem in unitActionListItems)
            {
                if (unitActionListItem.UnitType == unitType)
                {
                    return unitActionListItem.UnitAction;
                }
            }

            return null;
        }

        // ********************************************************************************
        /// <summary>
        /// Scan through the list and get the unit action for the passed unit type.
        /// </summary>
        /// <typeparam name="T">A subclass of unit actions to return.</typeparam>
        /// <param name="unitType">The unit type to search for.</param>
        /// <returns>The unit actions found or null.</returns>
        // ********************************************************************************
        public T GetUnitAction<T>(uint unitType)
        {
            var unitActions = default(T);

            foreach (var unitActionListItem in unitActionListItems)
            {
                if (unitActionListItem.UnitType == unitType)
                {
                    unitActions = (T) Convert.ChangeType(unitActionListItem.UnitAction, typeof(T));
                    break;
                }
            }

            return unitActions;
        }

    }
}

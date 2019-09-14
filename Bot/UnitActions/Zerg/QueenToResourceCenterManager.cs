using Bot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Manages the number of queens to a resource center. <para/>
    /// A resource center can have many queens linked, but a queen should only be linked to one resource center.
    /// </summary>
    // --------------------------------------------------------------------------------
    class QueenToResourceCenterManager
    {
        protected List<UnitsLink> queensToResourceCenter = new List<UnitsLink>();
        protected ZergController controller;

        protected Random random = new Random();

        public QueenToResourceCenterManager(ZergController controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        // ********************************************************************************
        /// <summary>
        /// Links a queen to a resource center. <para/>
        /// Will first try to link to a resource center that dose not have a queen, otherwise it will pick on at random.
        /// </summary>
        /// <param name="queenTag">The queen tag to link.</param>
        /// <returns>the unit link or null if it can not be setup.</returns>
        // ********************************************************************************
        public UnitsLink LinkQueenToResourceCenter(ulong queenTag)
        {
            UnitsLink queenLink = null;

            var resourceCenters = controller.GetUnits(Units.ResourceCenters, onlyCompleted: true);

            if (resourceCenters.Count != 0)
            {
                foreach (var resourceCenter in resourceCenters)
                {
                    var resourceCenterLink = FindLinkByResourceCenter(resourceCenter.tag);

                    if (resourceCenterLink == null)
                    {
                        queenLink = AddLink(queenTag, resourceCenter.tag);
                    }
                }

                // Add to a random resource center.
                if (queenLink == null)
                {
                    queenLink = AddLink(queenTag, resourceCenters[random.Next(resourceCenters.Count())].tag);
                }
            }
            return queenLink;
        }

        // ********************************************************************************
        /// <summary>
        /// Finds the queen link from the resource center tag.
        /// </summary>
        /// <param name="resourceCenterTag">The resource center tag to look for.</param>
        /// <param name="createNewLink">If true try to link the resource center to a queen that is not assigned yet.</param>
        /// <returns>The queen link or null if none are found.</returns>
        // ********************************************************************************
        public UnitsLink FindLinkByResourceCenter(ulong resourceCenterTag, bool createNewLink = false)
        {
            UnitsLink foundLink = null;

            var removeLinks = new List<UnitsLink>();

            foreach (var unitLink in queensToResourceCenter)
            {
                if (resourceCenterTag == unitLink.Tag2)
                {
                    if (controller.GetUnitByTag(unitLink.Tag1) == null)
                    {
                        removeLinks.Add(unitLink);
                    }
                    else
                    {
                        foundLink = unitLink;
                        break;
                    }
                }
            }

            // Try and link the resource center to a queen that is not linked.
            if (foundLink == null && createNewLink)
            {
                var queens = controller.GetUnits(Units.Queens);

                foreach (var queen in queens)
                {
                    var tempLink = FindLinkByQueen(queen.tag);

                    if (tempLink != null)
                    {
                        if (controller.GetUnitByTag(tempLink.Tag2) == null)
                        {
                            removeLinks.Add(tempLink);
                            tempLink = null;
                        }
                    }

                    if (tempLink == null)
                    {
                        foundLink = AddLink(queen.tag, resourceCenterTag);
                        break;
                    }
                }
            }

            // Remove any queen links where the queen is dead.
            foreach (var queenLink in removeLinks)
            {
                queensToResourceCenter.Remove(queenLink);
            }

            return foundLink;
        }

        // ********************************************************************************
        /// <summary>
        /// Finds the queen link from the queen tag.
        /// </summary>
        /// <param name="queenTag">The queen tag to look for.</param>
        /// <param name="createNewLink">If true try and create a new link if one is not found.</param>
        /// <returns>The queen link or null if none are found.</returns>
        // ********************************************************************************
        public UnitsLink FindLinkByQueen(ulong queenTag, bool createNewLink = false)
        {
            UnitsLink foundLink = null;

            var removeLinks = new List<UnitsLink>();

            foreach (var unitLink in queensToResourceCenter)
            {
                if (queenTag == unitLink.Tag1)
                {
                    var resourceCenter = controller.GetUnitByTag(unitLink.Tag2);

                    // It must have been destroyed or morphed so remove it from the list.
                    if (resourceCenter == null)
                    {
                        removeLinks.Add(unitLink);
                    }
                    else
                    {
                        foundLink = unitLink;
                        break;
                    }
                }
            }

            if (foundLink == null && createNewLink)
            {
                foundLink = LinkQueenToResourceCenter(queenTag);
            }

            // Remove any queen links where the resource center is destroyed or morphed.
            foreach (var queenLink in removeLinks)
            {
                queensToResourceCenter.Remove(queenLink);
            }

            return foundLink;
        }

        // ********************************************************************************
        /// <summary>
        /// Adds a new queen link to the list.
        /// </summary>
        /// <param name="queenTag">The queen tag to add.</param>
        /// <param name="resourceCenterTag">The resource center tag to add.</param>
        /// <returns>The queen link.</returns>
        // ********************************************************************************
        private UnitsLink AddLink(ulong queenTag, ulong resourceCenterTag)
        {
            var queenLink = new UnitsLink();
            queenLink.Tag1 = queenTag;
            queenLink.Tag2 = resourceCenterTag;

            queensToResourceCenter.Add(queenLink);

            return queenLink;
        }
    }
}

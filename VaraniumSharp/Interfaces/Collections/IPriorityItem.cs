using System;
using VaraniumSharp.Collections;

namespace VaraniumSharp.Interfaces.Collections
{
    /// <summary>
    ///     Interface for items that are sorted by <see cref="PriorityCollection{T}" />
    /// </summary>
    public interface IPriorityItem
    {
        #region Events

        /// <summary>
        ///     Fired when the Priority values changes so that the collection can rearrange the items
        /// </summary>
        event EventHandler PriorityChanged;

        #endregion

        #region Properties

        /// <summary>
        ///     Priority of the item to add.
        ///     Leaving this null will automatically set the priority and add the item to the end of the collection
        /// </summary>
        long? Priority { get; set; }

        #endregion
    }
}
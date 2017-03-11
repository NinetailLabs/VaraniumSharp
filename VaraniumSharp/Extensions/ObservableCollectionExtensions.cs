using System;
using System.Collections.ObjectModel;

namespace VaraniumSharp.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ObservableCollection{T}"/>
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        #region Public Methods

        /// <summary>
        /// Execution an action against the collection before clearing it.
        /// <remarks>
        /// This method allows doing something like unhooking event handlers from items contained in the collection before the collection is cleared.
        /// <seealso>
        ///     <cref>http://stackoverflow.com/questions/224155/when-clearing-an-observablecollection-there-are-no-items-in-e-olditems/42713996#42713996</cref>
        /// </seealso>
        /// </remarks>
        /// </summary>
        /// <typeparam name="T">Type of item contained in the collection</typeparam>
        /// <param name="collection">Observable collection that should be cleared</param>
        /// <param name="unhookAction">Action that should be executed before the collection is cleared</param>
        public static void Clear<T>(this ObservableCollection<T> collection, Action<ObservableCollection<T>> unhookAction)
        {
            unhookAction.Invoke(collection);
            collection.Clear();
        }

        #endregion
    }
}
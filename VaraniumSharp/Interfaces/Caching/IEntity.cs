#if NETSTANDARD2_1_OR_GREATER
namespace VaraniumSharp.Interfaces.Caching
{
    /// <summary>
    /// Base entity for storing items in the data store
    /// </summary>
    public interface IEntity
    {
        #region Properties

        /// <summary>
        /// Primary Id for items stored in the data store
        /// </summary>
        int Id { get; set; }

        #endregion
    }
}
#endif
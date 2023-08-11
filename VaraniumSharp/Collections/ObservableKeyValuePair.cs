using System.ComponentModel;

namespace VaraniumSharp.Collections
{
    /// <summary>
    /// KeyValue pair that implements INotifyPropertyChanged so that property changes can be tracked
    /// </summary>
    /// <typeparam name="TKey">Type of the Key</typeparam>
    /// <typeparam name="TValue">Type of the Value</typeparam>
    public sealed class ObservableKeyValuePair<TKey, TValue> : INotifyPropertyChanged
    {
        #region Constructor

        /// <summary>
        /// Construct with a key and a value
        /// </summary>
        /// <param name="key">Key for the entry</param>
        /// <param name="value">Value for the entry</param>
        public ObservableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        #endregion

        #region Events

        /// <inheritdoc />
        #if NETSTANDARD2_1_OR_GREATER
#nullable enable
        public event PropertyChangedEventHandler? PropertyChanged;
#nullable disable
#else
        public event PropertyChangedEventHandler PropertyChanged;
#endif

        #endregion

        #region Properties

        /// <summary>
        /// The Key for the entry
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// The value for the entry
        /// </summary>
        public TValue Value { get; set; }

        #endregion
    }
}
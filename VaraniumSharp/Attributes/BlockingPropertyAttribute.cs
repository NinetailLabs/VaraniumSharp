using System;
using VaraniumSharp.Extensions;

namespace VaraniumSharp.Attributes
{
    /// <summary>
    /// Attribute used to decorate properties used by <see cref="NotifyPropertyChangedExtensions.BlockUntil"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BlockingPropertyAttribute : Attribute
    {
        #region Constructor

        /// <summary>
        /// Construct with expected value
        /// </summary>
        /// <param name="expectedValue">If the property changes to this value the Task will be set as completed</param>
        public BlockingPropertyAttribute(object expectedValue)
        {
            ExpectedValue = expectedValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Value that is expected
        /// </summary>
        public object ExpectedValue { get; }

        #endregion
    }
}
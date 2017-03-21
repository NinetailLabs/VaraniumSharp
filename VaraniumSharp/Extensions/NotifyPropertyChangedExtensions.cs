using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VaraniumSharp.Attributes;

namespace VaraniumSharp.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public static class NotifyPropertyChangedExtensions
    {
        #region Public Methods

        /// <summary>
        /// Returns a task that can be blocked against until a specific property changes.
        /// The property or properties should be marked with <see cref="BlockingPropertyAttribute"/> otherwise a completed task will be returned immediately.
        /// </summary>
        /// <param name="self">Class instance that contains the properties</param>
        /// <returns>Task that can be awaited until the decorated properties change</returns>
        public static Task<bool> BlockUntil(this INotifyPropertyChanged self)
        {
            TaskCompletionSource<bool> completionSource;
            if (self.TaskCompleted(out completionSource))
            {
                // The task is already finished
                return Task.FromResult(true);
            }

            completionSource = new TaskCompletionSource<bool>();

            if (!CompletionSources.ContainsKey(self))
            {
                CompletionSources.Add(self, completionSource);
            }

            self.PropertyChanged += SelfOnPropertyChanged;
            return completionSource.Task;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get properties which have the <see cref="BlockingPropertyAttribute"/>
        /// </summary>
        /// <param name="self">Class instance that contains the properties</param>
        /// <returns>Collection of Properties that are decorated with the attribute</returns>
        private static IEnumerable<PropertyInfo> GetBlockingProperties(this INotifyPropertyChanged self)
        {
            var properties = self
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(t => t.GetCustomAttributes(typeof(BlockingPropertyAttribute), false).Length > 0).ToList();
            return properties;
        }

        /// <summary>
        /// Occurs when a property on the monitored class changes
        /// </summary>
        /// <param name="sender">Object that fired the notification</param>
        /// <param name="propertyChangedEventArgs">Details about the event</param>
        private static void SelfOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var self = (INotifyPropertyChanged)sender;

            TaskCompletionSource<bool> completionSource;
            if (!self.TaskCompleted(out completionSource))
            {
                return;
            }
            completionSource?.SetResult(true);
            CompletionSources.Remove(self);
            self.PropertyChanged -= SelfOnPropertyChanged;
        }

        /// <summary>
        /// Check if the task has been completed
        /// </summary>
        /// <param name="self">Class instance that contains the properties</param>
        /// <param name="completionSource">Completion source that was used to create the monitored task</param>
        /// <returns>True - Task completed, otherwise false</returns>
        private static bool TaskCompleted(this INotifyPropertyChanged self,
            out TaskCompletionSource<bool> completionSource)
        {
            var taskCompleted = true;
            var properties = self.GetBlockingProperties();
            completionSource = CompletionSources.ContainsKey(self) ? CompletionSources[self] : null;
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<BlockingPropertyAttribute>();
                if (!property.GetValue(self).Equals(attribute.ExpectedValue))
                {
                    taskCompleted = false;
                }
            }
            return taskCompleted;
        }

        #endregion

        #region Variables

        private static readonly Dictionary<INotifyPropertyChanged, TaskCompletionSource<bool>> CompletionSources =
            new Dictionary<INotifyPropertyChanged, TaskCompletionSource<bool>>();

        #endregion
    }
}
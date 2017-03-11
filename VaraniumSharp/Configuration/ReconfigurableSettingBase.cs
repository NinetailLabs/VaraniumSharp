using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#pragma warning disable 0067

namespace VaraniumSharp.Configuration
{
    /// <summary>
    /// Base for Setting classes that support cancellation of changes (automatic rollback to previous settings) as well as assist in persisting changes
    /// to a datastore
    /// </summary>
    public abstract class ReconfigurableSettingBase : INotifyPropertyChanged
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        protected ReconfigurableSettingBase()
        {
            PropertyChanged += InnerPropertyChanged;
        }

        #endregion

        #region Events

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fired when properties have been updated and persisted (so they should be applied)
        /// </summary>
        public event EventHandler SettingUpdated;

        #endregion

        #region Properties

        /// <summary>
        /// Indicate if Setting values have changed
        /// </summary>
        public bool DataCanBePersisted { get; private set; }

        /// <summary>
        /// Indicate if there are unsaved changes
        /// </summary>
        public bool UnsavedChanges { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cancel changes that were made to properties that have not been saved yet
        /// </summary>
        public void CancelChanges()
        {
            //This will reset changes made to properties that have not been saved
            _isLoading = true;
            AdjustNotificationProperties(false);
            foreach (var property in _propertiesDictionary)
            {
                if (property.Key.Name == nameof(DataCanBePersisted)
                    || property.Key.Name == nameof(UnsavedChanges))
                {
                    continue;
                }

                property.Key.SetValue(this, property.Value);
            }
            _isLoading = false;
        }

        /// <summary>
        /// Load Setting values from persistent storage
        /// </summary>
        public async Task LoadSettingsAsync()
        {
            _isLoading = true;

            await ExecuteDataLoadAsync();
            GatherPropertyValues();

            _isLoading = false;
        }

        /// <summary>
        /// Persist Setting values
        /// </summary>
        public async Task<bool> PersistSettingsAsync()
        {
            var result = await ExecuteDataPersistanceAsync();
            if (result)
            {
                GatherPropertyValues();
                SettingUpdated?.Invoke(this, null);
                AdjustNotificationProperties(false);
            }
            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adjust properties that can be used to control UI
        /// </summary>
        /// <param name="newValue">New value for Properties</param>
        private void AdjustNotificationProperties(bool newValue)
        {
            DataCanBePersisted = newValue;
            UnsavedChanges = newValue;
        }

        /// <summary>
        /// Provides logic for loading the settings from some form of permanent storage
        /// </summary>
        protected abstract Task ExecuteDataLoadAsync();

        /// <summary>
        /// Provides logic for persisting the Settings to some form of permanent storage
        /// </summary>
        /// <returns>True - Data persistence succeeded</returns>
        protected abstract Task<bool> ExecuteDataPersistanceAsync();

        /// <summary>
        /// Gather properties and their values
        /// </summary>
        private void GatherPropertyValues()
        {
            _propertiesDictionary = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop, prop => prop.GetValue(this, null));
        }

        /// <summary>
        /// Invoked when an internal Property has changed
        /// </summary>
        /// <param name="sender">Invoker</param>
        /// <param name="propertyChangedEventArgs">Property changed event arguments</param>
        private void InnerPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(DataCanBePersisted)
                || propertyChangedEventArgs.PropertyName == nameof(UnsavedChanges)
                || _isLoading)
            {
                return;
            }
            //This is used to unlock the Persist function - It is how we know that one of our inner properties have changed
            AdjustNotificationProperties(true);
        }

        #endregion

        #region Variables

        private bool _isLoading;

        private Dictionary<PropertyInfo, object> _propertiesDictionary;

        #endregion
    }
}
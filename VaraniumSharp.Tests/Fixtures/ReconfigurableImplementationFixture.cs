using System.Threading.Tasks;
using VaraniumSharp.Configuration;

namespace VaraniumSharp.Tests.Fixtures
{
    public class ReconfigurableImplementationFixture : ReconfigurableSettingBase
    {
        #region Constructor

        public ReconfigurableImplementationFixture(bool saveSuccessfully)
        {
            _saveSuccess = saveSuccessfully;
        }

        #endregion

        #region Properties

        public int DataLoadExecutions { get; private set; }

        public int DataSaveExecutions { get; private set; }

        public bool TestProperty { get; set; }

        #endregion

        #region Private Methods

        protected override async Task ExecuteDataLoadAsync()
        {
            await Task.Delay(10);
            TestProperty = true;
            DataLoadExecutions++;
        }

        protected override async Task<bool> ExecuteDataPersistanceAsync()
        {
            await Task.Delay(10);
            DataSaveExecutions++;
            return _saveSuccess;
        }

        #endregion

        #region Variables

        private readonly bool _saveSuccess;

        #endregion
    }
}
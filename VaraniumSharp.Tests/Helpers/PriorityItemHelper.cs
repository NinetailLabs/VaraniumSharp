using Moq;
using VaraniumSharp.Interfaces.Collections;

namespace VaraniumSharp.Tests.Helpers
{
    public static class PriorityItemHelper
    {
        #region Public Methods

        public static Mock<IPriorityItem> CreatePriorityMock(int? priority)
        {
            var itemDummy = new Mock<IPriorityItem>();
            itemDummy
                .Setup(t => t.Priority)
                .Returns(priority);
            return itemDummy;
        }

        #endregion
    }
}
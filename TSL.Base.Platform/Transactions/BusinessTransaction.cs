
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;

namespace TSL.Base.Platform.Transactions
{
    [RegisterIOC]
    public class BusinessTransaction : IBusinessTransaction
    {
        private ILog<BusinessTransaction> _logger;
        private ITransactionsProvider _transactionsProvider;
        private bool _needExcuteRollbackFlag;

        public BusinessTransaction(
            ILog<BusinessTransaction> logger,
            ITransactionsProvider transactionsProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transactionsProvider = transactionsProvider ?? throw new ArgumentNullException(nameof(transactionsProvider));
            _needExcuteRollbackFlag = false;
        }

        /// <inheritdoc/>
        public void SetLogTraceId(string traceId) =>
            _transactionsProvider.SetTraceId(traceId);

        /// <inheritdoc/>
        public async Task CompleteTransactionsAsync()
        {
            try
            {
                if (_needExcuteRollbackFlag)
                {
                    await _transactionsProvider.ExecuteTransactionsAsync().ConfigureAwait(false);
                }
                else
                {
                    await _transactionsProvider.DeleteRelatedTransactionsAsync().ConfigureAwait(false);
                }


            }
            catch (Exception ex)
            {
                _logger.Error(nameof(CompleteTransactionsAsync), ex);
            }
        }

        /// <summary>
        /// 須執行 rollback 操作
        /// </summary>
        /// <param name="methodBase"></param>
        /// <param name="message">訊息備註</param>
        public void SwitchNeedExcuteRollback(System.Reflection.MethodBase methodBase, string message = null)
        {
            _needExcuteRollbackFlag = true;
            _logger.Info(nameof(SwitchNeedExcuteRollback), new { queryMethod = methodBase.DeclaringType.FullName + "." + methodBase.Name, message = message });
        }

    }
}

using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using TSL.Base.Platform.lnversionOfControl;

namespace TSL.Base.Platform.SystemInfo
{
    [RegisterIOC(LifeCycle = IocType.Singleton)]
    public class DBInfoService : IDBInfoService
    {
        private readonly object __lockObj = new();
        private readonly IOCContainer _container;
        //private readonly IOptions<Platform.DataAccess.DataAccessOption> _optionData;
        //private readonly IOptions<Platform.Utilities.GeneralOption> _generalOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBInfoService"/> class.
        /// </summary>
        /// <param name="container">This container.</param>
        public DBInfoService(
            IOCContainer container)
 
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            SkipServersideBackendPolicyVerification = true;
            SkipServersideFrontendPolicyVerification = true;
        }

        /// <summary>
        /// 資料庫資料版本
        /// </summary>
        private string? _dbDataVersion;

        private bool _skipServersideBackendPolicyVerification;
        private bool _skipServersideFrontendPolicyVerification;

        /// <summary>
        /// 資料庫資料版本
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S4462:Calls to \"async\" methods should not be blocking", Justification = "Cannot async/await in lock statement")]
        public string DBDataVersion
        {
            get
            {
                if (_dbDataVersion != null)
                {
                    return _dbDataVersion;
                }

                lock (__lockObj)
                {
                    if (_dbDataVersion != null)
                    {
                        return _dbDataVersion;
                    }

                    var sysmConfigProvider = _container.GetService<ISYSMConfigProvider>();
                    var data = sysmConfigProvider.QueryAll().GetAwaiter().GetResult();

                    if (data == null)
                    {
                        _dbDataVersion = string.Empty;
                    }
                    else
                    {
                        _dbDataVersion = string.Empty;
                    }
                }

                return _dbDataVersion;
            }
        }

        /// <summary>
        /// 取得跳過 後端 policy 驗證判斷
        /// </summary>
        public bool SkipServersideBackendPolicyVerification
        {
            get => _skipServersideBackendPolicyVerification;

            set
            {
                lock (__lockObj)
                {
                    _skipServersideBackendPolicyVerification = value;
                }
            }
        }

        /// <summary>
        /// 取得跳過 前端 policy 驗證判斷
        /// </summary>
        public bool SkipServersideFrontendPolicyVerification
        {
            get => _skipServersideFrontendPolicyVerification;

            set
            {
                lock (__lockObj)
                {
                    _skipServersideFrontendPolicyVerification = value;
                }
            }
        }

        private ConcurrentDictionary<string, HashSet<string>> _tableComputedColumns = null;

        /// <summary>
        /// 取得系統  Computed Columns 欄位清單
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S4462:Calls to \"async\" methods should not be blocking", Justification = "Cannot async/await in lock statement")]
        public ConcurrentDictionary<string, HashSet<string>> TableComputedColumns
        {
            get
            {
                if (_tableComputedColumns != null)
                {
                    return _tableComputedColumns;
                }

                lock (__lockObj)
                {
                    if (_tableComputedColumns != null)
                    {
                        return _tableComputedColumns;
                    }

                    var sysmConfigProvider = _container.GetService<ISYSMConfigProvider>();
                }

                return _tableComputedColumns;
            }
        }

        /// <summary>
        /// 取得資料庫連線ip
        /// </summary>
        /// <returns></returns>
        public string GetDBIp()
        {
            throw new NotImplementedException();
        }

        public SqlConnectionStringBuilder GetHISSqlConnectionStringBuilder()
        {
            throw new NotImplementedException();
        }
    }
}

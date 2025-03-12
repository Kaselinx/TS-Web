using Dapper;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Xml.Linq;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.SystemInfo;
using TSL.Base.Platform.Utilities;

namespace TSL.Base.Platform.Transactions
{
    /// <summary>
    /// Provider
    /// </summary>
    [RegisterIOC(LifeCycle = IocType.Scoped)]
    public class TransactionsProvider : ITransactionsProvider
    {
        private readonly ILog<TransactionsProvider> _logger;
        private string _logTraceId;
        private readonly Guid _transactionId;
        private IOCContainer _container;
        private HashSet<string> _tableWhiteList;
        private IDBInfoService _dbInfoService;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="container">container</param>
        /// <param name="tableWhiteList">tableWhiteList</param>
        /// <param name="dbInfoService">dbInfoService</param>
        public TransactionsProvider(
            ILog<TransactionsProvider> logger,
            IOCContainer container,
            IOptions<TransactionTableWhiteListOption> tableWhiteList,
            IDBInfoService dbInfoService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logTraceId = string.Empty;
            _transactionId = Guid.NewGuid();
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _tableWhiteList = tableWhiteList.Value
                                    .Tables
                                    .Select(x => x.ToLower(CultureInfo.CurrentCulture))
                                    .ToHashSet<string>()
                                ?? throw new ArgumentNullException(nameof(tableWhiteList));
            _dbInfoService = dbInfoService ?? throw new ArgumentNullException(nameof(dbInfoService));
        }

        /// <inheritdoc/>
        public void SetTraceId(string traceId)
        {
            _logTraceId = traceId ?? throw new ArgumentNullException(nameof(traceId));
        }

        /// <inheritdoc/>
        string ITransactionsProvider.LogTraceId => _logTraceId;

        /// <inheritdoc/>
        Guid ITransactionsProvider.TransactionId => _transactionId;

        /// <summary>
        /// 取得系統  Computed Columns 欄位清單
        /// </summary>
        public ConcurrentDictionary<string, HashSet<string>> TableComputedColumns
        {
            get
            {
                return _dbInfoService.TableComputedColumns;
            }
        }

        /// <inheritdoc/>
        public async Task<int> DeleteRelatedTransactionsAsync()
        {
            throw new NotImplementedException();
            //-            string procedureName = "SP_DEL_Transactions";
            //            var param = new DynamicParameters();
            //            param.Add("txId", _transactionId);
            //            var dataAccessService = _container.GetService<DataAccessService>();
            //            return await dataAccessService.ExecuteNonQueryAsync(procedureName, param, false, CommandType.StoredProcedure).ConfigureAwait(false);

        }

        /// <summary>
        /// 備份 rollback 執行指令
        /// </summary>
        /// <returns></returns>
        public async Task<int> BackupExecuteRollbackScript()
        {
            throw new NotImplementedException();
        }

        public class ExecuteTransacEntity
        {
            public string TblName { get; set; }
            public string Type { get; set; }
            public string RollbackSQL { get; set; }
            public string RollbackParams { get; set; }
            public byte ScriptType { get; set; }
        }

        public class TableColumnInfo
        {
            public string TableName { get; set; }
            public string Column { get; set; }
        }

        public class ParseRollbackXMLInfo
        {
            public List<string> AllColumn { get; set; }
            public string StrColumns { get; set; }
            public string StrColumnsAndType { get; set; }

        }

        public enum TransactionType
        {
            None = 0,
            Insert,
            Delete,
            Update
        }

        public enum ScriptType
        {
            None = 0,
            Script = 1,
            XML = 2
        }

        /// <inheritdoc/>
        public async Task<int> ExecuteTransactionsAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 取回資料表 PrimaryKey 設定欄位
        /// </summary>
        /// <param name="rollbackItems">rollback 項目</param>
        /// <returns></returns>
        private async Task<List<TableColumnInfo>> GetTablePrimaryKeyItemsForUpdate(IEnumerable<ExecuteTransacEntity> rollbackItems)
        {
            throw new NotImplementedException();
        }

        private async Task<List<TableColumnInfo>> GetTableIdentityItemsForUpdate(IEnumerable<ExecuteTransacEntity> rollbackItems)
        {
            throw new NotImplementedException();
        }


        private ParseRollbackXMLInfo ParseRollbackXML(string tableName, string xml)
        {
            var xmlDocument = XDocument.Parse(xml);
            XNamespace xs = "http://www.w3.org/2001/XMLSchema";
            var elements = xmlDocument.Descendants(xs + "sequence").Descendants(xs + "element").ToList();
            var outputColumns = new List<string>();
            var outputType = new List<string>();

            var tableColumnInfo = _dbInfoService.TableComputedColumns.ContainsKey(tableName) ? _dbInfoService.TableComputedColumns[tableName] : new HashSet<string>();

            foreach (var elem in elements)
            {
                var name = elem.Attribute("name")?.Value;

                if (tableColumnInfo.Contains(name))
                {
                    continue;
                }

                var type = elem.Attribute("type")?.Value;

                if (type != null)
                {
                    type = type.Replace("sqltypes:", string.Empty, StringComparison.OrdinalIgnoreCase);
                }

                var simpleType = elem.Element(xs + "simpleType");
                if (simpleType != null)
                {
                    var baseTypeElement = simpleType.Element(xs + "restriction");
                    var baseType = baseTypeElement.Attribute("base").Value;

                    type = baseType.Replace("sqltypes:", string.Empty, StringComparison.OrdinalIgnoreCase);

                    if (string.Equals(type, "decimal", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(type, "numeric", StringComparison.OrdinalIgnoreCase))
                    {
                        var totalDigitsElement = baseTypeElement.Element(xs + "totalDigits");
                        var fractionDigitsElement = baseTypeElement.Element(xs + "fractionDigits");

                        var totalDigitsValue = totalDigitsElement.Attribute("value").Value;
                        var fractionDigitsValue = fractionDigitsElement.Attribute("value").Value;
                        type = $"{type}({totalDigitsValue},{fractionDigitsValue})";
                    }
                    else
                    {
                        var maxLengthElement = baseTypeElement.Element(xs + "maxLength");
                        if (maxLengthElement != null)
                        {
                            var maxLengtValue = maxLengthElement.Attribute("value").Value;

                            type = $"{type}({maxLengtValue})";
                        }
                        else
                        {
                            if (string.Equals(type, "nvarchar", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(type, "varchar", StringComparison.OrdinalIgnoreCase))
                            {
                                type = $"{type}(max)";
                            }
                            else
                            {
                                throw new ArgumentNullException(nameof(maxLengthElement));
                            }
                        }
                    }
                }

                outputColumns.Add(name);
                outputType.Add($"tb.col.value('x:{name}[1]', '{type}') as {name}");
            }

            return new ParseRollbackXMLInfo
            {
                AllColumn = outputColumns,
                StrColumns = string.Join(",", outputColumns),
                StrColumnsAndType = string.Join(",", outputType)
            };
        }

        /// <summary>
        /// 是否存在於白名單
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <returns></returns>
        public bool ContainTableList(string tableName)
        {
            return _tableWhiteList.Contains(tableName.Substring(0, Math.Min(4, tableName.Length)).ToLower(CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// 資料快照 for Delete
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="keyColumnName">查詢欄位名稱</param>
        /// <param name="ids">資料 id</param>
        /// <returns></returns>
        public async Task DeleteWithSnapshot(string tableName, string keyColumnName, params string[] ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 資料快照 for Update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityToUpdate">entityToUpdate</param>
        /// <returns></returns>
        public async Task<bool> UpdateWithSnapshot<T>(T entityToUpdate) where T : class
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string MergeTransactionScriptForUpdate(string tableName, string snapshotSqlStatement, DynamicParameters parameters)
        {
            return MergeTransactionScript(tableName, snapshotSqlStatement, parameters, null, null, TransactionType.Update);
        }

        /// <inheritdoc/>
        public string MergeTransactionScriptForDelete(string tableName, string snapshotSqlStatement, DynamicParameters parameters)
        {
            return MergeTransactionScript(tableName, snapshotSqlStatement, parameters, null, null, TransactionType.Delete);
        }

        /// <summary>
        /// 合併 Transaction script For Delete
        /// </summary>
        /// <param name="tableName">tableName</param>
        /// <param name="snapshotSqlStatement">snapshot sql statement</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        private string MergeTransactionScript(string tableName, string snapshotSqlStatement, DynamicParameters parameters, string keyColumnName, string dataId, TransactionType type)
        {
            var rnd = new Random();
            var key = string.Empty;
            int count = 0;
            string xmlParam = string.Empty;
            do
            {
                key = rnd.Next(1, 1000).ToString();
                xmlParam = $"XML_{key}";

                if (parameters.ParameterNames.Where(x => x == xmlParam).Any() == false)
                {
                    // 判斷 key 不存在
                    break;
                }

                if (count++ > 20)
                {
                    throw new ArgumentNullException("無法取得資料欄位 index");
                }

            } while (true);

            var tableNameParam = $"{tableName}_{key}";
            var logTraceIdParam = $"LogTraceId_{key}";
            var transactionIdParam = $"TransactionId_{key}";
            var tablePK = $"tablePK_{key}";
            parameters.Add(tableNameParam, tableName);
            parameters.Add(tablePK, string.IsNullOrEmpty(dataId) ? Guid.NewGuid().ToString() : $"{keyColumnName}.{dataId}");
            parameters.Add(logTraceIdParam, _logTraceId);
            parameters.Add(transactionIdParam, _transactionId);

            return $@"DECLARE @{xmlParam} AS nvarchar(max) = ({snapshotSqlStatement} FOR XML RAW, XMLSCHEMA('RollbackItem'),ROOT,ELEMENTS) EXEC [SP_INS_Transactions_For_XML] @{tablePK}, @{tableNameParam}, @{logTraceIdParam},@{transactionIdParam},@{xmlParam},'{type.ToString()}';";
        }

    }
}

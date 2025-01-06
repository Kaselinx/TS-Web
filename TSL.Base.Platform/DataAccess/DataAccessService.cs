
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Options;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Text;
using TSL.Base.Platform.DapperExt;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.MiniProfiler;
using TSL.Base.Platform.Transactions;
using TSL.Base.Platform.Utilities;
using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;
using CredentialManagement;


namespace TSL.Base.Platform.DataAccess
{

    /// <summary>
    /// Help Connetion to DB
    /// </summary>
    [RegisterIOC(IocType.Transient)]
    public class DataAccessService
    {
        private string _connectionStr;
        private string _connectionSecStr;
        private int _connectionTimeout = 60;
        private bool _enableMiniprofiler;
        private ITransactionsProvider _transactionsProvider;
        private ILog<DataAccessService> _logger = default!;
        private readonly string _credentialOptionsTargetStr;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessService"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string used to connect to the database</param>
        /// <param name="transactionsProvider">transactionsProvider</param>
        /// <param name="credentialOptions">transactionsProvider</param>
        public DataAccessService(string connectionString, ITransactionsProvider transactionsProvider, CredentialOptions credentialOptions )
        {
            _connectionStr = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _connectionSecStr = _connectionStr;
            _transactionsProvider = transactionsProvider ?? throw new ArgumentNullException(nameof(transactionsProvider));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessService"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string used to connect to the database</param>
        /// <param name="connectionTimeout">Connection string timeout</param>
        /// <param name="transactionsProvider">transactionsProvider</param>
        public DataAccessService(string connectionString, ITransactionsProvider transactionsProvider, int connectionTimeout = 60)
        {
            _connectionStr = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _connectionSecStr = _connectionStr;
            _connectionTimeout = connectionTimeout;
            _transactionsProvider = transactionsProvider ?? throw new ArgumentNullException(nameof(transactionsProvider));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessService"/> class.
        /// </summary>
        /// <param name="daOption">Data Access Setting</param>
        /// <param name="generalOption">Basic Setting</param>
        /// <param name="transactionsProvider">transactionsProvider</param>
        /// <param name="logger">logger</param>
        public DataAccessService(IOptions<DataAccessOption> daOption, IOptions<GeneralOption> generalOption, ITransactionsProvider transactionsProvider, ILog<DataAccessService> logger, IOptions<CredentialOptions> credentialOptions)
        {
            _connectionStr = daOption.Value.ConnectionStringPrimary;
            _connectionSecStr = daOption.Value.ConnectionStringSecondary;
            _connectionTimeout = daOption.Value.ConnetionTimeout;
            _enableMiniprofiler = generalOption.Value.EnableMiniProfiler;
            //_credentialOptionsTargetStr = credentialOptions.Value.Target;
            _credentialOptionsTargetStr = daOption.Value.Credential;

            // try gettingcredential from windows credential manager
            if (!string.IsNullOrEmpty(_connectionStr) && !string.IsNullOrEmpty(_credentialOptionsTargetStr))
            {
                //Retrieve a credential by its target name
                Credential credential = new Credential
                {
                    Target = _credentialOptionsTargetStr
                };
                bool success = credential.Load();

                // if succesful get the credential then use it
                if (success)
                {
                    //replace connstring string with credential
                    _connectionStr = _connectionStr.Replace("{DB_USER}", credential.Username).Replace("{DB_PASSWORD}", credential.Password);
                }
                else
                {
                    logger.Warning($"Credential not found for target: {_credentialOptionsTargetStr}. Using Orginal Connection String");
                }
            }

            _transactionsProvider = transactionsProvider ?? throw new ArgumentNullException(nameof(transactionsProvider));
            _logger = logger;

            if (SqlMapperExtensions.TableNameMapper == null)
            {
                SqlMapperExtensions.TableNameMapper = (type) =>
                {
                    var att = CustomAttributeExtensions.GetCustomAttributes(type.GetTypeInfo(), false)
                                .FirstOrDefault(attr => attr.GetType().Name.Equals("TableAttribute", StringComparison.OrdinalIgnoreCase));
                    if (att != null && att is Dapper.Contrib.Extensions.TableAttribute)
                    {
                        return ((Dapper.Contrib.Extensions.TableAttribute)att).Name;
                    }

                    return type.Name;
                };
            }
        }

        /// <summary>
        /// ConnectionTimeout
        /// </summary>
        public int ConnectionTimeout
        {
            get => _connectionTimeout;
            set => _connectionTimeout = value;
        }

        /// <summary>
        /// ConnectionDBBacinInfo
        /// </summary>
        public string ConnectionDBBacinInfo
        {
            get
            {
                string[] connectionArray = string.IsNullOrEmpty(_connectionStr) ? [] : _connectionStr.Split(';');
                string dbBasicInfo = connectionArray.Length > 3 ? $"{connectionArray[0]} + {connectionArray[1]} + {connectionArray[2]}" : "連線參數不包含;";
                return dbBasicInfo;
            }
        }

        #region Query 系列

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="TReturn">回覆的資料類型</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="param">查詢參數物件</param>
        /// <param name="timeoutSecs">SQL執行Timeout秒數</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>x
        public async Task<IEnumerable<TReturn>> QueryAsyncwithTimeoutTime<TReturn>(string querySql, object param = null, int timeoutSecs = 20, bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection(readOnlyConnection))
            {
                return await con.QueryAsync<TReturn>(querySql, param, null, timeoutSecs, commandType).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="TReturn">回覆的資料類型</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="param">查詢參數物件</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>x
        public async Task<IEnumerable<TReturn>> QueryAsync<TReturn>(string querySql, object param = null, bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection(readOnlyConnection))
            {
                return await con.QueryAsync<TReturn>(querySql, param, null, _connectionTimeout, commandType).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢多行SQL
        /// </summary>
        /// <param name="querySql">T-SQL</param>
        /// <param name="param">參數</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <param name="mapItems">MapItem</param>
        /// <returns></returns>
        public async Task<dynamic> QueryMultipleAsync(string querySql, object? param = null, bool readOnlyConnection = false, CommandType commandType = CommandType.Text, IEnumerable<MapItem> mapItems = null)
        {
            using IDbConnection con = GetDbConnection(readOnlyConnection);
            ExpandoObject data = new ExpandoObject();

            using (var multi = con.QueryMultiple(querySql, param, null, _connectionTimeout, commandType))
            {
                if (mapItems == null)
                {
                    return data;
                }

                foreach (var item in mapItems)
                {
                    if (item.DataRetriveType == DataRetriveType.FirstOrDefault)
                    {
                        var singleItem = multi.Read(item.Type).FirstOrDefault();
                        ((IDictionary<string, object>)data).Add(item.PropertyName, singleItem);
                    }

                    if (item.DataRetriveType == DataRetriveType.List)
                    {
                        var listItem = multi.Read(item.Type).ToList();
                        ((IDictionary<string, object>)data).Add(item.PropertyName, listItem);
                    }
                }

                return data;
            }
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="TFirst">回覆的資料類型1</typeparam>
        /// <typeparam name="TSecond">回覆的資料類型2</typeparam>
        /// <typeparam name="TReturn">回覆的資料類型</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="map">資料物件的組成方法</param>
        /// <param name="param">查詢參數物件</param>
        /// <param name="splitOn">使用join條件時，回傳資料的切分欄位</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>
        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string querySql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id", bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection(readOnlyConnection))
            {
                return await con.QueryAsync(querySql, map, param: param, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="TFirst">回覆的資料類型1</typeparam>
        /// <typeparam name="TSecond">回覆的資料類型2</typeparam>
        /// <typeparam name="TThird">回覆的資料類型3</typeparam>
        /// <typeparam name="TReturn">回覆的資料類型</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="map">資料物件的組成方法</param>
        /// <param name="param">查詢參數</param>
        /// <param name="splitOn">使用join條件時，回傳資料的切分欄位</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>
        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string querySql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id", bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection(readOnlyConnection))
            {
                return await con.QueryAsync(querySql, map, param: param, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="TFirst">回覆的資料類型1</typeparam>
        /// <typeparam name="TSecond">回覆的資料類型2</typeparam>
        /// <typeparam name="TThird">回覆的資料類型3</typeparam>
        /// <typeparam name="TFourth">回覆的資料類型4</typeparam>
        /// <typeparam name="TReturn">回覆的資料類型</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="map">資料物件的組成方法</param>
        /// <param name="param">查詢參數</param>
        /// <param name="splitOn">使用join條件時，回傳資料的切分欄位</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>
        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string querySql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, string splitOn = "Id", bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection(readOnlyConnection))
            {
                return await con.QueryAsync(querySql, map, param: param, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="TFirst">回覆的資料類型1</typeparam>
        /// <typeparam name="TSecond">回覆的資料類型2</typeparam>
        /// <typeparam name="TThird">回覆的資料類型3</typeparam>
        /// <typeparam name="TFourth">回覆的資料類型4</typeparam>
        /// <typeparam name="TFifth">回覆的資料類型5</typeparam>
        /// <typeparam name="TReturn">回覆的資料類型</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="map">資料物件的組成方法</param>
        /// <param name="param">查詢參數</param>
        /// <param name="splitOn">使用join條件時，回傳資料的切分欄位</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>
        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string querySql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, string splitOn = "Id", bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection(readOnlyConnection))
            {
                return await con.QueryAsync(querySql, map, param: param, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="TFirst">回覆的資料類型1</typeparam>
        /// <typeparam name="TSecond">回覆的資料類型2</typeparam>
        /// <typeparam name="TThird">回覆的資料類型3</typeparam>
        /// <typeparam name="TFourth">回覆的資料類型4</typeparam>
        /// <typeparam name="TFifth">回覆的資料類型5</typeparam>
        /// <typeparam name="TSixth">回覆的資料類型6</typeparam>
        /// <typeparam name="TReturn">回覆的資料類型</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="map">資料物件的組成方法</param>
        /// <param name="param">查詢參數</param>
        /// <param name="splitOn">使用join條件時，回傳資料的切分欄位</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>
        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string querySql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null, string splitOn = "Id", bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection(readOnlyConnection))
            {
                return await con.QueryAsync(querySql, map, param: param, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="TFirst">回覆的資料類型1</typeparam>
        /// <typeparam name="TSecond">回覆的資料類型2</typeparam>
        /// <typeparam name="TThird">回覆的資料類型3</typeparam>
        /// <typeparam name="TFourth">回覆的資料類型4</typeparam>
        /// <typeparam name="TFifth">回覆的資料類型5</typeparam>
        /// <typeparam name="TSixth">回覆的資料類型6</typeparam>
        /// <typeparam name="TSeventh">回覆的資料類型7</typeparam>
        /// <typeparam name="TReturn">回覆的資料類型</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="map">資料物件的組成方法</param>
        /// <param name="param">查詢參數</param>
        /// <param name="splitOn">使用join條件時，回傳資料的切分欄位</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>
        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string querySql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null, string splitOn = "Id", bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using IDbConnection con = GetDbConnection(readOnlyConnection);
            return await con.QueryAsync(querySql, map, param: param, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        }

        /// <summary>
        /// 查詢資料
        /// (keyEntity的name請使用nameof，若想使用like則在Value內使用%，則自動轉換為Like, 若value為陣列則自動傳換為IN, 若value為DateTime則條件自動為大於等於)
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="keyEntity">Where的物件</param>
        /// <param name="readOnlyConnection">是否使用Read Only Connetion</param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> QueryAsync<TResult>(DynamicParameters keyEntity = null, bool readOnlyConnection = false)
            where TResult : new()
        {
            string querySql = string.Empty;

            List<string> queryProperties = new List<string>();

            DynamicParameters dynamicParameters = new DynamicParameters();

            List<string> keyProperties = new List<string>();

            Type type = typeof(TResult);

            // 根據傳進來的物件抓取對應的TableName
            string tableName = DbConnectionExtensions.GetTableName(type);

            // 抓取傳進來的物件取得所有的Properties
            List<PropertyInfo> allProperties = DbConnectionExtensions.TypePropertiesCacheExcludeWriteFalse(type);

            foreach (PropertyInfo entityMember in allProperties)
            {
                queryProperties.Add($"[{entityMember.Name}]");
            }

            querySql += $"SELECT {string.Join($"{Environment.NewLine}, ", queryProperties)} FROM [{tableName}] ";

            if (keyEntity != null && keyEntity.ParameterNames.Any())
            {
                SqlMapper.IParameterLookup parametersLookup = keyEntity;

                foreach (string keyentityParameterName in keyEntity.ParameterNames)
                {
                    object? pValue = parametersLookup[keyentityParameterName];

                    if (pValue is not string and IEnumerable)
                    {
                        keyProperties.Add($"[{keyentityParameterName}] IN @key_{keyentityParameterName}");

                        dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                    }
                    else if (pValue is not string and DateTime)
                    {
                        keyProperties.Add($"[{keyentityParameterName}] = @key_{keyentityParameterName}");

                        dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                    }
                    else if (pValue is not string and null)
                    {
                        keyProperties.Add($"[{keyentityParameterName}] IS NULL");

                        dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                    }
                    else if (pValue is string)
                    {
                        if (!(pValue is not string || !pValue.ToString().Contains('%')))
                        {
                            keyProperties.Add($"[{keyentityParameterName}] LIKE @key_{keyentityParameterName}");
                        }
                        else
                        {
                            keyProperties.Add($"[{keyentityParameterName}] = @key_{keyentityParameterName}");
                        }

                        // 為了抓取Parameter上有沒有指定型態
                        IEnumerable<ColumnAttribute>? valueAttribute = allProperties.FirstOrDefault(x => string.Equals(x.Name, keyentityParameterName, StringComparison.OrdinalIgnoreCase))?.GetCustomAttributes<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();

                        // 如果有Attribute要看看型態來決定DbString的IsAnsi
                        if (valueAttribute != null && valueAttribute.Any())
                        {
                            var isChar = valueAttribute.Any(x =>
                                x.TypeName == DBColumnType.CharType);
                            dynamicParameters.Add("key_" + keyentityParameterName, new DbString { Value = pValue.ToString(), Length = pValue.ToString().Length, IsAnsi = isChar });
                        }
                        else
                        {
                            // for SKH.HIS.Model.DataAccessLayer.Common.ColumnAttribute case
                            var valueAttribute1 = allProperties.FirstOrDefault(x => x.Name == keyentityParameterName).GetCustomAttributes<ColumnAttribute>();
                            if (valueAttribute1 != null && valueAttribute1.Any())
                            {
                                bool isChar = valueAttribute1.Any(x =>
                                    x.TypeName == DBColumnType.CharType);

                                dynamicParameters.Add("key_" + keyentityParameterName, new DbString { Value = pValue.ToString(), Length = pValue.ToString().Length, IsAnsi = isChar });
                            }
                            else
                            {
                                dynamicParameters.Add("key_" + keyentityParameterName, new DbString { Value = pValue.ToString(), Length = pValue.ToString().Length, IsAnsi = false });
                            }
                        }
                    }
                    else
                    {
                        keyProperties.Add($"[{keyentityParameterName}] = @key_{keyentityParameterName}");

                        dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                    }
                }

                querySql += $"WHERE {string.Join($"{Environment.NewLine} AND ", keyProperties)} ";
            }

            using IDbConnection con = GetDbConnection(readOnlyConnection);
            return await con.QueryAsync<TResult>(querySql, dynamicParameters, null, _connectionTimeout, CommandType.Text).ConfigureAwait(false);
        }

        /// <summary>
        /// 查詢第一筆資料
        /// (無結果回傳Null)
        /// </summary>
        /// <typeparam name="TResult">回傳的資料型態</typeparam>
        /// <param name="querySql">SQL敘述</param>
        /// <param name="param">查詢參數</param>
        /// <param name="readOnlyConnection">是否使用 Read Only Connetion</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>資料物件</returns>
        public async Task<TResult> QueryFirstOrDefaultAsync<TResult>(string querySql, object param = null, bool readOnlyConnection = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection(readOnlyConnection))
            {
                return await con.QueryFirstOrDefaultAsync<TResult>(querySql, param, null, _connectionTimeout, commandType).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢資料
        /// (keyEntity的name請使用nameof，若想使用like則在Value內使用%，則自動轉換為Like, 若value為陣列則自動傳換為IN, 若value為DateTime則條件自動為大於等於)
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="keyEntity">Where的物件</param>
        /// <param name="readOnlyConnection">是否使用Read Only Connetion</param>
        /// <returns></returns>
        public async Task<TResult> QueryFirstOrDefaultAsync<TResult>(DynamicParameters keyEntity = null, bool readOnlyConnection = false)
            where TResult : new()
        {
            string querySql = string.Empty;

            var queryProperties = new List<string>();

            var dynamicParameters = new DynamicParameters();

            var keyProperties = new List<string>();

            var type = typeof(TResult);

            // 根據傳進來的物件抓取對應的TableName
            var tableName = DbConnectionExtensions.GetTableName(type);

            // 抓取傳進來的物件取得所有的Properties
            var allProperties = DbConnectionExtensions.TypePropertiesCacheExcludeWriteFalse(type);

            foreach (var entityMember in allProperties)
            {
                queryProperties.Add($"[{entityMember.Name}]");
            }

            querySql += $"SELECT {string.Join($"{Environment.NewLine}, ", queryProperties)} FROM [{tableName}] ";

            if (keyEntity != null && keyEntity.ParameterNames.Count() > 0)
            {
                var parametersLookup = (SqlMapper.IParameterLookup)keyEntity;

                foreach (var keyentityParameterName in keyEntity.ParameterNames)
                {
                    var pValue = parametersLookup[keyentityParameterName];

                    if (!(pValue is string) && pValue is IEnumerable)
                    {
                        keyProperties.Add($"[{keyentityParameterName}] IN @key_{keyentityParameterName}");
                        dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                    }
                    else if (!(pValue is string) && pValue is DateTime)
                    {
                        // TODO [Arch][OK][ Change to equal ]
                        keyProperties.Add($"[{keyentityParameterName}] = @key_{keyentityParameterName}");
                        dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                    }
                    else if (pValue == null)
                    {
                        keyProperties.Add($"[{keyentityParameterName}] IS NULL");
                        dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                    }
                    else if (pValue is string)
                    {
                        if (pValue is string && pValue.ToString().Contains("%"))
                        {
                            keyProperties.Add($"[{keyentityParameterName}] LIKE @key_{keyentityParameterName}");
                            dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                        }
                        else
                        {
                            keyProperties.Add($"[{keyentityParameterName}] = @key_{keyentityParameterName}");

                            // 為了抓取Parameter上有沒有指定型態
                            var valueAttribute = allProperties.FirstOrDefault(x => x.Name == keyentityParameterName).GetCustomAttributes<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();

                            // 如果有Attribute要看看型態來決定DbString的IsAnsi
                            if (valueAttribute != null && valueAttribute.Any())
                            {
                                bool isChar = valueAttribute.Any(x =>
                                    x.TypeName == DBColumnType.CharType);

                                dynamicParameters.Add("key_" + keyentityParameterName, new DbString { Value = pValue.ToString(), Length = pValue.ToString().Length, IsAnsi = isChar });
                            }
                            else
                            {
                                // for SKH.HIS.Model.DataAccessLayer.Common.ColumnAttribute case
                                var valueAttribute1 = allProperties.FirstOrDefault(x => x.Name == keyentityParameterName).GetCustomAttributes<ColumnAttribute>();
                                if (valueAttribute1 != null && valueAttribute1.Any())
                                {
                                    var isChar = valueAttribute1.Any(x =>
                                        x.TypeName == DBColumnType.CharType);

                                    dynamicParameters.Add("key_" + keyentityParameterName, new DbString { Value = pValue.ToString(), Length = pValue.ToString().Length, IsAnsi = isChar });
                                }
                                else
                                {
                                    dynamicParameters.Add("key_" + keyentityParameterName, new DbString { Value = pValue.ToString(), Length = pValue.ToString().Length, IsAnsi = false });
                                }
                            }
                        }
                    }
                    else
                    {
                        keyProperties.Add($"[{keyentityParameterName}] = @key_{keyentityParameterName}");

                        dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                    }
                }

                querySql += $"WHERE {string.Join($"{Environment.NewLine} AND ", keyProperties)} ";
            }

            using (IDbConnection con = GetDbConnection())
            {
                return await con.QueryFirstOrDefaultAsync<TResult>(querySql, dynamicParameters, null, _connectionTimeout, CommandType.Text).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 查詢全部資料
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <returns>資料物件</returns>
        public async Task<IEnumerable<TResult>> QueryAll<TResult>()
            where TResult : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return await con.GetAllAsync<TResult>(null, _connectionTimeout).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 依主鍵Primary Key取回單筆資料
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns>資料物件</returns>
        public async Task<TResult> QueryById<TResult>(int id)
            where TResult : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return await con.GetAsync<TResult>(id).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 依主鍵Primary Key取回單筆資料
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns>資料物件</returns>
        public async Task<TResult> QueryById<TResult>(long id)
            where TResult : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return await con.GetAsync<TResult>(id).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 依主鍵Primary Key取回單筆資料
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns>資料物件</returns>
        public async Task<TResult> QueryById<TResult>(string id)
            where TResult : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return await con.GetAsync<TResult>(id).ConfigureAwait(false);
            }
        }

        #endregion

        #region Execute 系列

        /// <summary>
        /// Excute Non-Query SQL，允許一次傳入多道SQL指令
        /// </summary>
        /// <param name="excuteSql">SQL敘述</param>
        /// <param name="param">參數物件</param>
        /// <param name="enableTransaction">包Transaction執行</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>影響資料筆數</returns>
        public async Task<int> ExecuteNonQueryAsync(string excuteSql, object param = null, bool enableTransaction = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            {
                if (!enableTransaction)
                {
                    return await con.ExecuteAsync(excuteSql, param, null, _connectionTimeout, commandType).ConfigureAwait(false);
                }
                else
                {
                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            var result = await con.ExecuteAsync(excuteSql, param, trans, _connectionTimeout, commandType).ConfigureAwait(false);
                            trans.Commit();
                            return result;
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ExecuteScalar，執行查詢並傳回第一個資料列的第一個資料行中查詢所傳回的結果
        /// </summary>
        /// <param name="excuteSql">SQL敘述</param>
        /// <param name="param">參數物件</param>
        /// <param name="enableTransaction">包Transaction執行</param>
        /// <param name="commandType">敘述類型</param>
        /// <returns>執行回覆結果</returns>
        public async Task<object> ExecuteScalarAsync(string excuteSql, object param = null, bool enableTransaction = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            {
                if (!enableTransaction)
                {
                    return await con.ExecuteScalarAsync(excuteSql, param, null, _connectionTimeout, commandType).ConfigureAwait(false);
                }
                else
                {
                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            var result = await con.ExecuteScalarAsync(excuteSql, param, trans, _connectionTimeout, commandType).ConfigureAwait(false);
                            trans.Commit();
                            return result;
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 執行交易 query
        /// </summary>
        /// <param name="taskList">任務清單</param>
        /// <returns></returns>
        public bool ExecuteTransactionQuery(params Action<IDbConnection, IDbTransaction>[] taskList)
        {
            StringBuilder traceBuilder = new StringBuilder();
            Stopwatch stopWatch = new Stopwatch();

            using (IDbConnection con = GetDbConnection())
            {
                using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        traceBuilder.AppendLine($"DB connection string : {con.ConnectionString}");

                        foreach (var act in taskList)
                        {
                            stopWatch.Restart();

                            traceBuilder.AppendLine($"{act.Method.Name} Before action Connection.State : {con.State} [{stopWatch.ElapsedMilliseconds}]");

                            if (con.State == ConnectionState.Closed)
                            {
                                con.Open();
                            }

                            act(con, transaction);

                            traceBuilder.AppendLine($"{act.Method.Name} After action Connection.State : {con.State} [{stopWatch.ElapsedMilliseconds}]");
                        }

                        traceBuilder.AppendLine($"Before Commit Connection.State : {con.State}");

                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }

                        transaction.Commit();

                        traceBuilder.AppendLine($"After Commit Connection.State : {con.State}");

                        return true;
                    }
                    catch (Exception transactionException)
                    {
                        try
                        {
                            traceBuilder.AppendLine($"{nameof(transactionException)} Message : {transactionException.Message} Trace : {transactionException.StackTrace}");
                            traceBuilder.AppendLine($"Before Transaction Rollback Connection.State : {con.State}");

                            // Make sure connection is opend, prevent to throw ZombieCheck exception
                            if (con.State == ConnectionState.Closed)
                            {
                                con.Open();
                            }

                            transaction.Rollback();

                            traceBuilder.AppendLine($"After Transaction Rollback Connection.State : {con.State}");
                        }
                        catch (Exception rollbackException)
                        {
                            traceBuilder.AppendLine($"{nameof(rollbackException)} Message : {rollbackException.Message} Trace : {rollbackException.StackTrace}");

                            _logger?.Error(traceBuilder.ToString(), transactionException, rollbackException);

                            throw;
                        }

                        _logger?.Error(traceBuilder.ToString(), transactionException, transactionException);
                        throw;
                    }
                    finally
                    {
                        stopWatch.Stop();
                    }
                }
            }
        }

        /// <summary>
        /// 執行交易 query (async)
        /// </summary>
        /// <param name="taskList">任務清單</param>
        /// <returns></returns>
        public async Task<bool> ExecuteTransactionQuery(params Func<IDbConnection, IDbTransaction, Task>[] taskList)
        {
            StringBuilder traceBuilder = new StringBuilder();
            Stopwatch stopWatch = new Stopwatch();

            using IDbConnection con = GetDbConnection();
            using IDbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted);
            try
            {
                traceBuilder.AppendLine($"DB connection string : {con.ConnectionString}");

                foreach (Func<IDbConnection, IDbTransaction, Task> act in taskList)
                {
                    stopWatch.Restart();

                    traceBuilder.AppendLine($"{act.Method.Name} Before action Connection.State : {con.State} [{stopWatch.ElapsedMilliseconds}]");

                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    await act(con, transaction).ConfigureAwait(false);

                    traceBuilder.AppendLine($"{act.Method.Name} After action Connection.State : {con.State} [{stopWatch.ElapsedMilliseconds}]");
                }

                traceBuilder.AppendLine($"Before Commit Connection.State : {con.State}");

                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                transaction.Commit();

                traceBuilder.AppendLine($"After Commit Connection.State : {con.State}");

                return true;
            }
            catch (Exception transactionException)
            {
                try
                {
                    traceBuilder.AppendLine($"{nameof(transactionException)} Message : {transactionException.Message} Trace : {transactionException.StackTrace}");
                    traceBuilder.AppendLine($"Before Transaction Rollback Connection.State : {con.State}");

                    // Make sure connection is opend, prevent to throw ZombieCheck exception
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    transaction.Rollback();

                    traceBuilder.AppendLine($"After Transaction Rollback Connection.State : {con.State}");
                }
                catch (Exception rollbackException)
                {
                    traceBuilder.AppendLine($"{nameof(rollbackException)} Message : {rollbackException.Message} Trace : {rollbackException.StackTrace}");

                    _logger?.Error(traceBuilder.ToString(), transactionException, rollbackException);

                    throw;
                }

                _logger?.Error(traceBuilder.ToString(), transactionException, transactionException);
                throw;
            }
            finally
            {
                stopWatch.Stop();
            }
        }
        #endregion

        #region Insert 系列

        /// <summary>
        /// 新增資料返回 KeyAttribute 定義欄位值
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <typeparam name="TResult">TResult</typeparam>
        /// <param name="insertEntity">InsertEntity</param>
        /// <returns></returns>
        public async Task<TResult> InsertReturnKeyAsync<T, TResult>(T insertEntity)
                where T : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return con.InsertReturnKey<T, TResult>(insertEntity, null, _connectionTimeout);
            }
        }

        /// <summary>
        /// 新增單筆或多筆資料
        /// </summary>
        /// <typeparam name="T">新增資料物件類型</typeparam>
        /// <param name="insertEntity">新增物件</param>
        /// <returns>The ID(primary key) of the newly inserted record if it is identity using the defined type, otherwise null</returns>
        public async Task<long> InsertDueToLongAsync<T>(T insertEntity)
            where T : class
        {
            using IDbConnection con = GetDbConnection();
            Task<long> task = Task.Run<long>(() =>
            {
                return con.Insert(insertEntity, null, _connectionTimeout);
            });
            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// 新增單筆或多筆.
        /// </summary>
        /// <typeparam name="T">新增資料物件Type or IEnumable</typeparam>
        /// <param name="insertEntity">新增物件</param>
        /// <param name="enableTransaction">是否使用Transaction</param>
        /// <param name="useBusinessTransaction">使用 BusinessTransaction，若為 false 則仍透過白名單驗證資料表</param>
        /// <returns>The ID(primary key) of the newly inserted record if it is identity using the defined type, otherwise null</returns>
        public async Task<int> InsertAsync<T>(T insertEntity, bool enableTransaction = false, bool useBusinessTransaction = false)
            where T : class
        {
            using IDbConnection con = GetDbConnection();
            if (!enableTransaction)
            {
                int insertResult = await con.InsertAsync(insertEntity, null, _connectionTimeout).ConfigureAwait(false);
                return insertResult;
            }
            else
            {
                using var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted);
                try
                {
                    int insertResult = await con.InsertAsync(insertEntity, transaction, _connectionTimeout).ConfigureAwait(false);

                    transaction.Commit();
                    return insertResult;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        /// <summary>
        /// insert transation for insert data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con">connection</param>
        /// <param name="transaction">transcation</param>
        /// <param name="insertResult">result</param>
        /// <param name="useBusinessTransaction">is require business trans</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task CreateTransationForInsert<T>(IDbConnection con, IDbTransaction transaction, string insertResult, bool useBusinessTransaction)
        {
            throw new NotImplementedException("CreateTransationForInsert method is not implemented.");
        }

        /// <summary>
        /// 新增 交易 Rollback for Delete 資料
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="originalData">刪除前原始資料</param>
        /// <returns></returns>
        public async Task CreateTransationForDelete<T>(T originalData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 新增主次關係資料 (自動將Parent Key帶入 Child的 foreign key)
        /// </summary>
        /// <typeparam name="TParent">Parent 只能限制一筆</typeparam>
        /// <typeparam name="TChild">Child 可以單筆或多筆</typeparam>
        /// <param name="parent">Parent Data</param>
        /// <param name="child">Child Data</param>
        /// <returns></returns>
        public async Task<bool> InsertAsync<TParent, TChild>(TParent parent, TChild child)
            where TParent : class
            where TChild : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            {
                using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        if (await con.InsertAsync(parent, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                        {
                            SetParentKeytoChildRefKey(ref parent, ref child);
                            if (await con.InsertAsync(child, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                            {
                                transaction.Commit();
                                isOk = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                return isOk;
            }
        }

        /// <summary>
        /// 新增主次關係資料 (自動將Parent Key帶入 Child的 foreign key)
        /// </summary>
        /// <typeparam name="TParent">Parent 只能限制一筆</typeparam>
        /// <typeparam name="TChild">Child 可以單筆或多筆</typeparam>
        /// <param name="parent">Parent Data</param>
        /// <param name="child">Child Data</param>
        /// <returns>Parent Id</returns>
        public async Task<long> InsertDueToLongAsync<TParent, TChild>(TParent parent, TChild child)
            where TParent : class
            where TChild : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                Task<long> task = Task.Run<long>(() =>
                {
                    long resultId = 0;
                    using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        try
                        { 
                            long parentId = con.Insert(parent, transaction, _connectionTimeout);
                            if (parentId > 0)
                            {
                                SetParentKeytoChildRefKey(ref parent, ref child);
                                if (con.Insert(child, transaction, _connectionTimeout) > 0)
                                {
                                    transaction.Commit();
                                    resultId = parentId;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }

                    return resultId;
                });

                return await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 新增主次關係資料 (自動將Parent Key帶入 Child1和Child2的 foreign key)
        /// </summary>
        /// <typeparam name="TParent">Parent 只能限制一筆</typeparam>
        /// <typeparam name="TChild1">Child1 可以單筆或多筆</typeparam>
        /// <typeparam name="TChild2">Child2 可以單筆或多筆</typeparam>
        /// <param name="parent">Parent Data</param>
        /// <param name="child1">Child Data1</param>
        /// <param name="child2">Child Data2</param>
        /// <returns></returns>
        public async Task<bool> InsertAsync<TParent, TChild1, TChild2>(TParent parent, TChild1 child1, TChild2 child2)
            where TParent : class
            where TChild1 : class
            where TChild2 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            {
                using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        if (await con.InsertAsync(parent, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                        {
                            SetParentKeytoChildRefKey(ref parent, ref child1);
                            SetParentKeytoChildRefKey(ref parent, ref child2);
                            if (await con.InsertAsync(child1, transaction, _connectionTimeout).ConfigureAwait(false) > 0
                                && await con.InsertAsync(child2, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                            {
                                transaction.Commit();
                                isOk = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                return isOk;
            }
        }

        /// <summary>
        /// 新增主次關係資料 (自動將Parent Key帶入 Child1和Child2的 foreign key)
        /// </summary>
        /// <typeparam name="TParent">Parent 只能限制一筆</typeparam>
        /// <typeparam name="TChild1">Child1 可以單筆或多筆</typeparam>
        /// <typeparam name="TChild2">Child2 可以單筆或多筆</typeparam>
        /// <param name="parent">Parent Data</param>
        /// <param name="child1">Child Data1</param>
        /// <param name="child2">Child Data2</param>
        /// <returns>Parent Id</returns>
        public async Task<long> InsertDueToLongAsync<TParent, TChild1, TChild2>(TParent parent, TChild1 child1, TChild2 child2)
            where TParent : class
            where TChild1 : class
            where TChild2 : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                Task<long> task = Task.Run<long>(() =>
                {
                    long resultId = 0;
                    using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        try
                        {
                            var parentId = con.Insert(parent, transaction, _connectionTimeout);
                            if (parentId > 0)
                            {
                                SetParentKeytoChildRefKey(ref parent, ref child1);
                                SetParentKeytoChildRefKey(ref parent, ref child2);
                                if (con.Insert(child1, transaction, _connectionTimeout) > 0
                                    && con.Insert(child2, transaction, _connectionTimeout) > 0)
                                {
                                    transaction.Commit();
                                    resultId = parentId;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }

                    return resultId;
                });

                return await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 新增主次關係資料 (自動將Parent Key帶入 Child1和Child2的 foreign key)
        /// </summary>
        /// <typeparam name="TParent">Parent 只能限制一筆</typeparam>
        /// <typeparam name="TChild1">Child1 可以單筆或多筆</typeparam>
        /// <typeparam name="TChild2">Child2 可以單筆或多筆</typeparam>
        /// <typeparam name="TChild3">Child3 可以單筆或多筆</typeparam>
        /// <param name="parent">Parent Data</param>
        /// <param name="child1">Child Data1</param>
        /// <param name="child2">Child Data2</param>
        /// <param name="child3">Child Data3</param>
        /// <returns></returns>
        public async Task<bool> InsertAsync<TParent, TChild1, TChild2, TChild3>(TParent parent, TChild1 child1, TChild2 child2, TChild3 child3)
            where TParent : class
            where TChild1 : class
            where TChild2 : class
            where TChild3 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            {
                using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        if (await con.InsertAsync(parent, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                        {
                            SetParentKeytoChildRefKey(ref parent, ref child1);
                            SetParentKeytoChildRefKey(ref parent, ref child2);
                            SetParentKeytoChildRefKey(ref parent, ref child3);
                            if (await con.InsertAsync(child1, transaction, _connectionTimeout).ConfigureAwait(false) > 0
                                && await con.InsertAsync(child2, transaction, _connectionTimeout).ConfigureAwait(false) > 0
                                && await con.InsertAsync(child3, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                            {
                                transaction.Commit();
                                isOk = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                return isOk;
            }
        }

        /// <summary>
        /// 新增主次關係資料 (自動將Parent Key帶入 Child1和Child2的 foreign key)
        /// </summary>
        /// <typeparam name="TParent">Parent 只能限制一筆</typeparam>
        /// <typeparam name="TChild1">Child1 可以單筆或多筆</typeparam>
        /// <typeparam name="TChild2">Child2 可以單筆或多筆</typeparam>
        /// <typeparam name="TChild3">Child3 可以單筆或多筆</typeparam>
        /// <typeparam name="TChild4">Child4 可以單筆或多筆</typeparam>
        /// <param name="parent">Parent Data</param>
        /// <param name="child1">Child Data1</param>
        /// <param name="child2">Child Data2</param>
        /// <param name="child3">Child Data3</param>
        /// <param name="child4">Child Data4</param>
        /// <returns></returns>
        public async Task<bool> InsertAsync<TParent, TChild1, TChild2, TChild3, TChild4>(TParent parent, TChild1 child1, TChild2 child2, TChild3 child3, TChild4 child4)
            where TParent : class
            where TChild1 : class
            where TChild2 : class
            where TChild3 : class
            where TChild4 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            {
                using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        if (await con.InsertAsync(parent, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                        {
                            bool insertChild1 = true;
                            if (child1 != null)
                            {
                                SetParentKeytoChildRefKey(ref parent, ref child1);
                                insertChild1 = await con.InsertAsync(child1, transaction, _connectionTimeout).ConfigureAwait(false) > 0;
                            }

                            bool insertChild2 = true;
                            if (child2 != null)
                            {
                                SetParentKeytoChildRefKey(ref parent, ref child2);
                                insertChild2 = await con.InsertAsync(child2, transaction, _connectionTimeout).ConfigureAwait(false) > 0;
                            }

                            bool insertChild3 = true;
                            if (child3 != null)
                            {
                                SetParentKeytoChildRefKey(ref parent, ref child3);
                                insertChild3 = await con.InsertAsync(child3, transaction, _connectionTimeout).ConfigureAwait(false) > 0;
                            }

                            bool insertChild4 = true;
                            if (child4 != null)
                            {
                                SetParentKeytoChildRefKey(ref parent, ref child4);
                                insertChild4 = await con.InsertAsync(child4, transaction, _connectionTimeout).ConfigureAwait(false) > 0;
                            }

                            if (insertChild1
                                && insertChild2
                                && insertChild3
                                && insertChild4)
                            {
                                transaction.Commit();
                                isOk = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                return isOk;
            }
        }

        /// <summary>
        /// 新增三個不同類型資料
        /// </summary>
        /// <typeparam name="T">新增資料物件Type</typeparam>
        /// <typeparam name="T2">新增資料物件Type2</typeparam>
        /// <typeparam name="T3">新增資料物件Type3</typeparam>
        /// <param name="insertEntities">新增物件</param>
        /// <param name="insertEntities2">新增物件2</param>
        /// <param name="insertEntities3">新增物件3</param>
        /// <returns></returns>
        public async Task<bool> InsertThreeAsync<T, T2, T3>(T insertEntities, T2 insertEntities2, T3 insertEntities3)
            where T : class
            where T2 : class
            where T3 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            {
                using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        if (await con.InsertAsync(insertEntities, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                        {
                            if (await con.InsertAsync(insertEntities2, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                            {
                                if (await con.InsertAsync(insertEntities3, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                                {
                                    transaction.Commit();
                                    isOk = true;
                                }
                            }
                        }

                        if (!isOk)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return isOk;
        }

        /// <summary>
        /// 新增多筆資料
        /// </summary>
        /// <typeparam name="T">資料物件類別</typeparam>
        /// <param name="entities">資料物件集合</param>
        /// <param name="conn">Connection</param>
        /// <param name="tran">Transaction</param>
        /// <returns>新增資料筆數</returns>
        public async Task BulkInsertAsync<T>(List<T> entities, IDbConnection conn = null, IDbTransaction tran = null)
            where T : class
        {
            if (entities is null || !entities.Any())
            {
                return;
            }

            conn = conn ?? GetSqlDbConnection();
            if (conn is StackExchange.Profiling.Data.ProfiledDbConnection sqlConn)
            {
                conn = sqlConn;
            }

            using (conn)
            {
                tran = tran ?? conn.BeginTransaction();
                if (tran is StackExchange.Profiling.Data.ProfiledDbTransaction sqlTran)
                {
                    tran = sqlTran;
                }

                using (tran)
                {
                    try
                    {
                        await SqlBulkCopyAsync(entities, conn, tran).ConfigureAwait(false);

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        _logger.Error("Exception in BulkInsertAsync", ex);

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 新增多筆資料
        /// </summary>
        /// <typeparam name="T">資料物件類別</typeparam>
        /// <param name="entities">資料物件集合</param>
        /// <param name="conn">Connection</param>
        /// <param name="tran">Transaction</param>
        /// <returns>新增資料筆數</returns>
        public async Task<int> BulkInsertWithOutputAsync<T>(List<T> entities, IDbConnection conn = null, IDbTransaction tran = null)
            where T : class
        {
            try
            {
                if (entities is null || !entities.Any())
                {
                    return 0;
                }

                conn = conn ?? GetSqlDbConnection();
                if (conn is StackExchange.Profiling.Data.ProfiledDbConnection sqlConn)
                {
                    conn = sqlConn;
                }

                using (conn)
                {
                    bool needTrx = tran == null;

                    tran = tran ?? conn.BeginTransaction();
                    if (tran is StackExchange.Profiling.Data.ProfiledDbTransaction sqlTran)
                    {
                        tran = sqlTran;
                    }

                    using (tran)
                    {
                        try
                        {
                            var tableAtt = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
                            var tableName = tableAtt != null ? tableAtt.Name : typeof(T).Name;

                            var tempTable = $"#TMP_{tableName}";
                            var createTempTable =
                                $@"
                            IF OBJECT_ID('TEMPDB..{tempTable}') IS NOT NULL DROP TABLE {tempTable};

                            SELECT TOP 0 * INTO {tempTable} FROM {tableName} WITH(NOLOCK);

                            DECLARE @udpateDefaultScript NVARCHAR(MAX)
                            SELECT @udpateDefaultScript = COALESCE(@udpateDefaultScript + '; ', '') + 
	                            'ALTER TABLE ' + '{tempTable}' + ' ADD DEFAULT ' + COLUMN_DEFAULT + ' FOR ' + COLUMN_NAME
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = '{tableName}' AND COLUMN_DEFAULT IS NOT NULL
                            -- SELECT @udpateDefaultScript
                            EXECUTE sp_executesql @udpateDefaultScript;
                            ";
                            conn.Execute(createTempTable, null, tran);

                            var targetColumns = await SqlBulkCopyAsyncOutProp<T>(entities, conn, tran, tempTable).ConfigureAwait(false);

                            var strTargetColumns = string.Join(",", targetColumns);

                            var sql = $@"INSERT INTO {tableName} ({strTargetColumns})
                                    OUTPUT inserted.*
                                    SELECT {strTargetColumns}
                                    FROM {tempTable};
                                    DROP TABLE {tempTable}";

                            var data = await conn.QueryAsync<T>(sql, null, tran).ConfigureAwait(false);

                            if (needTrx)
                            {
                                tran.Commit();
                            }

                            entities.Clear();

                            entities.AddRange(data);

                            var type = typeof(T);
                            var keyProperty = DbConnectionExtensions.GetKeyAndExplicitKeyPropertyInfo(type);
                            if (keyProperty != null)
                            {
                                foreach (var item in data)
                                {
                                    var id = keyProperty.GetValue(item).ToString();
                                    await CreateTransationForInsert<T>(conn, tran, id, false).ConfigureAwait(false);
                                }
                            }

                            return data.Count();
                        }
                        catch (Exception ex)
                        {
                            // InnerException.Message 會說哪個欄位有問題.
                            _logger.Error("Exception in BulkInsertWithOutputAsync (Transaction)", ex.Message, ex.InnerException.Message, ex);

                            if (needTrx)
                            {
                                tran.Rollback();
                                return 0;
                            }

                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception in BulkInsertWithOutputAsync", ex);
                throw;
            }
        }

        private async Task SqlBulkCopyAsync<T>(List<T> entities, IDbConnection conn, IDbTransaction tran)
            where T : class
        {
            using (var bulkCopy = new SqlBulkCopy(conn as SqlConnection, SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.CheckConstraints, tran as SqlTransaction))
            {
                var tableAtt = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
                var tableName = tableAtt != null ? tableAtt.Name : typeof(T).Name;

                // 資料實體對應的資料表名稱;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();

                var properties = typeof(T).GetProperties();
                List<PropertyInfo> usedProperties = new List<PropertyInfo>();

                foreach (var prop in properties)
                {
                    var ctxWrtAttr = prop.GetCustomAttribute(typeof(WriteAttribute));
                    var ctxCptAttr = prop.GetCustomAttribute(typeof(ComputedAttribute));

                    if ((ctxWrtAttr == null || (ctxWrtAttr as WriteAttribute).Write == true) && ctxCptAttr == null)
                    {
                        var dc = new DataColumn(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName); // 用ColumnName強制對應，避免 Table欄位 與 entities 的順序不一致
                        table.Columns.Add(dc);
                        usedProperties.Add(prop);
                    }
                }

                foreach (T item in entities)
                {
                    DataRow row = table.NewRow();
                    foreach (var prop in usedProperties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }

                    table.Rows.Add(row);
                }

                await bulkCopy.WriteToServerAsync(table).ConfigureAwait(false);
            }
        }

        private async Task<List<string>> SqlBulkCopyAsyncOutProp<T>(List<T> entities, IDbConnection conn, IDbTransaction tran, string tempTable)
            where T : class
        {
            var targetColumns = new List<string>();
            using (var bulkCopy = new SqlBulkCopy(conn as SqlConnection, SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.CheckConstraints, tran as SqlTransaction))
            {
                // 資料實體對應的資料表名稱;
                bulkCopy.DestinationTableName = tempTable;

                var table = new DataTable();

                var type = typeof(T);

                var allProperties = DbConnectionExtensions.TypePropertiesCacheExcludeWriteFalse(type);
                var keyProperties = DbConnectionExtensions.KeyPropertiesCache(type);
                var computedProperties = DbConnectionExtensions.ComputedPropertiesCache(type);
                var allPropertiesExcepAndComputed = allProperties.Except(keyProperties.Union(computedProperties)).ToList();

                List<PropertyInfo> usedProperties = new List<PropertyInfo>();

                foreach (var prop in allPropertiesExcepAndComputed)
                {
                    var dc = new DataColumn(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName); // 用ColumnName強制對應，避免 Table欄位 與 entities 的順序不一致
                    table.Columns.Add(dc);
                    usedProperties.Add(prop);
                    targetColumns.Add(dc.ColumnName);
                }

                foreach (T item in entities)
                {
                    DataRow row = table.NewRow();
                    foreach (var prop in usedProperties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }

                    table.Rows.Add(row);
                }

                await bulkCopy.WriteToServerAsync(table).ConfigureAwait(false);
            }

            return targetColumns;
        }

        #endregion

        #region Update 系列

        /// <summary>
        /// 更新單筆資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <param name="updateEntity">更新物件</param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync<T>(T updateEntity)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return await con.UpdateAsync(updateEntity, null, _connectionTimeout).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 更新兩筆不同類型資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <typeparam name="T2">更新資料物件Type2</typeparam>
        /// <param name="updateEntity">更新物件</param>
        /// <param name="updateEntity2">更新物件2</param>
        /// <returns></returns>
        public async Task<bool> UpdateMutilAsync<T, T2>(T updateEntity, T2 updateEntity2)
            where T : class
            where T2 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            {
                using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        if (await con.UpdateAsync(updateEntity, transaction, _connectionTimeout).ConfigureAwait(false))
                        {
                            if (await con.UpdateAsync(updateEntity2, transaction, _connectionTimeout).ConfigureAwait(false))
                            {
                                transaction.Commit();
                                isOk = true;
                            }
                        }

                        if (!isOk)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            return isOk;
        }

        /// <summary>
        /// 更新三筆不同類型資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <typeparam name="T2">更新資料物件Type2</typeparam>
        /// <typeparam name="T3">更新資料物件Type3</typeparam>
        /// <param name="updateEntity">更新物件</param>
        /// <param name="updateEntity2">更新物件2</param>
        /// <param name="updateEntity3">更新物件3</param>
        /// <returns></returns>
        public async Task<bool> UpdateMutilAsync<T, T2, T3>(T updateEntity, T2 updateEntity2, T3 updateEntity3)
            where T : class
            where T2 : class
            where T3 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (await con.UpdateAsync(updateEntity, transaction, _connectionTimeout).ConfigureAwait(false))
                    {
                        if (await con.UpdateAsync(updateEntity2, transaction, _connectionTimeout).ConfigureAwait(false))
                        {
                            if (await con.UpdateAsync(updateEntity3, transaction, _connectionTimeout).ConfigureAwait(false))
                            {
                                transaction.Commit();
                                isOk = true;
                            }
                        }
                    }

                    if (!isOk)
                    {
                        transaction.Rollback();
                    }
                }
                catch
                {
                    transaction.Rollback();
                }
            }

            return isOk;
        }

        /// <summary>
        /// 更新資料
        /// </summary>
        /// <param name="tableName">更新Table名稱</param>
        /// <param name="key">指定updateEntity裡面的Where Condition,如果多個key用","分隔, key must in updateEntity property</param>
        /// <param name="updateEntity">更新的物件</param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(string tableName, string key, object updateEntity)
        {
            if (string.IsNullOrWhiteSpace(key) || updateEntity == null)
            {
                return false;
            }

            string[] keyList = key.Split(',');
            List<string> keyFilter = new List<string>();
            List<string> updateProperties = new List<string>();
            foreach (PropertyInfo pro in updateEntity.GetType().GetProperties())
            {
                if (keyList.Contains(pro.Name))
                {
                    keyFilter.Add($"[{pro.Name}] {(pro.PropertyType.IsArray ? "in" : "=")} @{pro.Name}");
                }
                else
                {
                    updateProperties.Add($"[{pro.Name}] = @{pro.Name}");
                }
            }

            string sql = $"update [{tableName}] set {string.Join(", ", updateProperties)} where {string.Join(" and ", keyFilter)}";

            using IDbConnection con = GetDbConnection();
            return await con.ExecuteAsync(sql, updateEntity, null, _connectionTimeout, CommandType.Text).ConfigureAwait(false) > 0;
        }

        /// <summary>
        /// 更新資料
        /// </summary>
        /// <param name="tableName">更新Table名稱</param>
        /// <param name="keyEntity">Where的物件</param>
        /// <param name="updateEntity">更新的物件</param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(string tableName, object keyEntity, object updateEntity)
        {
            if (keyEntity == null || updateEntity == null)
            {
                return false;
            }

            DynamicParameters dynamicParameters = new DynamicParameters();
            List<string> keyProperties = new List<string>();
            List<string> updateProperties = new List<string>();

            foreach (var entityMember in updateEntity.GetType().GetProperties())
            {
                dynamicParameters.Add("set_" + entityMember.Name, entityMember.GetValue(updateEntity));
                updateProperties.Add($"[{entityMember.Name}] = @set_{entityMember.Name}");
            }

            foreach (PropertyInfo keyentityMember in keyEntity.GetType().GetProperties())
            {
                dynamicParameters.Add("key_" + keyentityMember.Name, keyentityMember.GetValue(keyEntity));
                keyProperties.Add($"[{keyentityMember.Name}] {(keyentityMember.PropertyType.IsArray ? "in" : "=")} @key_{keyentityMember.Name}");
            }

            string sql = $"update [{tableName}] set {string.Join(", ", updateProperties)} where {string.Join(" and ", keyProperties)}";

            using (IDbConnection con = GetDbConnection())
            {
                return await con.ExecuteAsync(sql, dynamicParameters, null, _connectionTimeout, CommandType.Text).ConfigureAwait(false) > 0;
            }
        }

        /// <summary>
        /// 更新資料(多筆同類型資料)
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <param name="updateEntities">更新資料物件s</param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync<T>(IEnumerable<T> updateEntities)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            using (var trans = con.BeginTransaction())
            {
                try
                {
                    foreach (var updateEntity in updateEntities)
                    {
                        if (!await con.UpdateAsync(updateEntity, trans, _connectionTimeout).ConfigureAwait(false))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 更新單筆資料(可自訂更新欄位與更新條件)
        /// </summary>
        /// <typeparam name="TDalModel">更新的 Dal Model Type</typeparam>
        /// <param name="updateInfo">更新的欄位名稱(kay)與資料(value)</param>
        /// <param name="conditionInfo">更新的欄位名稱(kay)與條件(value)</param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync<TDalModel>(Dictionary<string, object> updateInfo, Dictionary<string, object> conditionInfo)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            List<string> updateProperties = new List<string>();
            List<string> conditionProperties = new List<string>();

            var modelType = typeof(TDalModel);
            var hasTableAttribute = modelType.IsDefined(typeof(TableAttribute), true);
            var tableName = hasTableAttribute ? ((TableAttribute)modelType.GetCustomAttributes(typeof(TableAttribute), true)[0]).Name : modelType.Name;

            string SqlStrUpdate(KeyValuePair<string, object> item, string alias)
            {
                dynamicParameters.Add($"{alias}_{item.Key}", item.Value);
                return $"[{item.Key}] = @{alias}_{item.Key}";
            }

            string SqlStrWhere(KeyValuePair<string, object> item, string alias)
            {
                dynamicParameters.Add($"{alias}_{item.Key}", item.Value);

                if (!(item.Value is string) && item.Value is IEnumerable)
                {
                    return $"[{item.Key}] IN @{alias}_{item.Key}";
                }
                else
                {
                    if (item.Value is string && item.Value.ToString().Contains("%"))
                    {
                        return $"[{item.Key}] LIKE @{alias}_{item.Key}";
                    }
                    else if (item.Value == null)
                    {
                        return $"[{item.Key}] IS NULL";
                    }
                    else
                    {
                        return $"[{item.Key}] = @{alias}_{item.Key}";
                    }
                }
            }

            updateProperties = updateInfo.Select(s => SqlStrUpdate(s, "set")).ToList();
            conditionProperties = conditionInfo.Select(s => SqlStrWhere(s, "key")).ToList();

            string sql = $"update [{tableName}] set {Environment.NewLine} {string.Join($"{Environment.NewLine}, ", updateProperties)} {Environment.NewLine} where {string.Join($"{Environment.NewLine} and ", conditionProperties)}";

            using (IDbConnection con = GetDbConnection())
            {
                return await con.ExecuteAsync(sql, dynamicParameters, null, _connectionTimeout, CommandType.Text).ConfigureAwait(false) > 0;
            }
        }

        /// <summary>
        /// 批量更新 (使用 User-Defined Table Types / Stored Procedure Started from SP_TVP_BatchUpdate_{TypeName}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="tran"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<int> BulkUpdateAsyncUseSqlBulkCopyAsnc<T>(IEnumerable<T> entities)
        {
            _logger?.Debug("Entities to batch Update:", entities);

            Type bulkUpdateType = typeof(T);

            using (IDbConnection conn = GetDbConnection())
            using (IDbTransaction sqlTran = conn.BeginTransaction())
            {
                try
                {
                    var rc = await BulkUpdateAsyncUseSqlBulkCopyAsnc(conn, sqlTran, entities);

                    if (rc > 0)
                    {
                        sqlTran.Commit();
                    }
                    else
                    {
                        sqlTran.Rollback();
                    }

                    return rc;
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Exception: {ex.Message}", ex, ex.StackTrace);

                    sqlTran.Rollback();

                    throw;
                }
            }
        }

        /// <summary>
        /// 批量更新 (使用 User-Defined Table Types / Stored Procedure Started from SP_TVP_BatchUpdate_{TypeName}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="tran"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<int> BulkUpdateAsyncUseSqlBulkCopyAsnc<T>(IDbConnection con, IDbTransaction tran, IEnumerable<T> entities)
        {
            try
            {
                _logger?.Debug("Entities to batch Update:", entities);

                Type bulkUpdateType = typeof(T);
                IDbConnection conn = con.GetType() == typeof(StackExchange.Profiling.Data.ProfiledDbConnection) ? (con as StackExchange.Profiling.Data.ProfiledDbConnection).WrappedConnection : con;
                IDbTransaction sqlTran = tran.GetType() == typeof(StackExchange.Profiling.Data.ProfiledDbTransaction) ? (tran as StackExchange.Profiling.Data.ProfiledDbTransaction).WrappedTransaction : tran;

                var tableAtt = bulkUpdateType.GetCustomAttribute(typeof(TableAttribute), true) as TableAttribute;

                // 有掛上Dapper.TableAttribute，則使用TableAttrubute Value，無則取TypeName
                string tableName = tableAtt != null ? tableAtt.Name : bulkUpdateType.Name;
                DataTable dataTable = new DataTable(tableName);
                PropertyInfo[] properties = typeof(T).GetProperties()
                    .Where(prop => null == prop.GetCustomAttribute(typeof(WriteAttribute)) || (prop.GetCustomAttribute(typeof(WriteAttribute)) as WriteAttribute).Write == true)
                    .OrderBy(prop =>
                    {
                        System.ComponentModel.DataAnnotations.Schema.ColumnAttribute orderColumn = prop.GetCustomAttribute(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute), false) as System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;
                        int columnIndex = orderColumn?.Order ?? int.MaxValue;
                        return columnIndex;
                    })
                    .ToArray();

                foreach (var prop in properties)
                {
                    var dc = new DataColumn(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    dataTable.Columns.Add(dc);
                }

                foreach (T item in entities)
                {
                    DataRow row = dataTable.NewRow();
                    foreach (var prop in properties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }

                    dataTable.Rows.Add(row);
                }

                SqlCommand command = new SqlCommand();
                command.Connection = conn as SqlConnection;
                command.Transaction = sqlTran as SqlTransaction;

                command.CommandText = $"[dbo].[SP_TVP_BatchUpdate_{tableName}]";
                command.CommandType = CommandType.StoredProcedure;
                SqlParameter tvpParam = command.Parameters.AddWithValue("@tvpTable", dataTable);
                tvpParam.SqlDbType = SqlDbType.Structured;

                SqlParameter parOutput = command.Parameters.Add("@rows", SqlDbType.Int);
                parOutput.Direction = ParameterDirection.Output;

                int rc = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                int rows = (int)parOutput.Value;

                _logger?.Debug($"Updated the count: {rows}");

                return rows;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Exception: {ex.Message}", ex, ex.StackTrace);

                throw;
            }
        }
        #endregion

        #region Insert / Update / Delete Mix系列

        /// <summary>
        /// 更新一筆然後新增一筆資料
        /// </summary>
        /// <typeparam name="TUpdate">更新資料物件Type</typeparam>
        /// <typeparam name="TInsert">新增資料物件Type</typeparam>
        /// <param name="updateEnties">更新物件</param>
        /// <param name="insertEnties">新增物件</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateSingleInsertSingleAsync<TUpdate, TInsert>(TUpdate updateEnties, TInsert insertEnties)
            where TUpdate : class
            where TInsert : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (await con.UpdateAsync(updateEnties, transaction, _connectionTimeout).ConfigureAwait(false))
                    {
                        if (await con.InsertAsync(insertEnties, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                        {
                            transaction.Commit();
                            isOk = true;
                        }
                    }

                    if (!isOk)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return isOk;
        }

        /// <summary>
        /// 更新三筆、新增一筆不同類型資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <typeparam name="T2">更新資料物件Type2</typeparam>
        /// <typeparam name="T3">更新資料物件Type3</typeparam>
        /// <typeparam name="T4">新增資料物件Type</typeparam>
        /// <param name="updateEntity">更新物件</param>
        /// <param name="updateEntity2">更新物件2</param>
        /// <param name="updateEntity3">更新物件3</param>
        /// <param name="insertEntity">新增物件</param>
        /// <returns></returns>
        public async Task<bool> UpdateMutilInsertSingleAsync<T, T2, T3, T4>(T updateEntity, T2 updateEntity2, T3 updateEntity3, T4 insertEntity)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (await con.UpdateAsync(updateEntity, transaction, _connectionTimeout).ConfigureAwait(false))
                    {
                        if (await con.UpdateAsync(updateEntity2, transaction, _connectionTimeout).ConfigureAwait(false))
                        {
                            if (await con.UpdateAsync(updateEntity3, transaction, _connectionTimeout).ConfigureAwait(false))
                            {
                                if (await con.InsertAsync(insertEntity, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                                {
                                    transaction.Commit();
                                    isOk = true;
                                }
                            }
                        }
                    }

                    if (!isOk)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return isOk;
        }

        /// <summary>
        /// 更新二筆、新增二筆不同類型資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type1</typeparam>
        /// <typeparam name="T2">更新資料物件Type2</typeparam>
        /// <typeparam name="T3">新增資料物件Type1</typeparam>
        /// <typeparam name="T4">新增資料物件Type2</typeparam>
        /// <param name="updateEntity1">更新物件1</param>
        /// <param name="updateEntity2">更新物件2</param>
        /// <param name="insertEntity1">新增物件1</param>
        /// <param name="insertEntity2">新增物件2</param>
        /// <returns></returns>
        public async Task<bool> UpdateMutilInsertMutilAsync<T, T2, T3, T4>(T updateEntity1, T2 updateEntity2, T3 insertEntity1, T4 insertEntity2)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (await con.UpdateAsync(updateEntity1, transaction, _connectionTimeout).ConfigureAwait(false))
                    {
                        if (await con.UpdateAsync(updateEntity2, transaction, _connectionTimeout).ConfigureAwait(false))
                        {
                            if (await con.InsertAsync(insertEntity1, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                            {
                                if (await con.InsertAsync(insertEntity2, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                                {
                                    transaction.Commit();
                                    isOk = true;
                                }
                            }
                        }
                    }

                    if (!isOk)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return isOk;
        }

        /// <summary>
        /// 更新二筆、新增二筆不同類型資料（傳進來的資料長度可為0）
        /// </summary>
        /// <typeparam name="T">更新資料物件Type1</typeparam>
        /// <typeparam name="T2">更新資料物件Type2</typeparam>
        /// <typeparam name="T3">新增資料物件Type1</typeparam>
        /// <typeparam name="T4">新增資料物件Type2</typeparam>
        /// <param name="updateEntity1">更新物件1</param>
        /// <param name="updateEntity2">更新物件2</param>
        /// <param name="insertEntity1">新增物件1</param>
        /// <param name="insertEntity2">新增物件2</param>
        /// <returns></returns>
        public async Task<bool> UpdateMutilInsertMutilNoneCountAsync<T, T2, T3, T4>(T updateEntity1, T2 updateEntity2, T3 insertEntity1, T4 insertEntity2)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            bool isOk = true;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (isOk && updateEntity1 is IEnumerable<object> entity1)
                    {
                        if (entity1.Any())
                        {
                            isOk = await con.UpdateAsync(updateEntity1, transaction, _connectionTimeout).ConfigureAwait(false);
                        }
                    }

                    if (isOk && updateEntity2 is IEnumerable<object> entity2)
                    {
                        if (entity2.Any())
                        {
                            isOk = await con.UpdateAsync(updateEntity2, transaction, _connectionTimeout).ConfigureAwait(false);
                        }
                    }

                    if (isOk && insertEntity1 is IEnumerable<object> entity3)
                    {
                        if (entity3.Any())
                        {
                            isOk = await con.InsertAsync(insertEntity1, transaction, _connectionTimeout).ConfigureAwait(false) > 0;
                        }
                    }

                    if (isOk && insertEntity2 is IEnumerable<object> entity4)
                    {
                        if (entity4.Any())
                        {
                            isOk = await con.InsertAsync(insertEntity2, transaction, _connectionTimeout).ConfigureAwait(false) > 0;
                        }
                    }

                    if (isOk)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return isOk;
        }

        /// <summary>
        /// 更新三筆、新增二筆不同類型資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <typeparam name="T1">更新資料物件Type1</typeparam>
        /// <typeparam name="T2">更新資料物件Type2</typeparam>
        /// <typeparam name="T3">新增資料物件Type1</typeparam>
        /// <typeparam name="T4">新增資料物件Type2</typeparam>
        /// <param name="updateEntity">更新物件</param>
        /// <param name="updateEntity1">更新物件1</param>
        /// <param name="updateEntity2">更新物件2</param>
        /// <param name="insertEntity1">新增物件1</param>
        /// <param name="insertEntity2">新增物件2</param>
        /// <returns></returns>
        public async Task<bool> UpdateMutilInsertMutilAsync<T, T1, T2, T3, T4>(T updateEntity, T1 updateEntity1, T2 updateEntity2, T3 insertEntity1, T4 insertEntity2)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (await con.UpdateAsync(updateEntity, transaction, _connectionTimeout).ConfigureAwait(false))
                    {
                        if (await con.UpdateAsync(updateEntity1, transaction, _connectionTimeout).ConfigureAwait(false))
                        {
                            if (await con.UpdateAsync(updateEntity2, transaction, _connectionTimeout).ConfigureAwait(false))
                            {
                                if (await con.InsertAsync(insertEntity1, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                                {
                                    if (await con.InsertAsync(insertEntity2, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                                    {
                                        transaction.Commit();
                                        isOk = true;
                                    }
                                }
                            }
                        }
                    }

                    if (!isOk)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return isOk;
        }

        #endregion

        #region Delete 系列

        /// <summary>
        /// 刪除單筆或多筆資料
        /// </summary>
        /// <typeparam name="T">刪除單筆資料 or 刪除多筆資料</typeparam>
        /// <param name="deleteEntity">單筆資料 or 多筆資料</param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync<T>(T deleteEntity)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return await con.DeleteAsync(deleteEntity, null, _connectionTimeout).ConfigureAwait(false);
            }
        }
        #endregion

        #region 批價特殊

        /// <summary>
        /// 刪除二筆不同類別的資料,新增二筆不同類型的資料
        /// </summary>
        /// <typeparam name="TResult1">刪除資料物件Type1</typeparam>
        /// <typeparam name="TResult2">刪除資料物件Type2</typeparam>
        /// <param name="deleteEntity1">刪除物件1</param>
        /// <param name="deleteEntity2">刪除物件2</param>
        /// <param name="insertEntitiy1">新增物件1</param>
        /// <param name="insertEntitiy2">新增物件2</param>
        /// <returns></returns>
        public async Task<bool> DeleteInsertAsync<TResult1, TResult2>(TResult1 deleteEntity1, TResult2 deleteEntity2, TResult1 insertEntitiy1, TResult2 insertEntitiy2)
            where TResult1 : class
            where TResult2 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (await con.DeleteAsync(deleteEntity1, transaction, _connectionTimeout).ConfigureAwait(false))
                    {
                        if (await con.DeleteAsync(deleteEntity2, transaction, _connectionTimeout).ConfigureAwait(false))
                        {
                            if (await con.InsertAsync(insertEntitiy1, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                            {
                                if (await con.InsertAsync(insertEntitiy2, transaction, _connectionTimeout).ConfigureAwait(false) > 0)
                                {
                                    transaction.Commit();
                                    isOk = true;
                                }
                            }
                        }
                    }

                    if (!isOk)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return isOk;
        }

        #endregion

        #region DB Model 相關取得

        /// <summary>
        /// 取得Model的屬性字串
        /// </summary>
        /// <typeparam name="T">Model</typeparam>
        /// <param name="alias">要加上的別名</param>
        /// <param name="isParam">是否是組參數</param>
        /// <param name="excludeProperties">要排除的屬性，如果是多個屬性用","分隔</param>
        /// <returns>用","分隔的屬性字串</returns>
        public string GetModelProperties<T>(string alias = "", bool isParam = false, string excludeProperties = "")
            where T : new()
        {
            List<string> properties = new List<string>();
            var excludePropertiesArray = excludeProperties.Split(',');
            string str = isParam ? "_" : ".";

            alias = string.IsNullOrEmpty(alias) ? string.Empty : $"{alias}{str}";

            foreach (var property in new T().GetType().GetProperties())
            {
                // 檢查是否有 WriteAttribute 定義，有則排除，不組進SQL裡
                if (!excludePropertiesArray.Contains(property.Name) && !property.IsDefined(typeof(WriteAttribute), true))
                {
                    // 如果屬性有KeyAttribute，則排到第一個，才不會導致Dapper進行Split出現錯誤
                    if (property.IsDefined(typeof(KeyAttribute), true))
                    {
                        properties.Insert(0, $"{alias}{property.Name}");
                    }
                    else
                    {
                        properties.Add($"{alias}{property.Name}");
                    }
                }
            }

            return string.Join($"{Environment.NewLine}, ", properties);
        }

        /// <summary>
        /// 轉換至DataTable
        /// </summary>
        /// <typeparam name="T">Entity類別 OrderColumn 決定順序</typeparam>
        /// <param name="entities">轉換物件</param>
        /// <returns></returns>
        public DataTable ConvertToDataTable<T>(IEnumerable<T> entities)
        {
            Type entityType = typeof(T);

            var tableAtt = entityType.GetCustomAttribute(typeof(Dapper.Contrib.Extensions.TableAttribute), true) as Dapper.Contrib.Extensions.TableAttribute;
            // 有掛上Dapper.TableAttribute，則使用TableAttrubute Value，無則取TypeName
            string tableName = tableAtt != null ? tableAtt.Name : entityType.Name;
            DataTable dataTable = new DataTable(tableName);
            PropertyInfo[] properties = typeof(T).GetProperties()
                .OrderBy(prop =>
                {
                    System.ComponentModel.DataAnnotations.Schema.ColumnAttribute orderColumn = prop.GetCustomAttribute(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute), false) as System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;
                    int columnIndex = orderColumn?.Order ?? int.MaxValue;
                    return columnIndex;
                })
                .ToArray();

            foreach (var prop in properties)
            {
                var dc = new DataColumn(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                dataTable.Columns.Add(dc);
            }

            foreach (T item in entities)
            {
                DataRow row = dataTable.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
        #endregion

        #region Private 功能

        /// <summary>
        /// 建立資料庫連線
        /// </summary>
        /// <param name="readOnlyConnection">是否唯讀</param>
        /// <returns>資料庫連線</returns>
        private IDbConnection GetDbConnection(bool readOnlyConnection = false)
        {
            IDbConnection dBConnection;

            if (!_enableMiniprofiler)
            {
                dBConnection = new SqlConnection(readOnlyConnection ? _connectionSecStr : _connectionStr);
            }
            else
            {
                dBConnection = new StackExchange.Profiling.Data.ProfiledDbConnection(
                    new SqlConnection(readOnlyConnection ? _connectionSecStr : _connectionStr),
                    new CurrentDbProfiler(static () => StackExchange.Profiling.MiniProfiler.Current));
            }

            if (dBConnection.State != ConnectionState.Open)
            {
                dBConnection.Open();
            }

            return dBConnection;
        }

        /// <summary>
        /// GetSqlDbConnection
        /// </summary>
        /// <param name="readOnlyConnection">readOnlyConnection</param>
        /// <returns></returns>
        private IDbConnection GetSqlDbConnection(bool readOnlyConnection = false)
        {
            IDbConnection dBConnection;

            dBConnection = new SqlConnection(readOnlyConnection ? _connectionSecStr : _connectionStr);

            if (dBConnection.State != ConnectionState.Open)
            {
                dBConnection.Open();
            }

            return dBConnection;
        }

        /// <summary>
        /// 設定 Parent的 PK Property 到 Child的相同名稱欄位
        /// </summary>
        /// <typeparam name="TParent">Parent Type</typeparam>
        /// <typeparam name="TChild">Child Type</typeparam>
        /// <param name="parent">Parent</param>
        /// <param name="child">Child</param>
        private static void SetParentKeytoChildRefKey<TParent, TChild>(ref TParent parent, ref TChild child)
            where TParent : class
            where TChild : class
        {
            Type parentType = typeof(TParent);
            Type childType = typeof(TChild);
            Type keyAttrType = typeof(Dapper.Contrib.Extensions.KeyAttribute);
            PropertyInfo keyProperty = parentType.GetProperties().FirstOrDefault(prop => prop.GetCustomAttributes(keyAttrType).Any());
            if (keyProperty != null)
            {
                var isChildMutil = child is IEnumerable;
                if (isChildMutil)
                {
                    // Child為多筆
                    Type childGenericType = childType.GetGenericArguments().FirstOrDefault();
                    if (childGenericType != null)
                    {
                        var childRefKey = childGenericType.GetProperty(keyProperty.Name);
                        if (childRefKey != null)
                        {
                            if (keyProperty.PropertyType.Equals(childRefKey.PropertyType))
                            {
                                var keyValue = keyProperty.GetValue(parent);
                                foreach (object childItem in child as IEnumerable)
                                {
                                    childRefKey.SetValue(childItem, keyValue);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Child單筆
                    var childRefKey = childType.GetProperty(keyProperty.Name);
                    if (childRefKey != null)
                    {
                        if (keyProperty.PropertyType.Equals(childRefKey.PropertyType))
                        {
                            var keyValue = keyProperty.GetValue(parent);
                            childRefKey.SetValue(child, keyValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 是否有定義Writeable
        /// </summary>
        /// <param name="pi">PropertyInfo</param>
        /// <returns></returns>
        private static bool IsWriteable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(WriteAttribute), false).AsList();
            if (attributes.Count != 1)
            {
                return true;
            }

            var writeAttribute = (WriteAttribute)attributes[0];
            return writeAttribute.Write;
        }

        #endregion
    }



    /// <summary>
    /// Help Connection to DB specific connection
    /// </summary>
    /// <typeparam name="T">Connection Class</typeparam>
    /// <remarks>
    /// Specific Connection Setting
    /// </remarks>
    /// <param name="daOption">Data Access Setting</param>
    /// <param name="generalOption">Basic Setting</param>
    /// <param name="transactionsProvider">transactionsProvider</param>
    /// <param name="logger">logger</param>
    /// <param name="credentialOptions">credential</param>
    [RegisterIOC(typeof(DataAccessService<>), typeof(DataAccessService<>), IocType.Transient)]
    public class DataAccessService<T>(IOptions<T> daOption, IOptions<GeneralOption> generalOption, ITransactionsProvider transactionsProvider, ILog<DataAccessService> logger, IOptions<CredentialOptions> credentialOptions) : DataAccessService(daOption, generalOption, transactionsProvider, logger, credentialOptions)
         where T : DataAccessOption, new()
    {
    }
}

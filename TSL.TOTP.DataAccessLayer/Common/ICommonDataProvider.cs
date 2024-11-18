

using System.Data;
using TSL.Base.Platform.DataAccess;

namespace TSL.Common.Interface.DataAccessLayer.Common
{
    /// <summary>
    /// CommonDataProvider 的 Interface
    /// </summary>
    /// <typeparam name="T">任意類別</typeparam>
    public interface ICommonDataProvider
    {

        /// <summary>
        /// ConnectionTimeout
        /// </summary>
        int ConnectionTimeout { get; }

        #region Query 系列

        /// <summary>
        /// 查詢全部
        /// </summary>
        /// <typeparam name="T">任意類別</typeparam>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAllAsync<T>()
            where T : class;

        /// <summary>
        /// 查詢資料
        /// (keyEntity的name請使用nameof，若想使用like則在Value內使用%，則自動轉換為Like, 若value為陣列則自動傳換為IN)
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="paramDictoionary">查詢條件</param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> QueryAsync<TResult>(Dictionary<string, object> paramDictoionary)
            where TResult : new();

        /// <summary>
        /// 依主鍵(Primary Key)取回單筆資料，Model內的主鍵屬性需加[Key] Attribute
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns>資料物件</returns>
        Task<TResult> QueryAsyncById<TResult>(int id)
             where TResult : class;

        /// <summary>
        /// 依主鍵(Primary Key)取回單筆資料，Model內的主鍵屬性需加[Key] Attribute
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns>資料物件</returns>
        Task<TResult> QueryAsyncById<TResult>(long id)
             where TResult : class;

        /// <summary>
        /// 依主鍵陣列Primary Key取回多筆資料，Model內的主鍵屬性需加[Key] Attribute
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="ids">Primary Key List</param>
        /// <returns>資料物件</returns>
        Task<IEnumerable<TResult>> QueryAsyncByIds<TResult>(IEnumerable<int> ids)
            where TResult : new();

        /// <summary>
        /// 依主鍵陣列Primary Key取回多筆資料，Model內的主鍵屬性需加[Key] Attribute
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="ids">Primary Key List</param>
        /// <returns>資料物件</returns>
        Task<IEnumerable<TResult>> QueryAsyncByIds<TResult>(IEnumerable<long> ids)
            where TResult : new();

        /// <summary>
        /// 查詢資料(單筆)
        /// (keyEntity的name請使用nameof，若想使用like則在Value內使用%，則自動轉換為Like, 若value為陣列則自動傳換為IN)
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="paramDictoionary">查詢條件</param>
        /// <returns></returns>
        Task<TResult> QueryFirstOrDefaultAsync<TResult>(Dictionary<string, object> paramDictoionary)
            where TResult : new();

        /// <summary>
        /// 查詢特定Table的新增值(Identity)
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <returns></returns>
        Task<int> QueryTableIdentity<TResult>() where TResult : new();

        #endregion

        #region Insert 系列

        /// <summary>
        /// 新增多筆資料
        /// </summary>
        /// <typeparam name="TResult">資料物件類別</typeparam>
        /// <param name="entities">資料物件集合</param>
        /// <returns>新增資料筆數</returns>
        Task<bool> BulkInsertAsync<TResult>(IEnumerable<TResult> entities)
            where TResult : class;

        /// <summary>
        /// 新增多筆資料
        /// </summary>
        /// <typeparam name="TResult">資料物件類別</typeparam>
        /// <param name="entities">資料物件集合</param>
        /// <returns>新增資料筆數</returns>
        Task<bool> BulkInsertWithOutputAsync<TResult>(List<TResult> entities)
            where TResult : class;

        /// <summary>
        /// 新增多筆資料
        /// </summary>
        /// <typeparam name="TResult">資料物件類別</typeparam>
        /// <param name="con">資料庫連結</param>
        /// <param name="tran">trans</param>
        /// <param name="entities">資料物件集合</param>
        /// <returns>新增資料筆數</returns>
        Task<bool> BulkInsertAsync<TResult>(IDbConnection con, IDbTransaction tran, IEnumerable<TResult> entities)
            where TResult : class;

        /// <summary>
        /// 新增單一檔案或多筆，Insert後的Key值會在原物件內的Key屬性裡
        /// </summary>
        /// <typeparam name="T">資料封裝的物件</typeparam>
        /// <param name="insertObject">DB Object</param>
        /// <param name="useBusinessTransaction">使用 BusinessTransaction，若為 false 則仍透過白名單驗證資料表</param>
        /// <returns>是否成功</returns>
        Task<bool> InsertAsync<T>(T insertObject, bool useBusinessTransaction = false)
            where T : class;

        /// <summary>
        /// 新增單一檔案
        /// </summary>
        /// <typeparam name="T">資料封裝的物件</typeparam>
        /// <param name="insertObject">DB Object</param>
        /// <param name="enableTransaction">使用 BusinessTransaction</param>
        /// <returns>primary key</returns>
        Task<long> InsertAsyncReturnKey<T>(T insertObject, bool enableTransaction = false)
            where T : class;

        /// <summary>
        /// 新增主次關係資料
        /// (自動將Parent Key帶入 Child的 foreign key， Key Name需一樣，並且Parent的Key需放上Dapper.KeyAttribute)
        /// </summary>
        /// <typeparam name="TParent">Parent 只能限制一筆</typeparam>
        /// <typeparam name="TChild">Child 可以單筆或多筆</typeparam>
        /// <param name="parent">Parent Data</param>
        /// <param name="child">Child Data</param>
        /// <returns></returns>
        Task<bool> InsertAsync<TParent, TChild>(TParent parent, TChild child)
            where TParent : class
            where TChild : class;

        /// <summary>
        /// 新增主次關係資料 (自動將Parent Key帶入 Child的 foreign key)
        /// </summary>
        /// <typeparam name="TParent">Parent 只能限制一筆</typeparam>
        /// <typeparam name="TChild">Child 可以單筆或多筆</typeparam>
        /// <param name="parent">Parent Data</param>
        /// <param name="child">Child Data</param>
        /// <returns>Parent Id</returns>
        Task<long> InsertDueToLongAsync<TParent, TChild>(TParent parent, TChild child)
            where TParent : class
            where TChild : class;

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
        Task<bool> InsertAsync<TParent, TChild1, TChild2>(TParent parent, TChild1 child1, TChild2 child2)
            where TParent : class
            where TChild1 : class
            where TChild2 : class;

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
        Task<long> InsertDueToLongAsync<TParent, TChild1, TChild2>(TParent parent, TChild1 child1, TChild2 child2)
            where TParent : class
            where TChild1 : class
            where TChild2 : class;

        /// <summary>
        /// 新增主次關係資料 (自動將Parent Key帶入 Child1和Child2和Child3和Child4的 foreign key)
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
        Task<bool> InsertAsync<TParent, TChild1, TChild2, TChild3, TChild4>(TParent parent, TChild1 child1, TChild2 child2, TChild3 child3, TChild4 child4)
            where TParent : class
            where TChild1 : class
            where TChild2 : class
            where TChild3 : class
            where TChild4 : class;

        #endregion

        #region Update 系列

        /// <summary>
        /// 更新單筆或多筆資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <param name="updateObject">更新物件</param>
        /// <returns></returns>
        Task<bool> UpdateAsync<T>(T updateObject)
            where T : class;

        /// <summary>
        /// 資料快照 for Update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityToUpdate">entityToUpdate</param>
        /// <returns></returns>
        Task<bool> UpdateWithSnapshot<T>(T entityToUpdate)
               where T : class;

        /// <summary>
        /// 更新單筆資料(可自訂更新欄位與更新條件)
        /// </summary>
        /// <typeparam name="TDalModel">更新的 Dal Model Type</typeparam>
        /// <param name="updateInfo">更新的欄位名稱(kay)與資料(value)</param>
        /// <param name="conditionInfo">更新的欄位名稱(kay)與條件(value)</param>
        /// <returns></returns>
        Task<bool> UpdateAsync<TDalModel>(Dictionary<string, object> updateInfo, Dictionary<string, object> conditionInfo)
            where TDalModel : class;

        /// <summary>
        /// 更新兩筆不同類型資料
        /// </summary>
        /// <typeparam name="T1">更新資料物件Type</typeparam>
        /// <typeparam name="T2">更新資料物件Type2</typeparam>
        /// <param name="updateEntity1">更新物件</param>
        /// <param name="updateEntity2">更新物件2</param>
        /// <returns></returns>
        Task<bool> UpdateMutilAsync<T1, T2>(T1 updateEntity1, T2 updateEntity2)
            where T1 : class
            where T2 : class;

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
        Task<bool> UpdateMutilInsertSingleAsync<T, T2, T3, T4>(T updateEntity, T2 updateEntity2, T3 updateEntity3, T4 insertEntity)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class;

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
        Task<bool> UpdateMutilInserMutileAsync<T, T2, T3, T4>(T updateEntity1, T2 updateEntity2, T3 insertEntity1, T4 insertEntity2)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class;

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
        Task<bool> UpdateMutilInsertMutilNoneCountAsync<T, T2, T3, T4>(T updateEntity1, T2 updateEntity2, T3 insertEntity1, T4 insertEntity2)
          where T : class
          where T2 : class
          where T3 : class
          where T4 : class;

        /// <summary>
        /// 更新多筆不同類型資料
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="con"></param>
        /// <param name="tran"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<bool> BulkUpdateAsync<TResult>(IDbConnection con, IDbTransaction tran, IEnumerable<TResult> entities)
            where TResult : class;

        /// <summary>
        /// 更新多筆不同類型資料
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<bool> BulkUpdateAsync<TResult>(IEnumerable<TResult> entities)
            where TResult : class;
        #endregion

        #region Delete 系列

        /// <summary>
        /// 刪除單筆或多筆資料
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="deleteObject">刪除的物件</param>
        /// <returns></returns>
        Task<bool> DeleteAsync<T>(T deleteObject)
            where T : class;

        /// <summary>
        /// 依主鍵Primary Key刪除單筆資料
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns></returns>
        Task<int> DeleteAsyncById<TResult>(int id)
            where TResult : new();

        /// <summary>
        /// 依主鍵Primary Key刪除單筆資料
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns></returns>
        Task<int> DeleteAsyncById<TResult>(long id)
            where TResult : new();

        /// <summary>
        /// 依主鍵陣列Primary Key刪除多筆資料
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="ids">Primary Key List</param>
        /// <returns></returns>
        Task<int> DeleteAsyncByIds<TResult>(IEnumerable<int> ids)
            where TResult : new();

        /// <summary>
        /// 依主鍵陣列Primary Key刪除多筆資料
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="ids">Primary Key List</param>
        /// <returns></returns>
        Task<int> DeleteAsyncByIds<TResult>(IEnumerable<long> ids)
            where TResult : new();

        /// <summary>
        /// 刪除資料 By 輸入參數
        /// (keyEntity的name請使用nameof，若想使用like則在Value內使用%，則自動轉換為Like, 若value為陣列則自動傳換為IN, 若value為DateTime則條件自動為大於等於)
        /// </summary>
        /// <typeparam name="TResult">資料封裝的物件類型</typeparam>
        /// <param name="paramDictoionary">刪除條件</param>
        /// <param name="withSnapshot">使用資料快照 for rollback</param>
        /// <returns></returns>
        Task<int> DeleteAsyncByParam<TResult>(Dictionary<string, object> paramDictoionary, bool withSnapshot = false)
            where TResult : new();

        #endregion

        #region Update / Insert 系列 (留修改歷程類使用)

        /// <summary>
        /// 更新原物件、新增一物件(留修改歷程)
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <typeparam name="T1">新增資料物件Type</typeparam>
        /// <param name="updateEnties">更新物件</param>
        /// <param name="insertEnties">新增物件</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateSingleInsertSingleAsync<T, T1>(T updateEntites, T1 insertEntities)
            where T : class
            where T1 : class;

        #endregion

        #region Delete / Insert (批價特殊)

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
        Task<bool> DeleteInsertAsync<TResult1, TResult2>(TResult1 deleteEntity1, TResult2 deleteEntity2, TResult1 insertEntitiy1, TResult2 insertEntitiy2)
            where TResult1 : class
            where TResult2 : class;

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
        Task<bool> UpdateMutilInserMutileAsync<T, T1, T2, T3, T4>(T updateEntity, T1 updateEntity1, T2 updateEntity2, T3 insertEntity1, T4 insertEntity2)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class;

        #endregion

        #region Get Sequence Number

        /// <summary>
        /// 取得 DB Sequence Number
        /// </summary>
        /// <param name="sequenceName">Sequence名稱</param>
        /// <returns></returns>
        Task<long> GetNextSequenceNumber(string sequenceName);

        #endregion

        #region TransactionQuery

        /// <summary>
        /// 執行交易 query
        /// </summary>
        /// <param name="taskList">任務清單</param>
        /// <returns></returns>
        bool ExecuteTransactionQuery(params Action<IDbConnection, IDbTransaction>[] taskList);

        /// <summary>
        /// 執行交易 query (async)
        /// </summary>
        /// <param name="taskList">任務清單</param>
        /// <returns></returns>
        Task<bool> ExecuteTransactionQuery(params Func<IDbConnection, IDbTransaction, Task>[] taskList);
        #endregion
    }

    /// <summary>
    /// 基本檔界面 (擴充DataAccessService Config Setting)
    /// </summary>
    /// <typeparam name="TConnectionConfig">Connection Setting</typeparam>
    public interface ICommonDataProvider<TConnectionConfig> : ICommonDataProvider
        where TConnectionConfig : DataAccessOption, new()
    {
    }
}

using TSL.Base.Platform.DataAccess;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Common.Model.DataAccessLayer.TOTP;
using TSL.Common.Model.DataAccessLayer.TSTD;

namespace TSL.TSTD.DataAccessLayer.TSTDHelloWorld
{
    /// <summary>
    /// provider for TSTDTable , 
    /// please make sure that try and catch block is added in the provider class. 
    /// </summary>
    [RegisterIOC]
    public class TSTDHelloWorldProvider : ITSTDHelloWorldProvider
    {
        // both service were  injected from TSL.TSTD.API's program.cs
        private readonly DataAccessService<TSTDDataAccessOption> _TSTDDBAccessService;
        private readonly ILog<TSTDHelloWorldProvider>? _logger;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="tSTDDatatAccessService">data service for TSTD table</param>
        /// <param name="logger">nlog logger</param>
        public TSTDHelloWorldProvider(DataAccessService<TSTDDataAccessOption> tSTDDatatAccessService , ILog<TSTDHelloWorldProvider>? logger) {
            _TSTDDBAccessService = tSTDDatatAccessService;
            _logger = logger;
        }
        /// <summary>
        /// Get all documents  非同步
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<TSTDTable>> GetAllDocuments()
        {
            _logger?.Info("GetAllDocuments");
            string query = "SELECT * FROM TSTDTable";
            return await _TSTDDBAccessService.QueryAsync<TSTDTable>(query);
        }

        /// <summary>
        /// get document by id
        /// </summary>
        /// <param name="tSTDTableId">document Id</param>
        /// <returns></returns>
        public async Task<TSTDTable> GetDocumentById(int tSTDTableId)
        {
            _logger?.Info("GetDocumentById", tSTDTableId);
            string query = "SELECT * FROM TSTDTable WHERE TSTDTableId =@TSTDTableId";
            dynamic param = new { TSTDTableId = tSTDTableId };
            return await _TSTDDBAccessService.QueryFirstOrDefaultAsync<TSTDTable>(query, param);
        }

        /// <summary>
        /// insert data to TSTDTable
        /// </summary>
        /// <param name="tSTDTable">table id</param>
        /// <returns>primary key id</returns>
        public async Task<int> InsertTSTDTableAsync(TSTDTable tSTDTable)
        {
            _logger?.Info("InsertTSTDTableAsync", tSTDTable);

            int rowsAffected = await _TSTDDBAccessService.InsertAsync(tSTDTable, false, false);
            return rowsAffected;
        }

        /// <summary>
        /// update message field by id
        /// </summary>
        /// <param name="tSTDTableId">tstd table pk</param>
        /// <param name="message">free keyin message</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public Task<int> UpdateDocumentMessageById(int tSTDTableId, string message)
        {
            _logger?.Info("UpdateDocumentMessageById", tSTDTableId, message);
            string sql = "UPDATE TSTDTable SET message = @Message WHERE TSTDTableId = @TSTDTableId";
           dynamic param = new { Message = message, TSTDTableId = tSTDTableId };
           return  _TSTDDBAccessService.ExecuteNonQueryAsync(sql, param);
        }
    }
}

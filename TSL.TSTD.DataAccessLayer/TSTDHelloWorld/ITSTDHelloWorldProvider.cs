using TSL.Common.Model.DataAccessLayer.TSTD;

namespace TSL.TSTD.DataAccessLayer.TSTDHelloWorld
{
    public interface ITSTDHelloWorldProvider
    {
        /// <summary>
        /// query all data from TSTDTable
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TSTDTable>> GetAllDocuments();

        /// <summary>
        /// query data by id
        /// </summary>
        /// <param name="tSTDTableId">table id</param>
        /// <returns></returns>
        Task<TSTDTable> GetDocumentById(int tSTDTableId);


        /// <summary>
        /// update message by id
        /// </summary>
        /// <param name="tSTDTableId">table id</param>
        /// <param name="message">free keyin message</param>
        /// <returns></returns>
        Task<int> UpdateDocumentMessageById(int tSTDTableId, string message);


        /// <summary>
        ///  omsert data to TSTDTable
        /// </summary>
        /// <param name="tSTDTable">table object</param>
        /// <returns></returns>
        Task<int> InsertTSTDTableAsync(TSTDTable tSTDTable);
    }
}

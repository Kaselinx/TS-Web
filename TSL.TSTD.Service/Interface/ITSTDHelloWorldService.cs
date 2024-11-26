using TSL.Base.Platform.Utilities;
using TSL.Common.Model.Service.TSTD;

namespace TSL.TSTD.Service.Interface
{
    public interface ITSTDHelloWorldService
    {
        /// <summary>
        /// query all data from TSTDTable
        /// </summary>
        /// <returns></returns>
        Task<ServiceResult<IEnumerable<TSTDTableServiceModel>>> GetAllDocuments();

        /// <summary>
        /// query data by id
        /// </summary>
        /// <param name="tSTDTableId">table id</param>
        /// <returns></returns>
        ServiceResult<TSTDTableServiceModel> GetDocumentById(int tSTDTableId);


        /// <summary>
        /// update message by id
        /// </summary>
        /// <param name="tSTDTableId">table id</param>
        /// <param name="message">free keyin message</param>
        /// <returns></returns>
        ServiceResult<int> UpdateDocumentMessageById(int tSTDTableId, string message);



        /// <summary>
        ///  omsert data to TSTDTable
        /// </summary>
        /// <param name="tSTDTable">table object</param>
        /// <returns></returns>
        Task<ServiceResult<int>> InsertTSTDTableAsync(TSTDTableServiceModel tSTDTable);

    }
}

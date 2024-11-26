using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.DataAccessLayer.TSTD;
using TSL.Common.Model.Service.TSTD;
using TSL.TSTD.DataAccessLayer.TSTDHelloWorld;
using TSL.TSTD.Service.Interface;

namespace TSL.TSTD.Service.TSTDHelloWorld
{
    [RegisterIOC]
    public class TSTDHelloWorldService : ITSTDHelloWorldService
    {
        private readonly ILog<TSTDHelloWorldService> _logger;
        private readonly ITSTDHelloWorldProvider _iTSTDHelloWorldProvider;


        /// <summary>
        ///  constructor
        /// </summary>
        /// <param name="logger">lgger service</param>
        /// <param name="iTSTDHelloWorldProvider">TSD data provider</param>
        public TSTDHelloWorldService(ILog<TSTDHelloWorldService> logger, ITSTDHelloWorldProvider iTSTDHelloWorldProvider)
        {
            _logger = logger;
            _iTSTDHelloWorldProvider = iTSTDHelloWorldProvider;
        }

        /// <summary>
        /// getting all data from TSTDTable
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceResult<IEnumerable<TSTDTableServiceModel>>> GetAllDocuments()
        {
            // entry logging
            _logger.Info(nameof(GetAllDocuments));

            try
            {
                // call provider to get data
                IEnumerable<TSTDTable> result = await _iTSTDHelloWorldProvider.GetAllDocuments();

                // no error, but no data
                if (result.Any() == false)
                {
                    return new ServiceResult<IEnumerable<TSTDTableServiceModel>>(true, "No data");
                }
                else
                {
                    //convert provider model to service model
                    IEnumerable<TSTDTableServiceModel> serviceModels = result.Select(ProviderToServiceModel);
                    //return service result
                    return new ServiceResult<IEnumerable<TSTDTableServiceModel>>(true, "OK", serviceModels);
                }
            }
            catch (Exception ex)
            {
                // error logging
                _logger.Error("error", ex, nameof(GetAllDocuments));

                // return false result
                return new ServiceResult<IEnumerable<TSTDTableServiceModel>>(false, ex.Message);
            }
        }


        /// <summary>
        /// query data by id, plase  compare with qwuery all, you will find that the only difference is the parameter.
        /// and return type
        /// </summary>
        /// <param name="tSTDTableId">tstd table pimary key</param>
        /// <returns></returns>
        public  ServiceResult<TSTDTableServiceModel> GetDocumentById(int tSTDTableId)
        {
            _logger.Info(nameof(GetDocumentById), tSTDTableId);
            try
            {
                // call provider to get data by id , and wait for result 
                // this is just wan tto show you how to use sync method, but in real project, you should use async method
                TSTDTable result = _iTSTDHelloWorldProvider.GetDocumentById(tSTDTableId).GetAwaiter().GetResult();

                //convert provider model to service model
                TSTDTableServiceModel serviceModel = ProviderToServiceModel(result);

                //return service result
                return new ServiceResult<TSTDTableServiceModel> (true, "OK", serviceModel);
            }
            catch (Exception ex)
            {
                // error logging
                _logger.Error("error", ex, nameof(GetDocumentById));

                // return false result
                return new ServiceResult<TSTDTableServiceModel>(false, ex.Message);
            }
        }

        /// <summary>
        /// insert a new record to TSTDTable
        /// </summary>
        /// <param name="tSTDTable">tstd table</param>
        /// <returns></returns>
        public async Task<ServiceResult<int>> InsertTSTDTableAsync(TSTDTableServiceModel tSTDTable)
        {
            _logger.Info(nameof(InsertTSTDTableAsync), tSTDTable);

            try
            {
                // convert to provider model and call provider to insert data
                int result = await _iTSTDHelloWorldProvider.InsertTSTDTableAsync(ServiceModelToProviderModel(tSTDTable));

                if (result > 0)
                {
                    // return service result
                    return new ServiceResult<int>(true, "OK", result);
                }
                else
                {
                    // return service result with failure
                    return new ServiceResult<int>(false, "Insert failed", 0);
                }
            }
            catch (Exception ex)
            {
                // error logging
                _logger.Error("error", ex, nameof(InsertTSTDTableAsync), tSTDTable);

                // return false result
                return new ServiceResult<int>(false, ex.Message, 0);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tSTDTableId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ServiceResult<int> UpdateDocumentMessageById(int tSTDTableId, string message)
        {
            _logger.Info(nameof(UpdateDocumentMessageById), new { tSTDTableId, message });
            try
            {
                // convert to provider model and call provider to insert data
                int result =  _iTSTDHelloWorldProvider.UpdateDocumentMessageById(tSTDTableId, message).Result;

                if (result > 0)
                {
                    // return service result
                    return new ServiceResult<int>(true, "OK", result);
                }
                else
                {
                    // return service result with failure
                    return new ServiceResult<int>(false, "update failed", 0);
                }
            }
            catch (Exception ex)
            {
                // error logging
                _logger.Error("error", ex, nameof(UpdateDocumentMessageById), new { tSTDTableId, message });

                // return false result
                return new ServiceResult<int>(false, ex.Message, 0);
            }
        }


        #region privte method

        /// <summary>
        /// convert from provider model to service. 
        /// </summary>
        /// <param name="tSTDTable">provider model. </param>
        /// <returns></returns>
        private TSTDTableServiceModel ProviderToServiceModel(TSTDTable tSTDTable)
        {
            TSTDTableServiceModel ServiceModel = new TSTDTableServiceModel
            {
                CreatedAt = tSTDTable.CreatedAt,
                Username = tSTDTable.Username,
                TSTDTableId = tSTDTable.TSTDTableId,
                Message = tSTDTable.Message
            };

            return ServiceModel;
        }

        /// <summary>
        /// convert from provider model to service. 
        /// </summary>
        /// <param name="tSTDTable">service model. </param>
        /// <returns></returns>
        private TSTDTable ServiceModelToProviderModel(TSTDTableServiceModel tSTDTableServiceModel)
        {
            TSTDTable tSTDTable = new TSTDTable
            {
                CreatedAt = tSTDTableServiceModel.CreatedAt,
                Username = tSTDTableServiceModel.Username,
                TSTDTableId = tSTDTableServiceModel.TSTDTableId,
                Message = tSTDTableServiceModel.Message
            };

            return tSTDTable;
        }

        #endregion
    }
}

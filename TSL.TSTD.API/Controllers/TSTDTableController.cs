using Microsoft.AspNetCore.Mvc;
using System.Net;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.Service.TSTD;
using TSL.TSTD.Service.Interface;

namespace TSL.TSTD.API.Controllers
{
    /// <summary>
    /// TAAA Controller
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiVersion("1.0")]
    public class TSTDTableController : Controller
    {
        private readonly ITSTDHelloWorldService _iTSTDHelloWorldService;
        private readonly ILog<TSTDTableController> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iTSTDHelloWorldService"></param>
        /// <param name="logger"></param>
        public TSTDTableController(ITSTDHelloWorldService iTSTDHelloWorldService, ILog<TSTDTableController> logger)
        {
            _iTSTDHelloWorldService = iTSTDHelloWorldService;
            _logger = logger;
        }

        /// <summary>
        /// query all data from TSTDTable
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetAllDocuments()
        {
            _logger.Info(nameof(GetAllDocuments), "TSTD GetAllDocuments");

            try
            {
                ServiceResult<IEnumerable<TSTDTableServiceModel>> result = _iTSTDHelloWorldService.GetAllDocuments().GetAwaiter().GetResult();

                if (result.IsOk)
                {
                    _logger.Info(nameof(GetAllDocuments), "TSTD GetAllDocuments", result.Data);
                    return Json(result.Data);
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, new { });
                return BadRequest(new { message = ex.Message }); ;
            }
        }

        /// <summary>
        /// query data by id
        /// </summary>
        /// <param name="tSTDTableId">table id</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetDocumentById(int tSTDTableId)
        {
            _logger.Info(nameof(GetDocumentById), "TSTD GetDocumentById");

            try
            {
                // call service to get data by id
                ServiceResult<TSTDTableServiceModel> result = _iTSTDHelloWorldService.GetDocumentById(tSTDTableId);

                if (result.IsOk)
                {
                    _logger.Info(nameof(GetDocumentById), "TSTD GetDocumentById", result.Data);
                    return Json(result.Data);
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, new { });
                return BadRequest(new { message = ex.Message }); ;
            }
        }



        /// <summary>
        /// update message by id
        /// </summary>
        /// <param name="tSTDTableId">table id</param>
        /// <param name="message">free keyin message</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult UpdateDocumentMessageById(int tSTDTableId, string message)
        {
            _logger.Info(nameof(UpdateDocumentMessageById), "TSTD UpdateDocumentMessageById");

            try
            {
                // call service to get data by id
                ServiceResult<int> result = _iTSTDHelloWorldService.UpdateDocumentMessageById(tSTDTableId, message);

                if (result.IsOk)
                {
                    _logger.Info(nameof(UpdateDocumentMessageById), "TSTD UpdateDocumentMessageById", result.Data);

                    //成功更新
                    if (result.Data > 0)
                    {
                        return Json(true);
                    }
                    else
                    {
                        return Json(false);
                    }
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, new { });
                return BadRequest(new { message = ex.Message }); ;
            }
        }



        /// <summary>
        ///  omsert data to TSTDTable
        /// </summary>
        /// <param name="tSTDTable">table object</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult InsertTSTDTableAsync(TSTDTableServiceModel tSTDTable)
        {
            _logger.Info(nameof(InsertTSTDTableAsync), "TSTD InsertTSTDTableAsync");

            try
            {
                // call service to get data by id
                ServiceResult<int> result = _iTSTDHelloWorldService.InsertTSTDTableAsync(tSTDTable).Result;

                if (result.IsOk)
                {
                    _logger.Info(nameof(InsertTSTDTableAsync), "TSTD InsertTSTDTableAsync", result.Data);

                    //成功更新
                    if (result.Data > 0)
                    {
                        return Json(true);
                    }
                    else
                    {
                        return Json(false);
                    }
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, new { });
                return BadRequest(new { message = ex.Message }); ;
            }
        }
    }
}

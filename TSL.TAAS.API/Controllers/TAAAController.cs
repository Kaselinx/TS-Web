using Microsoft.AspNetCore.Mvc;
using System.Net;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.Service.TOTP;
using TSL.TAAA.Service.Interface;
using TSL.TAAS.API.Model;
using TSL.TOTP.Service.Interface;

namespace TSL.TAAS.API.Controllers
{

    /// <summary>
    /// TAAA Controller
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiVersion("1.0")]
    public class TAAAController : ControllerBase 
    {
        private readonly ILog<TAAAController> logger;
        private readonly ITOTPService iTOTPService;
        private readonly IOTPService iOTPService;
        private readonly IPOTService iPOTService;

        public TAAAController(ILog<TAAAController> logger, ITOTPService iTOTPService, IOTPService iOTPService, IPOTService iPOTService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.iTOTPService = iTOTPService ?? throw new ArgumentNullException(nameof(iTOTPService));
            this.iOTPService = iOTPService ?? throw new ArgumentNullException(nameof(iOTPService));
            this.iPOTService = iPOTService ?? throw new ArgumentNullException(nameof(iPOTService));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterOTPTUser(string employeeId, string label)
        {
            logger.Info(nameof(RegisterOTPTUser), "Register new OTPTUser ", employeeId, label);
            // async call.. recommand way for web api. 

            ServiceResult<string> result = await iTOTPService.GenerateQRCodeAsync(employeeId, label);
            if (result.IsOk)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }


        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOTPTUserByCriteria(string employeeId, string label,bool isActive )
        {
            logger.Info(nameof(GetOTPTUserByCriteria), employeeId, label, isActive);
            // async call.. recommand way for web api. 

            ServiceResult<IEnumerable<SecretDataServiceModel>> result = await iTOTPService.GetOTPTUsersByCriteria(employeeId, label, isActive);
            if (result.IsOk)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }


        /// <summary>
        /// vaildate totp token by secret key
        /// </summary>
        /// <param name="employeeId">employee Id</param>
        /// <param name="totp">totp key</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ValidateTotpUserByToken(string employeeId, string totp)
        {
            logger.Info(nameof(ValidateTotpUserByToken), employeeId, totp);

            var result = iTOTPService.ValidateUserByTotpTokenAsync(employeeId, totp).Result;
            if (result.IsOk)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("This is version 1.0");
        }
    }
}

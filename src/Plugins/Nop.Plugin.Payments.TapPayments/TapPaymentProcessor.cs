using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.TapPayments.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using RestSharp;

namespace Nop.Plugin.Payments.TapPayments
{
    /// <summary>
    /// TapStandard payment processor
    /// </summary>
    public class TapPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IAddressService _addressService;
        private readonly ICurrencyService _currencyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly TapPaymentSettings _tapPaymentSettings;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly INopFileProvider _fileProvider;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        #endregion

        #region Ctor

        public TapPaymentProcessor(CurrencySettings currencySettings,
            IAddressService addressService,
            ICurrencyService currencyService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            TapPaymentSettings tapPaymentSettings,
            IStoreContext storeContext,
            ILogger logger,
            INopFileProvider fileProvider,
            IWorkContext workContext,
            ILanguageService languageService)
        {
            _currencySettings = currencySettings;
            _addressService = addressService;
            _currencyService = currencyService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderService = orderService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _tapPaymentSettings = tapPaymentSettings;
            _storeContext = storeContext;
            _logger = logger;
            _fileProvider = fileProvider;
            _workContext = workContext;
            _languageService = languageService;
        }

        #endregion

        #region Utilities

        /// <summary>
        ///Import Resource string from xml and save
        /// </summary>
        protected virtual async void InstallLocaleResourcesAsync()
        {
            //'English' language
            var languageService = EngineContext.Current.Resolve<ILanguageService>();

            var languages = await languageService.GetAllLanguagesAsync();
            var language = languages.Where(p => p.Name == "EN").FirstOrDefault();

            if (language != null)
            {
                //save resources
                foreach (var filePath in Directory.EnumerateFiles(_fileProvider.MapPath("~/Plugins/Payments.TapPayments/Localization/ResourceString"),
                    "ResourceString.xml", SearchOption.TopDirectoryOnly))
                {
                    var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                    using (var streamReader = new StreamReader(filePath))
                    {
                        await localizationService.ImportResourcesFromXmlAsync(language, streamReader);
                    }
                }
            }
        }

        ///<summry>
        ///Delete Resource String
        ///</summry>
        protected virtual async void DeleteLocalResourcesAsync()
        {
            var file = Path.Combine(_fileProvider.MapPath("~/Plugins/Payments.TapPayments/Localization/ResourceString"), "ResourceString.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                await _localizationService.DeleteLocaleResourceAsync(item);
            }
        }


        #endregion

        #region Methods


        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;

            var orderAddress = await _addressService.GetAddressByIdAsync(
                (postProcessPaymentRequest.Order.PickupInStore ? postProcessPaymentRequest.Order.PickupAddressId : postProcessPaymentRequest.Order.ShippingAddressId) ?? 0);

            var model = new RequestModel()
            {
                Amount = order.OrderTotal,
                Currency = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode,
                ThreeDSecure = true,
                SaveCard = false,
                Description = _storeContext.GetCurrentStore().Name,
                StatementDescriptor = _storeContext.GetCurrentStore().Name,
                Reference = new Reference() { 
                    Transaction = $"txn_{order.Id}",
                    Order = $"{order.Id}",
                },
                Receipt= new Receipt() { 
                    Email = false,
                    Sms =true,
                },
                Customer = new Customer() { 
                    FirstName = orderAddress.FirstName,
                    LastName = orderAddress.LastName,
                    Email = orderAddress.Email,
                    Phone = new Phone() {
                        CountryCode = "",
                        Number = orderAddress.PhoneNumber
                    }
                },
                Source = new Source() { 
                    Id= _tapPaymentSettings.PaymentMethods
                },
                Redirect = new Post() { 
                    Url = $"{_storeContext.GetCurrentStore().Url}PaymentTap/TapPaymentHandler"
                }
            };

            //var language = _languageService.GetTwoLetterIsoLanguageName(await _workContext.GetWorkingLanguageAsync());

            try
            {
                var client = new RestClient("https://api.tap.company/v2/charges");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("lang_code", _tapPaymentSettings.Language);
                request.AddHeader("authorization", $"Bearer {_tapPaymentSettings.PrivateKey}");
                request.AddParameter("application/json", RequestModelSerialize.ToJson(model), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                var responceModel = ResponceModel.FromJson(response.Content);
                if (responceModel.Transaction != null)
                {
                    _httpContextAccessor.HttpContext.Response.Redirect(responceModel.Transaction.Url.ToString());
                }
                else
                {
                    await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Error in payment", response.Content);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error in payment", ex);
            }
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue - hide; false - display.
        /// </returns>
        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the additional handling fee
        /// </returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _paymentService.CalculateAdditionalFeeAsync(cart, 0, false);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the capture payment result
        /// </returns>
        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            //return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
            var result = new RefundPaymentResult();
            try
            {
                var storeName = _storeContext.GetCurrentStore().Name;
                var model = new RefundRequestModel()
                {
                    ChargeId = refundPaymentRequest.Order.AuthorizationTransactionId,
                    Amount = refundPaymentRequest.AmountToRefund,
                    Currency = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode,
                    Description = await GetMsg("Plugins.Payments.TapPayments.Refund.Description", storeName, refundPaymentRequest.Order.Id),
                    Reason = await GetMsg("Plugins.Payments.TapPayments.Refund.Reason", storeName),
                    Reference = new Reference()
                    {
                        Merchant = $"txn_{refundPaymentRequest.Order.Id}"
                    }
                };


                var client = new RestClient("https://api.tap.company/v2/refunds");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", $"Bearer {_tapPaymentSettings.PrivateKey}");
                request.AddParameter("application/json", RefundRequestModelSerialize.ToJson(model), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    var responceModel = RefundResponseModel.FromJson(response.Content);

                    var sb = new StringBuilder();
                    sb.AppendLine("Tap Token:");
                    sb.AppendLine("Refund Status: " + responceModel.Status);
                    sb.AppendLine("mc_currency: " + responceModel.Currency);
                    sb.AppendLine("txn_id: " + responceModel.ChargeId);
                    sb.AppendLine("Refund_id: " + responceModel.Id);

                    //order note
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = refundPaymentRequest.Order.Id,
                        Note = sb.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    if (refundPaymentRequest.IsPartialRefund)
                    {
                        result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
                    }
                    else
                    {
                        result.NewPaymentStatus = PaymentStatus.Refunded;
                    }

                }
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync("Refund", exc);
                result.AddError(await GetMsg("Plugins.Payments.TapPayments.Refund.Exception"));
            }

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of validating errors
        /// </returns>
        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment info holder
        /// </returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentTap/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentTap";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new TapPaymentSettings
            {
                PaymentMethods = "src_all"
            });

            //Local resource
            InstallLocaleResourcesAsync();

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<TapPaymentSettings>();

            //Local resource
            DeleteLocalResourcesAsync();

            await base.UninstallAsync();
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.TapPayments.PaymentMethodDescription");
        }

        private async Task<string> GetMsg(string key, params Object[] args)
        {
            return string.Format(await _localizationService.GetResourceAsync(key), args);
        }

        Type IPaymentMethod.GetPublicViewComponent()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        #endregion
    }
}
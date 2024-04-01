using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.TapPayments.Models;
using Nop.Plugin.Payments.TapPayments.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using RestSharp;

namespace Nop.Plugin.Payments.TapPayments.Controllers
{
    public class PaymentTapController : BasePaymentController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TapPaymentSettings _tapPaymentSettings;
        private readonly ICustomerActivityService _customerActivityService;
        #endregion

        #region Ctor

        public PaymentTapController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            ShoppingCartSettings shoppingCartSettings,
            TapPaymentSettings tapPaymentSettings,
            ICustomerActivityService customerActivityService)
        {
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            _tapPaymentSettings = tapPaymentSettings;
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region Utilities
        private List<SelectListItem> PreparePaymentMethod(string selectedValue) 
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Text = "All",
                Value = "src_all",
                Selected = "src_all" == selectedValue
            });
            list.Add(new SelectListItem()
            {
                Text = "Knet",
                Value = "src_kw.knet",
                Selected = "src_kw.knet" == selectedValue
            });
            
            list.Add(new SelectListItem()
            {
                Text = "Cards",
                Value = "src_card",
                Selected = "src_card" == selectedValue
            });
            list.Add(new SelectListItem()
            {
                Text = "Mada",
                Value = "src_sa.mada",
                Selected = "src_sa.mada" == selectedValue
            });
            list.Add(new SelectListItem()
            {
                Text = "BENEFIT",
                Value = "src_bh.benefit",
                Selected = "src_bh.benefit" == selectedValue
            });
            list.Add(new SelectListItem()
            {
                Text = "Fawry",
                Value = "src_eg.fawry",
                Selected = "src_eg.fawry" == selectedValue
            });

            return list;
        }
        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var tapPaymentSettings = await _settingService.LoadSettingAsync<TapPaymentSettings>(storeScope);
            var model = new ConfigurationModel
            {
                //UseSandbox = tapPaymentSettings.UseSandbox,
                //AdditionalFee = tapPaymentSettings.AdditionalFee,
                //AdditionalFeePercentage = tapPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope,
                //PublicKey = tapPaymentSettings.PublicKey,
                PrivateKey = tapPaymentSettings.PrivateKey,
                MerchantId = tapPaymentSettings.MerchantId,
                //Currency = tapPaymentSettings.Currency,
                PaymentMethods = tapPaymentSettings.PaymentMethods,
                Language = tapPaymentSettings.Language
            };

            var list = new List<SelectListItem>();
            model.AvailableLanguages.Add(new SelectListItem()
            {
                Text = Languages.AR.ToString(),
                Value = Languages.AR.ToString(),
                Selected = Languages.AR.ToString() == tapPaymentSettings.Language
            });
            model.AvailableLanguages.Add(new SelectListItem()
            {
                Text = Languages.EN.ToString(),
                Value = Languages.EN.ToString(),
                Selected = Languages.EN.ToString() == tapPaymentSettings.Language
            });


            model.AvailablePaymentMethods = PreparePaymentMethod(tapPaymentSettings.PaymentMethods);


            if (storeScope <= 0)
                return View("~/Plugins/Payments.TapPayments/Views/Configure.cshtml", model);

            //model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.UseSandbox, storeScope);
            //model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.AdditionalFee, storeScope);
            //model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            //model.PublicKey_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.PublicKey, storeScope);
            model.PrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.PrivateKey, storeScope);
            model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.MerchantId, storeScope);
            model.Language_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.Language, storeScope);
            //model.Currency_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.Currency, storeScope);
            model.PaymentMethods_OverrideForStore = await _settingService.SettingExistsAsync(tapPaymentSettings, x => x.PaymentMethods, storeScope);

            return View("~/Plugins/Payments.TapPayments/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var tapPaymentSettings = await _settingService.LoadSettingAsync<TapPaymentSettings>(storeScope);

            //save settings
            //tapPaymentSettings.UseSandbox = model.UseSandbox;
            //tapPaymentSettings.AdditionalFee = model.AdditionalFee;
            //tapPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            //tapPaymentSettings.PublicKey = model.PublicKey;
            tapPaymentSettings.PrivateKey = model.PrivateKey;
            tapPaymentSettings.MerchantId = model.MerchantId;
            tapPaymentSettings.Language = model.Language;
            //tapPaymentSettings.Currency = model.Currency;
            tapPaymentSettings.PaymentMethods = model.PaymentMethods;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            //await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.PublicKey, model.PublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.PrivateKey, model.PrivateKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.Language, model.Language_OverrideForStore, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.Currency, model.Currency_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(tapPaymentSettings, x => x.PaymentMethods, model.PaymentMethods_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        //action displaying notification (warning) to a store owner about inaccurate Tap rounding
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> RoundingWarning(bool passProductNamesAndTotals)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prices and total aren't rounded, so display warning
            if (passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = await _localizationService.GetResourceAsync("Plugins.Payments.TapPayments.RoundingWarning") });

            return Json(new { Result = string.Empty });
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> TapPaymentHandler()
        {
            var tx = _webHelper.QueryString<string>("tap_id");

            var client = new RestClient("https://api.tap.company/v2/charges/" + tx);
            var request = new RestRequest(Method.GET);
            //request.AddHeader("authorization", "Bearer sk_test_XKokBfNWv6FIYuTMg5sLPjhJ");
            request.AddHeader("authorization", $"Bearer {_tapPaymentSettings.PrivateKey}");
            request.AddParameter("undefined", "{}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                var model = ResponceModel.FromJson(response.Content);

                var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(model.Reference.Order));

                if (order == null)
                    return RedirectToAction("Index", "Home", new { area = string.Empty });

                var sb = new StringBuilder();
                sb.AppendLine("Tap Token:");
                sb.AppendLine("Status: " + model.Status);
                if (model.Card != null)
                    sb.AppendLine("payment_method: " + model.Card.Brand);
                sb.AppendLine("mc_currency: " + model.Currency);
                sb.AppendLine("txn_id: " + model.Id);
                sb.AppendLine("payment_type: " + model.Object);

                var newPaymentStatus = TapHelper.GetPaymentStatus(model.Status, string.Empty);
                sb.AppendLine("New payment status: " + newPaymentStatus);

                //order note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = sb.ToString(),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                //validate order total
                var orderTotalSentToTap = await _genericAttributeService.GetAttributeAsync<decimal?>(order, TapHelper.OrderTotalSentToTap);
                if (orderTotalSentToTap.HasValue && model.Amount != orderTotalSentToTap.Value)
                {
                    var errorStr = $"PayPal PDT. Returned order total {model.Amount} doesn't equal order total {order.OrderTotal}. Order# {order.Id}.";
                    //log
                    await _logger.ErrorAsync(errorStr);
                    //order note
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = errorStr,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    return RedirectToAction("Index", "Home", new { area = string.Empty });
                }

                //clear attribute
                if (orderTotalSentToTap.HasValue)
                    await _genericAttributeService.SaveAttributeAsync<decimal?>(order, TapHelper.OrderTotalSentToTap, null);

                if (newPaymentStatus == PaymentStatus.Voided)
                {
                    await _orderProcessingService.CancelOrderAsync(order, true);

                    await _customerActivityService.InsertActivityAsync("OrgerCancelAtPayment",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
                    
                }

                if (newPaymentStatus != PaymentStatus.Paid)
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

                if (!_orderProcessingService.CanMarkOrderAsPaid(order))
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

                //mark order as paid
                order.AuthorizationTransactionId = model.Id;
                await _orderService.UpdateOrderAsync(order);
                await _orderProcessingService.MarkOrderAsPaidAsync(order);

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> CancelOrder()
        {
            var order = (await _orderService.SearchOrdersAsync((await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1)).FirstOrDefault();

            if (order != null)
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            return RedirectToRoute("Homepage");
        }

        #endregion
    }
}
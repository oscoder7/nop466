using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.TapPayments.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        protected string GetLanguageRoutePattern()
        {
            if (DataSettingsManager.IsDatabaseInstalled())
            {
                var localizationSettings = EngineContext.Current.Resolve<LocalizationSettings>();
                if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                    return $"{{{NopPathRouteDefaults.LanguageRouteValue}:maxlength(2):{NopPathRouteDefaults.LanguageParameterTransformer}=en}}";
            }

            return string.Empty;
        }

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var lang = GetLanguageRoutePattern();
            

            //Process payment page
            endpointRouteBuilder.MapControllerRoute(name: "PaymentProcess",
               pattern: $"{lang}/PaymentTap/paymentprocess/{{orderid}}",
               defaults: new { controller = "PaymentTap", action = "PaymentProcess" });

            endpointRouteBuilder.MapControllerRoute(name: "PaymentHandler",
               pattern: $"{lang}/PaymentTap/TapPaymentHandler",
               defaults: new { controller = "PaymentTap", action = "TapPaymentHandler" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.PayPalStandard.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //PDT
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayPalStandard.PDTHandler", "Plugins/PaymentKNETStandard/PDTHandler",
                 new { controller = "PaymentKNETStandard", action = "PDTHandler" });

            //IPN
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayPalStandard.IPNHandler", "Plugins/PaymentKNETStandard/IPNHandler",
                 new { controller = "PaymentKNETStandardIpn", action = "IPNHandler" });

            //Cancel
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayPalStandard.CancelOrder", "Plugins/PaymentKNETStandard/CancelOrder",
                 new { controller = "PaymentKNETStandard", action = "CancelOrder" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.TapPayments.Components
{
    [ViewComponent(Name = "PaymentTap")]
    public class PaymentTapViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.TapPayments/Views/PaymentInfo.cshtml");
        }
    }
}

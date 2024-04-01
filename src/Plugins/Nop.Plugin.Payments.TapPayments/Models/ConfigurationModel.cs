using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Payments.TapPayments.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableLanguages = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.UseSandbox")]
        //public bool UseSandbox { get; set; }
        //public bool UseSandbox_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.AdditionalFee")]
        //public decimal AdditionalFee { get; set; }
        //public bool AdditionalFee_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.AdditionalFeePercentage")]
        //public bool AdditionalFeePercentage { get; set; }
        //public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.PublicKey")]
        //public string PublicKey { get; set; }
        //public bool PublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.PrivateKey")]
        public string PrivateKey { get; set; }
        public bool PrivateKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.Language")]
        public string Language { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }
        public bool Language_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.Currency")]
        //public string Currency { get; set; }
        //public bool Currency_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.PaymentMethods")]
        public string PaymentMethods { get; set; }
        public IList<SelectListItem> AvailablePaymentMethods { get; set; }
        public bool PaymentMethods_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.TapPayments.Fields.ThreeDSecure")]
        //public bool ThreeDSecure { get; set; }
        //public bool ThreeDSecure_OverrideForStore { get; set; }
        
    }
}
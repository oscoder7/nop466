﻿using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.PayPalStandard.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.BusinessEmail")]
        public string BusinessEmail { get; set; }
        public bool BusinessEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PDTToken")]
        public string PdtToken { get; set; }
        public bool PdtToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PassProductNamesAndTotals")]
        public bool PassProductNamesAndTotals { get; set; }
        public bool PassProductNamesAndTotals_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.TranportalId")]
        public string TranportalId { get; set; }
        public bool TranportalId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.TranportalPwd")]
        public string TranportalPwd { get; set; }
        public bool TranportalPwd_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.TerminalResourceKey")]
        public string TerminalResourceKey { get; set; }
        public bool TerminalResourceKey_OverrideForStore { get; set; }

    }
}



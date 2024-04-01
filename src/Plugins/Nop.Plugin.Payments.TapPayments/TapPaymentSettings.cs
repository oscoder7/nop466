using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.TapPayments
{
    /// <summary>
    /// Represents settings of the Tap Standard payment plugin
    /// </summary>
    public class TapPaymentSettings : ISettings
    {
        ///// <summary>
        ///// Gets or sets a value indicating whether to use sandbox (testing environment)
        ///// </summary>
        //public bool UseSandbox { get; set; }

        ///// <summary>
        ///// Gets or sets an additional fee
        ///// </summary>
        //public decimal AdditionalFee { get; set; }

        ///// <summary>
        ///// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        ///// </summary>
        //public bool AdditionalFeePercentage { get; set; }

        //public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string MerchantId { get; set; }
        public string Language { get; set; }
        //public string Currency { get; set; }
        public string PaymentMethods { get; set; }

        //public bool ThreeDSecure { get; set; }
    }
}

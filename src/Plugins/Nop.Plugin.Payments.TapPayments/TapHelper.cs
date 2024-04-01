using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Payments.TapPayments
{
    /// <summary>
    /// Represents Tap helper
    /// </summary>
    public class TapHelper
    {
        #region Properties

        /// <summary>
        /// Get nopCommerce partner code
        /// </summary>
        public static string NopCommercePartnerCode => "nopCommerce_SP";

        /// <summary>
        /// Get the generic attribute name that is used to store an order total that actually sent to Tap (used to PDT order total validation)
        /// </summary>
        public static string OrderTotalSentToTap => "OrderTotalSentToTap";

        #endregion

        #region Methods

        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="paymentStatus">Tap payment status</param>
        /// <param name="pendingReason">Tap pending reason</param>
        /// <returns>Payment status</returns>
        public static PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            if (pendingReason == null)
                pendingReason = string.Empty;

            switch (paymentStatus.ToLowerInvariant())
            {
                case "in_progress":
                    result = (pendingReason.ToLowerInvariant()) switch
                    {
                        "authorization" => PaymentStatus.Authorized,
                        _ => PaymentStatus.Pending,
                    };
                    break;
                case "captured":
                case "void":
                    result = PaymentStatus.Paid;
                    break;
                case "abandoned":
                case "cancelled":
                case "failed":
                case "declined":
                case "restricted":
                case "unknown":
                case "timedout":
                    result = PaymentStatus.Voided;
                    break;
                default:
                    break;
            }

            return result;
        }

        #endregion
    }
}
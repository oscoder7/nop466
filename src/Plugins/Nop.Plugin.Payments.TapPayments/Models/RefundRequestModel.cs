using Newtonsoft.Json;

namespace Nop.Plugin.Payments.TapPayments.Models
{
    public partial class RefundRequestModel
    {
        [JsonProperty("charge_id")]
        public string ChargeId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("reference")]
        public Reference Reference { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("post")]
        public Post Post { get; set; }
    }

    public partial class Reference
    {
        [JsonProperty("merchant")]
        public string Merchant { get; set; }
    }

    public partial class RefundRequestModel
    {
        public static RefundRequestModel FromJson(string json) => JsonConvert.DeserializeObject<RefundRequestModel>(json, Converter.Settings);
    }

    public static class RefundRequestModelSerialize
    {
        public static string ToJson(this RefundRequestModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}

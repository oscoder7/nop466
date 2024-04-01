using Newtonsoft.Json;

namespace Nop.Plugin.Payments.TapPayments.Models
{
    public partial class RefundResponseModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        [JsonProperty("live_mode")]
        public bool LiveMode { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("charge_id")]
        public string ChargeId { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reference")]
        public Reference Reference { get; set; }

        [JsonProperty("response")]
        public Response Response { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("post")]
        public Post Post { get; set; }
    }

    public partial class RefundResponseModel
    {
        public static RefundResponseModel FromJson(string json) => JsonConvert.DeserializeObject<RefundResponseModel>(json, Converter.Settings);
    }

    public static class RefundResponseModelSerialize
    {
        public static string ToJson(this RefundResponseModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

}

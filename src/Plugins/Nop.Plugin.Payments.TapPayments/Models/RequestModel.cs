using Newtonsoft.Json;

namespace Nop.Plugin.Payments.TapPayments.Models
{
    public partial class RequestModel
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("threeDSecure")]
        public bool ThreeDSecure { get; set; }

        [JsonProperty("save_card")]
        public bool SaveCard { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("statement_descriptor")]
        public string StatementDescriptor { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("reference")]
        public Reference Reference { get; set; }

        [JsonProperty("receipt")]
        public Receipt Receipt { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("merchant")]
        public Merchant Merchant { get; set; }

        [JsonProperty("source")]
        public Source Source { get; set; }

        [JsonProperty("destinations")]
        public Destinations Destinations { get; set; }

        [JsonProperty("post")]
        public Post Post { get; set; }

        [JsonProperty("redirect")]
        public Post Redirect { get; set; }
    }

    public partial class RequestModel
    {
        public static RequestModel FromJson(string json) => JsonConvert.DeserializeObject<RequestModel>(json, Converter.Settings);
    }
    public static class RequestModelSerialize
    {
        public static string ToJson(this RequestModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

}

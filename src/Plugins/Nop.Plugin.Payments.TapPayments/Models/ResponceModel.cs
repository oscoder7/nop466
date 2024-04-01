using Newtonsoft.Json;

namespace Nop.Plugin.Payments.TapPayments.Models
{
    public partial class ResponceModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("live_mode")]
        public bool LiveMode { get; set; }

        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("threeDSecure")]
        public bool ThreeDSecure { get; set; }

        [JsonProperty("card_threeDSecure")]
        public bool CardThreeDSecure { get; set; }

        [JsonProperty("save_card")]
        public bool SaveCard { get; set; }

        [JsonProperty("merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("statement_descriptor")]
        public string StatementDescriptor { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("transaction")]
        public Transaction Transaction { get; set; }

        [JsonProperty("reference")]
        public Reference Reference { get; set; }

        [JsonProperty("response")]
        public Response Response { get; set; }

        [JsonProperty("receipt")]
        public Receipt Receipt { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("source")]
        public Source Source { get; set; }

        [JsonProperty("redirect")]
        public Post Redirect { get; set; }

        [JsonProperty("post")]
        public Post Post { get; set; }

        [JsonProperty("card")]
        public Card Card { get; set; }

    }

    public partial class ResponceModel
    {
        public static ResponceModel FromJson(string json) => JsonConvert.DeserializeObject<ResponceModel>(json, Converter.Settings);
    }
}


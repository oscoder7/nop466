using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.TapPayments.Models
{

    public partial class Customer
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("middle_name")]
        public string MiddleName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public Phone Phone { get; set; }
    }

    public partial class Phone
    {
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }

    public partial class Metadata
    {
        [JsonProperty("udf1")]
        public string Udf1 { get; set; }

        [JsonProperty("udf2")]
        public string Udf2 { get; set; }
    }

    public partial class Post
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public partial class Receipt
    {
        [JsonProperty("email")]
        public bool Email { get; set; }

        [JsonProperty("sms")]
        public bool Sms { get; set; }
    }

    public partial class Reference
    {
        [JsonProperty("transaction")]
        public string Transaction { get; set; }

        [JsonProperty("order")]
        public string Order { get; set; }
    }

    public partial class Response
    {
        [JsonProperty("code")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class Source
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Transaction
    {
        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("expiry")]
        public Expiry Expiry { get; set; }

        [JsonProperty("asynchronous")]
        public bool Asynchronous { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public partial class Expiry
    {
        [JsonProperty("period")]
        public long Period { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }



    public partial class Destinations
    {
        [JsonProperty("destination")]
        public List<Destination> Destination { get; set; }
    }

    public partial class Destination
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public partial class Merchant
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Card
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("first_six")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long FirstSix { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("last_four")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LastFour { get; set; }
    }


    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}

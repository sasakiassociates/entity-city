/**
 * AirtableRecord
 *
 * Data structure to unmarshal JSON strings of records returned from
 * the Airtable API.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace Airtable
{
    internal class AirtableRecord 
    {

        [JsonProperty("id")]
        public string Id { get; internal set; }

        [JsonProperty("createdTime")]
        public string CreatedTime { get; internal set; }

        [JsonProperty("fields")]
        public Dictionary<string, string> Fields { get; internal set; }


        public Dictionary<string, string> Eject() {
            var primitive = new Dictionary<string, string>(Fields);

            primitive.Add("id", Id);
            primitive.Add("createdTime", CreatedTime);

            return primitive;
        }

    }
}

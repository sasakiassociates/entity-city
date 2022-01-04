/**
 * AirtableResponse
 *
 * Data structure to unmarshal JSON responses returned from
 * the Airtable API.
 */

using Newtonsoft.Json;
using System.Collections.Generic;


namespace Airtable
{
    internal class AirtableResponse
    {

        [JsonProperty("offset")]
        public string Offset { get; internal set; }

        [JsonProperty("records")]
        public AirtableRecord[] Records { get; internal set; }


        public Dictionary<string, string>[] Eject() {
            var primitive = new Dictionary<string, string>[Records.Length];
            var i = 0;

            foreach (var record in Records) {
                primitive[i++] = record.Eject();
            }

            return primitive;
        }

    }
}

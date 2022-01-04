using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Airtable {
    public class AirtableClient {

        private const string BASE_URL = "https://api.airtable.com/v0";

        private readonly HttpClient http;
        private readonly string uri;

        public AirtableClient(string apiKey, string appId)
            {
                uri = BASE_URL + "/" + appId + "/Tiles";

                http = new HttpClient();
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

        public async Task<Dictionary<string, string>[]> ListRecords()
            {
                var (body, ok) = await GET();

                if (ok)  {
                    var response = JsonConvert.DeserializeObject<AirtableResponse>(body);
                    return response.Eject();
                } else {
                    return new Dictionary<string, string>[0];
                }
            }

        public async Task<string> CreateRecord(Dictionary<string, string> record)
            {
                string serialized = JsonConvert.SerializeObject(record);

                Debug.Log("AIR Table" + serialized);
                var (body, ok) = await POST("{\"records\":[{\"fields\":" + serialized + "}]}");
                
                return body;
            }

        private async Task<(string, bool)> GET()
            {
                var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
                var body = await response.Content.ReadAsStringAsync();

                return (body, true);
            }

        private async Task<(string, bool)> POST(string content)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                var response = await http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                return (body, true);
            }

    }
}
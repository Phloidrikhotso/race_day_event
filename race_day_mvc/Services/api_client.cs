namespace race_day_mvc.Services
{//start of namespace
    public class api_client
    {//start of class

        // One shared HttpClient for the whole app (the cookie is handled by hand below)
        private static readonly HttpClient http_client = new HttpClient(new HttpClientHandler { UseCookies = false });

        private readonly string base_url;
        private readonly IHttpContextAccessor http_context_accessor;

        public api_client(IConfiguration configuration, IHttpContextAccessor accessor)
        {//start of constructor

            base_url = configuration["ApiBaseUrl"] ?? "";
            http_context_accessor = accessor;
        }//end of constructor

        // GET request to the API
        public async Task<(int status, string message)> get(string path, Dictionary<string, string>? query = null)
        {
            return await send(HttpMethod.Get, path, query);
        }

        // POST request to the API
        public async Task<(int status, string message)> post(string path, Dictionary<string, string>? query = null)
        {
            return await send(HttpMethod.Post, path, query);
        }

        // PUT request to the API
        public async Task<(int status, string message)> put(string path, Dictionary<string, string>? query = null)
        {
            return await send(HttpMethod.Put, path, query);
        }

        // DELETE request to the API
        public async Task<(int status, string message)> delete(string path, Dictionary<string, string>? query = null)
        {
            return await send(HttpMethod.Delete, path, query);
        }

        // Sends the request, forwards the API session cookie and returns the status code and the text
        private async Task<(int status, string message)> send(HttpMethod method, string path, Dictionary<string, string>? query)
        {//start of send

            try
            {//start of try

                ISession session = http_context_accessor.HttpContext!.Session;

                // Build the address (the API reads its values from the query string)
                string url = base_url.TrimEnd('/') + "/" + path.TrimStart('/');

                if (query != null && query.Count > 0)
                {
                    url += "?" + string.Join("&", query.Select(pair =>
                        Uri.EscapeDataString(pair.Key) + "=" + Uri.EscapeDataString(pair.Value ?? "")));
                }

                HttpRequestMessage request = new HttpRequestMessage(method, url);

                // Send the API session cookie so the API knows who the user is
                string? cookie = session.GetString("api_cookie");

                if (!string.IsNullOrEmpty(cookie))
                {
                    request.Headers.Add("Cookie", cookie);
                }

                HttpResponseMessage response = await http_client.SendAsync(request);

                // Keep the API session cookie for the next request
                if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? set_cookies))
                {
                    foreach (string set_cookie in set_cookies)
                    {
                        if (set_cookie.StartsWith(".AspNetCore.Session="))
                        {
                            session.SetString("api_cookie", set_cookie.Split(';')[0]);
                        }
                    }
                }

                string body = await response.Content.ReadAsStringAsync();

                return ((int)response.StatusCode, body);
            }//end of try
            catch (Exception)
            {//start of catch

                // Status 0 means the API could not be reached
                return (0, "The service is not available right now, please try again later...");
            }//end of catch
        }//end of send


    }//end of class
}//end of namespace

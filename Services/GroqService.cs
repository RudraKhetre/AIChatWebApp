using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AIChatWebApp.Services
{
    public class GroqService : IGroqService
    {
        private readonly string apiKey;
        private readonly HttpClient _httpClient;

        public GroqService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            apiKey = configuration["Groq:ApiKey"];

            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AIChatWebApp/1.0");
        }
        public async Task<string> AskAI(string prompt)
        {
            // Ensure Authorization header is set per request to avoid leaking between callers
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var request = new
            {
                model = "groq/compound-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is System.Net.Sockets.SocketException se)
                    return $"AI request error: Network/DNS failure ({se.SocketErrorCode}) - {se.Message}. Check DNS/proxy/firewall.";
                return $"AI request error: {ex.Message}";
            }

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"AI response error: {response.StatusCode} - {result}";
            }

            try
            {
                using var doc = JsonDocument.Parse(result);
                var root = doc.RootElement;

                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];

                    if (TryGetStringFromElement(first, out var fromChoices) && !string.IsNullOrEmpty(fromChoices))
                        return fromChoices;

                    if (first.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
                    {
                        if (message.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.String)
                            return contentProp.GetString() ?? string.Empty;
                    }

                    if (first.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                        return textProp.GetString() ?? string.Empty;
                }

                foreach (var name in new[] { "output", "outputs", "data", "result", "response" })
                {
                    if (root.TryGetProperty(name, out var prop) && TryGetStringFromElement(prop, out var v) && !string.IsNullOrEmpty(v))
                        return v;
                }

                if (TryFindFirstString(root, out var found) && !string.IsNullOrEmpty(found))
                    return found;

                return $"AI response error: No text found in response. Raw response: {result}";
            }
            catch (Exception ex)
            {
                return $"AI response error: {ex.Message}\nRaw response: {result}";
            }
        }

        private static bool TryGetStringFromElement(JsonElement element, out string? value)
        {
            value = null;

            if (element.ValueKind == JsonValueKind.String)
            {
                value = element.GetString();
                return true;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.String)
                {
                    value = contentProp.GetString();
                    return true;
                }

                if (element.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                {
                    value = textProp.GetString();
                    return true;
                }

                if (element.TryGetProperty("message", out var message))
                {
                    if (message.ValueKind == JsonValueKind.String)
                    {
                        value = message.GetString();
                        return true;
                    }

                    if (message.ValueKind == JsonValueKind.Object && message.TryGetProperty("content", out var msgContent) && msgContent.ValueKind == JsonValueKind.String)
                    {
                        value = msgContent.GetString();
                        return true;
                    }
                }

                foreach (var prop in element.EnumerateObject())
                {
                    if (TryGetStringFromElement(prop.Value, out value) && !string.IsNullOrEmpty(value))
                        return true;
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (TryGetStringFromElement(item, out value) && !string.IsNullOrEmpty(value))
                        return true;
                }
            }

            return false;
        }

        private static bool TryFindFirstString(JsonElement element, out string? value)
        {
            value = null;

            if (element.ValueKind == JsonValueKind.String)
            {
                value = element.GetString();
                return true;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (TryFindFirstString(prop.Value, out value) && !string.IsNullOrEmpty(value))
                        return true;
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (TryFindFirstString(item, out value) && !string.IsNullOrEmpty(value))
                        return true;
                }
            }

            return false;
        }
    }
}

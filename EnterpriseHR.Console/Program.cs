using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SysConsole = System.Console;

namespace EnterpriseHR.Console {
    public class Program {
        static async Task Main(string[] args) {
            SysConsole.Title = "EnterpriseHR AI Assistant";

            var apiBaseUrl = "https://localhost:7073";

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);
            httpClient.DefaultRequestHeaders.Add("X-Dev-User", "user2");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            SysConsole.WriteLine("==================================================");
            SysConsole.WriteLine("        EnterpriseHR AI Assistant");
            SysConsole.WriteLine("==================================================");
            SysConsole.WriteLine();

            try {
                // Create one conversation session.
                var sessionResponse = await httpClient.PostAsync("/chat/sessions", null);
                sessionResponse.EnsureSuccessStatusCode();

                var sessionJson = await sessionResponse.Content.ReadAsStringAsync();
                using var sessionDocument = JsonDocument.Parse(sessionJson);
                var sessionId = sessionDocument.RootElement.GetProperty("chatSessionId").GetGuid();

                SysConsole.WriteLine("Chat session started.");
                SysConsole.WriteLine("Ask a question about HR policies or type 'exit' to quit.");
                SysConsole.WriteLine();

                while (true) {
                    SysConsole.Write("You: ");
                    var question = SysConsole.ReadLine();

                    if (string.IsNullOrWhiteSpace(question))
                        continue;

                    if (question.Equals("exit", StringComparison.OrdinalIgnoreCase))
                        break;

                    var request = new {
                        sessionId = sessionId,
                        question = question
                    };

                    var response = await httpClient.PostAsJsonAsync("/chat/ask", request);

                    if (!response.IsSuccessStatusCode) {
                        var error = await response.Content.ReadAsStringAsync();
                        SysConsole.WriteLine();
                        SysConsole.WriteLine($"Request failed: {(int)response.StatusCode} {response.StatusCode}");
                        SysConsole.WriteLine(error);
                        SysConsole.WriteLine();
                        continue;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(json);
                    var root = document.RootElement;

                    SysConsole.WriteLine();
                    SysConsole.WriteLine("Assistant:");
                    SysConsole.WriteLine(root.GetProperty("answer").GetString());

                    var citations = root.GetProperty("citations");

                    if (citations.GetArrayLength() > 0) {
                        SysConsole.WriteLine();
                        SysConsole.WriteLine("Sources:");

                        foreach (var citation in citations.EnumerateArray()) {
                            var sourceNumber = citation.GetProperty("sourceNumber").GetInt32();
                            var title = citation.GetProperty("title").GetString();
                            var pageNumber = citation.GetProperty("pageNumber").GetInt32();

                            SysConsole.WriteLine($"[{sourceNumber}] {title}, Page {pageNumber}");
                        }
                    }

                    var inputTokens = root.GetProperty("inputTokens").GetInt32();
                    var outputTokens = root.GetProperty("outputTokens").GetInt32();
                    var totalTokens = root.GetProperty("totalTokens").GetInt32();
                    var estimatedCost = root.GetProperty("estimatedCost").GetDecimal();

                    SysConsole.WriteLine();
                    SysConsole.WriteLine("--------------------------------------------------");
                    SysConsole.WriteLine("Token Usage");
                    SysConsole.WriteLine($"Input Tokens:   {inputTokens:N0}");
                    SysConsole.WriteLine($"Output Tokens:  {outputTokens:N0}");
                    SysConsole.WriteLine($"Total Tokens:   {totalTokens:N0}");
                    SysConsole.WriteLine($"Estimated Cost: ${estimatedCost:F6}");
                    SysConsole.WriteLine("--------------------------------------------------");
                    SysConsole.WriteLine();
                }

                SysConsole.WriteLine();
                SysConsole.WriteLine("Chat ended.");
            } catch (Exception ex) {
                SysConsole.WriteLine();
                SysConsole.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}


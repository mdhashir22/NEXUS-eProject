using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NEXUS_eProject.Services
{
    public class NexusAiService : INexusAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NexusAiService> _logger;

        public NexusAiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<NexusAiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Please enter a message so I can help you.";
            }

            string? apiKey =
                _configuration["Gemini:ApiKey"];

            string model =
                _configuration["Gemini:Model"]
                ?? "gemini-3.5-flash-lite";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "NEXUS AI configuration error: Gemini API key was not found.";
            }

            const string nexusKnowledge = """
You are NEXUS AI, the official AI customer support assistant
for the NEXUS Internet Service Provider web application.

==================================================
YOUR ROLE
==================================================

You help customers understand NEXUS services, plans,
applications, connections, billing, payments and website usage.

You should behave like a friendly, intelligent customer support
assistant rather than a predefined FAQ bot.

Answer naturally and conversationally.

==================================================
MULTILINGUAL LANGUAGE BEHAVIOR
==================================================

NEXUS AI is a multilingual assistant.

Always detect the language of the customer's LATEST message
and reply naturally in the SAME language.

IMPORTANT LANGUAGE RULES:

- If the customer writes in English, reply entirely in English.

- If the customer writes in Roman Urdu, reply naturally
  in Roman Urdu.

- If the customer writes in Urdu script, reply naturally
  in Urdu script.

- If the customer writes in Hindi, reply in Hindi.

- If the customer writes in Arabic, reply in Arabic.

- If the customer writes in French, reply in French.

- If the customer writes in Spanish, reply in Spanish.

- If the customer writes in German, reply in German.

- If the customer writes in Chinese, reply in Chinese.

- If the customer writes in any other supported language,
  reply in that same language.

- Do NOT use Roman Urdu as the default response language.

- Roman Urdu is supported, but it is only appropriate when
  the customer writes in Roman Urdu or explicitly requests it.

- If the customer mixes languages, respond naturally using
  approximately the same language mix.

- If the customer explicitly requests a response in a particular
  language, follow the requested response language even if the
  message itself is written in another language.

- Do not translate the customer's message unless the customer
  specifically asks for translation.

- Keep technical names, service names, plan names, Account IDs,
  status names and official NEXUS terminology unchanged when
  appropriate.

Examples:

Customer:
"What happens after my payment is verified?"

Reply in English:
"After your payment is verified, the next step is Technical
installation. Once the installation is completed successfully,
your connection will become Active."

Customer:
"Bhai meri payment verify hogayi hai ab agy kya hoga?"

Reply in Roman Urdu:
"Agar aapki payment verify ho gayi hai to next step Technical
installation hai. Installation complete hone ke baad aapka
connection Active ho jayega."

Customer:
"میری پیمنٹ ویریفائی ہو گئی ہے، اب کیا ہوگا؟"

Reply naturally in Urdu script.

Customer:
"मेरी पेमेंट वेरिफाई हो गई है, अब क्या होगा?"

Reply naturally in Hindi.

Always follow the language of the latest customer message unless
the customer explicitly requests another response language.

Keep responses concise unless the customer asks for details.

==================================================
NEXUS
==================================================

NEXUS is an Internet Service Provider management system.

NEXUS supports services including:

- Broadband
- Dial-Up
- Telephone

Customers can:

- Register an account
- Login
- Browse plans
- Submit a connection application
- Track applications
- View connections
- View Account ID
- View bills
- Submit payments
- Track payment verification
- View connection status
- Submit feedback

==================================================
CONNECTION WORKFLOW
==================================================

The NEXUS workflow is:

Customer Registration
→ Customer Application
→ Retail Review
→ Retail Accepted
→ Technical Feasibility
→ Feasible
→ Connection Created
→ Account ID Generated
→ Billing
→ Payment
→ Payment Verification
→ Technical Installation
→ Active Connection

Detailed process:

1. Customer registers or logs in.

2. Customer chooses a service and available plan.

3. Customer submits an application/order.

4. Retail Employee reviews the application.

5. Retail can accept or reject it.

6. Accepted applications go to Technical.

7. Technical performs feasibility checking.

Technical may check:

- Customer area
- Server availability
- Infrastructure
- Distance
- Line availability

8. If feasibility fails, Technical may reject the application.

9. If feasibility succeeds, the application becomes Feasible.

10. Technical creates the Connection.

11. The system generates an Account ID.

12. The connection moves to Accounts/Billing.

13. Accounts generates a bill.

14. Customer views the bill.

15. Customer submits payment.

16. Payment becomes Pending Verification.

17. Accounts verifies the payment.

18. Verified payment moves the connection toward installation.

19. Technical performs installation.

20. Technical activates the connection.

21. Customer's connection becomes Active.

==================================================
ORDER STATUSES
==================================================

Pending / Submitted:
The customer has submitted the application and it is waiting
for Retail review.

Retail Accepted:
Retail has accepted the application.

Retail Rejected:
Retail rejected the application.

Feasible:
Technical confirmed that the connection is technically possible.

Technical Rejected:
Technical feasibility failed.

Connection Created:
Technical created the connection and Account ID.

Billing Pending:
The connection is waiting for billing.

Payment Pending:
A bill exists and payment is required.

Payment Verification Pending:
Customer submitted payment and Accounts must verify it.

Payment Verified:
Accounts successfully verified the payment.

Installation Pending:
Technical installation is required.

Active:
Installation is complete and the connection is active.

==================================================
PLANS AND PACKAGES
==================================================

NEXUS plans are stored in the NEXUS database.

Plans may have:

- Connection Type
- Plan Name
- Speed
- Duration
- Price
- Description

Do NOT invent package names, speeds or prices.

If live plan information has not been provided to you,
tell the customer that available packages can be viewed
from the Plans section of the NEXUS website.

==================================================
ACCOUNT ID
==================================================

A unique Account ID is generated after Technical approves
feasibility and creates the connection.

The Account ID identifies the customer's NEXUS connection.

Do not invent an Account ID.

==================================================
BILLING
==================================================

A NEXUS bill may contain:

- Plan Amount
- Equipment Amount
- Previous Due
- Discount
- Tax
- Total Amount
- Billing Date
- Due Date

The actual bill displayed in the customer's account is the
authoritative amount.

Never invent a customer's bill amount.

==================================================
PAYMENTS
==================================================

Submitting payment does not automatically activate a connection.

The normal payment workflow is:

Payment Submitted
→ Pending Verification
→ Accounts Verification
→ Payment Verified
→ Installation
→ Active

==================================================
WEBSITE GUIDANCE
==================================================

If someone asks how to get a NEXUS connection:

Explain that they should:

1. Register or login.
2. Open Plans.
3. Select a suitable service/plan.
4. Submit an application.
5. Track the application.
6. Complete billing/payment when available.
7. Wait for verification and installation.

If someone asks about packages:

Explain available package information if it is present in
the supplied information.

Otherwise direct them to the Plans section and do not
invent prices.

If someone asks about application status:

Explain the status they provide.

If they do not provide a status, tell them to check their
customer dashboard/application area.

If someone asks about a bill:

Direct them to their latest bill in the customer portal.

If someone asks about payment:

Explain that submitted payments require Accounts verification.

==================================================
SECURITY
==================================================

Never request:

- Passwords
- OTP codes
- CVV
- Credit/debit card numbers
- Authentication secrets
- API keys

Never pretend that you performed an action that you cannot perform.

Do not claim that you changed:

- Orders
- Connections
- Payments
- Bills
- Customer accounts

You currently provide customer guidance and information.

==================================================
LIVE CUSTOMER INFORMATION
==================================================

At this stage you may not have access to the customer's
live database information.

Never invent customer-specific information.

If customer-specific information is unavailable, say so clearly
and explain where the customer can view it.

==================================================
GENERAL QUESTIONS
==================================================

You are primarily the NEXUS support assistant.

You may also answer normal harmless general questions naturally.

However, keep general responses reasonably concise because
you are running inside the NEXUS customer support widget.

==================================================
PERSONALITY
==================================================

Be:

- Friendly
- Helpful
- Professional
- Natural
- Concise
- Conversational

Do not sound robotic.

Do not repeatedly introduce yourself.

Do not unnecessarily say "As an AI".

When appropriate, use simple numbered steps.

Always represent yourself as NEXUS AI.

==================================================
FINAL LANGUAGE INSTRUCTION
==================================================

Before generating every response, identify the language of the
customer's latest message.

Your response MUST use that same language unless the customer
explicitly asks you to respond in another language.

English message = English response.
Roman Urdu message = Roman Urdu response.
Urdu script message = Urdu script response.
Other language = Same language response.

Never default to Roman Urdu.
""";

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = nexusKnowledge
                        }
                    }
                },

                contents = new[]
                {
                    new
                    {
                        role = "user",

                        parts = new[]
                        {
                            new
                            {
                                text = userMessage
                            }
                        }
                    }
                },

                generationConfig = new
                {
                    temperature = 0.7,
                    topP = 0.9,
                    maxOutputTokens = 700
                }
            };

            string json =
                JsonSerializer.Serialize(requestBody);

            string url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    url
                );

            request.Headers.Add(
                "x-goog-api-key",
                apiKey
            );

            request.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            try
            {
                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

                string responseJson =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string error =
                        ExtractGeminiError(responseJson);

                    _logger.LogError(
                        "Gemini API failed. Status: {Status}. Error: {Error}",
                        (int)response.StatusCode,
                        error
                    );

                    return
                        $"NEXUS AI Error {(int)response.StatusCode}: {error}";
                }

                using JsonDocument document =
                    JsonDocument.Parse(responseJson);

                JsonElement root =
                    document.RootElement;

                if (
                    root.TryGetProperty(
                        "candidates",
                        out JsonElement candidates
                    ) &&
                    candidates.ValueKind == JsonValueKind.Array &&
                    candidates.GetArrayLength() > 0
                )
                {
                    JsonElement candidate =
                        candidates[0];

                    if (
                        candidate.TryGetProperty(
                            "content",
                            out JsonElement content
                        ) &&
                        content.TryGetProperty(
                            "parts",
                            out JsonElement parts
                        ) &&
                        parts.ValueKind == JsonValueKind.Array
                    )
                    {
                        StringBuilder answerBuilder =
                            new StringBuilder();

                        foreach (JsonElement part in parts.EnumerateArray())
                        {
                            if (
                                part.TryGetProperty(
                                    "text",
                                    out JsonElement textElement
                                )
                            )
                            {
                                string? text =
                                    textElement.GetString();

                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    if (answerBuilder.Length > 0)
                                    {
                                        answerBuilder.AppendLine();
                                    }

                                    answerBuilder.Append(text);
                                }
                            }
                        }

                        string answer =
                            answerBuilder.ToString().Trim();

                        if (!string.IsNullOrWhiteSpace(answer))
                        {
                            return answer;
                        }
                    }
                }

                _logger.LogWarning(
                    "Gemini returned success but no text was found. Response: {Response}",
                    responseJson
                );

                return
                    "NEXUS AI received a response from the AI service, but no text answer was generated. Please try again.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Gemini HTTP connection error."
                );

                return
                    "NEXUS AI internet/API connection error: " +
                    ex.Message;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "Gemini request timed out."
                );

                return
                    "NEXUS AI request timed out. Please try again.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Gemini response parsing failed."
                );

                return
                    "NEXUS AI received an invalid response from the AI service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected Gemini/NEXUS AI error."
                );

                return
                    "NEXUS AI error: " +
                    ex.Message;
            }
        }

        private static string ExtractGeminiError(
            string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return "No additional error information was returned.";
            }

            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(responseJson);

                JsonElement root =
                    document.RootElement;

                if (
                    root.TryGetProperty(
                        "error",
                        out JsonElement error
                    ) &&
                    error.TryGetProperty(
                        "message",
                        out JsonElement message
                    )
                )
                {
                    return
                        message.GetString()
                        ?? "Unknown Gemini API error.";
                }

                return responseJson;
            }
            catch
            {
                return responseJson;
            }
        }
    }
}
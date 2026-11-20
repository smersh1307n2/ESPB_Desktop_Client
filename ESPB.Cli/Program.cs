using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

const string defaultBaseUrl = "http://espb.runasp.net/api/v1";

var options = ParseArgs(args);
if (options.ContainsKey("help") || args.Length == 0)
{
    PrintUsage();
    return 0;
}

var apiKey = Get("api-key") ?? Environment.GetEnvironmentVariable("ESPB_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Missing API key. Set ESPB_API_KEY or pass --api-key.");
    return 2;
}

var baseUrl = (Get("base-url") ?? defaultBaseUrl).TrimEnd('/');
using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

try
{
    if (options.ContainsKey("limits"))
    {
        using var limitsResponse = await http.GetAsync($"{baseUrl}/limits");
        var body = await limitsResponse.Content.ReadAsStringAsync();
        if (!limitsResponse.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"Limits request failed: {(int)limitsResponse.StatusCode} {limitsResponse.ReasonPhrase}\n{body}");
            return 1;
        }

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;
        Console.WriteLine($"Requests remaining: {root.GetProperty("remainingRequests").GetInt32()}/{root.GetProperty("totalRequests").GetInt32()} ({root.GetProperty("subscriptionType").GetString()})");
        return 0;
    }

    var bcPath = Get("bc");
    if (string.IsNullOrWhiteSpace(bcPath) || !File.Exists(bcPath))
    {
        Console.Error.WriteLine("A valid --bc <file.bc> is required.");
        return 2;
    }

    var outputPath = Get("out");
    if (string.IsNullOrWhiteSpace(outputPath))
        outputPath = Path.ChangeExtension(Path.GetFullPath(bcPath), ".espb");

    string? ToBase64(string key)
    {
        var path = Get(key);
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path)
            ? Convert.ToBase64String(File.ReadAllBytes(path))
            : null;
    }

    var payload = new
    {
        fileName = Path.GetFileName(bcPath),
        bcFileContent = Convert.ToBase64String(await File.ReadAllBytesAsync(bcPath)),
        hintsFileContent = ToBase64("hints"),
        idxFileContent = ToBase64("idf-symbols"),
        customFileContent = ToBase64("custom-symbols"),
        options = new
        {
            enableRegisterPatching = !options.ContainsKey("no-register-patching"),
            enablePeephole = !options.ContainsKey("no-peephole"),
            enableLlvmOptimizations = !options.ContainsKey("no-llvm-optimizations"),
            enableCallResultOptimizations = !options.ContainsKey("no-call-result-optimizations"),
            enableAllocaOpt = !options.ContainsKey("no-alloca-optimization"),
            enableCompaction = !options.ContainsKey("no-compaction")
        }
    };

    Console.WriteLine($"Translating {bcPath} ...");
    using var response = await http.PostAsJsonAsync($"{baseUrl}/translate", payload);
    var responseBody = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
    {
        Console.Error.WriteLine($"Translation request failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseBody}");
        return 1;
    }

    using var result = JsonDocument.Parse(responseBody);
    var rootResult = result.RootElement;
    var status = rootResult.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null;
    if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
    {
        var message = rootResult.TryGetProperty("errorMessage", out var error) ? error.GetString() : "Unknown translation error";
        Console.Error.WriteLine($"Translation failed: {message}");
        return 1;
    }

    var encoded = rootResult.GetProperty("espbFileContent").GetString();
    if (string.IsNullOrWhiteSpace(encoded))
    {
        Console.Error.WriteLine("Translation response did not contain espbFileContent.");
        return 1;
    }

    var bytes = Convert.FromBase64String(encoded);
    var fullOutput = Path.GetFullPath(outputPath);
    Directory.CreateDirectory(Path.GetDirectoryName(fullOutput)!);
    await File.WriteAllBytesAsync(fullOutput, bytes);
    Console.WriteLine($"Created {fullOutput} ({bytes.Length} bytes)");

    if (rootResult.TryGetProperty("remainingRequests", out var remaining) &&
        rootResult.TryGetProperty("totalRequests", out var total))
        Console.WriteLine($"Requests remaining: {remaining.GetInt32()}/{total.GetInt32()}");

    if (rootResult.TryGetProperty("translationLog", out var log) && log.ValueKind == JsonValueKind.String)
        Console.WriteLine(log.GetString());

    return 0;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"API connection error: {ex.Message}");
    return 1;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"Invalid API response: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

string? Get(string key) => options.TryGetValue(key, out var value) ? value : null;

static Dictionary<string, string?> ParseArgs(string[] input)
{
    var parsed = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < input.Length; i++)
    {
        var token = input[i];
        if (!token.StartsWith("--", StringComparison.Ordinal))
            throw new ArgumentException($"Unexpected argument: {token}");
        var key = token[2..];
        if (key is "help" or "limits" or "no-register-patching" or "no-peephole" or
            "no-llvm-optimizations" or "no-call-result-optimizations" or
            "no-alloca-optimization" or "no-compaction")
            parsed[key] = null;
        else if (i + 1 < input.Length && !input[i + 1].StartsWith("--", StringComparison.Ordinal))
            parsed[key] = input[++i];
        else
            throw new ArgumentException($"Missing value for {token}");
    }
    return parsed;
}

static void PrintUsage()
{
    Console.WriteLine("""
ESPB CLI translator

Usage:
  dotnet run --project ESPB.Cli -- --bc input.bc [options]
  dotnet run --project ESPB.Cli -- --limits

Authentication:
  Set ESPB_API_KEY in the environment (recommended), or use --api-key.

Options:
  --bc FILE                       LLVM bitcode input (required)
  --out FILE                      ESPB output (default: input.espb)
  --hints FILE                    Optional translator hints
  --idf-symbols FILE              Optional IDF symbol map
  --custom-symbols FILE           Optional custom symbol map
  --base-url URL                  API base URL
  --api-key KEY                   API key (prefer ESPB_API_KEY)
  --limits                        Show remaining translation quota
  --no-register-patching          Disable register patching
  --no-peephole                   Disable peephole optimization
  --no-llvm-optimizations         Disable LLVM optimizations
  --no-call-result-optimizations  Disable call-result optimizations
  --no-alloca-optimization       Disable alloca optimization
  --no-compaction                Disable bytecode compaction
  --help                          Show this help
""");
}

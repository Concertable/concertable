using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Concertable.Web.E2ETests.Infrastructure;

public class StripeCliFixture : IAsyncLifetime
{
    private Process? process;
    private readonly string apiKey;
    private readonly string forwardUrl;

    public string ApiKey => apiKey;
    public string WebhookSecret { get; private set; } = null!;

    public StripeCliFixture(string apiKey, string forwardUrl)
    {
        this.apiKey = apiKey;
        this.forwardUrl = forwardUrl;
    }

    public async Task InitializeAsync()
    {
        var secretFound = new TaskCompletionSource<string>();

        process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"run --rm stripe/stripe-cli listen --api-key {apiKey} --forward-to {forwardUrl.Replace("localhost", "host.docker.internal")}/api/Webhook",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };

        void ParseSecret(string? line)
        {
            if (line is null) return;
            var match = Regex.Match(line, @"whsec_\w+");
            if (match.Success)
                secretFound.TrySetResult(match.Value);
        }

        process.OutputDataReceived += (_, e) => ParseSecret(e.Data);
        process.ErrorDataReceived += (_, e) => ParseSecret(e.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        cts.Token.Register(() => secretFound.TrySetCanceled());

        WebhookSecret = await secretFound.Task;
    }

    public Task DisposeAsync()
    {
        if (process is not null && !process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.Dispose();
        }
        return Task.CompletedTask;
    }
}

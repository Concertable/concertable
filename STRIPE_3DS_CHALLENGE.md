# Stripe 3DS Challenge — iframe wait problem

## The setup

Playwright .NET E2E test. After clicking Confirm on a Stripe checkout, a 3DS challenge modal appears. The modal is a nested iframe chain:

```
Page
└── <iframe src="...three-ds-2-challenge...">   ← outer iframe
    └── <iframe id="challengeFrame" name="stripe-challenge-frame">   ← inner iframe (Stripe injects this)
        └── ACS test page with <button id="test-source-authorize-3ds">COMPLETE</button>
```

## What WORKS reliably

This code passes every time:

```csharp
var outer = Page.Locator("iframe[src*='three-ds-2-challenge']");
await outer.WaitForAsync(new() { Timeout = 30_000 });

// This dump is the only thing making it work — it reads the outer iframe body
await DumpIframeStateAsync();

await Page.FrameLocator("iframe[src*='three-ds-2-challenge']")
    .FrameLocator("#challengeFrame")
    .Locator("#test-source-authorize-3ds")
    .ClickAsync(new() { Timeout = 30_000 });

await outer.WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = 120_000 });
```

The dump method does this (among other things):

```csharp
await Page
    .FrameLocator("iframe[src*='three-ds-2-challenge']")
    .Locator("body")
    .EvaluateAsync<string>("el => el.outerHTML");
```

This `EvaluateAsync` on the outer frame's body is providing the implicit wait that makes the click succeed. Without it, the click is attempted too early.

## What does NOT work

All of the following fail — the COMPLETE button is never clicked:

### Attempt 1 — WaitForAsync on the button itself
```csharp
var authorizeButton = Page.FrameLocator("iframe[src*='three-ds-2-challenge']")
    .FrameLocator("#challengeFrame")
    .Locator("#test-source-authorize-3ds");

await authorizeButton.WaitForAsync(new() { Timeout = 30_000 });
await authorizeButton.ClickAsync(new() { Timeout = 30_000 });
```

### Attempt 2 — WaitForAsync(Attached) on #challengeFrame element inside the outer frame
```csharp
await Page.FrameLocator("iframe[src*='three-ds-2-challenge']")
    .Locator("#challengeFrame")
    .WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 30_000 });

var authorizeButton = Page.FrameLocator("iframe[src*='three-ds-2-challenge']")
    .FrameLocator("#challengeFrame")
    .Locator("#test-source-authorize-3ds");

await authorizeButton.ClickAsync(new() { Timeout = 30_000 });
```

### Attempt 3 — FrameNavigated event for stripe-challenge-frame
```csharp
var frameTcs = new TaskCompletionSource<IFrame>(TaskCreationOptions.RunContinuationsAsynchronously);

void OnFrameNavigated(object? _, IFrame frame)
{
    if (frame.Name == "stripe-challenge-frame")
        frameTcs.TrySetResult(frame);
}

Page.FrameNavigated += OnFrameNavigated;
try
{
    var existing = Page.Frames.FirstOrDefault(f => f.Name == "stripe-challenge-frame");
    if (existing is not null)
        frameTcs.TrySetResult(existing);

    var challengeFrame = await frameTcs.Task.WaitAsync(TimeSpan.FromSeconds(30));
    await challengeFrame.Locator("#test-source-authorize-3ds").ClickAsync(new() { Timeout = 30_000 });
}
finally
{
    Page.FrameNavigated -= OnFrameNavigated;
}
```

## The question

What is the Playwright .NET equivalent of "wait until the outer iframe's document context is ready for JS execution" — i.e., what explicit wait replicates what `EvaluateAsync("el => el.outerHTML")` on the outer frame body is implicitly providing?

The outer frame is cross-origin (Stripe). The inner `#challengeFrame` is also cross-origin (the ACS simulator). Both are served from stripe.com/stripe.network subdomains.

## Environment

- Playwright .NET (`Microsoft.Playwright`) latest stable
- `FrameLocator` chained from `IPage`
- The outer iframe appears reliably (`outer.WaitForAsync` succeeds)
- The DOM dump confirms `<iframe id="challengeFrame" name="stripe-challenge-frame">` is present in the outer frame body when the dump runs
- `Page.Frames` shows `stripe-challenge-frame` as a child of the outer 3DS frame when the dump runs

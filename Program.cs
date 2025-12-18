builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.DisconnectedCircuitMaxRetained = 100;
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    });
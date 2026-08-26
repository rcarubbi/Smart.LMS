var builder = Host.CreateApplicationBuilder(args);

// TickerQ jobs + dashboard are wired in the background jobs stage.
await builder.Build().RunAsync();

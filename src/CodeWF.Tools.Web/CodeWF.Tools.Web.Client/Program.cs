WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddCodeWFService();

await builder.Build().RunAsync();
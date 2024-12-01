using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FrontendBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5257/") });
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<LaundryRoomService>();
builder.Services.AddScoped<AuthStateService>();



builder.Services.AddBlazoredLocalStorage(); // Required for using ILocalStorageService
builder.Services.AddHttpClient();

await builder.Build().RunAsync();
using Receiver.Services;
using Common;
using Grpc.Net.Client;
using gRPCagent;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Receiver.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Utilizam url-ul nostru personal declarat in propria librarie Common
builder.WebHost.UseUrls(EndpointsConstants.SubscriberAddress);
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
// P.S Pe urma schimbam la serviciul nou, deoarece am sters la moment GreeterService.cs
app.MapGrpcService<NotificationService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.StartAsync();

// Chemam metoda noastra de abonare a receiver-ului
await SubscribeHelper.SubscribeAsync(app);

await app.WaitForShutdownAsync();
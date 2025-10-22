// Program.cs - punctul de intrare al aplicației și rădăcina de compoziție (composition root) pentru serviciul Broker
using Broker.Services;
using Broker.Services.Interfaces;
using Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Utilizam url-ul nostru personal declarat in propria librarie Common
builder.WebHost.UseUrls(EndpointsConstants.BrokerAddress);
builder.Services.AddGrpc();
// AddSingletion permite sa avem o instanta al acestui serviciu per toata aplicatia...
// ... pentru ca atunci cand facem request la PublisherService unele servicii se vor recrea...
// ... iar storage-ul nostru se va crea din nou si mesajele vor disparea din coada.
builder.Services.AddSingleton<IMessageStorageService, MessageStorageService>();
// Aici la fel pastram o singura lista de conexiuni per toata aplicatia...
// ...ca mai apoi sa putem executa diferite actiuni pe acestea
builder.Services.AddSingleton<IConnectionStorageService, ConnectionStorageService>();

// Serviciu care se va ocupa de trimiterea mesajelor către conexiunile existente
builder.Services.AddHostedService<SenderWorker>();

var app = builder.Build();

app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
app.MapGrpcService<PublisherService>();
app.MapGrpcService<SubscriberService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
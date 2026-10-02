using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;

Console.WriteLine("Starting Haenam MQTT Client");

IMqttClient mqttClient;
MqttClientOptions _mqttClientOptions;
Task _heartbeatTask;
CancellationTokenSource _cts = new();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient("RestApi", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7113"); // Your API URL
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YourToken");
        });
    })
    .Build();

DotNetEnv.Env.Load();

var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient("RestApi");


const string AreaPrefix = "haenam";
string _stationId = "3399003";
string _bitId = "AUDIT-BIT-S1-03";
string statusTopic = $"{AreaPrefix}/status/{_bitId}";

JsonSerializerOptions JsonOptions = new()
{
    PropertyNameCaseInsensitive = true
};

await ConnectToMQTTServer();

await host.RunAsync();

async Task ConnectToMQTTServer()
{
    try
    {
        string serverAddress = Environment.GetEnvironmentVariable("MQTT_SERVER");
        int serverPort = int.TryParse(Environment.GetEnvironmentVariable("MQTT_PORT"), out int port) ? port : 1883;

        _mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(serverAddress, serverPort)
            .WithCleanSession(true)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
            .WithClientId(_bitId)
            //.WithCredentials(Environment.GetEnvironmentVariable("MQTT_USER"), Environment.GetEnvironmentVariable("MQTT_PASSWORD"))
            .WithCredentials(_bitId, Environment.GetEnvironmentVariable("MQTT_PASSWORD"))
            .Build();

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        var mqttFactory = new MqttFactory();
        mqttClient = mqttFactory.CreateMqttClient();

        mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        mqttClient.DisconnectedAsync += OnDisconnectedAsync;

        await TryConnectAsync();
    }
    catch (Exception ee)
    {        
        Debug.WriteLine($"### {ee.StackTrace}\r\n{ee.Message} ###");
    }
}

async Task TryConnectAsync()
{
    while (!mqttClient.IsConnected && !_cts.Token.IsCancellationRequested)
    {
        try
        {
            var result = await mqttClient.ConnectAsync(_mqttClientOptions, _cts.Token);

            if (result.ResultCode == MqttClientConnectResultCode.Success)
            {   
                await SubscribeToTopicAsync();
                await SendStatusAsync(isInitStatus: true);
                StartHeartbeatLoop();
                return;
            }
        }
        catch (Exception ee)
        {
            Debug.WriteLine($"### {ee.StackTrace}\r\n{ee.Message} ###");
        }

        await Task.Delay(TimeSpan.FromSeconds(60), _cts.Token);
    }
}

void StartHeartbeatLoop()
{
    _heartbeatTask = Task.Run(async () =>
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        while (await timer.WaitForNextTickAsync(_cts.Token) && mqttClient.IsConnected)
        {
            await SendStatusAsync(isInitStatus: false);
        }
    });
}

async Task SendStatusAsync(bool isInitStatus)
{
    try
    {
        string statusAsJson;

        if (isInitStatus)
        {
            statusAsJson = JsonSerializer.Serialize(new { bitId = _bitId, status = "OFFLINE" });
        }
        else
        {
            statusAsJson = JsonSerializer.Serialize(new DeviceStatusResponse(
                BitId: _bitId,
                Status: "NORMAL",
                BatteryPercent: 35,
                Temperature: 21,
                Temperature2: 22,
                Humidity: 12,
                Luminance: 500,
                Shock: 0,
                Door: 0,
                Illumination: 100,
                AppVersion: "v1.0.0"
            ));
        }

        await mqttClient.PublishStringAsync(statusTopic, statusAsJson, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, cancellationToken: _cts.Token);
    }
    catch (Exception ee)
    {
        Debug.WriteLine($"### {ee.StackTrace}\r\n{ee.Message} ###");
    }
}

async Task SubscribeToTopicAsync()
{
    string[] topics =
    {            $"{AreaPrefix}/arrival/{_stationId}",
        };

    var factory = new MqttFactory();
    var subscribeOptionsBuilder = factory.CreateSubscribeOptionsBuilder();

    foreach (var topic in topics)
    {
        subscribeOptionsBuilder.WithTopicFilter(topic);
    }

    await mqttClient.SubscribeAsync(subscribeOptionsBuilder.Build(), _cts.Token);
    Console.WriteLine($"Subscribed to topics: {string.Join(", ", topics)}");
}

async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
{
    try
    {
        await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);
        await TryConnectAsync();
    }
    catch (TaskCanceledException) { /* Clean shutdown ignored */ }
}

async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
{
    try
    {
        string topic = e.ApplicationMessage.Topic;
        byte[] payloadBytes = e.ApplicationMessage.PayloadSegment.ToArray();

        string[] topicSegments = topic.Split('/');
        if (topicSegments.Length < 2) return;

        string action = topicSegments[1]; // Extracts "timesync", "arrival", "weather", etc.

        switch (action)
        {           
            case "arrival":
               await HandleBusArrival(payloadBytes);
                break;           
            default:
                Debug.WriteLine($"### Unhandled topic [{topic}] received. Payload: {System.Text.Encoding.UTF8.GetString(payloadBytes)}");
                break;
        }
    }
    catch (Exception ex)
    {
            Debug.WriteLine($"메시지 라우팅 오류 [{e.ApplicationMessage.Topic}]: {ex.Message}");
    }

    return;
}

async Task HandleBusArrival(byte[] payload)
{    
    try
    {
        var data = JsonSerializer.Deserialize<StationArrivalResponse>(payload, JsonOptions);

        if (data == null) return;

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/bus-arrivals", data);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("[HTTP] Successfully forwarded data to API.");
        }
        else
        {
            Console.WriteLine($"[HTTP] API returned an error: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[HTTP] Failed to make POST call: {ex.Message}");
    }
}

public record DeviceStatusResponse(
    [property: JsonPropertyName("bitId")] string BitId,
    [property: JsonPropertyName("status")] string Status,

    // Optional fields marked with '?' to handle the short payload
    [property: JsonPropertyName("battPercent")] int? BatteryPercent = null,
    [property: JsonPropertyName("temperature")] int? Temperature = null,
    [property: JsonPropertyName("temperature2")] int? Temperature2 = null,
    [property: JsonPropertyName("Humidity")] int? Humidity = null,
    [property: JsonPropertyName("Luminance")] int? Luminance = null,
    [property: JsonPropertyName("Shock")] int? Shock = null,
    [property: JsonPropertyName("Door")] int? Door = null,
    [property: JsonPropertyName("Illumination")] int? Illumination = null,
    [property: JsonPropertyName("appVersion")] string? AppVersion = null
);

public record StationArrivalResponse(
    [property: JsonPropertyName("stationId")] string StationId,
    [property: JsonPropertyName("stationName")] string StationName,
    [property: JsonPropertyName("updateTime")] DateTimeOffset UpdateTime,
    [property: JsonPropertyName("busList")] List<BusArrivalResponse> BusList
);

public record BusArrivalResponse(
    [property: JsonPropertyName("routeId")] string RouteId,
    [property: JsonPropertyName("routeNo")] string RouteNo,
    [property: JsonPropertyName("routeDirection")] string RouteDirection,
    [property: JsonPropertyName("currSttnName")] string CurrentStationName, // Renamed for better readability
    [property: JsonPropertyName("operationMode")] int OperationMode,
    [property: JsonPropertyName("busType")] int BusType,
    [property: JsonPropertyName("predictType")] int PredictType,
    [property: JsonPropertyName("remainStop")] int RemainStop,
    [property: JsonPropertyName("remainTime")] int RemainTime // Time in seconds		
);
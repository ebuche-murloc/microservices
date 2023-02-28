using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.DataServices.Async;

public class MessageBusClient : IMessageBusClient
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public MessageBusClient(IConfiguration configuration)
    {
        _configuration = configuration;
        var factory = new ConnectionFactory()
            { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]) };
        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            Console.WriteLine("--Connected to message bus");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not connect to the message bus: {e}");
        }
    }

    public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
    {
        var message = JsonSerializer.Serialize(platformPublishedDto);

        if (_connection.IsOpen)
        {
            Console.WriteLine("--RabbitMQ connection open - sending message...");
            SendMessage(message);
        }
        else
        {
            Console.WriteLine("--RabbitMQ connection is closed - not sending message");
        }
    }

    private void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "trigger", routingKey: "", basicProperties: null, body: body);
        Console.WriteLine($"-- Sent message {message}");
    }

    private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--Connection shutdown");
    }

    public void Dispose()
    {
        Console.WriteLine("--Message bus disposed");

        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
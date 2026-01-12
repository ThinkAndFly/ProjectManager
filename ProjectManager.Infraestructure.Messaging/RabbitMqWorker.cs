using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProjectManager.Infraestructure.Messaging
{
    public class RabbitMqWorker : BackgroundService
    {
        private readonly ILogger<RabbitMqWorker> _logger;
        private readonly RabbitMqOptions _options;
        private IConnection? _connection;
        private IChannel? _channel;

        private const int MaxRetryAttempts = 10;
        private const int InitialRetryDelayMs = 2000;
        private const int MaxRetryDelayMs = 30000;

        public RabbitMqWorker(
            ILogger<RabbitMqWorker> logger,
            IOptions<RabbitMqOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        private async Task<bool> InitAsync(CancellationToken cancellationToken)
        {
            if (_connection != null && _connection.IsOpen) return true;

            var retryCount = 0;
            var retryDelay = InitialRetryDelayMs;

            while (retryCount < MaxRetryAttempts && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation(
                        "Attempting to connect to RabbitMQ at {HostName} (attempt {Attempt}/{MaxAttempts})...",
                        _options.HostName, retryCount + 1, MaxRetryAttempts);

                    var factory = new ConnectionFactory
                    {
                        HostName = _options.HostName,
                        UserName = _options.UserName,
                        Password = _options.Password,
                        Port = _options.Port,
                        RequestedConnectionTimeout = TimeSpan.FromSeconds(10),
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                    };

                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    _channel = await _connection.CreateChannelAsync();

                    await _channel.QueueDeclareAsync(
                        queue: _options.QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: cancellationToken
                    );

                    _logger.LogInformation(
                        "RabbitMQ connection established successfully to {HostName}, queue: {Queue}",
                        _options.HostName, _options.QueueName);

                    return true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    
                    if (retryCount >= MaxRetryAttempts)
                    {
                        _logger.LogError(ex,
                            "Failed to connect to RabbitMQ after {MaxAttempts} attempts. Giving up.",
                            MaxRetryAttempts);
                        return false;
                    }

                    _logger.LogWarning(ex,
                        "Failed to connect to RabbitMQ (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}ms...",
                        retryCount, MaxRetryAttempts, retryDelay);

                    try
                    {
                        await Task.Delay(retryDelay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Connection retry cancelled");
                        return false;
                    }

                    // Exponential backoff with max cap
                    retryDelay = Math.Min(retryDelay * 2, MaxRetryDelayMs);
                }
            }

            return false;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Worker starting...");

            var connected = await InitAsync(stoppingToken);

            if (!connected || _channel == null)
            {
                _logger.LogError("Could not establish RabbitMQ connection. Worker will not process messages.");
                return;
            }

            try
            {
                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        _logger.LogInformation("Received message from queue {Queue}: {Message}",
                            _options.QueueName, message);

                        // Here the application layer would process the message.

                        await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from queue {Queue}", _options.QueueName);
                        
                        // Optionally reject and requeue the message
                        try
                        {
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                        }
                        catch (Exception nackEx)
                        {
                            _logger.LogError(nackEx, "Error sending NACK for message");
                        }
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: _options.QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken
                );

                _logger.LogInformation("Started consuming messages from queue: {Queue}", _options.QueueName);

                // Keep the service running until cancellation is requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RabbitMQ worker is stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in RabbitMQ worker");
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
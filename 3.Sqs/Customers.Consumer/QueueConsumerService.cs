using Amazon.SQS;
using Amazon.SQS.Model;
using Customers.Consumer.Messages;
using MediatR;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Consumer
{
    public class QueueConsumerService : BackgroundService
    {
        private readonly IAmazonSQS _sqs;
        private readonly QueueSettings _queueSettings;
        private readonly ILogger<QueueConsumerService> _logger;
        private readonly IMediator _mediator;

        public QueueConsumerService(IAmazonSQS sqs, IOptions<QueueSettings> queueSettings, ILogger<QueueConsumerService> logger, IMediator mediator)
        {
            _sqs = sqs;
            _queueSettings = queueSettings.Value;
            _logger = logger;
            _mediator = mediator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QueueConsumerService is starting.");

            var queueUrlResponse = await _sqs.GetQueueUrlAsync(_queueSettings.Name);

            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrlResponse.QueueUrl,
                AttributeNames = new List<string> { "All" },
                MessageAttributeNames = new List<string> { "All" },
                MaxNumberOfMessages = 1
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _sqs.ReceiveMessageAsync(request, stoppingToken);

                foreach (var message in response.Messages)
                {
                    var messageType = message.MessageAttributes["Type"].StringValue;
                    var type = Type.GetType($"Customers.Consumer.Messages.{messageType}");

                    if(type is null)
                    {
                        _logger.LogError($"Message type {messageType} not found");
                        continue;
                    }

                    object typedMessage = (ISqsMessage)JsonSerializer.Deserialize(message.Body, type)!;

                    try
                    {
                        await _mediator.Send(typedMessage, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                    }

                    _logger.LogInformation($"Received message: {message.Body}");

                    await _sqs.DeleteMessageAsync(_queueSettings.Name, message.ReceiptHandle, stoppingToken);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

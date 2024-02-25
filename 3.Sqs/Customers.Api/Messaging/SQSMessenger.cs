using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Api.Messaging
{
    public class SQSMessenger : ISQSMessenger
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IOptions<QueueSettings> _queueSettings;
        private string? _queueUrl;

        public SQSMessenger(IAmazonSQS sqsClient, IOptions<QueueSettings> queueSettings)
        {
            _sqsClient = sqsClient;
            _queueSettings = queueSettings;
        }

        public async Task<SendMessageResponse> SendMessageAsync<T>(T message)
        {
            string queueUrl = await GetQueueUrlAsync();

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = JsonSerializer.Serialize(message),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "Type",
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = typeof(T).Name
                        }
                    }
                }
            };

            return await _sqsClient.SendMessageAsync(sendMessageRequest);
        }

        private async Task<string> GetQueueUrlAsync()
        {
            if (_queueUrl is not null)
                return _queueUrl;


            var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(_queueSettings.Value.Name);
            _queueUrl = queueUrlResponse.QueueUrl;
            return _queueUrl;
        }
    }
}

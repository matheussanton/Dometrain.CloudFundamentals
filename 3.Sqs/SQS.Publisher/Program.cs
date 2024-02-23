using Amazon.SQS;
using Amazon.SQS.Model;
using SQS.Publisher;
using System.Text.Json;

var sqsClient = new AmazonSQSClient();

var customer = new CustomerCreated
{
    Id = Guid.NewGuid(),
    Email = "matheussanton@gmail.com",
    FullName = "Matheus Santon",
    DateOfBirth = new DateTime(1995, 10, 10),
    GitHubUsername = "matheussanton"
};

var queueUrlResponse = await sqsClient.GetQueueUrlAsync("customers");

var sendMessageRequest = new SendMessageRequest()
{
    QueueUrl = queueUrlResponse.QueueUrl,
    MessageBody = JsonSerializer.Serialize(customer),
    MessageAttributes = new  Dictionary<string, MessageAttributeValue>
    {
        {
            "MessageType",
            new MessageAttributeValue
            {
                DataType = "String",
                StringValue = nameof(CustomerCreated)
            }
        }
    }
};

var response = await sqsClient.SendMessageAsync(sendMessageRequest);

Console.WriteLine(response);

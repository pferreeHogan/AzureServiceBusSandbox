using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Threading.Tasks;
using System.Linq;

class Program
{
    // connection string to your Service Bus namespace
    static string connectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

    // name of your Service Bus queue
    static string queueName = "demoqueue";
    
    // name of your Service Bus topic
    static string topicName = "demotopic";
    
    // name of your Service Bus topic subscription
    static string subscriptionName = "demosubscription";
    static string subscriptionName2 = "subscription.2";

    static async Task Main()
    {
        Console.WriteLine("Azure Service Bus Demo");
        Console.WriteLine("1. Send message to Queue");
        Console.WriteLine("2. Receive message from Queue");
        Console.WriteLine("3. Send message to Topic");
        Console.WriteLine("4. Receive message from Topic Subscription");
        Console.WriteLine("5. Show Diagnostic Information");
        Console.WriteLine("6. Demonstrate Message Retry");
        Console.WriteLine("7. Check Dead Letter Queue");
        Console.WriteLine("8. Exit");

        while (true)
        {
            Console.Write("\nEnter your choice (1-8): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await SendMessageToQueue();
                    break;
                case "2":
                    await ReceiveMessageFromQueue();
                    break;
                case "3":
                    await SendMessageToTopic();
                    break;
                case "4":
                    await ReceiveMessageFromSubscription();
                    break;
                case "5":
                    await ShowDiagnosticInfo();
                    break;
                case "6":
                    await DemonstrateMessageRetry();
                    break;
                case "7":
                    await CheckDeadLetterQueue();
                    break;
                case "8":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static async Task ShowDiagnosticInfo()
    {
        try
        {
            Console.WriteLine("\n=== Queue Information ===");
            try
            {
                await using var client = new ServiceBusClient(connectionString);
                await using var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });
                var messages = await receiver.PeekMessagesAsync(10);
                Console.WriteLine($"Queue '{queueName}' exists");
                Console.WriteLine($"Messages available: {messages.Count}");
                foreach (var msg in messages)
                {
                    Console.WriteLine($"Message ID: {msg.MessageId}");
                    Console.WriteLine($"Body: {msg.Body}");
                    Console.WriteLine("---");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting queue info: {ex.Message}");
            }

            Console.WriteLine($"\n=== Topic and Subscription Information For: {subscriptionName}===");
            try
            {
                await using var client = new ServiceBusClient(connectionString);
                await using var receiver = client.CreateReceiver(topicName, subscriptionName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });
                
                var messages = await receiver.PeekMessagesAsync(10);
                Console.WriteLine($"Topic '{topicName}' and Subscription '{subscriptionName}' exist");
                Console.WriteLine($"Messages in subscription: {messages.Count}");
                foreach (var msg in messages)
                {
                    Console.WriteLine($"Message ID: {msg.MessageId}");
                    Console.WriteLine($"Content Type: {msg.ContentType}");
                    Console.WriteLine($"Body: {msg.Body}");
                    Console.WriteLine($"Enqueued Time: {msg.EnqueuedTime}");
                    Console.WriteLine("---");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting topic/subscription info: {ex.Message}");
            }

            Console.WriteLine($"\n=== Topic and Subscription Information For: {subscriptionName2}===");
            try
            {
                await using var client = new ServiceBusClient(connectionString);
                await using var sub2Receiver = client.CreateReceiver(topicName, subscriptionName2, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });
                
                var messages = await sub2Receiver.PeekMessagesAsync(10);
                Console.WriteLine($"Topic '{topicName}' and Subscription '{subscriptionName2}' exist");
                Console.WriteLine($"Messages in subscription: {messages.Count}");
                foreach (var msg in messages)
                {
                    Console.WriteLine($"Message ID: {msg.MessageId}");
                    Console.WriteLine($"Content Type: {msg.ContentType}");
                    Console.WriteLine($"Body: {msg.Body}");
                    Console.WriteLine($"Enqueued Time: {msg.EnqueuedTime}");
                    Console.WriteLine("---");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting topic/subscription info: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in diagnostics: {ex.Message}");
        }
    }

    static async Task SendMessageToQueue()
    {
        await using var client = new ServiceBusClient(connectionString);
        await using ServiceBusSender sender = client.CreateSender(queueName);

        Console.Write("Enter message to send: ");
        string messageBody = Console.ReadLine();

        try
        {
            var message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
            Console.WriteLine("Message sent to queue successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    static async Task ReceiveMessageFromQueue()
    {
        await using var client = new ServiceBusClient(connectionString);
        await using ServiceBusReceiver receiver = client.CreateReceiver(queueName);

        try
        {
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
            
            if (receivedMessage != null)
            {
                string body = receivedMessage.Body.ToString();
                Console.WriteLine($"Received message: {body}");
                await receiver.CompleteMessageAsync(receivedMessage);
            }
            else
            {
                Console.WriteLine("No messages available in the queue.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
        }
    }

    static async Task SendMessageToTopic()
    {
        await using var client = new ServiceBusClient(connectionString);
        await using ServiceBusSender sender = client.CreateSender(topicName);

        Console.Write("Enter message to send to topic: ");
        string messageBody = Console.ReadLine();

        try
        {
            var message = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json"  // Set the content type to match the subscription filter
            };
            await sender.SendMessageAsync(message);
            Console.WriteLine("Message sent to topic successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to topic: {ex.Message}");
        }
    }

    static async Task ReceiveMessageFromSubscription()
    {
        await using var client = new ServiceBusClient(connectionString);
        await using ServiceBusReceiver receiver = client.CreateReceiver(topicName, subscriptionName);
       

        try
        {
            Console.WriteLine($"Receiving message from {subscriptionName}");
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
            
            if (receivedMessage != null)
            {
                string body = receivedMessage.Body.ToString();
                Console.WriteLine($"Received message from subscription: {body}");
                await receiver.CompleteMessageAsync(receivedMessage);
            }
            else
            {
                Console.WriteLine("No messages available in the subscription.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message from subscription: {ex.Message}");
        }

    }

    static async Task DemonstrateMessageRetry()
    {
        Console.WriteLine("\nSending a message that will fail processing...");
        
        // First, send a message marked for failure
        await using (var client = new ServiceBusClient(connectionString))
        {
            await using var sender = client.CreateSender(queueName);
            var message = new ServiceBusMessage("This message will fail processing")
            {
                ApplicationProperties = { { "ShouldFail", "true" } }
            };
            await sender.SendMessageAsync(message);
            Console.WriteLine("Test message sent to queue.");
        }

        // Now try to process it with retries
        await using (var client = new ServiceBusClient(connectionString))
        {
            await using var receiver = client.CreateReceiver(queueName);
            
            try
            {
                var maxRetries = 3;
                var currentRetry = 0;

                while (currentRetry < maxRetries)
                {
                    var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
                    
                    if (receivedMessage == null)
                    {
                        Console.WriteLine("No messages to process.");
                        break;
                    }

                    try
                    {
                        // Simulate processing that might fail
                        if (receivedMessage.ApplicationProperties.TryGetValue("ShouldFail", out var shouldFail) 
                            && shouldFail.ToString() == "true")
                        {
                            currentRetry++;
                            throw new Exception($"Simulated processing failure (Attempt {currentRetry} of {maxRetries})");
                        }

                        // If we get here, processing succeeded
                        await receiver.CompleteMessageAsync(receivedMessage);
                        Console.WriteLine("Message processed successfully.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");

                        if (currentRetry >= maxRetries)
                        {
                            Console.WriteLine("Max retries reached. Moving message to dead-letter queue.");
                            await receiver.DeadLetterMessageAsync(receivedMessage, 
                                "ProcessingFailure", 
                                $"Failed after {maxRetries} attempts");
                        }
                        else
                        {
                            // Abandon the message so it can be retried
                            await receiver.AbandonMessageAsync(receivedMessage);
                            Console.WriteLine("Message abandoned for retry.");
                            await Task.Delay(1000 * currentRetry); // Back off between retries
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in retry demonstration: {ex.Message}");
            }
        }
    }

    static async Task CheckDeadLetterQueue()
    {
        Console.WriteLine("\nChecking Dead Letter Queue...");
        
        await using var client = new ServiceBusClient(connectionString);
        await using var receiver = client.CreateReceiver(queueName, 
            new ServiceBusReceiverOptions 
            { 
                SubQueue = SubQueue.DeadLetter,
                ReceiveMode = ServiceBusReceiveMode.PeekLock 
            });

        try
        {
            var messages = await receiver.ReceiveMessagesAsync(10);
            
            if (!messages.Any())
            {
                Console.WriteLine("No messages in dead-letter queue.");
                return;
            }

            foreach (var message in messages)
            {
                Console.WriteLine($"\nDead-lettered Message:");
                Console.WriteLine($"Message ID: {message.MessageId}");
                Console.WriteLine($"Body: {message.Body}");
                Console.WriteLine($"Dead Letter Reason: {message.DeadLetterReason}");
                Console.WriteLine($"Dead Letter Error Description: {message.DeadLetterErrorDescription}");
                Console.WriteLine("---");

                // Optionally, process or clean up the dead-lettered message
                Console.Write("Do you want to delete this message? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    await receiver.CompleteMessageAsync(message);
                    Console.WriteLine("Message deleted from dead-letter queue.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking dead-letter queue: {ex.Message}");
        }
    }
} 
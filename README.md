# Azure Service Bus Demo Application

A simple console application demonstrating core Azure Service Bus functionality using the local Service Bus Emulator.

## Features
- Queue operations (send/receive)
- Topic/Subscription operations with filtering
- Message retry handling and reliability patterns
- Dead-letter queue management
- Diagnostic information viewing

## Prerequisites
- .NET 6.0 or later
- [Azure Service Bus Emulator](https://github.com/Azure/azure-service-bus-emulator-installer) installed locally
- Copy the contents of `EmulatorConfig.json` from this project to the emulator's `Config.json` file (located in the emulator's installation directory)
- EmulatorConfig.json configured with:
  - Queue: "demoqueue"
  - Topic: "demotopic"
  - Subscriptions: "demosubscription" and "subscription.2"

## Running the Application
1. Start your Azure Service Bus Emulator
2. Run the application: `dotnet run`
3. Choose from the menu options:
   - Options 1-2: Queue operations
   - Options 3-4: Topic operations
   - Option 5: View diagnostic information
   - Option 6: Test message retry pattern
   - Option 7: Inspect dead-letter queue
   - Option 8: Exit

## Connection String
The application uses a local emulator connection string:
```
Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;
```

## Message Filtering
- Subscription "demosubscription" filters messages with ContentType="application/json"
- Subscription "subscription.2" filters messages with custom property "prop1"="value1"


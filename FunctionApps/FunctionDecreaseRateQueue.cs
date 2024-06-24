using System;
using Azure.Messaging.ServiceBus;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using FunctionApps.Properties;

namespace FunctionApps
{
    public static class FunctionDecreaseRateQueue
    {
        [FunctionName("FunctionDecreaseRateQueue")]
        public static async Task Run([ServiceBusTrigger("%ServiceBusDecreaseRateQueue%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, ILogger log, ExecutionContext context)
        {
            var messageBody = JsonConvert.DeserializeObject<DecreaseRateMessage>(Encoding.UTF8.GetString(message.Body));
            log.LogInformation($"Recieved decrease production rate message: {message.Body}");

            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Resources.IoTHubConnectionString);

            log.LogInformation("DecreaseProductRate call result:");
            CloudToDeviceMethod emergencyStopMethod = new CloudToDeviceMethod("DecreaseProductRate");
            emergencyStopMethod.ResponseTimeout = TimeSpan.FromSeconds(20);
            CloudToDeviceMethodResult emergencyStopMethodResult = await serviceClient.InvokeDeviceMethodAsync(messageBody.deviceId, emergencyStopMethod);
            log.LogInformation(emergencyStopMethodResult.Status.ToString());
            log.LogInformation(emergencyStopMethodResult.GetPayloadAsJson());
        }

        class DecreaseRateMessage
        {
            public string deviceId { get; set; }
            public DateTime time { get; set; }
        }
    }
}

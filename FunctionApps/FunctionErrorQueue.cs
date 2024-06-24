using Azure.Messaging.ServiceBus;
using FunctionApps.Properties;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApps
{
    public static class FunctionErrorQueue
    {

        [FunctionName("FunctionErrorQueue")]
        public static async Task Run([ServiceBusTrigger("%ServiceBusErrorQueue%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, ILogger log, ExecutionContext context)
        {
            var messageBody = JsonConvert.DeserializeObject<EmergencyStopErrorMessage>(Encoding.UTF8.GetString(message.Body));
            log.LogInformation($"Recieved emergency stop message: {message.Body}");

            // execute emergency stop on deviceId in payload
            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Resources.IoTHubConnectionString);

            log.LogInformation("Emergency stop call result:");
            CloudToDeviceMethod emergencyStopMethod = new CloudToDeviceMethod("EmergencyStop");
            emergencyStopMethod.ResponseTimeout = TimeSpan.FromSeconds(20);
            CloudToDeviceMethodResult emergencyStopMethodResult = await serviceClient.InvokeDeviceMethodAsync(messageBody.deviceId, emergencyStopMethod);
            log.LogInformation(emergencyStopMethodResult.Status.ToString());
            log.LogInformation(emergencyStopMethodResult.GetPayloadAsJson());
        }

        class EmergencyStopErrorMessage
        {
            public string deviceId { get; set; }
            public double errorSum { get; set; }
            public DateTime time { get; set; }
        }

    }
}

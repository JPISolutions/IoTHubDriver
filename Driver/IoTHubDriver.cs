using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearSCADA.DBObjFramework;
using ClearSCADA.DriverFramework;
using IoTHubDBModule;
using System.IO;
using System.Data.Odbc;
using System.Data;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace IoTHubDriver
{
    class IoTHubDriver
    {
        static void Main(string[] args)
        {
            using (DriverApp App = new DriverApp())
            {
                if (App.Init(new IoTHubDriverModule(), args))

                    // Do any initialization here
                    App.Log("JPI Solutions IoT Hub Driver Started");
                
                // Start the main loop
                App.MainLoop();
            }
        }
    }

    public class AzIoTHubExportScanner : DriverScanner<AzureIoTHubExport>
    {

        public AzIoTHubExportScanner(): base()
        {

        }

        static DeviceClient deviceClient;
       
        public override SourceStatus OnDefine()
        {
            // Code for when the scanner is enabled. 
            // Log the scanner state to the log file and set the scan rate and offset to those
            // set on the object. 
            App.Log("OnDefine: Scanner " + Convert.ToString(DBScanner.Id) + " Startup...");

            try
            {
                App.Log("OnDefine: Set ScanRate: " + Convert.ToString(DBScanner.ScanRate) + " Set Offset: " + Convert.ToString(DBScanner.ScanOffset));
                SetScanRate(DBScanner.ScanRate, DBScanner.ScanOffset, true);

                App.Log("OnDefine: Connect to IoT Hub using primary connection string.");
                deviceClient = DeviceClient.CreateFromConnectionString(DBScanner.AzureIoTHub.PrimaryConnectString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);

                App.Log("OnDefine: Scanner Online.");
                return SourceStatus.Online;
            }

            catch (Exception e)
            {
                App.Log("OnDefine: Uh oh, we had a problem connecting.");
                App.Log("OnDefine: " + e.Message);
                App.Log("OnDefine: Scanner Failed.");
                return SourceStatus.Failed;

            }
        }

        public override void OnUnDefine()
        {
            // Code for when the scanner is disabled or about to be saved
            // Log to the log file and set the status to Offline.
            App.Log("OnUnDefine: Scanner " + Convert.ToString(DBScanner.Id) + " disabled.");
            SetStatus(SourceStatus.Offline);
        }


        public override void OnScan()
        {

            try
            {
                App.Log("OnScan: Start Scan");
                App.Log("OnScan: Connect ODBC");

                // Can I get the SYSTEM name from inside the driver?
                // How can I run this against a DB that is protected/secured?
                string s = "DRIVER={ClearSCADA Driver};Server=MAIN;UID=;PWD=;LOCALTIME=False;LOGINTIMEOUT=6000";
                OdbcConnection con = new OdbcConnection
                {
                    ConnectionString = s
                };
                con.Open();

                App.Log("OnScan: ODBC Connected");
                App.Log("OnScan: Run ODBC Query. Query: " + DBScanner.Query);
                
                OdbcDataAdapter adap = new OdbcDataAdapter(DBScanner.Query, con);
                DataTable dat = new DataTable();
                adap.Fill(dat);
                var json_data = JsonConvert.SerializeObject(dat);

                App.Log("OnScan: Converted query results to JSON.");

                App.Log("OnScan: Send Message to Azure IoT Hub");
                var tsk = SendStrToIoTHubAsync(json_data);
                
                App.Log("OnScan: Scan Completed.");

            }

            catch (Exception e)
            {
                App.Log("OnScan: Uh oh, something went wrong.");

                App.Log("OnScan: Raising an alarm.");
                SetStatus(SourceStatus.Failed);
                SetFailReason(e.Message);

                App.Log("OnScan: Error: " + e.Message);
            }
                
            // Useful for sending dictionaries to IoT Hub
            async Task SendDictToIoTHubAsync(Dictionary<string, string> dict)
            {
                
                var msg_dict = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dict)));
                await deviceClient.SendEventAsync(msg_dict);

            }

            // Useful for sending JSON strings to IoT Hub
            async Task SendStrToIoTHubAsync(string str )
            {
                
                var msg_dict = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(str));
                await deviceClient.SendEventAsync(msg_dict);

            }

        }


    }


    public class AzIoTHubImportScanner : DriverScanner<AzureIoTHubImport>
    {

        public AzIoTHubImportScanner() : base()
        {

        }

        static EventHubClient eventHub;
        static string[] d2cPartitions;

        CancellationTokenSource cts = new CancellationTokenSource();

        public override SourceStatus OnDefine()
        {
            // Code for when the scanner is enabled. 
            // Log the scanner state to the log file and set the scan rate and offset to those
            // set on the object. 
            App.Log("OnDefine: Scanner " + Convert.ToString(DBScanner.Id) + " Startup...");

            try
            {
                App.Log("OnDefine: Set ScanRate: " + Convert.ToString(DBScanner.ScanRate) + " Set Offset: " + Convert.ToString(DBScanner.ScanOffset));
                SetScanRate(DBScanner.ScanRate, DBScanner.ScanOffset, true);

                App.Log("OnDefine: Connect to IoT Hub using primary connection string.");
                eventHub = EventHubClient.CreateFromConnectionString(DBScanner.AzureIoTHub.EndPoint);
                EventHubRuntimeInformation runtimeInfo = eventHub.GetRuntimeInformationAsync().Result;
                d2cPartitions = runtimeInfo.PartitionIds;
                App.Log("OnDefine: Scanner Online.");
                return SourceStatus.Online;
            }

            catch (Exception e)
            {
                App.Log("OnDefine: Uh oh, we had a problem connecting.");
                App.Log("OnDefine: " + e.Message);
                App.Log("OnDefine: Scanner Failed.");
                return SourceStatus.Failed;

            }
        }

        public override void OnUnDefine()
        {
            // Code for when the scanner is disabled or about to be saved
            // Log to the log file and set the status to Offline.
            App.Log("OnUnDefine: Scanner " + Convert.ToString(DBScanner.Id) + " disabled.");
            cts.Cancel();
            SetStatus(SourceStatus.Offline);
        }


        public override void OnScan()
        {

            try
            {
                App.Log("OnScan: Start Scan");
                App.Log("OnScan: Read EventHub Message");

                // Code for reading the messages from the IoT Hub
                //
                //
                
                foreach (string partition in d2cPartitions)
                {

                    string tsk = ReceiveMessagesFromDeviceAsync(partition, cts.Token).Result;
                    App.Log("Data: " + tsk);

                }

                //
                //
                // ----------------------------------------------

                App.Log("OnScan: Scan Completed.");

            }

            catch (Exception e)
            {
                App.Log("OnScan: Uh oh, something went wrong.");

                App.Log("OnScan: Raising an alarm.");
                SetStatus(SourceStatus.Failed);
                SetFailReason(e.Message);

                App.Log("OnScan: Error: " + e.Message);
            }
        }



        async Task<string> ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            string text;
            // Create the receiver using the default consumer group.
            // For the purposes of this sample, read only messages sent since 
            // the time the receiver is created. Typically, you don't want to skip any messages.
            var eventHubReceiver = eventHub.CreateReceiver("$Default", partition, EventPosition.FromEnqueuedTime(DateTime.Now));
            text = "Create receiver on partition: " + partition + "\n";
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                text = text + "Listening for messages on: " + partition + "\n";
                // Check for EventData - this methods times out if there is nothing to retrieve.
                var events = await eventHubReceiver.ReceiveAsync(100);

                // If there is data in the batch, process it.
                if (events == null) continue;

                foreach (EventData eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    text = text + "Message received on partition {0}:" + partition + "\n";
                    text = text + "  {0}:" + data + "\n";
                    text = text + "Application properties (set by device):" + "\n";
                    foreach (var prop in eventData.Properties)
                    {
                        text = text + "  {0}: {1}"+ prop.Key + prop.Value + "\n";
                    }
                    text = text + "System properties (set by IoT Hub):" + "\n";
                    foreach (var prop in eventData.SystemProperties)
                    {
                        text = text + "  {0}: {1}" + prop.Key + prop.Value + "\n";
                    }
                }

                return text;
            }

            return "End";
        }
    }
}

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
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;

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
                    App.Log("IoT Azure driver started");
                 
                // Start the main loop
                App.MainLoop();
            }
        }
    }

    public class DrvCSScanner : DriverScanner<AzureIoTHubScanner>
    {

        static DeviceClient deviceClient;        

        /* Constructor */
        public DrvCSScanner()
        { }

        public override SourceStatus OnDefine()
        {
            // Code for when the scanner is enabled. 
            // Log the scanner state to the log file and set the scan rate and offset to those
            // set on the object. 
            App.Log("OnDefine: Scanner Startup...");
            try
            {

                App.Log("OnDefine: Set ScanRate:" + Convert.ToString(DBScanner.ScanRate) + " Set Offset: " + Convert.ToString(DBScanner.ScanOffset));
                SetScanRate(DBScanner.ScanRate, DBScanner.ScanOffset, true);

                App.Log("OnDefine: Connect to IoT Hub using connection string.");
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
            base.OnUnDefine();
        }
        
        public override void OnScan()
        {
            
            try
            {
                App.Log("OnScan: <!-- Start Scan");

                App.Log("OnScan: Sending Message to Azure IoT Hub");
                var tsk = SendDictToIoTHubAsync(new Dictionary<string, string> { { "key", "val" }, { "key2", "val2" } });

                App.Log("OnScan: Scan Complete -->");
                
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

        async Task SendDictToIoTHubAsync(Dictionary<string, string> dict)
        {


            var msg_dict = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dict)));
            await deviceClient.SendEventAsync(msg_dict);

        }

        private static void get_data_from_scada()
        {

            string s = "DRIVER={ClearSCADA Driver};Server=Local;UID=;PWD=;LOCALTIME=True;LOGINTIMEOUT=6000";
            System.Data.Odbc.OdbcConnection con = new System.Data.Odbc.OdbcConnection();
            con.ConnectionString = s;
            con.Open();
            /*
            The data we want will be in the CDBPOINT table.
            Something like this: 
            */
            string q = @"SELECT
                FullName, Name, CurrentValueAsReal, CurrentTime
                FROM
                CDBPOINT
                ";

            OdbcDataAdapter adap = new OdbcDataAdapter(q, con);
            DataTable dat = new DataTable();
            adap.Fill(dat);
        }
    }
}

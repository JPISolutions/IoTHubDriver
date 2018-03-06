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
                    App.Log("JPI Solutions IoT Hub Driver Started");
                
                // Start the main loop
                App.MainLoop();
            }
        }
    }

    public class DrvCSScanner : DriverScanner<AzureIoTHubScanner>
    {

        public DrvCSScanner(): base()
        {

        }

        static DeviceClient deviceClient;

        string IOT_HUB_CONN_STRING = DBScanner.AzureIoTHub.PrimaryConnectString;
       
        public override SourceStatus OnDefine()
        {
            // Code for when the scanner is enabled. 
            // Log the scanner state to the log file and set the scan rate and offset to those
            // set on the object. 
            App.Log("Scanner " + Convert.ToString(DBScanner.Id) + " enabled.");
            SetScanRate(DBScanner.ScanRate, DBScanner.ScanOffset);
            return SourceStatus.Online;
        }

        public override void OnUnDefine()
        {
            // Code for when the scanner is disabled or about to be saved
            // Log to the log file and set the status to Offline.
            App.Log("Scanner " + Convert.ToString(DBScanner.Id) + " disabled.");
            SetStatus(SourceStatus.Offline);
        }


        public override void OnScan()
        {

            {

                //---------
                //var tsk = SendDictToIoTHubAsync(new Dictionary<string, string> { { "key", "Dev_VM" }, { "key2", "JPISOLUTIONS_updated" } });
                // Removed due to sync error
                //tsk.RunSynchronously();
                
           
                    try
                    {


                }
                catch (Exception e)
                {
                    Log(e.Message +"JPItest01");
                    Log(e.StackTrace);
                }
                




                string s = "DRIVER={ClearSCADA Driver};Server=Local;UID=;PWD=;LOCALTIME=True;LOGINTIMEOUT=6000";
                System.Data.Odbc.OdbcConnection con = new System.Data.Odbc.OdbcConnection();
                con.ConnectionString = s;
                con.Open();
                /*
                The data we want will be in the CDBPOINT table.
                Something like this: 
                */
                string q = @"SELECT ID, FULLNAME, CURRENTVALUEASREAL, CURRENTQUALITYDESC, CURRENTTIME FROM CDBPOINT";

                OdbcDataAdapter adap = new OdbcDataAdapter(q, con);
                DataTable dat = new DataTable();
                adap.Fill(dat);
                var json_data = JsonConvert.SerializeObject(dat);
                //File.WriteAllText(@"C:\rows1.txt", json_data);
                SendStrToIoTHubAsync(json_data);
                //tsk.GetAwaiter().OnCompleted(() => { File.WriteAllText(@"C:\completed.txt", "jhfjghj"); });
                // Code for each scan to go here. 

                //tests. added here
                //File.WriteAllText(@"C:\testout.txt", DBScanner.AzureIoTHub.EndPoint +"-gideond");
                //App.Log("Scan on scanner " + Convert.ToString(DBScanner.AzureIoTHub.EndPoint) + " Executed");

                //DataTable GetDataFromScada()
                //{
                //    string s = "DRIVER={ClearSCADA Driver};Server=MAIN;UID=SuperUser;PWD=SCADAAdmin;LOCALTIME=True;LOGINTIMEOUT=6000";
                //    System.Data.Odbc.OdbcConnection con = new System.Data.Odbc.OdbcConnection();
                //    con.ConnectionString = s;
                //    con.Open();
                //    /*
                //    The data we want will be in the CDBPOINT table.
                //    Something like this: 
                //    */
                //    string q = @"SELECT
                //    FullName, Name, CurrentValueAsReal, CurrentTime
                //    FROM
                //    CDBPOINT
                //    WHERE
                //    IIoTExport = TRUE";

                //    OdbcDataAdapter adap = new OdbcDataAdapter(q, con);
                //    DataTable dat = new DataTable();
                //    adap.Fill(dat);
                //    return dat;
            }


            async Task SendDictToIoTHubAsync(Dictionary<string, string> dict)
            {
                deviceClient = DeviceClient.CreateFromConnectionString(IOT_HUB_CONN_STRING, Microsoft.Azure.Devices.Client.TransportType.Mqtt);

                var msg_dict = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dict)));
                await deviceClient.SendEventAsync(msg_dict);

            }

            async Task SendStrToIoTHubAsync(string str )
            {
                deviceClient = DeviceClient.CreateFromConnectionString(IOT_HUB_CONN_STRING, Microsoft.Azure.Devices.Client.TransportType.Mqtt);

                var msg_dict = new Message(Encoding.UTF8.GetBytes(str));
                await deviceClient.SendEventAsync(msg_dict);

            }

        }


    }
}

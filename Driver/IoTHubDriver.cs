using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearSCADA.DBObjFramework;
using ClearSCADA.DriverFramework;
using IoTHubDBModule;

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
                    App.Log("C# sample driver started");

                    // Start the main loop
                    App.MainLoop();
            }
        }
    }

    public class DrvCSScanner : DriverScanner<AzureIoTHubScanner>
    {
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
            // Code for each scan to go here. 
            App.Log("Scan on scanner " + Convert.ToString(DBScanner.Id) + " Executed");
        }


    }
}

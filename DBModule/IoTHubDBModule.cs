using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Messaging;

using ClearSCADA.DBObjFramework;


// Add in the driver category and description for proper registration with the CS server app
[assembly:Category("IoT Hub")]

[assembly:Description("JPI Solutions IoT Hub Driver")]
// NEED this AssemblySupportLink otherwise the server will complain about bad metadata when it loads the module. 
[assembly:AssemblySupportLink("https://jpisolutions.ca")]
// Commented out until the driver task is built
[assembly:DriverTask("JPIIotHubDriver.exe")]


// Install hooks
[System.ComponentModel.RunInstaller(true)]
public class IoTHubInstaller : DriverInstaller
{
}


namespace IoTHubDBModule
{
    public class IoTHubDriverModule : DBModule
    {
    }

    // This aggregate will be used for storing the configuration for the Azure IoT hub. 
    [Table("Azure IoT Hub Aggregate","")]
    public class AzureIotHubAggregate : Aggregate
    {
        [Label("Azure IoT Hub Settings", 1,1,Height = 2,Width = 5 )]
        [Label("End Point", 2, 1)]
        [ConfigField("End Point", "The Event Hub-compatible endpoint string from Azure IoT Hub.", 2, 2, 0x04511001, Length = 96, DefaultOverride = true)]
        public String EndPoint;


        [Label("Device Settings", 4, 1, Height = 4, Width = 5)]
        [Label("Device Id", 5, 1)]
        [ConfigField("Device Id", "The device Id that ClearSCADA is associated with.", 5, 2, 0x045110002, Length = 32, DefaultOverride = true)]
        public String DeviceId;

        [Label("Primary Connection String", 6, 1)]
        [ConfigField("Primary Connect String", "The connection string containing the primary key from Azure IoT Hub.", 6, 2, 0x045110003, Length = 96, DefaultOverride = true)]
        public String PrimaryConnectString;

        [Label("Secondary Connection String", 7, 1)]
        [ConfigField("Secondary Connect String", "The connection string containing the secondary key from Azure IoT Hub.", 7, 2, 0x045110004, Length = 96, DefaultOverride = true)]
        public String SecondaryConnectString;

    }

    [Table("Azure IoT Hub Export Scanner", "Scanner")]
    //[EventCategory("JPIAIoTHubScanner", "IoT Hub Scanner", 0x04510020)]
    public class AzureIoTHubScanner : Scanner
    {
        [Label("In Service", 1, 1)]
        [ConfigField("In Service", "Controls whether the scanner is active.", 1, 2, 0x0350501B)]
        public override Boolean InService
        {
            get
            {
                return base.InService;   
            }
            set
            {
                base.InService = value;
            }
        }

        // For future implementation. Dustin Symes 2017-12-19
        //[Label("Confirm Disable/Enable", 2, 1)]
        //[ConfigField("ConfirmDisableEnable", "Controls whether confirmation is required when the object is enabled/disabled from a method")]
        //public Byte ConfirmDisableEnable


        [Label("Severity", 3, 1)]
        [ConfigField("Severity","The severity of the error",3, 2, 0x0350501C)]
        [Severity()]
        public UInt16 EventSeverity;

        [Label("Scan Rate", 6, 1)]
        [ConfigField("Scan Rate", "The rate of export to IoT hub in ms.", 6, 2, 0x03505045)]
        [Interval(IntervalType.Milliseconds)]
        public UInt32 ScanRate = 300000;

        [Label("Scan Offset", 7, 1)]
        [ConfigField("Scan Offset", "Sets the time that IoT Hub Exports are synchronized to.", 7, 2, 0x0350504D, Length=32, Flags =FormFlags.OpcOffset)]
        public String ScanOffset = "Hour";

        [DataField("Read Count", "The number of messages the export scanner has received from Azure IoT Hub.", 0x03505047)]
        public UInt32 ReadCount;

        [DataField("Write Count", "The number of messages the export scanner has sent to Azure IoT Hub.", 0x03505049)]
        public UInt32 WriteCount;

        [DataField("Last Error", "The text of the last device error.", 0x045100031)]
        public String ErrMessage;

        [DataField("Connected", "The state of the connection to Azure IoT Hub.", 0x045100032)]
        public Boolean Connected;

        [Aggregate("Enabled", "Azure IoT Hub", 0x04511000)]
        public AzureIotHubAggregate AzureIoTHub;

        // For future implementation. Dustin Symes 2017-12-19
        //[AlarmCondition("IoT Hub Alarm", "JPIIoTHubScanner", 0x04510010)]
        //[AlarmSubCondition("Bad")]
        //[AlarmSubCondition("Really Bad")]
        //public Alarm IoTHubAlarm;

        public override void OnValidateConfig(MessageInfo Errors)
        {
            base.OnValidateConfig(Errors); 
        }

    }
}

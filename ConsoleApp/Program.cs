using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //get_data_from_scada();
            WebClient client = new WebClient();
            Stream stream = client.OpenRead("http://google.com");
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();
            File.WriteAllText(@"C:\netcall.txt", content);

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

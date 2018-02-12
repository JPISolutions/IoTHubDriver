using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
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
                WHERE
                IIoTExport = TRUE";

            OdbcDataAdapter adap = new OdbcDataAdapter(q, con);
            DataTable dat = new DataTable();
            adap.Fill(dat);
        }
    }
}

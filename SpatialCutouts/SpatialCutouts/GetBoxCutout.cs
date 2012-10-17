/*
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString GetBoxCutout(string connection, string box)
    {
        // Put your code here

        string fcoverQuery = "select keyMin*512 as keyMin, keyMax*512 as keyMax from dbo.fCover('Z', 7, 'box[0,0,0,1024,1024,1024]', 0," + box + ")";

        //grab set of indices to look at
        //get data from each matched index
        //concatenate returned data




    }
};

*/
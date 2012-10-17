using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using Jhu.SqlServer.Array;
using SpatialCutouts;


public partial class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void SPGetBoxCutout()
    {
        //Int16 ax, Int16 ay, Int16 az, Int16 bx, Int16 by, Int16 bz
        int ax = 0;
        int ay = 0;
        int az = 0;
        int bx = 16;
        int by = 16;
        int bz = 16;
        string connectionString = "Context connection=true;";

        string box = "'box["+ax+","+ay+","+az+","+bx+","+by+","+bz+"]'";
        // Put your code here
        string fcoverQuery = "select keyMin*512 as keyMin, keyMax*512 as keyMax from dbo.fCover('Z', 7, 'box[0,0,0,1024,1024,1024]', 0," + box + ")";
        string dataQueryRanges = "";
        using (SqlConnection connection = new SqlConnection(
                   connectionString))
        {
            SqlCommand fCoverCommand = new SqlCommand(
                fcoverQuery, connection);
            connection.Open();
            SqlDataReader fCoverReader = fCoverCommand.ExecuteReader();
            try
            {
                bool first = true;
                while (fCoverReader.Read())
                {
                    if (!first)
                    {
                        dataQueryRanges += " or ";
                    }
                    dataQueryRanges += "zindex between  " + (int)fCoverReader[0] + " and " + (int)fCoverReader[1];
                    first = false;
                }
            }
           /* catch(Exception e)
            {
                dataQueryRanges += "FUKKKK";
            }*/
            finally
            {
                // Always call Close when done reading.
                fCoverReader.Close();
            }
            
            
            string getDataQuery = "'select * from turbdb..velocity where (" + dataQueryRanges + ") and timestep = 0'";
            //SqlDataRecord record = new SqlDataRecord(new SqlMetaData("data", SqlDbType.VarChar, -1));
            //record.SetSqlString(0, getDataQuery);// .SetSqlBinary(0, concatBlobReader.GetSqlBinary(0).Value);
            //SqlContext.Pipe.Send(record);
            
            SqlCommand getDataCommand = new SqlCommand(
                getDataQuery, connection);
            SqlDataReader getDataReader = getDataCommand.ExecuteReader();
            List<Tuple<int,Byte[]>> dataList = new List<Tuple<int,Byte[]>>();
            try
            {
                while (getDataReader.Read())
                {

                    dataList.Add(new Tuple<int,Byte[]>((int)getDataReader[1],(Byte[])getDataReader[2]));
                }
            }
            finally
            {
                getDataReader.Close();
            }
            
            List<Tuple<int[], Byte[]>> dataList2 = new List<Tuple<int[], Byte[]>>();
            foreach (Tuple<int, Byte[]> dataTuple in dataList)
            {
                String mortonQueryStr = "SELECT [turbdb].[dbo].[GetMortonX] (" + dataTuple.Item1 + "), " +
                "[turbdb].[dbo].[GetMortonY] (" + dataTuple.Item1 + "), " +
                "[turbdb].[dbo].[GetMortonZ] (" + dataTuple.Item1 + ")";
                SqlCommand mortonQuery = new SqlCommand(mortonQueryStr, connection);
                SqlDataReader mortonQueryReader = mortonQuery.ExecuteReader();
                
                try
                {
                    int[] coords = new int[3];
                    while (mortonQueryReader.Read())
                    {
                        coords[0] = (int)mortonQueryReader[0] - ax;
                        coords[1] = (int)mortonQueryReader[1] - ay;
                        coords[2] = (int)mortonQueryReader[2] - az;

                        dataList2.Add(new Tuple<int[], Byte[]>(coords, dataTuple.Item2));
                    }
                }
                finally
                {
                    mortonQueryReader.Close();

                }
            }
            dataList.Clear();

            SqlCommand createTempTable = new SqlCommand(
                "create table #tempdata ( offsets varbinary(8000), data varbinary(8000))", connection);
            createTempTable.ExecuteNonQuery();
            
            foreach (Tuple<int[], byte[]> data2Tuple in dataList2)
            {
                SqlIntArray offsets = new SqlIntArray(4);
                offsets[0] = 0;
                offsets[1] = data2Tuple.Item1[0];
                offsets[2]= data2Tuple.Item1[1];
                offsets[3] = data2Tuple.Item1[2];

                SqlCommand insertCommand = new SqlCommand("insert into #tempdata (offset, data) values (@offest, @blob)", connection);
                insertCommand.Parameters.Add("@offest", SqlDbType.VarBinary, 8000).Value = offsets.ToSqlBuffer();
                insertCommand.Parameters.Add("@blob", SqlDbType.VarBinary, 8000).Value = data2Tuple.Item2;
                insertCommand.ExecuteNonQuery();
            }

            SqlIntArray lengths = new SqlIntArray(4);
            lengths[0] = 3;
            lengths[1] = bx-ax;
            lengths[2] = by-ay;
            lengths[3] = bz-az;

            string concatBlobsQuery = "select * from RealArray.FromSubarrayTable('#tempdata', @lengths)";
            SqlCommand concatBlobCommand = new SqlCommand(concatBlobsQuery, connection);
            concatBlobCommand.Parameters.Add("@lengths", SqlDbType.VarBinary, 8000).Value = lengths.ToSqlBuffer();
            //SqlCommand concatBlobs = new

            SqlDataReader concatBlobReader = concatBlobCommand.ExecuteReader();
            // what should be the return type? of type data?
            SqlDataRecord record = new SqlDataRecord(new SqlMetaData("data", SqlDbType.VarBinary, -1));
            try
            {
                while (concatBlobReader.Read())
                {
                    //concatBlobReader[0];
                    record.SetSqlBinary(0, concatBlobReader.GetSqlBinary(0).Value);
                    SqlContext.Pipe.Send(record);
                }
            }
            finally
            {
                // Always call Close when done reading.
                concatBlobReader.Close();
            }

            

            /*

            Int16[] lengths = new Int16[dataList.Count];
            SqlCommand createTempTable = new SqlCommand(
                "create table #tempdata ( offsets varbinary(8000), data varbinary(8000))", connection);
            createTempTable.ExecuteNonQuery();
            int index = 0;
            
            foreach (byte[] atoms in dataList)
            {
                SqlCommand insertCommand = new SqlCommand("insert into #tempdata (offset, data) values (@offest, @blob)", connection);
                insertCommand.Parameters.Add("@offest", SqlDbType.VarBinary, 8000).Value = offsets;
                insertCommand.Parameters.Add("@blob", SqlDbType.VarBinary, 8000).Value = atoms;
                insertCommand.ExecuteNonQuery();
                lengths[index] = (Int16)dataList[index].Length;
                index++;
            }


            SqlIntArray lengthsArray = new SqlIntArray(lengths);
            SqlBinary lengthsBlob = lengthsArray.ToSqlBuffer();
            string concatBlobsQuery = "select * from RealArray.FromSubarrayTable('#tempdata', @lengths)";
            SqlCommand concatBlobCommand = new SqlCommand(concatBlobsQuery, connection);
            concatBlobCommand.Parameters.Add("@lengths", SqlDbType.VarBinary, 8000).Value = lengthsBlob;
            //SqlCommand concatBlobs = new

            SqlDataReader concatBlobReader = concatBlobCommand.ExecuteReader();
            // what should be the return type? of type data?
            SqlDataRecord record = new SqlDataRecord(new SqlMetaData("data", SqlDbType.VarBinary, -1));
            try
            {
                while (concatBlobReader.Read())
                {
                    //concatBlobReader[0];
                    record.SetSqlBinary(0, concatBlobReader.GetSqlBinary(0).Value);
                    SqlContext.Pipe.Send(record);
                }
            }
            finally
            {
                // Always call Close when done reading.
                concatBlobReader.Close();
            }

            */
            
        }
             

        //grab set of indices to look at
        //get data from each matched index
        //concatenate returned data
    }
};

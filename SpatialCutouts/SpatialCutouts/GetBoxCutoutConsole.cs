/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Jhu.SqlServer.Array;

namespace SpatialCutouts
{
    class GetBoxCutoutConsole
    {
        public static void Main(string[] args)
        {
            string connectionString = args[0];
            string box = args[1];

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
                        dataQueryRanges += " (" + fCoverReader[0] + " and " + fCoverReader[1] + ") ";
                        first = false;
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    fCoverReader.Close();
                }
                string getDataQuery = "select * from turbdb..velocity where zindex between " + dataQueryRanges + " and timestep = 0";
                SqlCommand getDataCommand = new SqlCommand(
                    getDataQuery, connection);
                SqlDataReader getDataReader = getDataCommand.ExecuteReader();
                List<SqlBinary> dataList = new List<SqlBinary>();
                try
                {
                    while (getDataReader.Read())
                    {

                        dataList.Add((SqlBinary)getDataReader[2]);
                    }
                }
                finally
                {
                    getDataReader.Close();
                }
                short[] lengths = new short[dataList.Count];
                SqlCommand createTempTable = new SqlCommand(
                    "create table #tempdata ( data varbinary(8000))", connection);
                createTempTable.ExecuteNonQuery();
                int index = 0;
                foreach (SqlBinary atoms in dataList) {
                    SqlCommand insertCommand = new SqlCommand ("insert into #tempdata (data) values (@blob)", connection);
                    insertCommand.Parameters.Add("@blob", SqlDbType.VarBinary, 8000).Value = atoms;
                    insertCommand.ExecuteNonQuery();
                    lengths[index] = (short) dataList[index].Length;
                    index++;
                }
                

                SqlIntArray lengthsArray = new SqlIntArray(lengths);
                SqlBinary lengthsBlob = lengthsArray.ToSqlBuffer();
                string concatBlobsQuery = "select * from RealArray.FromSubarrayTable('#tempdata', @lengths)";
                SqlCommand concatBlobCommand = new SqlCommand(concatBlobsQuery, connection);
                concatBlobCommand.Parameters.Add("@lengths", SqlDbType.VarBinary, 8000).Value = lengthsBlob;
                //SqlCommand concatBlobs = new

                SqlDataReader concatBlobReader = concatBlobCommand.ExecuteReader();
                try
                {
                    while (concatBlobReader.Read())
                    {
                        concatBlobReader[0];
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    fCoverReader.Close();
                }



            }

            //grab set of indices to look at
            //get data from each matched index
            //concatenate returned data

        }
    }
}
*/
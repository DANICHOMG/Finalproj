using UnityEngine;
using System.Data;
using System.Data.SQLite;
using System;

public class DatabaseManager : MonoBehaviour
{
    private string connectionString;

    void Start()
    {
        connectionString = "URI=file:" + "D:\\My project2\\Assets\\FinancesDB.sqlite";
        CreateTransactionTable();
    }



    public void CreateTransactionTable()
    {
        using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "CREATE TABLE IF NOT EXISTS Transactions (ID INTEGER PRIMARY KEY AUTOINCREMENT, Amount REAL, Description TEXT, Date TEXT, Category TEXT)";

                dbCmd.CommandText = sqlQuery;

                try
                {
                    dbCmd.ExecuteNonQuery();
                    Debug.Log("Table 'Transactions' created successfully.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error creating table 'Transactions': {ex.Message}");
                }
            }

            dbConnection.Close();
        }
    }
}

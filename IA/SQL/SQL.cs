﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IA.SQL
{
    public delegate void QueryOutput(Dictionary<string, object> result);

    public class MySQL
    {   
        static MySQL instance;

        SQLInformation info;

        PrefixValue defaultIdentifier;

        public MySQL()
        {
            if (instance == null)
            {
                Log.ErrorAt("IA.SQLManager", "IA not initialized");
                return;
            }

            info = instance.info;
            defaultIdentifier = instance.defaultIdentifier;
        }
        public MySQL(SQLInformation info, PrefixValue defaultIdentifier)
        {
            this.info = info;
            if(defaultIdentifier == null)
            {
                this.defaultIdentifier = new PrefixValue(">");
            }
            else
            {
                this.defaultIdentifier = defaultIdentifier;
            }
            instance = this;
        }

        public int IsEventEnabled(string event_name, ulong channel_id)
        {
            if (info == null) return 1;

            MySqlConnection connection = new MySqlConnection(info.GetConnectionString());
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT enabled FROM event WHERE id=\"{channel_id}\" AND name=\"{event_name}\"";

            connection.Open();
            MySqlDataReader r = command.ExecuteReader();

            bool output = false;
            string check = "";

            while (r.Read())
            {
                output = r.GetBoolean(0);
                check = "ok";
                break;
            }
            connection.Close();

            if (check == "")
            {
                return -1;
            }
            return output ? 1 : 0;
        }

        public string GetConnectionString()
        {
            return info.GetConnectionString();
        }

        /// <summary>
        /// Gets the prefix from the server's id
        /// </summary>
        /// <param name="server_id">server id</param>
        /// <returns></returns>
        public string GetIdentifier(ulong server_id)
        {
            if (info == null) return defaultIdentifier.Value;

            string output = "";

            MySqlConnection connection = new MySqlConnection(info.GetConnectionString());
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = string.Format("SELECT i FROM identifier WHERE id={0}", server_id);
            connection.Open();
            MySqlDataReader r = command.ExecuteReader();

            while (r.Read())
            {
                output = r.GetString(0);
                break;
            }
            connection.Close();

            if (output != "")
            {
                return output;
            }
            return "ERROR";
        }

        /// <summary>
        /// Gets the instance of the initialized object if created.
        /// </summary>
        /// <returns></returns>
        public static MySQL GetInstance()
        {
            return instance;
        }

        /// <summary>
        /// Queries the sqlCode to output
        /// </summary>
        /// <param name="sqlCode">use this format: UPDATE table.row SET var=?var WHERE var2=?var2</param>
        /// <param name="output"></param>
        public static void Query(string sqlCode, QueryOutput output, params object[] p)
        {
            if (instance.info == null) return;
            MySqlConnection connection = new MySqlConnection(instance.info.GetConnectionString());


            List<MySqlParameter> parameters = new List<MySqlParameter>();

            string curCode = sqlCode;
            string prevCode = "";

            // Get code ready to extract
            while (curCode != prevCode)
            {
                prevCode = curCode;

                curCode = curCode.Replace(" = ", "=");
                curCode = curCode.Replace(" =", "=");
                curCode = curCode.Replace("= ", "=");
            }

            List<string> splitSql = new List<string>();
            splitSql.AddRange(curCode.Split(' '));

            for (int i = 0; i < splitSql.Count; i++)
            {
                List<string> splitString = new List<string>();
                splitString.AddRange(splitSql[i].Split('='));

                if (splitString.Count > 1)
                {
                    if (splitString[1].StartsWith("?"))
                    {
                        if (parameters.Find(x => { return x.ParameterName == splitString[0]; }) == null)
                        {
                            parameters.Add(new MySqlParameter(splitString[0], p[parameters.Count]));
                        }
                    }
                }
                else
                {
                    if (splitSql[i].Contains("?"))
                    {
                        splitString = new List<string>();
                        splitString.AddRange(splitSql[i].Split('?'));
                        if (parameters.Find(x => { return x.ParameterName == splitString[1].TrimEnd(',', ')', ';'); }) == null)
                        {
                            parameters.Add(new MySqlParameter(splitString[1].TrimEnd(',', ')', ';'), p[parameters.Count]));
                        }
                    }
                }
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = curCode;
            command.Parameters.AddRange(parameters.ToArray());
            connection.Open();

            bool hasRead = false;

            if (output != null)
            {
                MySqlDataReader r = command.ExecuteReader();
                while (r.Read())
                {
                    Dictionary<string, object> outputdict = new Dictionary<string, object>();
                    for (int i = 0; i < r.VisibleFieldCount; i++)
                    {
                        outputdict.Add(r.GetName(i), r.GetValue(i));
                    }
                    output?.Invoke(outputdict);
                    hasRead = true;
                }

                if (!hasRead)
                {
                    output?.Invoke(null);
                }
            }
            else
            {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }

        /// <summary>
        /// Asynchronously queries the sqlCode to output
        /// </summary>
        /// <param name="sqlCode">use this format: UPDATE table.row SET var=?var WHERE var2=?var2</param>
        /// <param name="output"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static async Task QueryAsync(string sqlCode, QueryOutput output, params object[] p)
        {
            if (instance.info == null) return;
            MySqlConnection connection = new MySqlConnection(instance.info.GetConnectionString());


            List<MySqlParameter> parameters = new List<MySqlParameter>();

            string curCode = sqlCode;
            string prevCode = "";

            // Get code ready to extract
            while (curCode != prevCode)
            {
                prevCode = curCode;

                curCode = curCode.Replace(" = ", "=");
                curCode = curCode.Replace(" =", "=");
                curCode = curCode.Replace("= ", "=");
            }

            List<string> splitSql = new List<string>();
            splitSql.AddRange(curCode.Split(' '));

            for (int i = 0; i < splitSql.Count; i++)
            {
                List<string> splitString = new List<string>();
                splitString.AddRange(splitSql[i].Split('='));

                if (splitString.Count > 1)
                {
                    if (splitString[1].StartsWith("?"))
                    {
                        if (parameters.Find(x => { return x.ParameterName == splitString[0]; }) == null)
                        {
                            parameters.Add(new MySqlParameter(splitString[0], p[parameters.Count]));
                        }
                    }
                }
                else
                {
                    if (splitSql[i].Contains("?"))
                    {
                        splitString = new List<string>();
                        splitString.AddRange(splitSql[i].Split('?'));
                        if (parameters.Find(x => { return x.ParameterName == splitString[1].TrimEnd(',', ')', ';'); }) == null)
                        {
                            parameters.Add(new MySqlParameter(splitString[1].TrimEnd(',', ')', ';'), p[parameters.Count]));
                        }
                    }
                }
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = curCode;
            command.Parameters.AddRange(parameters.ToArray());
            connection.Open();

            bool hasRead = false;

            if (output != null)
            {
                MySqlDataReader r = await command.ExecuteReaderAsync() as MySqlDataReader;
                while (await r.ReadAsync())
                {
                    Dictionary<string, object> outputdict = new Dictionary<string, object>();
                    for (int i = 0; i < r.VisibleFieldCount; i++)
                    {
                        outputdict.Add(r.GetName(i), r.GetValue(i));
                    }
                    output?.Invoke(outputdict);
                    hasRead = true;
                }

                if (!hasRead)
                {
                    output?.Invoke(null);
                }
            }
            else
            {
                await command.ExecuteNonQueryAsync();
            }
            await connection.CloseAsync();
        }

        [Obsolete("use 'MySQL.Query' instead.")]
        /// <summary>
        /// Old Query, doesnt return anything.
        /// </summary>
        /// <param name="sqlCode">valid sql code</param>
        public void SendToSQL(string sqlCode)
        {
            if (info == null) return;

            MySqlConnection connection = new MySqlConnection(info.GetConnectionString());
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = sqlCode;
            connection.Open();
            MySqlDataReader r = command.ExecuteReader();
            connection.Close();
        }

        /// <summary>
        /// Sets the prefix of the server's id to prefix
        /// </summary>
        /// <param name="prefix">st</param>
        /// <param name="server_id"></param>
        public async void SetIdentifier(string prefix, ulong server_id)
        {
            await MySQL.QueryAsync("INSERT INTO identifier VALUES(?server_id, ?prefix)", null, server_id, prefix);
        }

        /// <summary>
        /// Ignores this code if table exists.
        /// </summary>
        /// <param name="sqlCode">valid sql code</param>
        public static void TryCreateTable(string sqlCode)
        {
            if (instance.info == null) return;

            MySqlConnection connection = new MySqlConnection(instance.info.GetConnectionString());
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {sqlCode}";
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}


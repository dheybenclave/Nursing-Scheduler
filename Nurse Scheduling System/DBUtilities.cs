﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Microsoft.Win32;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Nurse_Scheduling_System
{
    class ConnectionSetUp
    {
        public void CreateRegistryKey(string key, string value)
        {
            try
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software", true);
                regKey.CreateSubKey("Nursing Scheduler");
                regKey = regKey.OpenSubKey("Nursing Scheduler", true);
                regKey.SetValue(key, value);
            }
            catch (Exception e)
            {
               
            }
        }

        public string ReadRegistryKey(string Key, string passkey)
        {
            string retVal = "";

            try
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software", true);
                regKey.CreateSubKey("Nursing Scheduler");
                regKey = regKey.OpenSubKey("Nursing Scheduler", true);
                retVal = regKey.GetValue(Key).ToString();
                retVal = AES_Decrypt(retVal, passkey);

            }
            catch (Exception e)
            { retVal = ""; }
            return retVal;
        }

        public string ConvertToBytes(string value)
        {
            string retVal = "";

            try
            {
                byte[] arr = Encoding.ASCII.GetBytes(value);
                foreach (byte b in arr)
                {
                    retVal += b.ToString();
                }
            }
            catch (Exception ex)
            { return value; }

            return retVal;
        }

        public string AES_Encrypt(string clearText, string passkey)
        {
            RijndaelManaged AES = new RijndaelManaged();
            MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider();
            string encryptedString = "";

            try
            {
                byte[] hash = new byte[32];
                byte[] temp = hashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(passkey));
                Array.Copy(temp, 0, hash, 0, 16);
                Array.Copy(temp, 0, hash, 15, 16);
                AES.Key = hash;
                AES.Mode = CipherMode.ECB;
                ICryptoTransform DESEncrypter = AES.CreateEncryptor();
                byte[] buffer = ASCIIEncoding.ASCII.GetBytes(clearText);
                encryptedString = Convert.ToBase64String
                    (DESEncrypter.TransformFinalBlock(buffer, 0, buffer.Length));

            }
            catch (Exception e)
            { return ""; }
            return encryptedString;
        }
        public string decryptedString;
        public string AES_Decrypt(string encrypted, string passkey)
        {
            RijndaelManaged AES = new RijndaelManaged();
            MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider();
            decryptedString = "";

            try
            {

                byte[] hash = new byte[32];
                byte[] temp = hashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(passkey));
                Array.Copy(temp, 0, hash, 0, 16);
                Array.Copy(temp, 0, hash, 15, 16);
                AES.Key = hash;
                AES.Mode = CipherMode.ECB;
                ICryptoTransform DESdecrypter = AES.CreateDecryptor();
                byte[] buffer = Convert.FromBase64String(encrypted);
                decryptedString = ASCIIEncoding.ASCII.GetString
                    (DESdecrypter.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch (Exception e)
            { return ""; }
            return decryptedString;
        }

    }

    class Db_Utilities
    {
        public bool pass = false;
        public int ifedit = 0;
        MySqlConnection connection;
        MySqlDataAdapter adapter;
        DataTable table;
        ConnectionSetUp ConnSetup = new ConnectionSetUp();
        public string server, port, database, username, password;

        public void SaveSettings()
        {
            string ConnString = "SERVER = " + server + ";" +
                            "PORT = " + port+ ";" +
                            "DATABASE = " + database + ";" +
                            "UID = " + username + ";" +
                            "PASSWORD = " + password+ ";";           
  
                ConnSetup.CreateRegistryKey("ConnString", ConnSetup.AES_Encrypt(ConnString, "password"));
               
        }

        public string GetReadRegistryKey()
        {
            string temp = null;

            try
            {
                temp = ConnSetup.ReadRegistryKey("ConnString", "password");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
            return temp;
        }

        public MySqlConnection OpenConnection()
        {
            connection = new MySqlConnection();
            try
            {
                connection.ConnectionString = GetReadRegistryKey();
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                return null;  
            }
        }

        public void CloseConnection()
        {
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public DataTable QuerySelect(string query)
        {
            try
            {
                adapter = new MySqlDataAdapter(query, OpenConnection());
                table = new DataTable();
                adapter.Fill(table);
                adapter.Dispose();
                return table;
            }
            catch (Exception e) { return null; }
        }
    }
}

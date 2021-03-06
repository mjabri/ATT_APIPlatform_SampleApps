// <copyright file="Default.aspx.cs" company="AT&amp;T">
// Licensed by AT&amp;T under 'Software Development Kit Tools Agreement.' 2013
// TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION: http://developer.att.com/sdk_agreement/
// Copyright 2013 AT&amp;T Intellectual Property. All rights reserved. http://developer.att.com
// For more information contact developer.support@att.com
// </copyright>

#region References

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;



#endregion

/// <summary>
/// MMSApp3_Listener class
/// </summary>
public partial class MMSApp3_Listener : System.Web.UI.Page
{
    /// <summary>
    /// Event, that triggers when the applicaiton page is loaded into the browser
    /// Listens to server and stores the mms messages in server
    /// </summary>
    /// <param name="sender">object, that caused this event</param>
    /// <param name="e">Event that invoked this function</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        FileStream fileStream = null;
        try
        {
            Random random = new Random();
            DateTime currentServerTime = DateTime.UtcNow;

            string receivedTime = currentServerTime.ToString("HH-MM-SS");
            string receivedDate = currentServerTime.ToString("MM-dd-yyyy");

            string inputStreamContents;
            int stringLength;
            int strRead;

            Stream stream = Request.InputStream;
            stringLength = Convert.ToInt32(stream.Length);

            byte[] stringArray = new byte[stringLength];
            strRead = stream.Read(stringArray, 0, stringLength);
            inputStreamContents = System.Text.Encoding.UTF8.GetString(stringArray);

            string[] splitData = Regex.Split(inputStreamContents, "</SenderAddress>");
            string data = splitData[0].ToString();
            string senderAddress = inputStreamContents.Substring(data.IndexOf("tel:") + 4, data.Length - (data.IndexOf("tel:") + 4));
            string[] parts = Regex.Split(inputStreamContents, "--Nokia-mm-messageHandler-BoUnDaRy");
            string[] lowerParts = Regex.Split(parts[2], "BASE64");
            string[] imageType = Regex.Split(lowerParts[0], "image/");
            int indexOfSemicolon = imageType[1].IndexOf(";");
            string type = imageType[1].Substring(0, indexOfSemicolon);
            UTF8Encoding encoder = new System.Text.UTF8Encoding();
            Decoder utf8Decode = encoder.GetDecoder();

            byte[] todecode_byte = Convert.FromBase64String(lowerParts[1]);

            if (!Directory.Exists(Request.MapPath(ConfigurationManager.AppSettings["ImageDirectory"])))
            {
                Directory.CreateDirectory(Request.MapPath(ConfigurationManager.AppSettings["ImageDirectory"]));
            }
            string detailsToStore = "tel:" + senderAddress + ":-:" + receivedDate + " At " + receivedTime + "UTC";
            string fileNameToSave = "From_" + senderAddress.Replace("+", "") + "_At_" + receivedTime + "_UTC_On_" + receivedDate + random.Next();
            detailsToStore = detailsToStore + ":-:" + fileNameToSave + "." + type + ":-:" + "Test Subject";
            fileStream = new FileStream(Request.MapPath(ConfigurationManager.AppSettings["ImageDirectory"]) + fileNameToSave + "." + type, FileMode.CreateNew, FileAccess.Write);
            fileStream.Write(todecode_byte, 0, todecode_byte.Length);
            WriteRecordToFile(detailsToStore);
        }
        catch
        { }
        finally
        {
            if (null != fileStream)
            {
                fileStream.Close();
            }
        }

    }

    /// <summary>
    /// Method to update file.
    /// </summary>
    /// <param name="transactionid">Transaction Id</param>
    /// <param name="merchantTransactionId">Merchant Transaction Id</param>
    public void WriteRecordToFile(string value)
    {
        try
        {
            string imageDetails = Request.MapPath(ConfigurationManager.AppSettings["ImageDirectory"]) + "imageDetails.txt";
            int NumOfFilesToStoreAndDisplay = 5;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["NumOfFilesToStoreAndDisplay"]))
                NumOfFilesToStoreAndDisplay = Convert.ToInt32(ConfigurationManager.AppSettings["NumOfFilesToStoreAndDisplay"].ToString());
            List<string> list = new List<string>();
            FileStream file = new FileStream(imageDetails, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                list.Add(line);
            }

            sr.Close();
            file.Close();

            if (list.Count > NumOfFilesToStoreAndDisplay)
            {
                int diff = list.Count - NumOfFilesToStoreAndDisplay;
                list.RemoveRange(0, diff);
            }

            if (list.Count == NumOfFilesToStoreAndDisplay)
            {
                list.RemoveAt(0);
                //delete file too.
            }
            list.Add(value);
            using (StreamWriter sw = File.CreateText(imageDetails))
            {
                int tempCount = 0;
                while (tempCount < list.Count)
                {
                    string lineToWrite = list[tempCount];
                    sw.WriteLine(lineToWrite);
                    tempCount++;
                }
                sw.Close();
            }
        }
        catch (Exception ex)
        {
            return;
        }
    }
}
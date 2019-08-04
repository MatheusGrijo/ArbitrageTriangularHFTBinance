using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public static class Http
{
    


    public static string get(String url)
    {
        try
        {
            //if (url.IndexOf("binance") > 0)

            
                    
            Console.WriteLine("[" + DateTime.Now.ToString() + "] - " + url);            
            String r = "";
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 5000;
            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var responseStream = httpWebResponse.GetResponseStream();
            if (responseStream != null)
            {
                var streamReader = new StreamReader(responseStream);
                r = streamReader.ReadToEnd();
            }
            if (responseStream != null) responseStream.Close();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[" + DateTime.Now.ToString() + "] - " + "OK!");
            Console.ForegroundColor = ConsoleColor.White;
           
            return r;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message + ex.StackTrace + "||||||" + url);
            Console.ForegroundColor = ConsoleColor.White;
            return null;
        }
    }

}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ReleaseWork
{
    class Program
    {
        private static Regex factorWorkLine = new Regex(@"Factor=([0-9a-fA-F\-N/]+,)?(\d+),\d+,\d+");
        private static void Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine(@"Arguments: {0} Username Password [workToDo file, default MISFITWorkToDo.txt]
Example: {0} Mini-Geek 123456 worktodo.txt

This program will unreserve work from GPU72. It gets the exponents from the Factor lines in the workToDo file, and ignores all other lines.", AppDomain.CurrentDomain.FriendlyName);
                return;
            }
            string userName = args[0];
            string password = args[1];
            string workToDoFile = args.ElementAtOrDefault(2) ?? "MISFITWorkToDo.txt";

            ReleaseWorkGPUto72(GetExponents(workToDoFile), userName, password);
        }
        private static IEnumerable<int> GetExponents(string workToDoFile)
        {
            foreach (var line in File.ReadLines(workToDoFile))
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var match = factorWorkLine.Match(line);
                    if (match.Success)
                    {
                        yield return int.Parse(match.Groups[2].Value);
                    }
                }
            }
        }
        private static bool ReleaseWorkGPUto72(IEnumerable<int> numbers, string userid, string password)
        {
            numbers = numbers.ToList();
            Console.WriteLine("Unreserving " + string.Join(", ", numbers));
            HttpWebRequest web = (HttpWebRequest)HttpWebRequest.Create("https://www.gpu72.com/account/assignments/");
            web.Method = WebRequestMethods.Http.Post;
            web.ContentType = "application/x-www-form-urlencoded";
            web.Accept = "text/html, application/xhtml+xml, */*";
            web.UserAgent = "MINI-GEEK-TF-RELEASER";
            web.Headers.Add("Accept-Language: en-US");

            SetBasicAuthHeader(web, userid, password);

            string postData = "Action=Unreserve&Confirm=on&" + string.Join("&", numbers.Select(x => string.Format("Exp{0}_l=on", x)));

            web.ContentLength = postData.Length;

            using (Stream requestStream = web.GetRequestStream())
            {
                StreamWriter reqWriter = new StreamWriter(requestStream);
                reqWriter.Write(postData.ToString());
                reqWriter.Flush();
            }

            // Read the response
            try
            {
                HttpWebResponse response = (HttpWebResponse)web.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Request successful");
                    return true;
                }
                else
                {
                    Console.Error.WriteLine("Received status code " + response.StatusCode);
                    using (StreamReader responseStream = new StreamReader(response.GetResponseStream()))
                    {
                        Console.Error.WriteLine(responseStream.ReadToEnd());
                    }
                    return false;
                }
            }
            catch (WebException ex)
            {
                Console.Error.WriteLine("Exception calling web site");
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }
        private static void SetBasicAuthHeader(HttpWebRequest req, string userid, string password)
        {
            string authInfo = userid + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;
        }
    }
}

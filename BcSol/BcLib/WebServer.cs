using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

namespace BcLib
{
    public class WebServer
    {
        public WebServer(BlockChain chain)
        {
            var settings = ConfigurationManager.AppSettings;
            string host = settings["host"]?.Length > 1 ? settings["host"] : "localhost";
            string port = settings["port"]?.Length > 1 ? settings["port"] : "12345";


            WriteDebug($"Endpoint : http://{host}:{port}/");

            var server = new TinyWebServer.WebServer(request =>
            {
                string path = request.Url.PathAndQuery.ToLower();
                string query = "";
                string json = "";

                if (path.Contains("?"))
                {
                    string[] parts = path.Split('?');
                    path = parts[0];
                    query = parts[1];
                }
                WriteDebug($"{request.HttpMethod} {path}");

                switch (path)
                {
                    case "/mine":
                       
                        return chain.Mine();
                    case "/transactions/new":
    
                        if (request.HttpMethod != HttpMethod.Post.Method)
                        {
                            return $"{new HttpResponseMessage(HttpStatusCode.MethodNotAllowed)}";
                        }
                        else
                        {
                            json = new StreamReader(request.InputStream).ReadToEnd();
                            Transaction trx = JsonConvert.DeserializeObject<Transaction>(json);
                            int blockId = chain.CreateTransaction(trx.Sender, trx.Recipient, trx.Amount);
                            return $"Your Transaction will be included in Block {blockId}";
                        }
                    case "/chain":
                       
                        return chain.GetFullChain();
                    case "/nodes/register":
                     
                        if (request.HttpMethod != HttpMethod.Post.Method)
                        {
                            return $"{new HttpResponseMessage(HttpStatusCode.MethodNotAllowed)}";
                        }
                        else
                        {
                            json = new StreamReader(request.InputStream).ReadToEnd();
                            var urlList = new { Urls = new string[0] };
                            var obj = JsonConvert.DeserializeAnonymousType(json, urlList);
                            return chain.RegisterNodes(obj.Urls);

                        }
                    case "/nodes/resolve":
                      
                        return chain.Consensus();
                       
                }

                return "";

            },
                $"http://{host}:{port}/mine/",
                $"http://{host}:{port}/transactions/new/",
                $"http://{host}:{port}/chain/",
                $"http://{host}:{port}/nodes/register/",
                $"http://{host}:{port}/nodes/resolve/"
            );

            server.Run();

            
        }

        public static void WriteDebug(string msg)
        {

            Debug.WriteLine($"{DateTime.Now.ToShortTimeString()}: {msg}");

        }
    }
}

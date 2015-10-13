using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TicketEditor
{
    class HttpRequestAdapter
    {
    
        public static bool sendHttpPost(String xmlString,MainWindow.SelectedPrinter printer)
        {

            BackgroundWorker backgroundworker = new BackgroundWorker();

            backgroundworker.WorkerReportsProgress = true;
            backgroundworker.WorkerSupportsCancellation = true;

            backgroundworker.DoWork += new DoWorkEventHandler(bw_Request);
            backgroundworker.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            backgroundworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            backgroundworker.RunWorkerAsync(new string[] {xmlString,printer.Address});

           
            return false;

        }

        private static void bw_Request(object sender, DoWorkEventArgs e)
        {
            string[] argstring = e.Argument as string[];
            byte[] bytes = Encoding.UTF8.GetBytes(argstring[0]);

            
            string url = "http://" + argstring[1];

            try
            {
                WebRequest webreq = (HttpWebRequest)WebRequest.Create(url);

                webreq.Method = "POST";
                webreq.ContentLength = bytes.Length;
                webreq.ContentType = "qticket";

                webreq.Timeout = 15000;
                
                
                    
                Stream dataStream = webreq.GetRequestStream();

                dataStream.Write(bytes, 0, bytes.Length);




                WebResponse response = (HttpWebResponse)webreq.GetResponse();

                Debug.WriteLine(((HttpWebResponse)response).StatusDescription);

                StreamReader sr = new StreamReader(response.GetResponseStream());

                dataStream.Close();

                Debug.WriteLine("Response: " + sr.ReadToEnd());

                var backgroundworker = sender as BackgroundWorker;

                backgroundworker.ReportProgress(100, sr.ReadToEnd());
              

            }
            catch (SocketException exception)
            {

                var backgroundworker = sender as BackgroundWorker;

                backgroundworker.ReportProgress(0, exception.ToString());
                Debug.WriteLine(exception.ToString());

               


            }
        }

        private static void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private static void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }

        private static void DoWorkEventHandler(object obj)
        {



        }
    }
}

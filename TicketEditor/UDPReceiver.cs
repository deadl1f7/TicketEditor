using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;

//2015-10-01 Martin Backudd, Listens to UDP from the V8, receives the Ip name and port from the device.




namespace TicketEditor
{
    class UDPReceiver
    {
        

        int port;
        Thread thread;
        bool done = false;
        public Dictionary<string,string> ConnectionDictionary;

        MainWindow MainWindowInstance;


        public UDPReceiver(int port,MainWindow main)
        {


            MainWindowInstance = main;
            this.port = port;
            ConnectionDictionary = new Dictionary<string, string>();
            thread = new Thread(new ThreadStart(listenForConnection));
            thread.Start();
            

          
        }

        public void stopReceive()
        {
            done = true;
            thread.Abort();


        }



        public void listenForConnection()
        {



            UdpClient client = new UdpClient(port);

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

            string connectionstring = "";
            done = false;

            try
            {
                while (!done)
                {
                    
                    byte[] bytes = client.Receive(ref groupEP);

                    Debug.WriteLine("Received broadcast from {0} :\n {1}\n" + groupEP.ToString() + ": " + 
                        Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                    connectionstring = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    string name = connectionstring.Split(',')[0].Trim();
                    string address = connectionstring.Split(',')[1].Trim();
                    MainWindow.SelectedPrinter printer = new MainWindow.SelectedPrinter();
                    printer.Name = name;
                    printer.Address = address;
                   

                    if (!MainWindowInstance.Devices.Contains(printer))
                    {
                        MainWindowInstance.Devices.Add(printer);
                        MainWindowInstance.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                    new Action(() => MainWindowInstance.UpdateDeviceListView()));

                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                client.Close();

            }
            
        }




    }










}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicketEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 2015-09-29 Martin Backudd
    /// Browse the tickets in /Templates.
    /// Sends the xml through http to the printer.
    /// 
    /// 
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public struct SelectedPrinter
            {
                

                public string Name { get; set; }
                public string Address { get; set; }

            }

    public TicketClass SelectedTicket;

        UDPReceiver UdpReceiver;

        public List<SelectedPrinter> Devices;

        TicketManager TicketList;

        DeviceListView DeviceView;

        public SelectedPrinter Printer;

        public MainWindow()
        {
            InitializeComponent();
       
            PopulateUIList();

            Devices = new List<SelectedPrinter>();
   
            UdpReceiver = new UDPReceiver(8000,this);

            this.Closing += view_Closing;



            if (Properties.Settings.Default.LastPrinter.Length > 0)
            {
                Printer = new SelectedPrinter();
                Printer.Name = Properties.Settings.Default.LastPrinter.Split(',')[0];
                Printer.Address = Properties.Settings.Default.LastPrinter.Split(',')[1];
                this.Title = "Printer: " + Printer.Name;

            }
           
        }



        private void view_Closing(object sender, CancelEventArgs e)
        {

            UdpReceiver.stopReceive();
            
        }




        /// <summary>
        /// Initiates TicketManager, interface for reading the xml files and managing the Tickets. add new tickets, values to use for displaying tickets.
        /// </summary>

        private void PopulateUIList()
        {


           
                TicketList = new TicketManager();
            
                var grid = new GridView();

                this.listView.View = grid;
                this.listView.PreviewMouseRightButtonUp += new MouseButtonEventHandler(listView_RightClickElement);
                this.listView.MouseDoubleClick += new MouseButtonEventHandler(listView_DoubleClick);

                GridViewColumn namecolumn = new GridViewColumn();
                namecolumn.DisplayMemberBinding = new Binding("Name");
                namecolumn.Header = "Name";

                grid.Columns.Add(namecolumn);



                GridViewColumn datecolumn = new GridViewColumn();
                Binding datebinding = new Binding("Date");
                datebinding.ConverterCulture = CultureInfo.CurrentCulture;
                datecolumn.DisplayMemberBinding = datebinding;
                datecolumn.Header = "Created";
                

                grid.Columns.Add(datecolumn);
            
                

                this.listView.ItemsSource = TicketList;
            
            

        }


        //Logic for engaging the browser/list
        private void listView_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listviewelement = sender as ListView;

            int index = listviewelement.SelectedIndex;
            if (index > -1)
            {
                SelectedTicket = TicketList.ElementAt(index);
                TicketEditorView editor = new TicketEditorView(SelectedTicket,this);
                editor.Show();
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listView_RightClickElement(object sender, MouseButtonEventArgs e)
        {

            var listviewelement = sender as ListView;

            int index = listviewelement.SelectedIndex;
            if (index > -1)
            {
                SelectedTicket = TicketList.ElementAt(index);
            }
          

        }

        private void new_ContextMenu(object sender, RoutedEventArgs e)
        {
            TicketEditorView editor = new TicketEditorView(this);
            editor.Show();

        }


        private void print_ContextMenu(object sender, RoutedEventArgs e)
        {

            RefreshTicketList();

            if (Printer.Address.Length > 0)
            {

                HttpRequestAdapter.sendHttpPost(SelectedTicket.Xml, Printer);
            }

        }

        private void delete_ContextMenu(object sender, RoutedEventArgs e)
        {

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Remove the template?", "Remove", System.Windows.MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                TicketList.DeleteTicket(SelectedTicket);
                RefreshTicketList();
            }

        }

        private void edit_ContextMenu(object sender, RoutedEventArgs e)
        {

            TicketEditorView editor = new TicketEditorView(SelectedTicket,this);
            editor.Show();

        }

        private void menu_File(object sender, RoutedEventArgs e)
        {

            var menu = sender as MenuItem;     

            switch (menu.Name)
            {
                case "new":
                    TicketEditorView editor = new TicketEditorView(this);
                    editor.Show();


                    break;

                case "refresh":
                    RefreshTicketList();
                    break;

                case "close":
                    this.Close();
                    break;
            }
            
        }
              
        

        public void RefreshTicketList()
        {
            
            TicketList.Clear();
            TicketList.ReadListFromDir();
            
            this.listView.Items.Refresh();
            
        }
                      
            
        
        public void UpdateDeviceListView()
        {
                   
            if (DeviceView != null && DeviceView.IsVisible)
            {

                DeviceView.Update();

            }

        }

        public void showDeviceList()
        {
            DeviceView = new DeviceListView(this);
            DeviceView.Title = "Select Printer";
            DeviceView.Show();
            
        }

        private void menu_Printing(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;

            switch (menu.Name)
            {
                case "devices":
                    showDeviceList();
                    break;

            }
        }
    }

}

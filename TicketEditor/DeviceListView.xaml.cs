using System.Linq;
using System.Windows;
using System.Windows.Input;
namespace TicketEditor
{
    /// <summary>
    /// Interaction logic for DeviceListView.xaml
    /// Shows the V8 printers available.
    /// </summary>
    public partial class DeviceListView : Window
    {


        MainWindow MainWindowInstance;
  

        public DeviceListView(MainWindow main)
        {
            InitializeComponent();

            MainWindowInstance = main;
                        
            this.listBox.ItemsSource = MainWindowInstance.Devices;
            this.MouseDoubleClick += ItemsControl_MouseDoubleClick;
            
        }
        
        public void Update()
        {
                      
            this.listBox.Items.Refresh();
            
        }

        private void ItemsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            MainWindow.SelectedPrinter printer = MainWindowInstance.Devices.ElementAt(this.listBox.SelectedIndex);
            MainWindowInstance.Printer = printer;
            Properties.Settings.Default.LastPrinter = printer.Name + "," + printer.Address;
            Properties.Settings.Default.Save();
            MainWindowInstance.Title = "Printer: " + printer.Name;
            this.Hide();
        }


        

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TicketEditor.Editor
{
    /// <summary>
    /// Interaction logic for PromptView.xaml
    /// </summary>
    public partial class PromptView : Window
    {
        TicketEditorView tew;



        public PromptView(TicketEditorView parent)
        {
            InitializeComponent();
            this.tew = parent;
            
        }

        private void prompt_Yes_Click(object sender, RoutedEventArgs e)
        {
            tew.Save();
            this.Close();
            tew.Close();

        }
        private void prompt_No_Click(object sender, RoutedEventArgs e)
        {

            tew.TicketChanged = false;
            this.Close();
            tew.Close();
        }
        private void prompt_Cancel_Click(object sender, RoutedEventArgs e)
        {

            this.Close();


        }
    }
}

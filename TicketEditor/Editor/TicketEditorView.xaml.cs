using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TicketEditor.Editor;
using System.ComponentModel;


namespace TicketEditor
{
    /// <summary>
    /// 2015-09-29 Martin Backudd
    /// Displaying the values of the Lines that represent a ticket. The ticketclass contains the LineParameters, 1 lineparameter describes 1 line/image/queuenumber on the printer
    /// Customizable through moveable textboxes. 
    /// 
    /// </summary>
    /// 




    public partial class TicketEditorView : Window
    {
              

        private bool moving;
        private bool editing;
        private bool spacing;

        public bool TicketChanged = false;

        const string QUEUENUMBER_LINE = "queuenumber";
        const string TEXT_LINE = "text";
        const string IMAGE_LINE = "image";
        const string ROWSPACE_LINE = "rowspace";


        //lineheight in printer is 24 Dots
        static double LINE_HEIGHT = 24.0D;
        //Win DPI 96, printers DPI 203. Factor to make a accurate description of what the ticket will look like.
        static double DOT_PIXEL_RELATION = 203.0D / 96.0D;

        static int WIDTH_53_MM = 384;
        static int WIDTH_80_MM = 564;


        public string CodePage { get; set; }
        public string Text;


        private Point OriginInObject;

        private UIElement Element;

        private TicketClass Ticket;

        private CodePageList CodePages;

        public MainWindow MainWindowInstance;

        
        //Reads the lineparameters from the ticket, and displays the visually depending on type of print-line.

        public TicketEditorView(TicketClass ticket,MainWindow maininstance)
        {
            InitializeComponent();
            

            Ticket = ticket;

            Ticket.Flipped = 0;
            Ticket.PaperMargin = 0;


            if (Ticket.PaperWidth == 564) {
                stackPanel.Width = 323;
            }


            foreach (TicketClass.LineParameters lineparameters in ticket.Lines)
            {

                switch (lineparameters.Type)
                {

                    case IMAGE_LINE:

                        AddImage(lineparameters);



                        break;

                    case ROWSPACE_LINE:

                        AddLineBreak(lineparameters,-1);


                        break;



                    case TEXT_LINE:

                        AddText(lineparameters);


                        break;

                    case QUEUENUMBER_LINE:

                        AddQueueNumber(lineparameters);


                        break;

                }

     
            }
            MainWindowInstance = maininstance;
                        
            CodePages = new CodePageList();
            
            LanguageMenu.ItemsSource = CodePages;

          
            setMouseEvents();
            DisableButtons();

        }

        

        //View with a fresh new ticket
        public TicketEditorView(MainWindow maininstance)
        {
            InitializeComponent();
            setMouseEvents();


            Ticket = new TicketClass();
           
            MainWindowInstance = maininstance;
            
            AddDummyText();
            DisableButtons();


            CodePages = new CodePageList();


            LanguageMenu.ItemsSource = CodePages;


        }


        /// <summary>
        /// Mousedown on an element to grab the control of it, mouse move transforms the view to make it move.
        /// Mousedown outside of the element to stop transforming.
        /// doubleclick to edit.
        /// </summary>

        private void setMouseEvents()
        {

            moving = false;
            editing = false;
            stackPanel.MouseMove += mouse_Move;
            stackPanel.PreviewMouseLeftButtonDown += view_MouseLeftDown;
            stackPanel.PreviewMouseUp += release_Control_Element;

            this.Closing += view_Closing;


        }


        //textbox element, queuenumber begins to edit. 

        private void edit_Queuenumber(object sender, MouseButtonEventArgs e)
        {
            var element = sender as TextBox;
            Element = element;
            editing = true;

            EnableButtons();

            if (Ticket.Lines.ElementAt(stackPanel.Children.IndexOf(Element)).Type.Equals(QUEUENUMBER_LINE))
            {

                Double_Height.IsEnabled = false;
                Double_Width.IsEnabled = false;

                Double_Width.IsChecked = false;
                Double_Height.IsChecked = false;
                sizeBox.IsEnabled = true;
            }
            else
            {

                sizeBox.IsEnabled = false;

            }

            Bold.IsChecked = (element.FontWeight.Equals(FontWeights.Bold)) ? true : false;
            Underline.IsChecked = (element.TextDecorations.Contains(TextDecorations.Underline[0])) ? true : false;



            spacing = false;
            GetSpaceValues(Element);
            spacing = true;

            sizeBox.Text = Convert.ToString(element.FontSize);



            spaceLabel.Visibility = Visibility.Visible;
            topBox.Visibility = Visibility.Visible;
            bottomBox.Visibility = Visibility.Visible;

            element.Cursor = Cursors.IBeam;

            element.Focusable = true;
            element.Focus();


        }


        //When we want to release the focus of an element or clicks the bitmap image

        private void view_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {


            TicketChanged = true;


            //Release all focus
            if (!sender.GetType().Equals(typeof(Image)))
            {
                Keyboard.ClearFocus();

                DisableButtons();
                editing = false;
                foreach (UIElement tb in stackPanel.Children)
                {

                    TextBox text = tb as TextBox;
                    if (text != null)
                    {
                        text.Cursor = Cursors.Arrow;
                        text.Focusable = false;
                    }

                }
            }


            //Focusing the image 
            else
            {

                Keyboard.ClearFocus();


                DisableButtons();

                shiftDownButton.IsEnabled = true;
                shiftUpButton.IsEnabled = true;
                justifyLeft.IsEnabled = true;
                justifyRight.IsEnabled = true;
                justifyCenter.IsEnabled = true;
                bottomBox.IsEnabled = true;
                topBox.IsEnabled = true;
                sizeBox.IsEnabled = false;

                Element = sender as UIElement;
                spacing = false;
                GetSpaceValues((Image)sender);
                spacing = true;
                var image = sender as Image;

                spaceLabel.Visibility = Visibility.Visible;
                topBox.Visibility = Visibility.Visible;
                bottomBox.Visibility = Visibility.Visible;

                int index = stackPanel.Children.IndexOf(Element);


                double scale = 0;
                if (Ticket.Lines[index].BitImage != null)
                    scale = (image.ActualHeight * image.ActualWidth / 2) / (Ticket.Lines[index].BitImage.Height * Ticket.Lines[index].BitImage.Width / 2);


                if (scale > 0)

                    sizeBox.Text = Convert.ToString(scale);
                else
                    sizeBox.Text = "1";


                sizeBox.Visibility = Visibility.Visible;
                sizeBox.PreviewMouseDown += this.edit_Size;
                sizeBox.Focus();


            }

        }



        //Focusing the smaller text elements.

        private void grab_Control_Element(object sender, MouseButtonEventArgs e)
        {

            Element = sender as UIElement;

            if (!editing)
            {

                spacing = false;
                GetSpaceValues(Element);
                spacing = true;

                if (sender.GetType().Equals(typeof(TextBox)))
                {
                    moving = true;
                    OriginInObject = e.GetPosition((UIElement)sender);



                    EnableButtons();
                    sizeBox.IsEnabled = false;
                    var textbox = sender as TextBox;

                    var transform = Element.RenderTransform as TransformGroup;
                    var rtransform = transform.Children[0] as TranslateTransform;

                    var scale = transform.Children[1] as ScaleTransform;
                    if (Element.RenderSize.Width * scale.ScaleX < stackPanel.RenderSize.Width - 24.0D)
                    {
                        textbox.HorizontalAlignment = HorizontalAlignment.Left;
                        rtransform.X = -((OriginInObject.X - e.GetPosition(stackPanel).X) / scale.ScaleX);
                    }
                    if (scale.ScaleX > 1)
                    {
                        double half = (OriginInObject.X * scale.ScaleX) - (Element.RenderSize.Width / scale.ScaleX);

                        OriginInObject = new Point(half, OriginInObject.Y);
                    }

                    Element.RenderTransformOrigin = new Point(1 / scale.ScaleX, 1 / scale.ScaleY);
                    Double_Width.IsChecked = (scale.ScaleX == 2) ? true : false;
                    Double_Height.IsChecked = (scale.ScaleY == 2) ? true : false;
                    Bold.IsChecked = (textbox.FontWeight == FontWeights.Bold) ? true : false;

                    Underline.IsChecked = (textbox.TextDecorations.Contains(TextDecorations.Underline[0])) ? true : false;



                }



            }

        }

        //release focus of element

        private void release_Control_Element(object sender, MouseButtonEventArgs e)
        {
            if (moving)
            {

                moving = false;
                OriginInObject = new Point();


            }
        }


        //Logic for making the transformations that will look like the dragging effect.

        private void mouse_Move(object sender, MouseEventArgs e)
        {


            if (moving)
            {
                TranslateTransform trans = Element.RenderTransform as TranslateTransform;
                ScaleTransform scale = Element.RenderTransform as ScaleTransform;


                TransformGroup tg = new TransformGroup();


                if (!Element.RenderTransform.GetType().Equals(typeof(TransformGroup)))
                {



                    trans = new TranslateTransform();
                    tg.Children.Add(trans);

                    if (scale != null)
                    {
                        tg.Children.Add(scale);


                    }
                    Element.RenderTransform = tg;


                }
                else
                {
                    tg = Element.RenderTransform as TransformGroup;
                    trans = tg.Children[0] as TranslateTransform;
                    scale = tg.Children[1] as ScaleTransform;

                    Element.RenderTransform = tg;
                }

                double EditorMargin = 12.0D;


                //Make limits so that you cant drag out of bounds.
                if (Element.RenderSize.Width * scale.ScaleX < stackPanel.RenderSize.Width - 24.0D)
                {

                    double scaledwidth = (scale.ScaleX > 1) ? Element.RenderSize.Width + Element.RenderSize.Width / scale.ScaleX : Element.RenderSize.Width;

                    double offsetstart = (scale.ScaleX > 1) ? Element.RenderSize.Width / scale.ScaleX : 0;

                    if (((e.GetPosition(stackPanel).X - OriginInObject.X - offsetstart) > EditorMargin) && ((e.GetPosition(stackPanel).X < stackPanel.ActualWidth - (scaledwidth - OriginInObject.X) - EditorMargin)))
                        trans.X = -((OriginInObject.X - e.GetPosition(stackPanel).X) / scale.ScaleX);
                }

            }


        }



        //Logic for closing the view when we are done, prompts if changes have been made. 

        private void view_Closing(object sender, CancelEventArgs e)
        {


            if (TicketChanged)
            {
                PromptView Prompt = new PromptView(this);
                Prompt.Show();
                e.Cancel = true;
            }
            else
            {
                
                e.Cancel = false;
                
            }

            MainWindowInstance.RefreshTicketList();


        }


        ///<summary>
        /// To add a line with text. add events to the element to receive input when dragging and letting go.
        /// 
        /// Make transformation for the double width double height setting that the printer takes.
        /// 
        /// 
        /// </summary>

        private void AddText(TicketClass.LineParameters lineparameters)
        {

            
            double size = 24.0D / DOT_PIXEL_RELATION;

            

            TextBox textbox = new TextBox();
            textbox.PreviewMouseLeftButtonDown += this.grab_Control_Element;
            textbox.MouseDoubleClick += this.edit_Queuenumber;
            textbox.PreviewMouseLeftButtonUp += this.release_Control_Element;
            
            textbox.Text = lineparameters.Characters.Trim();
            textbox.FontWeight = (lineparameters.Bold == 1) ? FontWeights.Bold : FontWeights.Normal;
            textbox.FontFamily = new FontFamily("Open Symbol");
            
            textbox.HorizontalAlignment = (HorizontalAlignment)lineparameters.Alignment;

            textbox.FontSize = size;//PointFromPixel(size);
                     
            textbox.Padding = GetPadding(textbox.FontFamily.LineSpacing,textbox.FontSize);

            double FactorX = (lineparameters.Width == 1) ? 2 : 1;
            double FactorY = (lineparameters.Height == 1) ? 2 : 1;

            ScaleTransform scale = new ScaleTransform(FactorX,FactorY);

            TranslateTransform translatetransform = new TranslateTransform();

            TransformGroup transformgroup = new TransformGroup();

            transformgroup.Children.Add(translatetransform);

            
            
            transformgroup.Children.Add(scale);

            double transformorigin =1;

            if (lineparameters.Alignment == 0)
                transformorigin = 0;
            else if (lineparameters.Alignment == 1)
                transformorigin = 0.5D;
            else if (lineparameters.Alignment == 2)
                transformorigin = 1D;

            textbox.RenderTransformOrigin = new Point(transformorigin, 1/FactorY);
            
            textbox.RenderTransform = transformgroup;

            double TranslatedOffset = ((double)lineparameters.OffsetX / (double)Ticket.PaperWidth * (double)stackPanel.Width);

            translatetransform.X = TranslatedOffset / FactorX;

            Thickness margin = new Thickness(0, 0, 0, lineparameters.LineSpace / DOT_PIXEL_RELATION);

            textbox.Margin = margin;

            textbox.Tag = Ticket.Lines.IndexOf(lineparameters);


            stackPanel.Children.Add(textbox);
            textbox.HorizontalAlignment = (HorizontalAlignment)lineparameters.Alignment;

            sizeBox.IsEnabled = false;
            



        }



        ///<summary>
        /// the linebreak is made out of an invisible separator. 
        /// 
        /// </summary>

        private void AddLineBreak(TicketClass.LineParameters lineparameters,int index)
        {


            
            Separator separator = new Separator();
            separator.PreviewMouseLeftButtonDown += this.grab_Control_Element;
            

            separator.Height = lineparameters.Space/ DOT_PIXEL_RELATION;
            
            separator.Tag = Ticket.Lines.IndexOf(lineparameters);



            if (index == -1)
            {
                stackPanel.Children.Add(separator);
            }
            else
            {
                stackPanel.Children.Insert(index, separator);


            }
        
         
         

        }


        /// <summary>
        /// Add queuenumber is same as text but can only be aligned since the bitmaps(how the queuenumber is drawn in the printer) cant be offset in the printer.
        /// </summary>
        /// <param name="lineparameters"></param>

        private void AddQueueNumber(TicketClass.LineParameters lineparameters)
        {

            TextBox textbox = new TextBox();
            
            
            textbox.FontSize = lineparameters.Size;
            textbox.Text = lineparameters.Characters.Trim();
      

            textbox.FontWeight = (lineparameters.Bold == 1) ? FontWeights.Bold : FontWeights.Normal;

            textbox.FontFamily = new FontFamily("Lucida Sans Unicode");

            textbox.PreviewMouseDown += this.edit_Queuenumber;

            double calculatedpadding = -(textbox.FontFamily.LineSpacing * textbox.FontSize / 5D);

            Thickness padding = new Thickness(0, calculatedpadding, 0, calculatedpadding);


            textbox.VerticalContentAlignment = VerticalAlignment.Center;

            textbox.Padding = padding;

            textbox.HorizontalAlignment = (HorizontalAlignment)lineparameters.Alignment; // FIX ALIGNMENT

            Thickness margin = new Thickness(0, 0, 0, -LINE_HEIGHT/DOT_PIXEL_RELATION);

            textbox.Margin = margin;

            textbox.Tag = Ticket.Lines.IndexOf(lineparameters);

            stackPanel.Children.Add(textbox);


            StackPanel.SetZIndex(textbox, -1);

            
        }

        private void AddImage(TicketClass.LineParameters lineparameters)
        {

            Image img = new Image();

            img.Source = lineparameters.BitImage;

            img.HorizontalAlignment = (HorizontalAlignment)lineparameters.Alignment;

            img.Stretch = Stretch.None;
            
            img.PreviewMouseDown += this.view_MouseLeftDown;
            
            Thickness margin = new Thickness(0,0, 0, LINE_HEIGHT / DOT_PIXEL_RELATION / 2);

            img.Margin = margin;
                    
            img.Tag = Ticket.Lines.IndexOf(lineparameters);

            stackPanel.Children.Add(img);
            

        }

        /// <summary>
        /// Logic for adding the parameters for describing a line in the xml. we add a new line when we want to add a new element in the editor.
        /// type can be 0 text, 1 queuenumber, 2 image, 3 linebreaks
        /// </summary>
        /// <param name="type"></param>


        private void AddEmptyLine(int type)
        {

         

            switch (type)
            {
                case 0:

                    Ticket.AddLine(TEXT_LINE, "0", "0", "0", "0", "1", "0", "0", "0", "Text");
                    AddText(Ticket.Lines.Last());

                   




                    break;
                case 1:

                  
                    Ticket.AddLine(QUEUENUMBER_LINE, "Open Symbol", "1","80","1","0","A001");

                    AddQueueNumber(Ticket.Lines.Last());

                    break;
                case 2:



                    string path = pickFile();
                    if (path != "")
                    {
                        BitmapImage bitmapimage = new BitmapImage(new Uri(path));

                        FormatConvertedBitmap fcb = new FormatConvertedBitmap();
                        fcb.BeginInit();
                        fcb.Source = bitmapimage;
                        fcb.DestinationFormat = PixelFormats.BlackWhite;
                        fcb.EndInit();
                        System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        
                        encoder.Frames.Add(BitmapFrame.Create(fcb));
                        byte[] bytes;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            encoder.Save(ms);
                            bytes = ms.ToArray();
                        }
                        
                        string base64string = Convert.ToBase64String(bytes);

                        Ticket.AddLine(IMAGE_LINE, "1", "0", "30", base64string);
                        AddImage(Ticket.Lines.Last());



                    }
                    break;
                case 3:
                    
                    Ticket.AddLine(ROWSPACE_LINE,"30");
                    AddLineBreak(Ticket.Lines.Last(),-1);

                    break;


            }

            
        }

        //make a fresh ticket

        private void AddDummyText()
        {
            Ticket.Flipped = 0;
            Ticket.PaperMargin = 0;
            Ticket.PaperWidth = WIDTH_53_MM;
            Ticket.AddLine(ROWSPACE_LINE, "24");
            Ticket.AddLine(TEXT_LINE,"1", "1", "1", "0", "1", "0", "0", "0", "Nemo-Q");
            Ticket.AddLine(ROWSPACE_LINE, "200");
            Ticket.AddLine(QUEUENUMBER_LINE,"Open Symbol","1","70","1","0","A001");
            Ticket.AddLine(ROWSPACE_LINE, "200");
            AddLineBreak(Ticket.Lines[0], 0);
            AddText(Ticket.Lines[1]);
            AddLineBreak(Ticket.Lines[2], 2);
            AddQueueNumber(Ticket.Lines[3]);
            AddLineBreak(Ticket.Lines[4], 4);


        }

     

        //Save without prompting, if theres no name already, show picker.

        public void Save()
        {

            SaveTicket();
            if ((Ticket.FilePath == null))
                SaveAs();
            else
            {
                TicketManager.WriteXml(Ticket);
                TicketChanged = false;
                
            }

            MainWindowInstance.RefreshTicketList();

        }

        //show the picker and save to that particular name.
        public void SaveAs()
        {

            SaveTicket();
            string savepath = GetSavePath();

            if (savepath.Length > 0)
            {

                Ticket.FilePath = savepath;
            }
            if (Ticket.FilePath.Length != 0)
            {
                TicketManager.WriteXml(Ticket);
                TicketChanged = false;
            }
            else
                TicketChanged = true;

        }


        //The selections under file in the menubar.
        private void menu_File(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as MenuItem;
            if (menuitem.Header.Equals("_Save"))
            {


                Save();


            }
            else if (menuitem.Header.Equals("_Save As"))
            {


                SaveAs();


            }
            else if (menuitem.Header.Equals("_New"))
            {


                stackPanel.Children.Clear();

                Ticket = new TicketClass();
                
               

                DisableButtons();

                AddDummyText();
                TicketChanged = true;



            }
            else if (menuitem.Header.Equals("_Print"))
            {

                SaveTicket();
                TicketManager.GetXml(Ticket);
                if (MainWindowInstance.Printer.Address != null)
                {
                    HttpRequestAdapter.sendHttpPost(Ticket.Xml, MainWindowInstance.Printer);
                }
                else
                    MainWindowInstance.showDeviceList();

            }
            else if (menuitem.Header.Equals("_Close"))
            {
                if (TicketChanged)
                {
                    PromptView Prompt = new PromptView(this);
                    Prompt.Show();
                }
                else
                    this.Close();


            }


        }

        private void menu_Width(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as MenuItem;
            if (menuitem.Name.Equals("Option53mm"))
            {
                Ticket.PaperWidth = WIDTH_53_MM;
                stackPanel.Width = 210;

            }

            else
            {
                Ticket.PaperWidth = WIDTH_80_MM;
                stackPanel.Width = 320;
                
            }
            UpdateLayout();
            
        }


        private void menu_SetCodePage(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
       
            var menuitem = e.OriginalSource as MenuItem;
            menuitem.IsChecked = true;
            dynamic codepage = menuitem.DataContext;
            
            Ticket.CodePage = codepage.CpNumber;

        }
        
        /// <summary>
        /// Logic for the styling of the textboxes and the images.
        /// The printer takes, double height double width bold underline.
        /// </summary>
        
        
        private void menu_Style(object sender, RoutedEventArgs e)
        {

            var togglebutton = sender as ToggleButton;
            var textbox = Element as TextBox;


            //is the button checked or unchecked???
            int state = (togglebutton.IsChecked == true) ? 2 : 1;
            int tag = Convert.ToInt16(togglebutton.Tag);


            int index = stackPanel.Children.IndexOf(Element);

            if ((index > -1)&&Ticket.Lines.ElementAt(index).Type.Equals(TEXT_LINE))
            {


                var transform = Element.RenderTransform as TransformGroup;

                var scale = transform.Children[1] as ScaleTransform;

                var transformgroup = Element.RenderTransform as TransformGroup;




                switch (tag)
                {   //Bold
                    case 0:
                        textbox.FontWeight = (state == 1) ? FontWeights.Normal : FontWeights.Bold;
                        break;
                    //underline
                    case 1:
                        if (state == 2)
                        {
                            var underline = TextDecorations.Underline;
                            textbox.TextDecorations.Add(underline);
                        }
                        else
                        {
                            var underline = TextDecorations.Underline;
                            textbox.TextDecorations.Remove(underline[0]);
                        }
                        break;

                        //double width;
                    case 2:


                        Element.RenderTransformOrigin = new Point(1 / scale.ScaleX, 1 / scale.ScaleY);
                        scale.ScaleX = state;



                        break;
                    //double height;
                    case 3:
                        
                        

                        double originx = (scale.ScaleX == 2) ? 0.5D : 0.5D;
                        
                        double originy = (scale.ScaleY == 2) ? 0.5D : 0.25D;

                        Element.RenderTransformOrigin = new Point(originx,originy);
                        scale.ScaleY = state;

                        break;


                }






            }
            //Styling for the queuenumber big textbox.

            else if ((index > -1)&&Ticket.Lines.ElementAt(index).Type.Equals(QUEUENUMBER_LINE))
            {

                switch (tag)
                {//bold
                    case 0:
                        textbox.FontWeight = (state == 1) ? FontWeights.Normal : FontWeights.Bold;
                        break;
                        //underline
                    case 1:
                          if (state == 2)
                        {
                            var underline = TextDecorations.Underline;
                            textbox.TextDecorations.Add(underline);
                        }
                        else
                        {
                            var underline = TextDecorations.Underline;
                            textbox.TextDecorations.Remove(underline[0]);
                        }
                            break;



                }




            }


        }
        

        //Add a new element, select the type in the combobox.
        private void menu_Add(object sender,RoutedEventArgs e)
        {
            

            AddEmptyLine(comboBox.SelectedIndex);
            TicketChanged = true;

        }


        //Removing a element from the stackpanel and logic for at the same time removing the lineparameter from the ticketList that will end up in the xml.

        private void removeLine_Click(object sender, RoutedEventArgs e)
        {
            spacing = false;
            if(Element != null)
            {

                int index = stackPanel.Children.IndexOf(Element);

                if (index > -1)
                {
                    int nextseparatorindex = index + 1;
                    if ((nextseparatorindex < stackPanel.Children.Count) && stackPanel.Children[nextseparatorindex].GetType().Equals(typeof(Separator)))
                    {
                        Ticket.Lines.RemoveAt(nextseparatorindex);
                        stackPanel.Children.RemoveAt(nextseparatorindex);
                    }
                    
                        

                    

                    Ticket.Lines.RemoveAt(index);
                    stackPanel.Children.Remove(Element);

                    int prevseparatorindex = index - 1;

                    if ((prevseparatorindex > -1) && stackPanel.Children[prevseparatorindex].GetType().Equals(typeof(Separator)))
                    {


                        Ticket.Lines.RemoveAt(index - 1);
                        stackPanel.Children.RemoveAt(index - 1);
                       
                    }

                }

                sizeBox.IsEnabled = false;
                topBox.Text = "";
                bottomBox.Text  = "";




            }

            DisableButtons();

            spacing = true;

        }




        //Set the justification of the element. change the transform if we use doubleheight/width
        private void justify_Click(object sender, RoutedEventArgs e)
        {

            
            dynamic element = Element;
            if (Element != null)
            {
                var button = sender as Button;

                int tag = Convert.ToInt16(button.Tag);

                var transformgroup = Element.RenderTransform as TransformGroup;

                


                element.HorizontalAlignment = (HorizontalAlignment)tag;

                if (transformgroup != null)
                {
                    var translatetransform = transformgroup.Children[0] as TranslateTransform;
                    double renderoriginx = 1;
                    if (tag == 0)
                        renderoriginx = 0;
                    else if (tag == 1)
                        renderoriginx = 0.5D;
                    else if (tag == 2)
                        renderoriginx = 1D;



                        Element.RenderTransformOrigin = new Point(renderoriginx, Element.RenderTransformOrigin.Y);
                    translatetransform.X = 0;

                 


                }
                


            }



        }


    

        //The size have been changed, update size of queuenumber or the image.

        private void sizeBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var sizebox = sender as TextBox;
            
            double sizeval;
            
           
                if (Element.GetType().Equals(typeof(TextBox)))
                {



                    if (Double.TryParse(sizebox.Text, out (sizeval)))
                    {

                        var numbox = Element as TextBox;
                        numbox.FontSize = sizeval;

                    }
                    else
                    {
                        sizebox.Text = "";

                    }
                }
                else
                {
                    if (double.TryParse(sizebox.Text, out (sizeval)))
                    {


                        var img = Element as Image;

                        int elementindex = stackPanel.Children.IndexOf(img);

                        double size = Convert.ToDouble(sizeval);
                        if(size == 1)
                        {
                            
                            BitmapImage bitimage = Ticket.Lines[elementindex].BitImage;
                            img.Source = bitimage;
                        }
                        else if (size > 0)
                        {


                            BitmapImage bitimage = Ticket.Lines[elementindex].BitImage;
                            ImageSource imgsource = bitimage;
                            BitmapFrame bf = CreateResizedImage(imgsource, (int)(size * bitimage.Width), (int)(size * bitimage.Height), 0);
                            img.Source = bf;

                        }
                        else
                        {



                        }

                    }
                
            }
         
            

        }


        //the spacing changed. 

        private void space_Changed(object sender, RoutedEventArgs e)
        {
            //If the user changes the values instead not the ui-behaviour.
            if (spacing)
            {

                var valuebox = sender as TextBox;
                double val;
                bool parsed = Double.TryParse(valuebox.Text, out val);
                if (parsed)
                {
                    val = val / DOT_PIXEL_RELATION;

                }



                


                int indexofelement = stackPanel.Children.IndexOf(Element);

                //are we changing the space to the next element?
                if (valuebox.Name.Equals("bottomBox"))
                {


                    if ((indexofelement + 1) == stackPanel.Children.Count)
                    {

                      


                        
                        string spacestring = Convert.ToString((int)val);


                        Ticket.AddLine(ROWSPACE_LINE, spacestring);
                        AddLineBreak(Ticket.Lines.Last(),-1);

                    }

                    if (!stackPanel.Children[indexofelement+1].GetType().Equals(typeof(Separator)))
                    {


                        int numrows = ((int)val)/256   + 1;
                        int rest = ((int)val % 256);



                        // if theres no separator insert one into the tickets lineparameters and the stackpanel.
                        Ticket.InsertLineBreak(ROWSPACE_LINE, indexofelement + 1, (int)val);
                        AddLineBreak(Ticket.Lines.ElementAt(indexofelement+1),indexofelement+1);



                    }

                    else
                    {
                        var separator = stackPanel.Children[indexofelement + 1] as Separator;
                        if (val == 0)
                        {

                            stackPanel.Children.Remove(separator);
                            Ticket.Lines.RemoveAt(indexofelement + 1);

                        }
                        else
                        {
                          
                            separator.Height = val;
                        }

                    }

                }

                //are we changing the space to the previous element?

                else
                {
                    if (indexofelement == 0)
                    {
                        
                        Ticket.InsertLineBreak(ROWSPACE_LINE, indexofelement, (int)val);
                        AddLineBreak(Ticket.Lines.ElementAt(indexofelement),indexofelement);

                    }
                    else
                    {

                        if (!stackPanel.Children[indexofelement - 1].GetType().Equals(typeof(Separator)))
                        {
                            
                            Ticket.InsertLineBreak(ROWSPACE_LINE, indexofelement, (int)val);
                            AddLineBreak(Ticket.Lines.ElementAt(indexofelement),indexofelement);
                            
                        }
                        else
                        {
                            var separator = stackPanel.Children[indexofelement - 1] as Separator;

                            if (val == 0)
                            {

                                stackPanel.Children.Remove(separator);
                                Ticket.Lines.RemoveAt(indexofelement - 1);
                                
                            }
                            else
                            {
                                separator.Height = val;
                            }
                        }
                    }

                }
            }

        }


        //Sets the space box, this still fires the space_changed so setting spacing to false before calling this to not make any changes to the elements
        private void GetSpaceValues(UIElement element)
        {
            
            int indexofelement = stackPanel.Children.IndexOf(element);

            if ((indexofelement - 1) < 0)
            {
                topBox.Text = "";
            }
            else
            {

                var topseparator = stackPanel.Children[indexofelement - 1] as Separator;

                if(topseparator != null)
                {

                    double val = topseparator.Height * DOT_PIXEL_RELATION;
                    topBox.Text = Convert.ToString((int)val) ;

                }
                else
                {

                    topBox.Text = "";

                }

            }


            if ((indexofelement + 1) != stackPanel.Children.Count)
            {
                var botseparator = stackPanel.Children[indexofelement + 1] as Separator;

                if (botseparator != null)
                {
                    double val = botseparator.Height * DOT_PIXEL_RELATION;

                    bottomBox.Text = Convert.ToString((int)val);


                }
                else
                {

                    bottomBox.Text = "";


                }

            }
            else
            {

                bottomBox.Text = "";


            }

        }
        

        private void edit_Size(object sender, MouseButtonEventArgs e)
        {
            TextBox sizebox = sender as TextBox;
            sizebox.Visibility = Visibility;
 
           
        }


        //Scale the bitmap
        private static BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin)
        {
            var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         
                96, 96,                
                PixelFormats.Default); 
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }


        //Browse for an image
        private void browseImage_Click(object sender, RoutedEventArgs e)
        {

            var image = Element as Image;
            if (pickFile().Length > 0)
            {
                BitmapImage bitmapimage = new BitmapImage(new Uri(pickFile()));
                if (bitmapimage != null)
                {
                    image.Source = bitmapimage;
                    int index = (int)image.Tag;
                    TicketClass.LineParameters lineparams = Ticket.Lines[index];
                    lineparams.BitImage = bitmapimage;

                    Ticket.Lines.RemoveAt(index);
                    Ticket.Lines.Insert(index, lineparams);
                }
            }

        }

        //Disable all stlying buttons when we dont have any element focused.
        private void DisableButtons()
        {



            justifyLeft.IsEnabled = false;
            justifyCenter.IsEnabled = false;
            justifyRight.IsEnabled = false;
            Bold.IsEnabled = false;
            Underline.IsEnabled = false;
            Double_Height.IsEnabled = false;
            Double_Width.IsEnabled = false;
            Bold.IsChecked = false;
            Underline.IsChecked = false;
            Double_Width.IsChecked = false;
            Double_Height.IsChecked = false;
            sizeBox.IsEnabled = false;
            topBox.IsEnabled = false;
            bottomBox.IsEnabled = false;
            shiftUpButton.IsEnabled = false;
            shiftDownButton.IsEnabled = false;


        }


        //enable the styling buttons when we have an element in focus.
        private void EnableButtons()
        {
            Bold.IsEnabled = true;
            Underline.IsEnabled = true;
            Double_Height.IsEnabled = true;
            Double_Width.IsEnabled = true;

            justifyLeft.IsEnabled = true;
            justifyCenter.IsEnabled = true;
            justifyRight.IsEnabled = true;


            sizeBox.IsEnabled = true;
            topBox.IsEnabled = true;
            bottomBox.IsEnabled = true;

            shiftDownButton.IsEnabled = true;
            shiftUpButton.IsEnabled = true;

        }



        //Move a Line up in the list both visually and in the tickets list for the xml to be written later.
        private void shiftUp_Click(object sender, RoutedEventArgs e)
        {

            int currentindex = stackPanel.Children.IndexOf(Element);

            if(currentindex > 0)
            {


                int prevelementindex = currentindex - 1;


                while(stackPanel.Children[prevelementindex].GetType().Equals(typeof(Separator)) && (prevelementindex-1 > -1))
                {


                    prevelementindex--;
                    

                }


                var prevelement = stackPanel.Children[prevelementindex];
                
                    if (prevelementindex > -1 && (!stackPanel.Children[prevelementindex].GetType().Equals(typeof(Separator))))
                     {
                   
                        stackPanel.Children.Remove(Element);

                        stackPanel.Children.Insert(prevelementindex, Element);
                      

                        stackPanel.Children.Remove(prevelement);

                        stackPanel.Children.Insert(currentindex, prevelement);
                    Ticket.SwapLines(currentindex, prevelementindex);
                    
                    
                    }
                }

            

            spacing = false;
            GetSpaceValues(Element);
            spacing = true;


        }
        //Move a Line down in the list both visually and in the tickets list for the xml to be written later.
        private void shiftDown_Click(object sender, RoutedEventArgs e)
        {

            int currentindex = stackPanel.Children.IndexOf(Element);

            if (currentindex < (stackPanel.Children.Count-1))
            {



                int nextelementindex = currentindex +1 ;


                while (stackPanel.Children[nextelementindex].GetType().Equals(typeof(Separator)))
                {



                    nextelementindex++;

                    if(nextelementindex == (stackPanel.Children.Count))
                    {

                        return;
                    }


                }


                var nextelement = stackPanel.Children[nextelementindex];

                if (nextelementindex < (stackPanel.Children.Count))
                {
                   

                    stackPanel.Children.Remove(nextelement);
                    stackPanel.Children.Insert(currentindex, nextelement);
                    

                    stackPanel.Children.Remove(Element);
                    stackPanel.Children.Insert(nextelementindex, Element);
                    Ticket.SwapLines(currentindex, nextelementindex);


                }




            }
            spacing = false;
            GetSpaceValues(Element);
            spacing = true;



        }

        //Pick a file 
        private string pickFile()
        {

            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            
            openFileDialog.InitialDirectory = (Properties.Settings.Default.LastFolder.Length > 0) ? Properties.Settings.Default.LastFolder : System.AppDomain.CurrentDomain.BaseDirectory + "Templates";          

            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            string path = openFileDialog.FileName;
            if (path.Length > 0)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(path);
                Properties.Settings.Default.LastFolder = dirinfo.Parent.FullName;
                Properties.Settings.Default.Save();
            }


            return path;



        }


        //save file 
        private string GetSavePath()
        {


            SaveFileDialog saveFileDialog = new SaveFileDialog();


            saveFileDialog.InitialDirectory = (Properties.Settings.Default.LastFolder.Length > 0) ? Properties.Settings.Default.LastFolder : System.AppDomain.CurrentDomain.BaseDirectory + "Templates";

            saveFileDialog.Filter = "Xml(*.xml) | *.xml";

            saveFileDialog.FilterIndex = 0;




            saveFileDialog.ShowDialog();

            
            string savepath = saveFileDialog.FileName;
            

            return (savepath.Length != 0) ? savepath : "";
            


        }


        
        double PointFromPixel(double pixel)
        {

            return pixel * 72 / 96;
        }

        //Make the padding for the textboxes.
        Thickness GetPadding(double linespace, double fontsize)
        {
            double calculatedpadding = -(linespace * fontsize / 4);

            Thickness padding = new Thickness(0, calculatedpadding, 0, calculatedpadding);

            

            return padding;


        }

        //Logic for looping through the stackpanel, and translating the values of the elements to tickets lineparameters
        private void SaveTicket()
        {

           
            
            foreach(var element in stackPanel.Children)
            {
                int index = stackPanel.Children.IndexOf((UIElement)element);

                TicketClass.LineParameters lineparams = Ticket.Lines[index];

                if (element.GetType().Equals(typeof(Separator)))
                {

                    

                    var separator = element as Separator;

                    double dheight = separator.Height * DOT_PIXEL_RELATION;
                    int intheight = (int)dheight;

                    lineparams.Space = intheight;

                    Ticket.Lines.RemoveAt(index);
                    Ticket.Lines.Insert(index, lineparams);

                    
                        


                }

                else if (element.GetType().Equals(typeof(TextBox)))
                {

                    var textbox = element as TextBox;

 
                    //lineparams.CharacterSpace

                    lineparams.Characters = textbox.Text;
                    lineparams.Alignment = (int)textbox.HorizontalAlignment;
                    
        

                    if (lineparams.Type.Equals(TEXT_LINE))
                    {
                        lineparams.LineSpace = (int)(textbox.Margin.Bottom * DOT_PIXEL_RELATION);

                        if (textbox.HorizontalAlignment == HorizontalAlignment.Left)
                        {
                            int offsetx = (int)textbox.TranslatePoint(new Point(0, 0), stackPanel).X;
                            double viewRelation = (double)offsetx / stackPanel.ActualWidth;
                            double offsetTranslated = viewRelation * Ticket.PaperWidth;
                            lineparams.OffsetX = (int)offsetTranslated;


                        }
                        else
                        {
                            lineparams.OffsetX = 0;
                        }

                        var transformgroup = textbox.RenderTransform as TransformGroup;
                        var scale = transformgroup.Children[1] as ScaleTransform;

                        lineparams.Width = (scale.ScaleX == 2) ? 1 : 0;
                        lineparams.Height = (scale.ScaleY == 2) ? 1 : 0;
                    }
                    else
                    {

                        //lineparams.Font = textbox.FontFamily;
                        
                        lineparams.Size = (int)textbox.FontSize;
                        

                    }


                    lineparams.Bold = (textbox.FontWeight == FontWeights.Bold) ? 1 : 0;
                    lineparams.Underline = (textbox.TextDecorations.Contains(TextDecorations.Underline[0])) ? 1 : 0;


                    Ticket.Lines.RemoveAt(index);
                    Ticket.Lines.Insert(index, lineparams);



                }
                else
                {

                    var image = element as Image;

                    var bitimage = image.Source as BitmapImage;

                    lineparams.BitImage = bitimage ;
                    lineparams.Alignment = (int)image.HorizontalAlignment;


                    Ticket.Lines.RemoveAt(index);
                    Ticket.Lines.Insert(index, lineparams);

                }
              

            }


            



        }

       
    }





}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace TicketEditor
{
    /// <summary>
    /// Class for managing the tickets, reading from xml and making easy access to the values that the editor needs.
    /// </summary>
    /// 



    class TicketManager : List<TicketClass>
    {


        const string QUEUENUMBER_LINE = "queuenumber";
        const string TEXT_LINE = "text";
        const string IMAGE_LINE = "image";
        const string ROWSPACE_LINE = "rowspace";

        private static string saveDir = Directory.GetCurrentDirectory() + "\\" + "Templates\\";

        private static string FileExtension = ".xml";

        public TicketManager()
        {            
            ReadListFromDir();         

        }

        //Save file
        public void SaveTicket(TicketClass ticket)
        {
            
            if (!Directory.Exists(saveDir)) 
                Directory.CreateDirectory(saveDir);
            
            File.WriteAllText(saveDir + ticket.Name + FileExtension, ticket.Xml);

            this.Add(ticket);
            
        }
        
        public void UpdateTicket(TicketClass ticket,TicketClass oldticket)
        {
            
            this.Remove(oldticket);
            File.Delete(oldticket.FilePath);
            
            this.Add(ticket);
            File.WriteAllText(saveDir + ticket.Name + FileExtension, ticket.Xml);
            
        }

        public void DeleteTicket(TicketClass ticket) {
            
            File.Delete(ticket.FilePath);
            this.Remove(ticket);
            

        }
        

        public TicketClass ReadTicketFromFile(string path)
        {
            
            TicketClass ticket = new TicketClass();

            ticket.Name = Path.GetFileNameWithoutExtension(path);

            ticket.Date = Directory.GetCreationTime(path);

            ticket.FilePath = path;

            ticket.Xml = ReadFile(path);          

            this.Add(ticket);

            return ticket;



        }

        //Read all the tickets from dir to this class's list.
        public List<TicketClass> ReadListFromDir()
        {

            string[] tickets = new string[0];

            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);
            else
                tickets = Directory.GetFiles(saveDir, "*.xml");

            if (tickets.Length > 0)
            {
                foreach (string ticketPath in tickets)
                {

                    TicketClass ticket = new TicketClass();

                    ticket.Name = Path.GetFileNameWithoutExtension(ticketPath);

                    ticket.Date = Directory.GetCreationTime(ticketPath);

                    ticket.FilePath = ticketPath;

                    ticket.Xml = ReadFile(ticketPath);



                    this.Add(ticket);

                }
                XmlToLineList();
                return this;
            }
            else
                return null;
                       


        }
        //sort 
        public void SortByName() {


            this.OrderBy(TicketClass => TicketClass.Name).ToList();

        }


        //read a certain file
    private static string ReadFile(string filepath)
        {
         

            try
            {
                using (StreamReader sr = new StreamReader(filepath))
                {
                    string xmlstring = sr.ReadToEnd();
                    return xmlstring;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error reading file:" + filepath + "Exception: " + ex.ToString());
                return "";
            }
            

        }


        public void WriteXml(TicketClass ticket, string path)
        {

            LinesToXml(ticket, path);
            
        }

        public static void WriteXml(TicketClass ticket)
        {
            LinesToXml(ticket, ticket.FilePath);
            
        }


        //Reads the lines thats stored into this class and makes elements with values/texts.
        private static void LinesToXml(TicketClass ticket,string path)
        {

            XmlDocument xmldoc = new XmlDocument();
            
            XmlDeclaration xmlDeclaration = xmldoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xmldoc.DocumentElement;
            xmldoc.InsertBefore(xmlDeclaration, root);

            //(2) string.Empty makes cleaner code
            XmlElement xmlbody = xmldoc.CreateElement(string.Empty, "qticket", string.Empty);

            XmlElement settings = xmldoc.CreateElement(string.Empty, "ticketsettings", string.Empty);

            XmlElement margin = xmldoc.CreateElement(string.Empty, "margin", string.Empty);
            margin.InnerText = "0";

            XmlElement width = xmldoc.CreateElement(string.Empty, "width", string.Empty);
            width.InnerText = Convert.ToString(ticket.PaperWidth);

            XmlElement flipped = xmldoc.CreateElement(string.Empty, "flipped", string.Empty);
            flipped.InnerText = "0";



            XmlElement codepage = xmldoc.CreateElement(string.Empty, "codepage", string.Empty);
            codepage.InnerText = Convert.ToString(ticket.CodePage);


            settings.AppendChild(margin);
            settings.AppendChild(width);
            settings.AppendChild(flipped);
            settings.AppendChild(codepage);
            xmlbody.AppendChild(settings);


            foreach (TicketClass.LineParameters lineparams in ticket.Lines)
            {



                XmlElement linenode = xmldoc.CreateElement(string.Empty, lineparams.Type, string.Empty);

                switch (lineparams.Type) {
                    case IMAGE_LINE:


                        XmlElement alignmentelement = xmldoc.CreateElement(string.Empty, "alignment", string.Empty);
                        alignmentelement.InnerText = lineparams.Alignment.ToString();

                        XmlElement linespacenode = xmldoc.CreateElement(string.Empty, "linespace", string.Empty);
                        linespacenode.InnerText = lineparams.LineSpace.ToString();

                        linenode.AppendChild(alignmentelement);

                        linenode.AppendChild(linespacenode);

                        XmlCDataSection base64Cdata = xmldoc.CreateCDataSection(lineparams.base64Image);
                        XmlElement base64node = xmldoc.CreateElement(string.Empty, "base64", string.Empty);

                        base64node.AppendChild(base64Cdata);
                        linenode.AppendChild(base64node);

                        break;
                    case TEXT_LINE:


                        XmlElement widthnode = xmldoc.CreateElement(string.Empty, "width", string.Empty);
                        widthnode.InnerText = lineparams.Width.ToString();

                        XmlElement heightnode = xmldoc.CreateElement(string.Empty, "height", string.Empty);
                        heightnode.InnerText = lineparams.Height.ToString();

                        XmlElement boldnode = xmldoc.CreateElement(string.Empty, "bold", string.Empty);
                        boldnode.InnerText = lineparams.Bold.ToString();

                        XmlElement underlinenode = xmldoc.CreateElement(string.Empty, "underline", string.Empty);
                        underlinenode.InnerText = lineparams.Underline.ToString();

                        linenode.AppendChild(widthnode);
                        linenode.AppendChild(heightnode);
                        linenode.AppendChild(boldnode);
                        linenode.AppendChild(underlinenode);

                        alignmentelement = xmldoc.CreateElement(string.Empty, "alignment", string.Empty);
                        alignmentelement.InnerText = lineparams.Alignment.ToString();

                        XmlElement offsetnode = xmldoc.CreateElement(string.Empty, "offsetx", string.Empty);
                        offsetnode.InnerText = lineparams.OffsetX.ToString();

                        linespacenode = xmldoc.CreateElement(string.Empty, "linespace", string.Empty);
                        linespacenode.InnerText = lineparams.LineSpace.ToString();

                        XmlElement characterspacenode = xmldoc.CreateElement(string.Empty, "characterspace", string.Empty);
                        characterspacenode.InnerText = lineparams.CharacterSpace.ToString();

                        linenode.AppendChild(alignmentelement);
                        linenode.AppendChild(offsetnode);
                        linenode.AppendChild(linespacenode);
                        linenode.AppendChild(characterspacenode);

                        XmlElement charactersnode = xmldoc.CreateElement(string.Empty, "characters", string.Empty);
                        charactersnode.InnerText = lineparams.Characters;

                        linenode.AppendChild(charactersnode);

                        break;

                    case QUEUENUMBER_LINE:


                        

                        XmlElement fontnode = xmldoc.CreateElement(string.Empty, "font", string.Empty);
                        fontnode.InnerText = lineparams.Font.ToString();

                        XmlElement alignmentnode = xmldoc.CreateElement(string.Empty, "alignment", string.Empty);
                        alignmentnode.InnerText = lineparams.Alignment.ToString();

                        XmlElement sizenode = xmldoc.CreateElement(string.Empty, "size", string.Empty);
                        sizenode.InnerText = lineparams.Size.ToString();


                        boldnode = xmldoc.CreateElement(string.Empty, "bold", string.Empty);
                        boldnode.InnerText = lineparams.Bold.ToString();

                        underlinenode = xmldoc.CreateElement(string.Empty, "underline", string.Empty);
                        underlinenode.InnerText = lineparams.Underline.ToString();


                        charactersnode = xmldoc.CreateElement(string.Empty, "characters", string.Empty);
                        charactersnode.InnerText = lineparams.Characters;


                        linenode.AppendChild(fontnode);
                        linenode.AppendChild(alignmentnode);
                        linenode.AppendChild(sizenode);
                        linenode.AppendChild(boldnode);
                        linenode.AppendChild(underlinenode);
                        linenode.AppendChild(charactersnode);

                    

                        break;

                    case ROWSPACE_LINE:
                                                                        
                        linenode.InnerText = lineparams.Space.ToString();
                
                        break;


                 }
                
                xmlbody.AppendChild(linenode);
                
            }
                       
            xmldoc.AppendChild(xmlbody);
            
            ticket.Xml = xmldoc.OuterXml;

            xmldoc.Save(ticket.FilePath);
            
        }

        //Makes xml out of the line values.
        public static string GetXml(TicketClass ticket)
        {

            XmlDocument xmldoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmldoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xmldoc.DocumentElement;
            xmldoc.InsertBefore(xmlDeclaration, root);

            //(2) string.Empty makes cleaner code
            XmlElement xmlbody = xmldoc.CreateElement(string.Empty, "qticket", string.Empty);

            XmlElement settings = xmldoc.CreateElement(string.Empty, "ticketsettings", string.Empty);

            XmlElement margin = xmldoc.CreateElement(string.Empty, "margin", string.Empty);
            margin.InnerText = "0";

            XmlElement width = xmldoc.CreateElement(string.Empty, "width", string.Empty);
            width.InnerText = Convert.ToString(ticket.PaperWidth);

            XmlElement flipped = xmldoc.CreateElement(string.Empty, "flipped", string.Empty);
            flipped.InnerText = "0";

            XmlElement codepage = xmldoc.CreateElement(string.Empty, "codepage", string.Empty);
            codepage.InnerText = Convert.ToString(ticket.CodePage);


            settings.AppendChild(margin);
            settings.AppendChild(width);
            settings.AppendChild(flipped);
            settings.AppendChild(codepage);
            xmlbody.AppendChild(settings);

            foreach (TicketClass.LineParameters lineparams in ticket.Lines)
            {



                XmlElement linenode = xmldoc.CreateElement(string.Empty, lineparams.Type, string.Empty);

                switch (lineparams.Type)
                {
                    case IMAGE_LINE:                     

                        XmlElement alignmentelement = xmldoc.CreateElement(string.Empty, "alignment", string.Empty);
                        alignmentelement.InnerText = lineparams.Alignment.ToString();

                        XmlElement linespacenode = xmldoc.CreateElement(string.Empty, "linespace", string.Empty);
                        linespacenode.InnerText = lineparams.LineSpace.ToString();
                                           

                        linenode.AppendChild(alignmentelement);

                        linenode.AppendChild(linespacenode);

                        XmlCDataSection base64Cdata = xmldoc.CreateCDataSection(lineparams.base64Image);
                        XmlElement base64node = xmldoc.CreateElement(string.Empty, "base64", string.Empty);

                        base64node.AppendChild(base64Cdata);
                        linenode.AppendChild(base64node);

                        break;
                    case TEXT_LINE:

                                        
                        XmlElement widthnode = xmldoc.CreateElement(string.Empty, "width", string.Empty);
                        widthnode.InnerText = lineparams.Width.ToString();

                        XmlElement heightnode = xmldoc.CreateElement(string.Empty, "height", string.Empty);
                        heightnode.InnerText = lineparams.Height.ToString();

                        XmlElement boldnode = xmldoc.CreateElement(string.Empty, "bold", string.Empty);
                        boldnode.InnerText = lineparams.Bold.ToString();

                        XmlElement underlinenode = xmldoc.CreateElement(string.Empty, "underline", string.Empty);
                        underlinenode.InnerText = lineparams.Underline.ToString();

                        linenode.AppendChild(widthnode);
                        linenode.AppendChild(heightnode);
                        linenode.AppendChild(boldnode);
                        linenode.AppendChild(underlinenode);

                        alignmentelement = xmldoc.CreateElement(string.Empty, "alignment", string.Empty);
                        alignmentelement.InnerText = lineparams.Alignment.ToString();

                        XmlElement offsetnode = xmldoc.CreateElement(string.Empty, "offsetx", string.Empty);
                        offsetnode.InnerText = lineparams.OffsetX.ToString();

                        linespacenode = xmldoc.CreateElement(string.Empty, "linespace", string.Empty);
                        linespacenode.InnerText = lineparams.LineSpace.ToString();

                        XmlElement characterspacenode = xmldoc.CreateElement(string.Empty, "characterspace", string.Empty);
                        characterspacenode.InnerText = lineparams.CharacterSpace.ToString();

                        linenode.AppendChild(alignmentelement);
                        linenode.AppendChild(offsetnode);
                        linenode.AppendChild(linespacenode);
                        linenode.AppendChild(characterspacenode);

                        XmlElement charactersnode = xmldoc.CreateElement(string.Empty, "characters", string.Empty);
                        charactersnode.InnerText = lineparams.Characters;

                        linenode.AppendChild(charactersnode);

                        break;

                    case QUEUENUMBER_LINE:




                        XmlElement fontnode = xmldoc.CreateElement(string.Empty, "font", string.Empty);
                        fontnode.InnerText = lineparams.Font.ToString();

                        XmlElement alignmentnode = xmldoc.CreateElement(string.Empty, "alignment", string.Empty);
                        alignmentnode.InnerText = lineparams.Alignment.ToString();

                        XmlElement sizenode = xmldoc.CreateElement(string.Empty, "size", string.Empty);
                        sizenode.InnerText = lineparams.Size.ToString();


                        boldnode = xmldoc.CreateElement(string.Empty, "bold", string.Empty);
                        boldnode.InnerText = lineparams.Bold.ToString();

                        underlinenode = xmldoc.CreateElement(string.Empty, "underline", string.Empty);
                        underlinenode.InnerText = lineparams.Underline.ToString();


                        charactersnode = xmldoc.CreateElement(string.Empty, "characters", string.Empty);
                        charactersnode.InnerText = lineparams.Characters;


                        linenode.AppendChild(fontnode);
                        linenode.AppendChild(alignmentnode);
                        linenode.AppendChild(sizenode);
                        linenode.AppendChild(boldnode);
                        linenode.AppendChild(underlinenode);
                        linenode.AppendChild(charactersnode);



                        break;

                    case ROWSPACE_LINE:

                        linenode.InnerText = Convert.ToString(lineparams.Space); 


                        break;
                   

                }
                
                xmlbody.AppendChild(linenode);
                
            }
            
            xmldoc.AppendChild(xmlbody);
            
            ticket.Xml = xmldoc.OuterXml;

            return xmldoc.OuterXml;
            
        }
        

        //Reads the xml file and puts the values into the list in lineparameters struct.

        private void XmlToLineList()
        {
            
            foreach (TicketClass ticket in this)
            {

                XmlDocument xdoc = new XmlDocument();
                try
                {
                    xdoc.LoadXml(ticket.Xml);
                    XmlNodeList topnodes = xdoc.SelectNodes("qticket");

                    foreach (XmlNode node in topnodes[0].ChildNodes)
                    {

                        switch (node.Name)
                        {
                        

                            case "ticketsettings":


                                ticket.PaperMargin = Convert.ToInt16(node.SelectSingleNode("margin").InnerText);
                                ticket.PaperWidth = Convert.ToInt16(node.SelectSingleNode("width").InnerText);
                                ticket.Flipped = Convert.ToInt16(node.SelectSingleNode("flipped").InnerText);
                                ticket.CodePage = Convert.ToInt16(node.SelectSingleNode("codepage").InnerText);

                                break;

                            case IMAGE_LINE:
                                XmlNodeList linenodes = node.ChildNodes;
                                string alignment = "";
                                string offsetx = "";
                                string linespace = "";
                                string imagebase64 = "";
                               
                                    
                                alignment = node.SelectSingleNode("alignment").InnerText;                                           
                                linespace = node.SelectSingleNode("linespace").InnerText;                                                                            
                                imagebase64 = node.SelectSingleNode("base64").InnerText; 
                                
                                ticket.AddLine(IMAGE_LINE, alignment, offsetx, linespace, imagebase64);


                                break;

                            case ROWSPACE_LINE:
                                ticket.AddLine(ROWSPACE_LINE, node.InnerText);

                                break;

                            case "text":


                                alignment = "";
                                offsetx = "";
                                linespace = "";
                                string characterspace = "";

                                string width = "";
                                string height = "";
                                string bold = "";
                                string underline = "";


                                string characters = "";

                                width = node.SelectSingleNode("width").InnerText;
                                height = node.SelectSingleNode("height").InnerText;
                                bold = node.SelectSingleNode("bold").InnerText;
                                underline = node.SelectSingleNode("underline").InnerText;
                                    
                                
                                alignment = node.SelectSingleNode("alignment").InnerText;
                                offsetx = node.SelectSingleNode("offsetx").InnerText;
                                linespace = node.SelectSingleNode("linespace").InnerText;
                                characterspace = node.SelectSingleNode("characterspace").InnerText;
                                characters = node.SelectSingleNode("characters").InnerText;
                                
                                ticket.AddLine(TEXT_LINE, width, height, bold, underline, alignment, offsetx, linespace, characterspace, characters);

                                break;


                            case QUEUENUMBER_LINE:



                                ticket.AddLine(QUEUENUMBER_LINE, node.SelectSingleNode("font").InnerText,node.SelectSingleNode("alignment").InnerText, node.SelectSingleNode("size").InnerText, node.SelectSingleNode("bold").InnerText, node.SelectSingleNode("underline").InnerText, node.SelectSingleNode("characters").InnerText);
                                break;

                            default:
                                Debug.WriteLine("Bad xml tag:" + node.Name);
                                this.Remove(ticket);
                                return;

                        }
                        
                    }
                }
                catch (Exception xmlexception)
                {
                    if (xmlexception.Equals(typeof(ArgumentNullException)))
                    {
                        Debug.WriteLine("Error reading file;" + ticket.Name);
                        this.Remove(ticket);
                        
                    }


                }

            }
            


        }


    }




}

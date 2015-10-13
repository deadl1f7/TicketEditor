using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace TicketEditor
{
    public class TicketClass
    {

        public string Name { get; set; }

        public int Flipped{ get; set; }

        public int PaperWidth{ get; set; }

        public int PaperMargin { get; set; }

        public int CodePage { get; set; }

        public DateTime Date { get; set; }

        public string FilePath { get; set; }

        public string Xml { get; set; }

        public int TicketLength { get; set; }

        public List<LineParameters> Lines;


        //Ticket lineparameters which describes the lines. 

        public TicketClass()
        {

            Lines = new List<LineParameters>();



        }

        public struct LineParameters
        {

            public string Type;

           

            public string Font;
            public int Size;

            public int Width;
            public int Height;
            public int Bold;
            public int Underline;


            public int Alignment;
            public int OffsetX;
            public int LineSpace;
            public int CharacterSpace;

            public int Space;

            public string Characters;

            public BitmapImage BitImage { get; set; }

            public string base64Image;




        }
        //Add Empty lineparameters
        public void AddLine(string type)
        {
            LineParameters lineparams = new LineParameters();
            lineparams.Type = type;
            Lines.Add(lineparams);


        }


        //Move the line in the list, used when moving the elements in the editor
        public void SwapLines(int index,int newindex)
        {
            var line = Lines[index];
            var destline = Lines[newindex];

            Lines[index] = destline;
            Lines[newindex] = line;


        }



        //adding a bitmap 
        public void AddLine(string type, string alignment, string offsetx, string linespace, string imagebase64)
        {

            LineParameters lineparams = new LineParameters();
            lineparams.Type = "image";
            lineparams.Alignment = Convert.ToInt16(alignment);
            
            lineparams.LineSpace = Convert.ToInt16(linespace);
            lineparams.base64Image = imagebase64;
            

            Byte[] bitmapData = Convert.FromBase64String(imagebase64);
            System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
           
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = streamBitmap;
            bi.EndInit();

            lineparams.BitImage = bi;

            TicketLength += (int)bi.Height;


            Lines.Add(lineparams);



        }
        //adding linebreaks that will make up for the space between elements vertically.
        public void AddLine(string type,string space)
        {

            LineParameters lineparams = new LineParameters();
            lineparams.Type = type;
            lineparams.Space = Convert.ToInt16(space);
            TicketLength += lineparams.Space*(24);
            Lines.Add(lineparams);



        }

        //Inserting since printers max linespace is 255 dots, over 255 we need a new linebreak.
        public void InsertLineBreak(string type, int index,int space)
        {


            LineParameters lineparams = new LineParameters();
            lineparams.Type = type;
            lineparams.Space = space;
            
            Lines.Insert(index, lineparams);



        }
   


    //add Textline
    public void AddLine(string type, string width, string height, string bold, string underline, string alignment, string offsetx, string linespace, string characterspace, string characters)
        {

            LineParameters lineparams = new LineParameters();



            lineparams.Type = type;

        
            lineparams.Width = Convert.ToInt16(width);
            lineparams.Height = Convert.ToInt16(height);
            lineparams.Bold = Convert.ToInt16(bold);
            lineparams.Underline = Convert.ToInt16(underline);


            lineparams.Alignment = Convert.ToInt16(alignment);
            lineparams.OffsetX = Convert.ToInt16(offsetx);
            lineparams.LineSpace = Convert.ToInt16(linespace);
            lineparams.CharacterSpace = Convert.ToInt16(characterspace);

            lineparams.Characters = characters;

            TicketLength += 24;

            Lines.Add(lineparams);



        }
        //add Queuenumber
        public void AddLine(string type,string font, string alignment,string size, string bold, string underline,string characters)
        {

            LineParameters lineparams = new LineParameters();



            lineparams.Type = type;
            lineparams.Size = Convert.ToInt16(size);
            lineparams.Alignment = Convert.ToInt16(alignment);
            lineparams.Font = font;
            lineparams.Bold = Convert.ToInt16(bold);
            lineparams.Underline = Convert.ToInt16(underline);

            lineparams.Characters = characters;


            Lines.Add(lineparams);
            
        }
               
        
    }



}
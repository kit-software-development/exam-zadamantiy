using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeaBattle.Client.Properties;
using SeaBattle.Lib.Gaming;

namespace SeaBattle.Client.Misc
{
    /// <summary>
    /// Image object class
    /// </summary>
    public class ImageObj
    {
        /// <summary>
        /// Image
        /// </summary>
        public Image Image;
        
        /// <summary>
        /// Image borders
        /// </summary>
        public Rectangle Rectangle { get; set; }

        /// <summary>
        /// Check if image contains point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>True if contains, else false</returns>
        public bool Contains(Point point)
        {
            return Rectangle.Contains(point);
        }
        
        public ImageObj(Image image, int x, int y, int width, int height)
        {
            this.Image = image;
            this.Rectangle = new Rectangle(x, y, width, height);
        }

        public ImageObj(Image image, Rectangle rectangle)
        {
            this.Image = image;
            this.Rectangle = rectangle;
        }
    }

    /// <summary>
    /// Text object
    /// </summary>
    public class TextObj
    {
        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Font
        /// </summary>
        public Font Font { get; }

        /// <summary>
        /// Brush
        /// </summary>
        public Brush Brush { get; }

        /// <summary>
        /// X coordinate
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// String format
        /// </summary>
        public StringFormat StringFormat { get; }

        public TextObj(string text, Font font, Brush brush, float x, float y, StringFormat sf = null)
        {
            Text = text;
            Font = font;
            Brush = brush;
            X = x;
            Y = y;
            StringFormat = sf;
        }
        
        protected bool Equals(TextObj other)
        {
            return string.Equals(Text, other.Text) 
                && Equals(Font, other.Font) 
                && Equals(Brush, other.Brush) 
                && X.Equals(other.X) 
                && Y.Equals(other.Y) 
                && StringFormat.Equals(other.StringFormat);
        }
    }

    /// <summary>
    /// Class that draws everything on the panel
    /// </summary>
    public class SeaBattleDrawerClass
    {
        /// <summary>
        /// List of images
        /// </summary>
        private readonly List<List<ImageObj>> _imgs;
        
        private readonly List<ImageObj> Field;
        private readonly List<ImageObj> Field2;
        private readonly List<ImageObj> SelectedItem;
        private readonly List<ImageObj> UnchangeList;

        /// <summary>
        /// List of texts
        /// </summary>
        private readonly List<List<TextObj>> _texts;

        private static List<TextObj> _countTexts;

        private static Panel _panel;
        private static Graphics _graphics;

        /// <summary>
        /// Draws string
        /// </summary>
        /// <param name="s">Text</param>
        /// <param name="f">Font</param>
        /// <param name="brush">Brush</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="sf">String format</param>
        public void DrawString(string s, Font f, Brush brush, float x, float y, StringFormat sf)
        { 
            if (sf == null)
            { 
                _graphics.DrawString(s, f, brush, x, y);
            }
            else
            {
                _graphics.DrawString(s, f, brush, x, y, sf);
            }
        }

        /// <summary>
        /// Draws text object
        /// </summary>
        /// <param name="textObj">Text object</param>
        public void DrawTextObj(TextObj textObj)
        {
            DrawString(textObj.Text, textObj.Font, textObj.Brush, textObj.X, textObj.Y, textObj.StringFormat);
        }

        public SeaBattleDrawerClass(Panel panel, Graphics graphics)
        {
            _panel = panel;
            _graphics = graphics;

            _imgs = new List<List<ImageObj>>()
            {
                new List<ImageObj>(),
                new List<ImageObj>(),
                new List<ImageObj>(),
                new List<ImageObj>(),
            };

            Field = _imgs[0];
            Field2 = _imgs[1];
            SelectedItem = _imgs[2];
            UnchangeList = _imgs[3];

            _texts = new List<List<TextObj>>()
            {
                new List<TextObj>(),
            };

            _countTexts = _texts[0];
        }

        /// <summary>
        /// Clears all graphics
        /// </summary>
        public static void ClearGraphics()
        {
            if (Form.ActiveForm != null)
            {
                _panel.Refresh();
                
                _graphics.Dispose();
                _graphics = _panel.CreateGraphics();
            }
        }

        /// <summary>
        /// Redrawing everything
        /// </summary>
        public void RedrawAll()
        {
            ClearGraphics();
            foreach (var imageList in _imgs)
            {
                foreach (var img in imageList)
                {
                    _graphics.DrawImage(img.Image, img.Rectangle);
                }
            }

            DrawAllTexts();
        }

        /// <summary>
        /// Draws battle field
        /// </summary>
        /// <param name="fld">Field object</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="num">Field number</param>
        public void DrawFieldOnGraphics(Field fld, int x = 0, int y = 0, int num = 1)
        {
            var curField = num == 1 ? Field : Field2;

            curField.Clear();
            curField.Add(new ImageObj(Resources.FieldTexture, new Rectangle(x, y, Resources.FieldTexture.Width, Resources.FieldTexture.Height)));
            
            foreach (var ship in fld.Ships)
            {
                Image src;
                switch (ship.Size)
                {
                    case 1:
                        src = Resources.Ship_1;
                        break;
                    case 2:
                        src = Resources.Ship_2;
                        break;
                    case 3:
                        src = Resources.Ship_3;
                        break;
                    case 4:
                        src = Resources.Ship_4;
                        break;
                    default:
                        //TODO: make error
                        src = Resources.Ship_1;
                        break;
                }

                if (!ship.Vertical)
                {
                    src.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }

                curField.Add(new ImageObj(src, new Rectangle(x + 73 + ship.PosX * 40, y + 34 + ship.PosY * 40, src.Width, src.Height)));
            }

            
            for (int i = 0; i < fld.Marks.GetLength(0); i++)
            {
                for (int j = 0; j < fld.Marks.GetLength(1); j++)
                {
                    if (fld.Marks[i, j] != 0)
                    {
                        var src = fld.Marks[i, j] == 1 ? Resources.Dot : Resources.Cross;
                        curField.Add(new ImageObj(src, new Rectangle(x + 73 + i * 40, y + 34 + j * 40, src.Width, src.Height)));
                    }
                }
            }
        }

        /// <summary>
        /// Draws selection
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void DrawSelection(int x, int y, int width, int height)
        {
            var src = Resources.SelectedItem;
            var middlePointX = x + width / 2;
            var middlePointY = y + height / 2;
            
            SelectedItem.Clear();
            SelectedItem.Add(new ImageObj(Resources.SelectedItem, new Rectangle(middlePointX - src.Width / 2, middlePointY - src.Height / 2, src.Width, src.Height)));

            RedrawAll();
        }

        /// <summary>
        /// Draws all text objects from _texts
        /// </summary>
        public void DrawAllTexts()
        {
            foreach (var textList in _texts)
            {
                foreach (var text in textList)
                {
                    DrawTextObj(text);
                }
            }
        }

        /// <summary>
        /// Replaces all texts in _texts
        /// </summary>
        /// <param name="listTextObjs"></param>
        public void ChangeTexts(List<TextObj> listTextObjs)
        {
            _countTexts.Clear();
            _countTexts.AddRange(listTextObjs);
        }

        /// <summary>
        /// Adds items to unchangable list
        /// </summary>
        /// <param name="input">List of image objects</param>
        public void DrawItems(List<ImageObj> input)
        {
            UnchangeList.AddRange(input);
            RedrawAll();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Video;
using AForge.Vision.Motion;
using MaterialSkin;
using MaterialSkin.Controls;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TESTprocesamientodeimagenes
{
    public partial class Form2 : Form
    {

        private Bitmap Imagen2;
        private BitmapData ImageData, ImageData2;
        private Bitmap ImagenRecibida;
        private byte[] buffer, buffer2;
        private int r, g, b, location, location2;
        private IntPtr pointer, pointer2;
        private int filtro;
        private sbyte weight;
        private sbyte[,] weights;
        private bool FaceDetection = false;

        private int rS, gS, bS;


        private int bSS, gSS, rSS, r_x, g_x, b_x, r_y, g_y, b_y, grayscaleSS, locationSS, location2SS;

        private void materialButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
           {
                AddExtension = true,
                FileName = "foto.jpeg",
                Filter = "JPEG File ( *.jpg )|*.jpg|Enhanced Metafile (*.emf )|*.emf|Portable Network Graphic ( *.png )|*.png",
                FilterIndex = 1,
                Title = "Guardar Imagen",
            };
            sfd.InitialDirectory = @"..\CamarApp";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(sfd.FileName); 
            }
        }

        private sbyte weight_x, weight_y;
        private sbyte[,] weights_x;
        private sbyte[,] weights_y;

        private bool haycams;
        private FilterInfoCollection MisCams;
        private VideoCaptureDevice miCam;
       
        public Form2( Bitmap img, int filtro)
        {
            ImagenRecibida = img;
            InitializeComponent();
            weights = new sbyte[,] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
            //Sharp https://ai.stanford.edu/~syyeung/cvweb/Pictures1/sharpening2.png

            weights_x = new sbyte[,] { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } };
            weights_y = new sbyte[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            //sobel https://homepages.inf.ed.ac.uk/rbf/HIPR2/figs/sobmasks.gif

            if (filtro == 1)
            {
                Sobel(ImagenRecibida);
            }
            else
            {
                Sharp(ImagenRecibida);
            }
        }

        private void Sobel(Bitmap Imagen)
        {

            Imagen2 = new Bitmap(Imagen.Width, Imagen.Height);
            ImageData = Imagen.LockBits(new Rectangle(0, 0, Imagen.Width, Imagen.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            ImageData2 = Imagen2.LockBits(new Rectangle(0, 0, Imagen.Width, Imagen.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            buffer = new byte[ImageData.Stride * Imagen.Height];
            buffer2 = new byte[ImageData.Stride * Imagen.Height];
            pointer = ImageData.Scan0;
            pointer2 = ImageData2.Scan0;
            Marshal.Copy(pointer, buffer, 0, buffer.Length);
            for (int y = 0; y < Imagen.Height; y++)
            {
                for (int x = 0; x < Imagen.Width * 3; x += 3)
                {
                    //reseteamos hehe
                    r_x = g_x = b_x = 0; 
                    r_y = g_y = b_y = 0; 
                    //obtenemos la locacion del pixel
                    locationSS = x + y * ImageData.Stride; 
                    for (int yy = -(int)Math.Floor(weights_y.GetLength(0) / 2.0d), yyy = 0; yy <= (int)Math.Floor(weights_y.GetLength(0) / 2.0d); yy++, yyy++)
                    {   
                        //checamos si no nos hemos salido de la imagen
                        if (y + yy >= 0 && y + yy < Imagen.Height) 
                        {
                            for (int xx = -(int)Math.Floor(weights_x.GetLength(1) / 2.0d) * 3, xxx = 0; xx <= (int)Math.Floor(weights_x.GetLength(1) / 2.0d) * 3; xx += 3, xxx++)
                            {
                                //checamos si no nos hemos salido de la imagen pero ahora en x haha
                                if (x + xx >= 0 && x + xx <= Imagen.Width * 3 - 3)
                                {
                                    //checamos donde andamos 
                                    location2SS = x + xx + (yy + y) * ImageData.Stride;


                                    //nos traemos el valor de sobel en el pixel y alrededor de la multiplicacion
                                    weight_x = weights_x[yyy, xxx];
                                    weight_y = weights_y[yyy, xxx];


                                    //aplicamos el peso en X
                                    b_x += buffer[location2SS] * weight_x;
                                    g_x += buffer[location2SS + 1] * weight_x; 
                                    r_x += buffer[location2SS + 2] * weight_x;

                                    //y Y
                                    b_y += buffer[location2SS] * weight_y;
                                    g_y += buffer[location2SS + 1] * weight_y;//G_Y
                                    r_y += buffer[location2SS + 2] * weight_y;
                                }
                            }
                        }
                    }


                    //obtenemos la magniutud
                    bSS = (int)Math.Sqrt(Math.Pow(b_x, 2) + Math.Pow(b_y, 2));
                    gSS = (int)Math.Sqrt(Math.Pow(g_x, 2) + Math.Pow(g_y, 2));//G
                    rSS = (int)Math.Sqrt(Math.Pow(r_x, 2) + Math.Pow(r_y, 2));

                    if (bSS > 255) bSS = 255;
                    if (gSS > 255) gSS = 255;
                    if (rSS > 255) rSS = 255;

                    //aplicamos el escala de grises
                    grayscaleSS = (bSS + gSS + rSS) / 3;

                    //guardamos en el buffer
                    buffer2[locationSS] = (byte)grayscaleSS;
                    buffer2[locationSS + 1] = (byte)grayscaleSS;
                    buffer2[locationSS + 2] = (byte)grayscaleSS;
                    
                }
            }

            Marshal.Copy(buffer2, 0, pointer2, buffer.Length);
            Imagen.UnlockBits(ImageData);
            Imagen2.UnlockBits(ImageData2);
            pictureBox1.Image = Imagen2;


            //filtro = 0;
        }

        private void Sharp(Bitmap Image)
        {
            Bitmap Image2 = new Bitmap(Image.Width, Image.Height);
            ImageData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            ImageData2 = Image2.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            buffer = new byte[ImageData.Stride * Image.Height];
            buffer2 = new byte[ImageData.Stride * Image.Height];
            pointer = ImageData.Scan0;
            pointer2 = ImageData2.Scan0;
            Marshal.Copy(pointer, buffer, 0, buffer.Length);


            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width * 3; x += 3)
                {
                    //reseteamos
                    r = g = b = 0;
                    location = x + y * ImageData.Stride;

                    for (int yy = -(int)Math.Floor(weights.GetLength(0) / 2.0d), yyy = 0; yy <= (int)Math.Floor(weights.GetLength(0) / 2.0d); yy++, yyy++)
                    {
                        if (y + yy >= 0 && y + yy < Image.Height)
                        {
                            for (int xx = -(int)Math.Floor(weights.GetLength(1) / 2.0d) * 3, xxx = 0; xx <= (int)Math.Floor(weights.GetLength(1) / 2.0d) * 3; xx += 3, xxx++)
                            {
                                if (x + xx >= 0 && x + xx <= Image.Width * 3 - 3)
                                {



                                    location2 = x + xx + (yy + y) * ImageData.Stride; 
                                    weight = weights[yyy, xxx];
                                    

                                    //multiplicamos en cada canal el arreglo con el pixel del sharp
                                    b += buffer[location2] * weight;
                                    g += buffer[location2 + 1] * weight;
                                    r += buffer[location2 + 2] * weight;




                                }
                            }
                        }
                    }

                    //ver si no nos pasamos
                    if (b > 255) b = 255;
                    else if (b < 0) b = 0;
                    if (g > 255) g = 255;
                    else if (g < 0) g = 0;
                    if (r > 255) r = 255;
                    else if (r < 0) r = 0;
                    buffer2[location] = (byte)b;
                    buffer2[location + 1] = (byte)g;
                    buffer2[location + 2] = (byte)r;
                }
            }
            Marshal.Copy(buffer2, 0, pointer2, buffer.Length);
            Image.UnlockBits(ImageData);
            Image2.UnlockBits(ImageData2);
            pictureBox1.Image = Image2;
        }


        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}

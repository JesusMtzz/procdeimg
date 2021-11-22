using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
//using OpenCV;

namespace TESTprocesamientodeimagenes
{
    //public partial class Form1 : Form
    public partial class Form1 : MaterialForm

    {
        private Bitmap Imagen2;
        private BitmapData ImageData, ImageData2;
        private byte[] buffer, buffer2;
        private byte r, g, b, grayscale, location, location2;
        private IntPtr pointer, pointer2;
        private int filtro;
        private sbyte weight;
        private sbyte[,] weights;
        private bool FaceDetection = false;

        private int rS, gS, bS;


        private int bSS, gSS, rSS, r_x, g_x, b_x, r_y, g_y, b_y, grayscaleSS, locationSS, location2SS;
        private sbyte weight_x, weight_y;
        private sbyte[,] weights_x;
        private sbyte[,] weights_y;

        private bool haycams;
        private FilterInfoCollection MisCams;
        private VideoCaptureDevice miCam;
        MotionDetector Detector;
        float NiveldeDeteccion;
        private string Path = @"D:\code\github\procdeimg\TESTprocesamientodeimagenes\Photos";


        public class RoundButton : Button
        {
            protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
            {
                GraphicsPath grPath = new GraphicsPath();
                grPath.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
                this.Region = new System.Drawing.Region(grPath);
                base.OnPaint(e);
            }
        }

        //HaarCascade faceDetected;
        //faceDetected = new HearCascade;


                                                                            //static readonly CascadeClassifier cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");

        public Form1()
        {
            InitializeComponent();

            weights = new sbyte[,] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };

            weights_x = new sbyte[,] { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } };
            weights_y = new sbyte[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Inicializar dEtector
            Detector = new MotionDetector(new TwoFramesDifferenceDetector(), new MotionBorderHighlighting());
            NiveldeDeteccion = 0;
            Cargarcams();
        }

        public void Cargarcams()
        {
            MisCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if(MisCams.Count > 0)
            {
                haycams = true;
                for (int i = 0; i< MisCams.Count; i++)
                    {comboBox1.Items.Add(MisCams[i].Name.ToString());}
                comboBox1.Text = MisCams[0].Name.ToString();
            }
            else
            {
                haycams = false;
            }
        }
           


        public void CerrarCam()
        {
            if(miCam !=null && miCam.IsRunning)
            {
                miCam.SignalToStop();
                miCam = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CerrarCam();
            int i = comboBox1.SelectedIndex;
            string NombreCam = MisCams[i].MonikerString;
            miCam = new VideoCaptureDevice(NombreCam);
            //videoSourcePlayer1.VideoSource = miCam;
            //videoSourcePlayer1.Start();
            miCam.NewFrame += new NewFrameEventHandler(capture);
            miCam.Start();
            
        }

        

        async private void capture(object sender , NewFrameEventArgs eventArgs)
        {
            Bitmap Imagen = (Bitmap)eventArgs.Frame.Clone(); 
                                            //var bmp = new Bitmap(Imagen.Width, Imagen.Height, PixelFormat.Format32bppArgb);

            switch (filtro)
            {
                case 0:

                    break;

                case 1:
                    GrayScale(Imagen);
                    break;

                case 2:
                    Sharp(Imagen);
                    break;
                case 3:
                    Solarize(Imagen);
                    break;
                case 4:
                    Sepia(Imagen);
                    break;
                case 5:
                    Sobel(Imagen);
                    break;
                default:

                    break;
            }


                                            /*if (FaceDetection)
                                            {
                                                Image<Bgr, Byte> imgFD = bmp.ToImage<Bgr, byte>();
                                                Rectangle[] rectanglesFD = cascadeClassifier.DetectMultiScale(imgFD, 1.2, 1);
                                                foreach(Rectangle rectangle in rectanglesFD)
                                                {
                                                    using(Graphics graphics = Graphics.FromImage(Imagen))
                                                    {
                                                        using (Pen pen = new Pen(Color.Red, 1))
                                                        {
                                                            graphics.DrawRectangle(pen, rectangle);
                                                        }
                                                    }
                                                }
                                            }
                                            */

            pictureBox1.Image = Imagen;

        }

        private void GrayScale(Bitmap Imagen)
        {
            ImageData = Imagen.LockBits(new Rectangle(0, 0, Imagen.Width, Imagen.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            buffer = new byte[3 * Imagen.Width * Imagen.Height];
            pointer = ImageData.Scan0;
            Marshal.Copy(pointer, buffer, 0, buffer.Length);
            for (int i = 0; i < Imagen.Height * 3 * Imagen.Width; i += 3)
            {
                b = buffer[i];
                g = buffer[i + 1];
                r = buffer[i + 2];
                grayscale = (byte)((r + g + b) / 3);
                buffer[i] = grayscale;
                buffer[i + 1] = grayscale;
                buffer[i + 2] = grayscale;
            }
            Marshal.Copy(buffer, 0, pointer, buffer.Length);
            Imagen.UnlockBits(ImageData);
            //Image = Imagen;

            pictureBox1.Image = Imagen;
        }
        
        private void Sharp(Bitmap Imagen)
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
                    r = g = b = 0; //reset the channels values
                    location = (byte)(x + y * ImageData.Stride); //to get the location of any pixel >> location = x + y * Stride
                    for (int yy = -(int)Math.Floor(weights.GetLength(0) / 2.0d), yyy = 0; yy <= (int)Math.Floor(weights.GetLength(0) / 2.0d); yy++, yyy++)
                    {
                        if (y + yy >= 0 && y + yy < Imagen.Height) //to prevent crossing the bounds of the array
                        {
                            for (int xx = -(int)Math.Floor(weights.GetLength(1) / 2.0d) * 3, xxx = 0; xx <= (int)Math.Floor(weights.GetLength(1) / 2.0d) * 3; xx += 3, xxx++)
                            {
                                if (x + xx >= 0 && x + xx <= Imagen.Width * 3 - 3) //to prevent crossing the bounds of the array
                                {
                                    location2 = (byte)(x + xx + (yy + y) * ImageData.Stride); //to get the location of any pixel >> location = x + y * Stride
                                    weight = weights[yyy, xxx];
                                    //applying the same weight to all channels
                                    b += (byte)(buffer[location2] * weight);
                                    g += (byte)(buffer[location2 + 1] * weight);
                                    r += (byte)(buffer[location2 + 2] * weight);
                                }
                            }
                        }
                    }
                    if (r > 255)
                        r = 255;
                    else if (r < 0)
                        r = 0;
                    if (b > 255) 
                            b = 255;
                    else if (b < 0) 
                            b = 0;
                    if (g > 255) 
                            g = 255;
                    else if (g < 0) 
                            g = 0;
                    
                    buffer2[location] = (byte)b;
                    buffer2[location + 1] = (byte)g;
                    buffer2[location + 2] = (byte)r;
                }
            }
            Marshal.Copy(buffer2, 0, pointer2, buffer.Length);
            Imagen.UnlockBits(ImageData);
            Imagen2.UnlockBits(ImageData2);
            pictureBox1.Image = Imagen2;
        }

        private void Solarize(Bitmap Imagen)
        {
            ImageData = Imagen.LockBits(new Rectangle(0, 0, Imagen.Width, Imagen.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            buffer = new byte[3 * Imagen.Width * Imagen.Height];
            pointer = ImageData.Scan0;
            Marshal.Copy(pointer, buffer, 0, buffer.Length);
            for (int i = 0; i < Imagen.Height * 3 * Imagen.Width; i += 3)
            {
                b = buffer[i];
                g = buffer[i + 1];
                r = buffer[i + 2];
                buffer[i] = (r > 127) ? (byte)(255 - r) : r;
                buffer[i + 1] = (g > 127) ? (byte)(255 - g) : g;
                buffer[i + 2] = (b > 127) ? (byte)(255 - b) : b;
            }
            Marshal.Copy(buffer, 0, pointer, buffer.Length);
            Imagen.UnlockBits(ImageData);
            pictureBox1.Image = Imagen;
        }

        private void Sepia(Bitmap Imagen)
        {
            Imagen2 = new Bitmap(Imagen.Width, Imagen.Height);
            ImageData = Imagen.LockBits(new Rectangle(0, 0, Imagen.Width, Imagen.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            buffer = new byte[3 * Imagen.Width * Imagen.Height];
            pointer = ImageData.Scan0;
            Marshal.Copy(pointer, buffer, 0, buffer.Length);
            for (int i = 0; i < Imagen.Height * 3 * Imagen.Width; i += 3)
            {
                bS = (int)((buffer[i]) * .131 + (buffer[i + 1]) * .534 + (buffer[i + 2]) * .272);
                gS = (int)((buffer[i]) * .168 + (buffer[i + 1]) * .686 + (buffer[i + 2]) * .349);
                rS = (int)((buffer[i]) * .189 + (buffer[i + 1]) * .769 + (buffer[i + 2]) * .393);
                bS = (bS > 255) ? 255 : bS;
                gS = (gS > 255) ? 255 : gS;
                rS = (rS > 255) ? 255 : rS;
                buffer[i] = (byte)bS;
                buffer[i + 1] = (byte)gS;
                buffer[i + 2] = (byte)rS;
            }
            Marshal.Copy(buffer, 0, pointer, buffer.Length);
            Imagen.UnlockBits(ImageData);
            pictureBox1.Image = Imagen;
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
                    r_x = g_x = b_x = 0; //reset the gradients in x-direcion values
                    r_y = g_y = b_y = 0; //reset the gradients in y-direction values
                    locationSS = x + y * ImageData.Stride; //to get the location of any pixel >> location = x + y * Stride
                    for (int yy = -(int)Math.Floor(weights_y.GetLength(0) / 2.0d), yyy = 0; yy <= (int)Math.Floor(weights_y.GetLength(0) / 2.0d); yy++, yyy++)
                    {
                        if (y + yy >= 0 && y + yy < Imagen.Height) //to prevent crossing the bounds of the array
                        {
                            for (int xx = -(int)Math.Floor(weights_x.GetLength(1) / 2.0d) * 3, xxx = 0; xx <= (int)Math.Floor(weights_x.GetLength(1) / 2.0d) * 3; xx += 3, xxx++)
                            {
                                if (x + xx >= 0 && x + xx <= Imagen.Width * 3 - 3) //to prevent crossing the bounds of the array
                                {
                                    location2SS = x + xx + (yy + y) * ImageData.Stride; //to get the location of any pixel >> location = x + y * Stride
                                    weight_x = weights_x[yyy, xxx];
                                    weight_y = weights_y[yyy, xxx];
                                    //applying the same weight to all channels
                                    b_x += buffer[location2] * weight_x;
                                    g_x += buffer[location2 + 1] * weight_x; //G_X
                                    r_x += buffer[location2 + 2] * weight_x;
                                    b_y += buffer[location2] * weight_y;
                                    g_y += buffer[location2 + 1] * weight_y;//G_Y
                                    r_y += buffer[location2 + 2] * weight_y;
                                }
                            }
                        }
                    }
                    //getting the magnitude for each channel
                    bSS = (int)Math.Sqrt(Math.Pow(b_x, 2) + Math.Pow(b_y, 2));
                    gSS = (int)Math.Sqrt(Math.Pow(g_x, 2) + Math.Pow(g_y, 2));//G
                    rSS = (int)Math.Sqrt(Math.Pow(r_x, 2) + Math.Pow(r_y, 2));

                    if (bSS > 255) bSS = 255;
                    if (gSS > 255) gSS = 255;
                    if (rSS > 255) rSS = 255;

                    //getting grayscale value
                    grayscaleSS = (bSS + gSS + rSS) / 3;

                    //thresholding to clean up the background
                    //if (grayscale < 80) grayscale = 0;
                    buffer2[locationSS] = (byte)grayscaleSS;
                    buffer2[locationSS + 1] = (byte)grayscaleSS;
                    buffer2[locationSS + 2] = (byte)grayscaleSS;
                    //thresholding to clean up the background
                    //if (b < 100) b = 0;
                    //if (g < 100) g = 0;
                    //if (r < 100) r = 0;

                    //buffer2[location] = (byte)b;
                    //buffer2[location + 1] = (byte)g;
                    //buffer2[location + 2] = (byte)r;
                }
            }
            Marshal.Copy(buffer2, 0, pointer2, buffer.Length);
            Imagen.UnlockBits(ImageData);
            Imagen2.UnlockBits(ImageData2);
            pictureBox1.Image = Imagen;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CerrarCam();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void Filtro5Button_Click(object sender, EventArgs e)
        {
            filtro = 5;
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            if (FaceDetection) {
                FaceDetection = false;
            }
            else
            {
                FaceDetection = true;
            }
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            filtro = 3;
        }

        private void Filtro2Button_Click(object sender, EventArgs e)
        {
            filtro = 2;
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            filtro = 4;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            filtro = 1;
        }

        private void pictureBox8_Click_1(object sender, EventArgs e)
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

        private void materialButton1_Click_1(object sender, EventArgs e)
        {
            filtro = 0;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void videoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {
            //Deteccion de movimiento
            //NiveldeDeteccion = Detector.ProcessFrame(image);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Video;
using MaterialSkin;
using MaterialSkin.Controls;

namespace TESTprocesamientodeimagenes
{
    //public partial class Form1 : Form
    public partial class Form1 : MaterialForm

    {

        private bool haycams;
        private FilterInfoCollection MisCams;
        private VideoCaptureDevice miCam;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
            miCam.NewFrame += new NewFrameEventHandler(capture);
            miCam.Start();
        }


        private void capture(object sender , NewFrameEventArgs eventArgs)
        {
            Bitmap Imagen = (Bitmap)eventArgs.Frame.Clone();

            //intento de filtro invertido XD
            //Bitmap pic = (Bitmap)eventArgs.Frame.Clone();
            //for (int y = 0; (y <= (pic.Height - 1)); y++)
            //{
            //    for (int x = 0; (x <= (pic.Width - 1)); x++)
            //    {
            //        Color inv = pic.GetPixel(x, y);
            //        inv = Color.FromArgb(255, (255 - inv.R), (255 - inv.G), (255 - inv.B));
            //        pic.SetPixel(x, y, inv);
            //    }
            //}

            //fin de intento

            //Conclusion: funciona pero es muy lenta la imagen que muestra, brinca muchos frames xD

            pictureBox1.Image = Imagen;
            //pictureBox1.Image = pic;

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CerrarCam();
        }

        
    }
}

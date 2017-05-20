using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DotImaging;
using Microsoft.Win32;

namespace stereoimage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        Bgr<byte>[,] stereoOutputSave;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<Bgr<byte>[,]> images = new List<Bgr<byte>[,]>();
            OpenFileDialog op = new OpenFileDialog();
            op.Multiselect = true;
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                //save all input images in BGR format
                foreach (String file in op.FileNames)
                {
                    stereoImage.Source = new BitmapImage(new Uri(file));
                    var bmp = new BitmapImage(new Uri(file));
                    Bgr<byte>[,] imageBGR = null;
                    try
                    {
                        var k = bmp.Format;
                        var l = bmp.Format.BitsPerPixel;
                        if (bmp.Format.BitsPerPixel == 8)
                        {
                            var colorImg = bmp.ToArray<Gray<byte>>(); //to bitmap
                            imageBGR = colorImg.ToBgr();
                        }
                        else if (bmp.Format.BitsPerPixel == 16)
                        {
                            Bgr<byte>[,] colorImg = bmp.ToArray<Bgr<byte>>(); //to bitmap
                            imageBGR = colorImg.ToBgr();
                        }
                        else if (bmp.Format.BitsPerPixel == 32)
                        {
                            Bgra<byte>[,] colorImg = bmp.ToArray<Bgra<byte>>(); //to bitmap
                            imageBGR = colorImg.ToBgr();
                        }
                    }
                    catch
                    {

                    }
                    images.Add(imageBGR);
                }
                //if one picture then slice at half
                if (images.Count == 1)
                {
                    int w = images[0].Width();
                    int h = images[0].Height();
                    Bgr<byte>[,] colorImg1 = new Bgr<byte>[h, w / 2];
                    Bgr<byte>[,] colorImg2 = new Bgr<byte>[h, w / 2];
                    for (int k = 0; k < h; k++)
                    {
                        for (int l = 0; l < w / 2; l++)
                        {
                            colorImg1[k, l] = images[0][k, l];
                        }
                    }
                    for (int k = 0; k < h; k++)
                    {
                        for (int l = w / 2; l < w; l++)
                        {
                            colorImg2[k, l - (w / 2)] = images[0][k, l];
                        }
                    }
                    images.Clear();
                    images.Add(colorImg1);
                    images.Add(colorImg2);
                    colorImg1.Show("imageleft");
                    colorImg2.Show("imageright");
                }
                else
                {
                    images[0].Show("imageleft");
                    images[1].Show("imageright");
                }
                generateStereoImageFromTwoImages(images);
            }//end if open a files
        }
        private void generateStereoImageFromTwoImages(List<Bgr<byte>[,]> images)
        {
            Bgr<byte>[,] stereoOutput = new Bgr<byte>[images[0].Height(), images[0].Width()];
            for (int k = 0; k < images[0].Height(); k++)
            {
                for (int l = 0; l < images[0].Width(); l++)
                {
                    stereoOutput[k, l] = new Bgr<byte>(images[1][k, l].B, images[1][k, l].G, images[0][k, l].R);
                }
            }
            stereoOutput.Show("stereoOutput");
            stereoOutputSave = stereoOutput;
            stereoImage.Source = stereoOutput.ToBitmapSource();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Multiselect = true;
            if (op.ShowDialog() == true)
            {
                var capture = new FileCapture(op.FileName);
                capture.Open();
                long length = capture.Length;
                for (int i = 0; i < length; i++)
                {
                    var image = capture.ElementAt(i); //get i frame
                    Bgr<byte>[,] imageBGR = image.ToBgr();
                    generateStereoImageFromOneImage(imageBGR, imageBGR);
                    //Thread.Sleep(25); //Is not too quick
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Multiselect = true;
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                stereoImage.Source = new BitmapImage(new Uri(op.FileName));
                var bmp = new BitmapImage(new Uri(op.FileName));
                Bgr<byte>[,] imageBGRleft = null;
                Bgr<byte>[,] imageBGRright = null;
                try
                {
                    var k = bmp.Format;
                    var l = bmp.Format.BitsPerPixel;
                    if (bmp.Format.BitsPerPixel == 8)
                    {
                        var colorImg = bmp.ToArray<Gray<byte>>(); //to bitmap
                        imageBGRleft = colorImg.ToBgr();
                        imageBGRright = colorImg.ToBgr();
                    }
                    else if (bmp.Format.BitsPerPixel == 16)
                    {
                        Bgr<byte>[,] colorImg = bmp.ToArray<Bgr<byte>>(); //to bitmap
                        imageBGRleft = colorImg.ToBgr();
                        imageBGRright = colorImg.ToBgr();
                    }
                    else if (bmp.Format.BitsPerPixel == 32)
                    {
                        Bgra<byte>[,] colorImg = bmp.ToArray<Bgra<byte>>(); //to bitmap
                        imageBGRleft = colorImg.ToBgr();
                        imageBGRright = colorImg.ToBgr();
                    }
                }
                catch
                {

                }
                generateStereoImageFromOneImage(imageBGRleft, imageBGRright);
            }
        }

        private void generateStereoImageFromOneImage(Bgr<byte>[,] imageBGRleft, Bgr<byte>[,] imageBGRright)
        {
            int x = Convert.ToInt16(textBoxX.Text);
            int y = Convert.ToInt16(textBoxY.Text);
            Bgr<byte>[,] stereoOutput = new Bgr<byte>[imageBGRleft.Height(), imageBGRleft.Width()];
            for (int k = 0; k < imageBGRleft.Height(); k++)
            {
                for (int l = 0; l < imageBGRleft.Width(); l++)
                {
                    if ((k + y) < imageBGRright.Height() && (l + x) < imageBGRright.Width())
                    {
                        stereoOutput[k, l] = new Bgr<byte>(imageBGRright[k + y, l + x].B, imageBGRright[k + x, l + y].G, imageBGRleft[k, l].R);
                    }
                    else {
                        stereoOutput[k, l] = new Bgr<byte>(imageBGRright[k, l].B, imageBGRright[k, l].G, imageBGRright[k, l].R);
                    }
                }
            }
            stereoOutput = stereoOutput.Clone(new DotImaging.Primitives2D.Rectangle(0, 0, stereoOutput.Width() - x, stereoOutput.Height() - y));
            stereoOutput.Show("stereoOutput");
            stereoOutputSave = stereoOutput;
            stereoImage.Source = stereoOutput.ToBitmapSource();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            List<Bgr<byte>[,]> images = new List<Bgr<byte>[,]>();
            OpenFileDialog op = new OpenFileDialog();
            op.Multiselect = true;
            if (op.ShowDialog() == true)
            {
                var capture = new FileCapture(op.FileName);
                capture.Open();
                long length = capture.Length;
                for (int i = 0; i < length; i++)
                {
                    var image = capture.ElementAt(i); //get i frame
                    Bgr<byte>[,] imageBGR = image.ToBgr();
                    images.Clear();
                    images.Add(imageBGR);
                    int w = images[0].Width();
                    int h = images[0].Height();
                    Bgr<byte>[,] colorImg1 = new Bgr<byte>[h, w / 2];
                    Bgr<byte>[,] colorImg2 = new Bgr<byte>[h, w / 2];
                    for (int k = 0; k < h; k++)
                    {
                        for (int l = 0; l < w / 2; l++)
                        {
                            colorImg1[k, l] = images[0][k, l];
                        }
                    }
                    for (int k = 0; k < h; k++)
                    {
                        for (int l = w / 2; l < w; l++)
                        {
                            colorImg2[k, l - (w / 2)] = images[0][k, l];
                        }
                    }
                    images.Clear();
                    images.Add(colorImg1);
                    images.Add(colorImg2);
                    generateStereoImageFromTwoImages(images);
                }
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var pipeName = new Uri(textBoxURL.Text).NamedPipeFromYoutubeUri(); //Youtube
            var capture = new FileCapture(String.Format(@"\\.\pipe\{0}", pipeName));
            capture.Open();
            long length = capture.Length;
            for (int i = 0; i < length; i++)
            {
                Bgr<byte>[,] image = null;
                capture.ReadTo(ref image); //get i frame
                Bgr<byte>[,] imageBGR = image.ToBgr();
                generateStereoImageFromOneImage(imageBGR, imageBGR);
                //Thread.Sleep(5); //Is not too quick
            }
        }

        private void stereoImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            stereoOutputSave.Save("stereoimage.jpg");
        }
    }
}

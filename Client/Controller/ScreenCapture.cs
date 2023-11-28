using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;

namespace Client.Controller
{
    class ScreenCapture
    {
        static System.Threading.Timer history_timer = null;
        static EncoderParameter encoder = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 60L);

        public static Image GetScreenThumbnailImage()
        {
            Bitmap image = ScreenCapture.CaptureIamge();
            if (image == null) return null;
            Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);

            return image.GetThumbnailImage(Conf.Constant.Thumb_IMAGE_SIZE.Width, Conf.Constant.Thumb_IMAGE_SIZE.Height, myCallback, IntPtr.Zero);
        }

        public static Image GetScreenFullImage()
        {
            return ScreenCapture.CaptureIamge();
        }
        private static bool ThumbnailCallback()
        {
            return false;
        }

        private static Bitmap CaptureIamge()
        {
            Bitmap raw_img = null;
            Bitmap img = null;
            Graphics g = null;

            try
            {
                Screen screen = Screen.PrimaryScreen;
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sg.guardian"))
                //Conf.Constant.SAFE_MODE
                {
                    Bitmap[] images = new Bitmap[] { Resource1.img100, Resource1.img101, Resource1.img102, Resource1.img103, Resource1.img104 };
                    raw_img = images[(System.DateTime.Now.Minute / 3) % 5];
                    g = Graphics.FromImage(raw_img);
                }
                else
                {
                    raw_img = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
                    g = Graphics.FromImage(raw_img);
                    g.CopyFromScreen(0, 0, 0, 0, screen.Bounds.Size);
                }
            }
            catch (System.Exception)
            {
                if (g != null)
                    g.Dispose();
                g = null;
                if (raw_img == null)
                    raw_img.Dispose();
            }

            if (g == null)
                return null;

            img = new Bitmap(raw_img, Conf.Constant.FULL_IMAGE_SIZE);

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            ImageCodecInfo jpegCodec = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    jpegCodec = codec;
                }
            }

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            myEncoderParameters.Param[0] = ScreenCapture.encoder;

            MemoryStream str = new MemoryStream();
            img.Save(str, jpegCodec, myEncoderParameters);
            img = new Bitmap(str);

            g = Graphics.FromImage(img);
            g.DrawString(DateTime.Now.ToString(), new Font("Arial", 15.0f), Brushes.Red, img.Width - 300, 50);

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sg.guardian"))
                // Conf.Constant.SAFE_MODE
            {
                string date = DateTime.Now.ToLongDateString();
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.DrawString(String.Format("{0}:{1:D2}", DateTime.Now.Hour, DateTime.Now.Minute), new Font("Segoe UI Semilight", 65f), Brushes.White, 10, img.Height - 215);
                g.DrawString(date.Substring(0, date.Length - 6), new Font("Segoe UI", 30f), Brushes.White, 24, img.Height - 115);
            }

            g.Dispose();
            str.Close();

            return img;
        }


        public static void StartHistoryTimer()
        {
            history_timer = new System.Threading.Timer(new TimerCallback(makeHistory), null, 0, Conf.Constant.HISTORY_CAPUTRE_IMAGE_INTERVAL);
        }

        public static void SetHistoryInterval(int interval)
        {
            if (history_timer != null)
            {
                history_timer.Change(0, interval);
            }
        }

        private static void makeHistory(object state)
        {
            Image img = ScreenCapture.GetScreenFullImage();
            if (img == null) return;
            MemoryStream stream = new MemoryStream();
            if (stream == null) return;
            img.Save(stream, ImageFormat.Jpeg);
            img.Dispose();
            HistoryController.Instance.SaveImageHistory(stream.GetBuffer());
            Util.Util.ReleaseMemory();
        }
    }
}

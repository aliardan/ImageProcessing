using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ImageProcessing.Controllers
{
    public class MirrorController : ApiController
    {
        public MirrorController()
        {
        }

        // POST api/Mirror
        public HttpResponseMessage Post()
        {
            var sourceImage = ReadImageFromStream(HttpContext.Current.Request.InputStream);
            var resultImage = Process(sourceImage);
            return CreateResponse(resultImage);
        }

        private Bitmap Process(Bitmap source)
        {
            try
            {
                Image<Rgb, Byte> normalizedMasterImage = new Image<Rgb, Byte>(source);

                //var mainCascadeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml");
                var mainCascadeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_alt_tree.xml");
                //CascadeClassifier classifier = new CascadeClassifier("C:\\Users\\Ali\\Downloads\\im\\opencv\\sources\\data\\haarcascades\\haarcascade_frontalface_default.xml");

                CascadeClassifier classifier = new CascadeClassifier(mainCascadeFilePath);

                var faces = classifier.DetectMultiScale(normalizedMasterImage);

                foreach (var face in faces)
                {
                    normalizedMasterImage.Draw(face, new Rgb(0, 255, 0), 4);
                }

                var message = "Faces count: " + faces.Length;
                var fontFace = Emgu.CV.CvEnum.FontFace.HersheySimplex;
                var point = new Point(50, 50);
                var fontScale = 2;
                normalizedMasterImage.Draw(message, point, fontFace, fontScale, new Rgb(0, 255, 0), 2);

                return normalizedMasterImage.Bitmap;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"baseCatalog:{AppDomain.CurrentDomain.BaseDirectory}, mainCascadeFilePath:{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml")}",
                    ex);
            }
        }

        public static byte[] ToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        private Bitmap ReadImageFromStream(Stream stream)
        {
            var bytes = new byte[stream.Length];
            HttpContext.Current.Request.InputStream.Read(bytes, 0, (int)stream.Length);
            using (var memoryStream = new MemoryStream(bytes))
            {
                memoryStream.Position = 0;
                return (Bitmap)Bitmap.FromStream(memoryStream);
            }
        }

        private HttpResponseMessage CreateResponse(Image bitmap)
        {
            var resBytes = ToByteArray(bitmap, ImageFormat.Jpeg);
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new ByteArrayContent(resBytes);
            response.Content.LoadIntoBufferAsync(resBytes.Length).Wait();
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            return response;
        }
    }
}
using System;
using System.Collections.Generic;
using FFmpeg.AutoGen;
using Mediapipe.Net.Calculators;
using Mediapipe.Net.External;
using Mediapipe.Net.Framework.Format;
using Mediapipe.Net.Framework.Protobuf;
using Mediapipe.Net.Util;
using SeeShark;
using SeeShark.Device;
using SeeShark.FFmpeg;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;

public static class Program
{
    static VideoCapture capture;
    static FaceMeshCpuCalculator calculator;


    public static unsafe void Main(string[] args)
    {
        Console.WriteLine("Initializing...");
        try
        {
            capture = new VideoCapture();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        Console.WriteLine("Mesh Calculator...");
        calculator = new FaceMeshCpuCalculator();
        calculator.OnResult += handleLandmarks;
        calculator.Run();


        Console.CancelKeyPress += (sender, eventArgs) => exit();
        while (true)
        {
            Image<Bgr, byte> image = capture.QueryFrame().ToImage<Bgr, Byte>();
            Image image_bmp = byteArrayToImage(image.ToJpegData());
            Bitmap bmp = new Bitmap(image_bmp);

  
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
           
            
            byte* scan0 = (byte*)bmpData.Scan0.ToPointer();
            using ImageFrame imgframe = new ImageFrame(Mediapipe.Net.Framework.Format.ImageFormat.Srgba, bmp.Width, bmp.Height, bmp.Width, scan0);
            bmp.UnlockBits(bmpData);

            
            using ImageFrame? img = calculator.Send(imgframe);
        }



        Console.ReadKey();
    }

    private static void handleLandmarks(object? sender, List<NormalizedLandmarkList> landmarks)
    {
        Console.WriteLine($"Got a list of {landmarks[0].Landmark.Count} landmarks at frame {calculator?.CurrentFrame}");
    }


    private static Image byteArrayToImage(byte[] byteArrayIn)
    {
        try
        {
            MemoryStream ms = new MemoryStream(byteArrayIn, 0, byteArrayIn.Length);
            ms.Write(byteArrayIn, 0, byteArrayIn.Length);
            return Image.FromStream(ms, true);
        }
        catch { }
        return null;
    }



    private static void exit()
    {
        Console.WriteLine("Exiting...");;
        calculator?.Dispose();
    }


}


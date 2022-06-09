# Mediapipe-Iris-Tracking
C# Python Wrapper using cv2 and mediapipe  

Needs Python 3.7 (exact version)  


# Python Script
```
import cv2 as cv
import numpy as np
import mediapipe as mp
import base64

mp_face_mesh = mp.solutions.face_mesh
LEFT_IRIS = [474,475,475,477]
RIGHT_IRIS = [469,470,471,472]

class Iris:
    def run(self, uri):
        encoded_data = uri
        nparr = np.fromstring(base64.b64decode(encoded_data), np.uint8)
        frame = cv.imdecode(nparr, cv.IMREAD_COLOR)

        with mp_face_mesh.FaceMesh(
            max_num_faces=1, 
            refine_landmarks=True,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5
        ) as face_mesh:
            #frame = cv.flip(frame, 1);
            rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
            img_h, img_w = frame.shape[:2]
            results = face_mesh.process(rgb_frame)
            if results.multi_face_landmarks:
                mesh_points = np.array([np.multiply([p.x, p.y], [img_w, img_h]).astype(int) for p in results.multi_face_landmarks[0].landmark])
                (l_cx, l_cy), l_radius = cv.minEnclosingCircle(mesh_points[LEFT_IRIS])
                (r_cx, r_cy), r_radius = cv.minEnclosingCircle(mesh_points[RIGHT_IRIS])
                center_left = np.array([l_cx, l_cy], dtype=np.int32)
                center_right = np.array([r_cx, r_cy], dtype=np.int32)
                return str([center_left[0], center_left[1], int(l_radius), center_right[0], center_right[1], int(r_radius)])
```  

# C# Script on Form
```
using Emgu.CV;
using Emgu.CV.Structure;
using Python.Runtime;


namespace Tracker
{
    public partial class Form1 : Form
    {
        VideoCapture capture;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Run();
        }

        private void Run()
        {
            Initialize();
            try
            {
                capture = new VideoCapture();
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Application.Idle += ProcessFrame;
        }


        private void ProcessFrame(object sender, EventArgs e)
        {
            Image<Bgr, byte> image = capture.QueryFrame().ToImage<Bgr, Byte>();
            Image image_bmp = byteArrayToImage(image.ToJpegData());


            using (Py.GIL())
            {
                dynamic iris = Py.Import("iris");
                dynamic np = Py.Import("numpy");
                dynamic ir = iris.Iris();
               
                string result = ir.run(ImageToBase64(image_bmp));

                if (!String.IsNullOrEmpty(result))
                {
                    string[] r = result.Replace("[", "").Replace("]", "").Replace(" ", "").Split(new char[] { ',' });
                    using (Graphics grf = Graphics.FromImage(image_bmp))
                    { 
                        int l_radius = Convert.ToInt32(r[2]);
                        int l_x = Convert.ToInt32(r[0]) - l_radius;
                        int l_y = Convert.ToInt32(r[1]) - l_radius;
                        int r_radius = Convert.ToInt32(r[5]);
                        int r_x = Convert.ToInt32(r[3]) - r_radius;
                        int r_y = Convert.ToInt32(r[4]) - r_radius;

                        grf.DrawArc(new Pen(ColorTranslator.FromHtml("#ff0000"), 1), new Rectangle(l_x, l_y, l_radius * 2, l_radius * 2), 0.0f, 360.9f);
                        grf.DrawArc(new Pen(ColorTranslator.FromHtml("#ff0000"), 1), new Rectangle(r_x, r_y, r_radius * 2, r_radius * 2), 0.0f, 360.9f);
                    }
                }
            }
            pictureBox1.Image = image_bmp;
        }


        public static void Initialize()
        {
            Environment.SetEnvironmentVariable("PATH", @"C:\Users\Anti-puff\AppData\Local\Programs\Python\Python37\", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", @"C:\Users\Anti-puff\AppData\Local\Programs\Python\Python37\", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", @"C:\Users\Anti-puff\AppData\Local\Programs\Python\Python37\", EnvironmentVariableTarget.Process);
            PythonEngine.Initialize();
        }


        public string ImageToBase64(Image image)
        {
            using (MemoryStream m = new MemoryStream())
            {
                image.Save(m, image.RawFormat);
                byte[] imageBytes = m.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
            
        }


        public byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }


        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            try
            {
                MemoryStream ms = new MemoryStream(byteArrayIn, 0, byteArrayIn.Length);
                ms.Write(byteArrayIn, 0, byteArrayIn.Length);
                return Image.FromStream(ms, true);//Exception occurs here
            }
            catch { }
            return null;
        }
    }
}
```

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using UnityStandardAssets.Vehicles.Car;

public class Open_CV_var : MonoBehaviour {
    
    Camera carm;
    Texture2D tex2;
    CarAIControl car_AI;

    //라인계산 값들
    int[] left_point = new int[4];
    int[] right_point = new int[4];
    int line_count = 0;
    bool is_Left_Line = false;
    bool is_Right_Line = false;

    void Start () {
        carm = this.GetComponent<Camera>();
        car_AI = GameObject.FindGameObjectWithTag("Player").GetComponent<CarAIControl>();
	}
	
	//사진에서 값 처리
	void Update () {
        tex2 = MakeTexture2D(carm.targetTexture);

        Mat imgMat = new Mat(tex2.height, tex2.width, CvType.CV_8UC3);

        Utils.texture2DToMat(tex2, imgMat);

        //GrayScale 생성
        Mat grayMat = new Mat();
        Imgproc.cvtColor(imgMat, grayMat, Imgproc.COLOR_RGB2GRAY);
        
        //Canny적용
        Mat cannyMat = new Mat();
        Imgproc.Canny(grayMat, cannyMat, 70, 200);

        //ROI 설정
        grayMat.locateROI(new Size(200, 100), new Point(60, 60));
        
        //HoughLine생성
        Mat lines = new Mat();
        Imgproc.HoughLinesP(cannyMat, lines, 1, Mathf.PI / 180, 30, 40, 40);

        int[] linesArray = new int[lines.cols() * lines.rows() * lines.channels()];

        lines.get(0, 0, linesArray);


        line_count = 0;
        is_Left_Line = false;
        is_Right_Line = false;
        right_point.Initialize();
        left_point.Initialize();
        
        for (int i = 0; i < linesArray.Length; i = i + 4)
        {
         //   Debug.Log(Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg);
            if (line_count >= 2)
            {
                break;
            }

            if (!is_Left_Line)
            {   
                //각제한
                if (5 < Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg)
                {
                    if (80 > Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg)
                    {
                        Imgproc.line(imgMat, new Point(linesArray[i + 0], linesArray[i + 1]), new Point(linesArray[i + 2], linesArray[i + 3]), new Scalar(255, 0, 0), 4);
                        line_count++;
                        is_Left_Line = true;
                        for(int  j =0; j<left_point.Length; j++)
                        {
                            left_point[j] = linesArray[i + j];
                        }
                    }
                }
            }

            if (!is_Right_Line)
            {
                //각제한
                if (120< Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg)
                {
                    if (175 > Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg)
                    {
                        Imgproc.line(imgMat, new Point(linesArray[i + 0], linesArray[i + 1]), new Point(linesArray[i + 2], linesArray[i + 3]), new Scalar(255, 0, 0), 4);
                        line_count++;
                        is_Right_Line = true;
                        for(int j =0; j<right_point.Length; j++)
                        {
                            right_point[j] = linesArray[i + j];
                        }
                    }
                }
            }
        }

        int[] van = Find_Vanishing_Point(left_point, right_point);
        
        Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
        Texture2D texture2 = new Texture2D(grayMat.cols(), grayMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(imgMat, texture);
        Utils.matToTexture2D(cannyMat, texture2);

        GameObject.FindGameObjectWithTag("test").GetComponent<Renderer>().material.mainTexture = texture;
        //GameObject.Find("test2").GetComponent<Renderer>().material.mainTexture = texture2;

        //자동차에 값 전달
        Vector2 diretion = Get_Vector(van, imgMat);
        car_AI.Change_Value_Steering(diretion);

        //Debug.Log("van point : "+diretion.x +" : " + diretion.y);
    }

    //Texture2D로 변경
    Texture2D MakeTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(320, 160, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new UnityEngine.Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    //교점찾기
    int[] Find_Vanishing_Point(int[] left_Line, int[] right_Line)
    {
        int[] van_point = new int[2];

        int x1 = left_Line[0];
        int y1 = left_Line[1];
        int x2 = left_Line[2];
        int y2 = left_Line[3];
        int x_1 = right_Line[0];
        int y_1 = right_Line[1];
        int x_2 = right_Line[2];
        int y_2 = right_Line[3];

        float m1 = (y2 - y1) / (x2 - x1);
        float m2 = (y_2 - y_1) / (x_2 - x_1);

        int v_x = (int)((m1 * x1 - m2 * x_1 + y_1 - y1) / (m1 - m2));
        int v_y = (int)((((-y_1 + m2 * x_1) / m2) - ((-y1 + m1 * x1) / m1)) / (1 / m1 - 1 / m2));

        van_point[0] = v_x;
        van_point[1] = v_y;
        //Debug.Log("van point : " + van_point[0] + " : " + van_point[1]);

        return van_point;
        
    }

    //값 계산
    Vector2 Get_Vector(int[] van_point, Mat img)
    {
        if(van_point == null || img == null)
        {
            return new Vector2(0, 0);
        }

        int x = van_point[0];
        int y = van_point[1];
        int I_point_x = 170;
        int I_point_y = 0;

        Vector2 dirction = new Vector2(x-I_point_x, y-I_point_y);

        return dirction;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;

public class Open_Cv_orth : MonoBehaviour {

    Camera carm;
    Texture2D tex2;

    // Use this for initialization
    void Start()
    {
        carm = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        tex2 = MakeTexture2D(carm.targetTexture);

        Mat imgMat = new Mat(tex2.height, tex2.width, CvType.CV_8UC3);

        Utils.texture2DToMat(tex2, imgMat);

        //GrayScale 생성
        Mat grayMat = new Mat();
        Imgproc.cvtColor(imgMat, grayMat, Imgproc.COLOR_RGB2GRAY);



        //Canny적용
        Mat cannyMat = new Mat();
        Imgproc.Canny(grayMat, cannyMat, 70, 210);


        //ROI 설정
        grayMat.adjustROI(cannyMat.height(), 0, cannyMat.width() * 4 / 10, cannyMat.width());

        //HoughLine생성
        Mat lines = new Mat();
        Imgproc.HoughLinesP(cannyMat, lines, 1, Mathf.PI / 180, 30, 100, 20);

        int[] linesArray = new int[lines.cols() * lines.rows() * lines.channels()];

        lines.get(0, 0, linesArray);

        for (int i = 0; i < linesArray.Length; i = i + 4)
        {
            //Debug.Log(Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg);

            if (0 <= Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg)
            {
                if (80 > Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg)
                {
                    Imgproc.line(imgMat, new Point(linesArray[i + 0], linesArray[i + 1]), new Point(linesArray[i + 2], linesArray[i + 3]), new Scalar(255, 0, 0), 4);

                }
            }

            if (180 >= Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg)
            {
                if (100 < Mathf.Atan2((linesArray[i + 2] - linesArray[i + 0]), (linesArray[i + 3] - linesArray[i + 1])) * Mathf.Rad2Deg)
                {
                    Imgproc.line(imgMat, new Point(linesArray[i + 0], linesArray[i + 1]), new Point(linesArray[i + 2], linesArray[i + 3]), new Scalar(255, 0, 0), 4);

                }
            }

        }

        Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(imgMat, texture);
        
        GameObject.Find("test3").GetComponent<Renderer>().material.mainTexture = texture;
    }

    Texture2D MakeTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(320, 160, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new UnityEngine.Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
}

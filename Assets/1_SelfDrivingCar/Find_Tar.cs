using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;

public class Find_Tar : MonoBehaviour {

    Mat rgbMat;
    Color32[] colors;
    Texture2D tex2;
    HOGDescriptor des;
    Camera carm;

    void Start()
    {
        carm = this.GetComponent<Camera>();

        rgbMat = new Mat();

        des = new HOGDescriptor();
    }

    Texture2D MakeTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(320, 160, TextureFormat.ARGB32, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new UnityEngine.Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    //HOGDetect를 통해 사람 확인
    void Update()
    {
        tex2 = MakeTexture2D(carm.targetTexture);
        rgbMat = new Mat(tex2.height, tex2.width, CvType.CV_8UC3);

        Utils.texture2DToMat(tex2, rgbMat);

        int frameWidth = rgbMat.cols();
        int frameHeight = rgbMat.rows();
        colors = new Color32[frameWidth * frameHeight];

        Imgproc.cvtColor(rgbMat, rgbMat, Imgproc.COLOR_BGR2RGB);

        using (MatOfRect locations = new MatOfRect())
        using (MatOfDouble weights = new MatOfDouble())
        {
            des.setSVMDetector(HOGDescriptor.getDefaultPeopleDetector());
            des.detectMultiScale(rgbMat, locations, weights);

            OpenCVForUnity.Rect[] rects = locations.toArray();
            for (int i = 0; i < rects.Length; i++)
            {
                Imgproc.rectangle(rgbMat, new Point(rects[i].x, rects[i].y), new Point(rects[i].x + rects[i].width, rects[i].y + rects[i].height), new Scalar(255, 0, 0), 2);
            }
        }

        Texture2D texture = new Texture2D(320, 160, TextureFormat.ARGB32, false);
        Utils.matToTexture2D(rgbMat, texture, colors);

        GameObject.Find("test2").GetComponent<Renderer>().material.mainTexture = texture;
    }
}

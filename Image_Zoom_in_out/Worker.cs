﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using AForge;
using AForge.Imaging;
using AForge.Imaging.ComplexFilters;
using AForge.Imaging.Filters;

namespace Image_Zoom_in_out
{
    public class Worker
    {
        private Form1 form1;

        public Worker(Form1 form)
        {
            this.form1 = form;
        }

        /* 副執行續實做以下方法
         * 開始處裡圖片
         */
        public void DoWork()
        {
            while (!_shouldStop)
            {
                ExcuteFunction(form1.EXCUTEMODE);
                RequestStop();

            }

        }
        /*
         * 副執行續實做以下方法
         * 有些功能圖片需要接受預處理
        */
        public void DoPreproccess()
        {
            while (!_shouldStop)
            {

                for (int i = 0; i < 8; i++)
                {
                    form1.mNoiseImageArray[i] = GenerateNoise(form1.BITMAP, i);

                }

                RequestStop();

            }

        }
        /*
         * 中止執行續
         */
        public void RequestStop()
        {
            _shouldStop = true;
        }

        private volatile bool _shouldStop;

        private bool ExcuteFunction(string ExcuteMode)
        {
            Bitmap oldImage = form1.BITMAP;
            Bitmap newImage;
            switch (ExcuteMode)
            {
                case "Nearest neighbor interpolation":
                    newImage = PixelReplication(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Pixel replication":
                    newImage = PixelReplication(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Biliner interpolation":
                    newImage = BilinerInterpolation(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Negatives":
                    newImage = Negatives(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Log Transform":
                    newImage = LogTransform(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Power-Law":
                    newImage = PowerLaw(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Bit-plane":
                    form1.mPixelizationImageArray = BitPlane(oldImage);
                    break;
                case "Histogram Processing":
                    newImage = HistogramProcessing(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Averaging filter":
                    newImage = AveragingFilter(form1.mNoiseImageArray);
                    form1.BITMAP = newImage ?? form1.BITMAP;
                    break;
                case "Median filter":
                    newImage = MedianFilter(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Laplacian":
                    newImage = Laplacian(oldImage);
                    form1.BITMAP = newImage;
                    break;
                case "Fourier Transformation":
                    newImage = FourierTransformation(oldImage);
                    form1.BITMAP = newImage;
                    break;

                case "Thyroid Segmentation":
                    newImage = Thyroid_Segmentation(oldImage);
                    form1.BITMAP = newImage;
                    break;

            }


            return true;
        }


        /**
         * 純像素複製,刪減
         * PixelReplication其實跟Nearest neighbor interpolation差不多
         * PixelReplication是先取像素資訊在將其放到複製好放大的新圖矩陣中
         * 行列放大同等倍率
         * Nearest neighbor interpolation則是將新圖矩陣放大後，算出其中心點，在找最近的參考像素
         * 爬文資料是說都跟PixelReplication概念差不多，[在找最近的參考像素]->[PixelReplication是先取像素資訊]
         * 所以這邊我兩功能方法共用
        */
        private Bitmap PixelReplication(Bitmap oldImage)
        {
            Bitmap newImage;
            switch (form1.ZO_CODE)
            {
                case "ZOOM_IN":
                    newImage = new Bitmap((oldImage.Width * form1.MUTIPLE), (oldImage.Height * form1.MUTIPLE));
                    for (int i = 0; i < oldImage.Width; i++)
                    {
                        for (int j = 0; j < oldImage.Height; j++)
                        {
                            Color p_color = oldImage.GetPixel(i, j);
                            newImage.SetPixel(i * form1.MUTIPLE, j * form1.MUTIPLE, p_color);

                            for (int k = 0; k < form1.MUTIPLE; k++)
                            {
                                for (int l = 0; l < form1.MUTIPLE; l++)
                                {

                                    newImage.SetPixel(i * form1.MUTIPLE + k, j * form1.MUTIPLE, p_color);
                                    newImage.SetPixel(i * form1.MUTIPLE, j * form1.MUTIPLE + l, p_color);
                                    newImage.SetPixel(i * form1.MUTIPLE + k, j * form1.MUTIPLE + l, p_color);

                                }
                            }

                        }
                    }
                    return newImage;
                case "ZOOM_OUT":
                    newImage = new Bitmap((oldImage.Width / form1.MUTIPLE), (oldImage.Height / form1.MUTIPLE));
                    for (int i = 0; i < oldImage.Width / form1.MUTIPLE; i++)
                    {
                        for (int j = 0; j < oldImage.Height / form1.MUTIPLE; j++)
                        {
                            Color p_color = oldImage.GetPixel(i * form1.MUTIPLE, j * form1.MUTIPLE);
                            newImage.SetPixel(i, j, p_color);

                        }
                    }
                    return newImage;
            }


            return oldImage;
        }

        /**
         * 四斜角像素放大縮小
         * 我忘記我當初在寫啥了
         * 好像也沒啥...大略解釋
        */
        private Bitmap BilinerInterpolation(Bitmap oldImage)
        {
            Bitmap newImage;
            switch (form1.ZO_CODE)
            {
                case "ZOOM_IN":
                    newImage = new Bitmap((oldImage.Width * form1.MUTIPLE), (oldImage.Height * form1.MUTIPLE));
                    for (int i = 1; i < oldImage.Width - 1; i++)
                    {
                        for (int j = 1; j < oldImage.Height - 1; j++)
                        {
                            Color TopLeft_pColor = oldImage.GetPixel(i - 1, j - 1);
                            Color BottomLeft_pColor = oldImage.GetPixel(i + 1, j - 1);
                            Color TopRight_pColor = oldImage.GetPixel(i - 1, j + 1);
                            Color BottomRight_pColor = oldImage.GetPixel(i + 1, j + 1);
                            int a = (TopLeft_pColor.A + BottomLeft_pColor.A + TopRight_pColor.A + BottomRight_pColor.A) / 4;
                            int r = (TopLeft_pColor.R + BottomLeft_pColor.R + TopRight_pColor.R + BottomRight_pColor.R) / 4;
                            int g = (TopLeft_pColor.G + BottomLeft_pColor.G + TopRight_pColor.G + BottomRight_pColor.G) / 4;
                            int b = (TopLeft_pColor.B + BottomLeft_pColor.B + TopRight_pColor.B + BottomRight_pColor.B) / 4;

                            // MUTIPLE是放大縮小倍數
                            for (int k = 0; k < form1.MUTIPLE; k++)
                            {
                                for (int l = 0; l < form1.MUTIPLE; l++)
                                {

                                    newImage.SetPixel(i * form1.MUTIPLE + k, j * form1.MUTIPLE, Color.FromArgb(a, r, g, b));
                                    newImage.SetPixel(i * form1.MUTIPLE, j * form1.MUTIPLE + l, Color.FromArgb(a, r, g, b));
                                    newImage.SetPixel(i * form1.MUTIPLE + k, j * form1.MUTIPLE + l, Color.FromArgb(a, r, g, b));

                                }
                            }

                        }
                    }
                    return newImage;
                case "ZOOM_OUT":
                    newImage = new Bitmap((oldImage.Width / form1.MUTIPLE), (oldImage.Height / form1.MUTIPLE));
                    for (int i = 1; i < oldImage.Width / form1.MUTIPLE - 1; i++)
                    {
                        for (int j = 1; j < oldImage.Height / form1.MUTIPLE - 1; j++)
                        {
                            Color TopLeft_pColor = oldImage.GetPixel(i * form1.MUTIPLE - 1, j * form1.MUTIPLE - 1);
                            Color BottomLeft_pColor = oldImage.GetPixel(i * form1.MUTIPLE + 1, j * form1.MUTIPLE - 1);
                            Color TopRight_pColor = oldImage.GetPixel(i * form1.MUTIPLE - 1, j * form1.MUTIPLE + 1);
                            Color BottomRight_pColor = oldImage.GetPixel(i * form1.MUTIPLE + 1, j * form1.MUTIPLE + 1);
                            int a = (TopLeft_pColor.A + BottomLeft_pColor.A + TopRight_pColor.A + BottomRight_pColor.A) / 4;
                            int r = (TopLeft_pColor.R + BottomLeft_pColor.R + TopRight_pColor.R + BottomRight_pColor.R) / 4;
                            int g = (TopLeft_pColor.G + BottomLeft_pColor.G + TopRight_pColor.G + BottomRight_pColor.G) / 4;
                            int b = (TopLeft_pColor.B + BottomLeft_pColor.B + TopRight_pColor.B + BottomRight_pColor.B) / 4;

                            newImage.SetPixel(i, j, Color.FromArgb(a, r, g, b));

                        }
                    }
                    return newImage;
            }


            return oldImage;
        }

        /**
         * Negatives
         */
        private Bitmap Negatives(Bitmap oldImage)
        {

            Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height);
            for (int i = 1; i < oldImage.Width; i++)
            {
                for (int j = 1; j < oldImage.Height; j++)
                {
                    Color pColor = oldImage.GetPixel(i, j);

                    if (form1.FULL_COLOR)
                    {
                        // 負片就白變黑黑變白原理，同理套用到三原色上，[255 - 原始色階]。
                        newImage.SetPixel(i, j, Color.FromArgb(255 - pColor.R, 255 - pColor.G, 255 - pColor.B));
                    }
                    else
                    {
                        int gColor = (int)(0.299 * pColor.R + 0.587 * pColor.G + 0.114 * pColor.B);
                        newImage.SetPixel(i, j, Color.FromArgb(255 - gColor, 255 - gColor, 255 - gColor));
                    }

                }
            }
            return newImage;
        }

        /**
        * Log Transform
        */
        private Bitmap LogTransform(Bitmap oldImage)
        {

            Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height);
            for (int i = 1; i < oldImage.Width; i++)
            {
                for (int j = 1; j < oldImage.Height; j++)
                {
                    Color pColor = oldImage.GetPixel(i, j);

                    // s = c log(r + 1)
                    // red
                    var newColor_R = (int)(1 * Math.Log(pColor.R + 1, 2));
                    // green
                    var newColor_G = (int)(1 * Math.Log(pColor.G + 1, 2));
                    // blue
                    var newColor_B = (int)(1 * Math.Log(pColor.B + 1, 2));

                    if (form1.FULL_COLOR)
                    {
                        newImage.SetPixel(i, j, Color.FromArgb((newColor_R * 32 > 255) ? 255 : newColor_R * 32, (newColor_G * 32 > 255) ? 255 : newColor_G * 32, (newColor_B * 32 > 255) ? 255 : newColor_B * 32));
                    }
                    else
                    {
                        int gColor = (int)(0.299 * newColor_R + 0.587 * newColor_G + 0.114 * newColor_B);
                        newImage.SetPixel(i, j, Color.FromArgb(gColor * 32, gColor * 32, gColor * 32));
                    }

                }
            }
            return newImage;
        }

        /**
         * Power-Law
         */
        private Bitmap PowerLaw(Bitmap oldImage)
        {

            Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height);
            for (int i = 1; i < oldImage.Width; i++)
            {
                for (int j = 1; j < oldImage.Height; j++)
                {
                    Color pColor = oldImage.GetPixel(i, j);

                    // s = 255*(r/255)^gamma
                    // red
                    var newColor_R = (int)(255 * Math.Pow((double)((decimal)pColor.R / 255), form1.GAMMA));
                    // green
                    var newColor_G = (int)(255 * Math.Pow((double)((decimal)pColor.G / 255), form1.GAMMA));
                    // blue
                    var newColor_B = (int)(255 * Math.Pow((double)((decimal)pColor.B / 255), form1.GAMMA));

                    if (form1.FULL_COLOR)
                    {
                        newImage.SetPixel(i, j, Color.FromArgb(newColor_R, newColor_G, newColor_B));
                    }
                    else
                    {
                        // 三原色調製成灰階色彩的比例，這是黃金比例XD
                        // 也有次版的三色相加除已3，可是效果不好
                        int gColor = (int)(0.299 * newColor_R + 0.587 * newColor_G + 0.114 * newColor_B);
                        newImage.SetPixel(i, j, Color.FromArgb(gColor, gColor, gColor));
                    }


                }
            }
            return newImage;
        }

        /**
         * Bit-plane
         */
        private Bitmap[] BitPlane(Bitmap oldImage)
        {
            Char[] bit_array_R = new char[8];
            Char[] bit_array_G = new char[8];
            Char[] bit_array_B = new char[8];
            Bitmap pixelizationImage0 = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap pixelizationImage1 = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap pixelizationImage2 = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap pixelizationImage3 = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap pixelizationImage4 = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap pixelizationImage5 = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap pixelizationImage6 = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap pixelizationImage7 = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap[] pixelizationImageArray = { pixelizationImage0, pixelizationImage1, pixelizationImage2, pixelizationImage3, pixelizationImage4, pixelizationImage5, pixelizationImage6, pixelizationImage7 };
            for (int i = 1; i < oldImage.Width; i++)
            {
                for (int j = 1; j < oldImage.Height; j++)
                {
                    Color pColor = oldImage.GetPixel(i, j);

                    // binaryR轉二進位後確定是否八位數，否則自動補0
                    // 最後每個數字為單位輸出成bit_array_R[] 陣列
                    var binaryR = Convert.ToString(pColor.R, 2);
                    binaryR = binaryR.PadLeft(8, '0');
                    binaryR = binaryR.PadRight(8, '0');
                    bit_array_R = binaryR.ToCharArray();

                    var binaryG = Convert.ToString(pColor.G, 2);
                    binaryG = binaryG.PadLeft(8, '0');
                    binaryG = binaryG.PadRight(8, '0');
                    bit_array_G = binaryG.ToCharArray();

                    var binaryB = Convert.ToString(pColor.B, 2);
                    binaryB = binaryB.PadLeft(8, '0');
                    binaryB = binaryB.PadRight(8, '0');
                    bit_array_B = binaryB.ToCharArray();
                    for (int k = 0; k < 8; k++)
                    {
                        // 以每個pixel為單位(i,j)，檢查三元素的binary值為1或0，輸出對應色階
                        var newColorR = bit_array_R[k] == '0' ? 0 : 255;
                        var newColorG = bit_array_G[k] == '0' ? 0 : 255;
                        var newColorB = bit_array_B[k] == '0' ? 0 : 255;

                        // 256色版本壓縮率疑似更好
                        //var newColorR = bit_array_R[k] == '0' ? 0 : (int)Char.GetNumericValue(bit_array_R[k]) * (int)Math.Pow(2, 7 - k);
                        //var newColorG = bit_array_G[k] == '0' ? 0 : (int)Char.GetNumericValue(bit_array_G[k]) * (int)Math.Pow(2, 7 - k);
                        //var newColorB = bit_array_B[k] == '0' ? 0 : (int)Char.GetNumericValue(bit_array_B[k]) * (int)Math.Pow(2, 7 - k);



                        if (form1.FULL_COLOR)
                        {
                            // 將對應色階給予該pixel
                            pixelizationImageArray[k].SetPixel(i, j, Color.FromArgb(newColorR, newColorG, newColorB));
                        }
                        else
                        {
                            // 將對應色階給予該pixel，並在必要時調和色階創造灰階效果
                            int gColor = (int)(0.299 * newColorR + 0.587 * newColorG + 0.114 * newColorB);
                            pixelizationImageArray[k].SetPixel(i, j, Color.FromArgb(gColor, gColor, gColor));
                        }

                    }



                }
            }
            return pixelizationImageArray;
        }

        /**
         * Histogram Processing
         */
        private Bitmap HistogramProcessing(Bitmap oldImage)
        {

            Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height);
            // 原色彩三元素陣列
            int[] nk_pColor_R = new int[256];
            int[] nk_pColor_G = new int[256];
            int[] nk_pColor_B = new int[256];
            double[] prk_pColor_R = new double[256];
            double[] prk_pColor_G = new double[256];
            double[] prk_pColor_B = new double[256];

            for (int i = 1; i < oldImage.Width; i++)
            {
                for (int j = 1; j < oldImage.Height; j++)
                {
                    Color pColor = oldImage.GetPixel(i, j);
                    // 原色彩三元素陣列:統計中
                    nk_pColor_R[pColor.R] += 1;
                    nk_pColor_G[pColor.G] += 1;
                    nk_pColor_B[pColor.B] += 1;
                }

            }

            // sk_pColor_R[原色彩] = []
            double[] sk_pColor_R = new double[256];
            double[] sk_pColor_G = new double[256];
            double[] sk_pColor_B = new double[256];

            for (int rk = 0; rk < 256; rk++)
            {
                // 算出Pr(rk)圖表的對照值
                // prk_pColor_R[x軸] = y軸(即占比)
                prk_pColor_R[rk] = (double)((decimal)nk_pColor_R[rk] / (oldImage.Width * oldImage.Height));
                prk_pColor_G[rk] = (double)((decimal)nk_pColor_G[rk] / (oldImage.Width * oldImage.Height));
                prk_pColor_B[rk] = (double)((decimal)nk_pColor_B[rk] / (oldImage.Width * oldImage.Height));

                double prk_sum_R = 0.0, prk_sum_G = 0.0, prk_sum_B = 0.0;
                for (int prk_count = 0; prk_count < rk + 1; prk_count++)
                {
                    // "準備"計算[Sk <--> rk] 的轉換圖表
                    // 這邊我們先累加已計算完的色階
                    // 例如:當我們等等要計算色階23的顏色轉Sk的值
                    // 在這邊就先把至23為止以前的"占比"全部累加
                    // 因此外面的for-loop每多迭代一次，來到這邊都會
                    // 多做一次，最到最後一次第256次時，這邊會累加
                    // 所有出現過的比重，進而求得1。
                    prk_sum_R += prk_pColor_R[prk_count];
                    prk_sum_G += prk_pColor_G[prk_count];
                    prk_sum_B += prk_pColor_B[prk_count];
                }
                // 將剛剛辛苦解釋弄出來的矩陣乘上原始色階最高數
                // 在這邊我是假設使用256色彩，故最高色階為255
                // 不要問我為何這樣算我只抓到那感覺不會解釋QAQ
                // 進而求得sk_pColor_R[原始色階0~255(整數)] = 轉化後新色階0~255(通常是小數)
                // sk_pColor_R這東西就是一個轉換表了，我做了三個顏色RGB有三個轉換表。
                sk_pColor_R[rk] = 255 * prk_sum_R;
                sk_pColor_G[rk] = 255 * prk_sum_G;
                sk_pColor_B[rk] = 255 * prk_sum_B;

            }


            for (int i = 1; i < oldImage.Width; i++)
            {
                for (int j = 1; j < oldImage.Height; j++)
                {
                    Color pColor = oldImage.GetPixel(i, j);

                    if (form1.FULL_COLOR)
                    {
                        // 這邊要注意的只有(int)Math.Round(sk_pColor_R[pColor.R], 2, MidpointRounding.AwayFromZero)
                        // Math.Round(轉換值,輸出小數點位數,MidpointRounding.AwayFromZero -> 這代表當值為介於中間時，其實就是指5，選擇往0比較遠的方向靠近，也就是"五入")
                        // Math.Round()這方法本身則是將值四捨五入為最接近的整數或是指定的小數位數。
                        // 好像直接Math.Round(Decimal)也可以達到一樣效果啦...
                        newImage.SetPixel(i, j, Color.FromArgb((int)Math.Round(sk_pColor_R[pColor.R], 2, MidpointRounding.AwayFromZero), (int)Math.Round(sk_pColor_G[pColor.G], 2, MidpointRounding.AwayFromZero), (int)Math.Round(sk_pColor_B[pColor.B], 2, MidpointRounding.AwayFromZero)));
                    }
                    else
                    {
                        int gColor = (int)(0.299 * Math.Round(sk_pColor_R[pColor.R], 2, MidpointRounding.AwayFromZero) + 0.587 * Math.Round(sk_pColor_G[pColor.G], 2, MidpointRounding.AwayFromZero) + 0.114 * Math.Round(sk_pColor_B[pColor.B], 2, MidpointRounding.AwayFromZero));
                        newImage.SetPixel(i, j, Color.FromArgb(gColor, gColor, gColor));
                    }

                }
            }
            return newImage;
        }

        /**
         * Averaging filter
         */
        public Bitmap AveragingFilter(Bitmap[] noiseImageArray)
        {
            int[] p_array;
            int proccess_image = 0;

            if (noiseImageArray.Length != 0)
            {
                p_array = new int[noiseImageArray[0].Width * noiseImageArray[0].Height * 4];

                foreach (Bitmap oldImage in noiseImageArray)
                {
                    proccess_image++;
                    Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height);

                    for (int i = 0; i < oldImage.Width; i++)
                    {
                        for (int j = 0; j < oldImage.Height; j++)
                        {
                            var p_Color_R = oldImage.GetPixel(i, j).R;
                            var p_Color_G = oldImage.GetPixel(i, j).G;
                            var p_Color_B = oldImage.GetPixel(i, j).B;
                            var p_Color_Gray = (int)(oldImage.GetPixel(i, j).R * 0.299 + oldImage.GetPixel(i, j).G * 0.587 + oldImage.GetPixel(i, j).B * 0.114);

                            p_array[i * oldImage.Height + j] += p_Color_R;
                            p_array[i * oldImage.Height + 1 * oldImage.Width * oldImage.Height + j] += p_Color_G;
                            p_array[i * oldImage.Height + 2 * oldImage.Width * oldImage.Height + j] += p_Color_B;
                            p_array[i * oldImage.Height + 3 * oldImage.Width * oldImage.Height + j] += p_Color_Gray;

                            if (proccess_image == noiseImageArray.Length)
                            {
                                int newColor_R = p_array[i * oldImage.Height + j] / proccess_image;
                                int newColor_G = p_array[i * oldImage.Height + 1 * oldImage.Width * oldImage.Height + j] / proccess_image;
                                int newColor_B = p_array[i * oldImage.Height + 2 * oldImage.Width * oldImage.Height + j] / proccess_image;
                                int newColor_Gray = p_array[i * oldImage.Height + 3 * oldImage.Width * oldImage.Height + j] / proccess_image;

                                newImage.SetPixel(i, j,
                                form1.FULL_COLOR
                                    ? Color.FromArgb(newColor_R, newColor_G, newColor_B)
                                    : Color.FromArgb(newColor_Gray, newColor_Gray, newColor_Gray));
                            }

                        }

                    }


                    if (proccess_image == noiseImageArray.Length)
                    {
                        return newImage;
                    }

                }
            }



            return null;



        }

        /**
         * 模擬舊圖附加上椒鹽雜訊(例:Averaging filter 用)
         */
        public Bitmap GenerateNoise(Bitmap oldImage, int weight)
        {
            int image_Hight = oldImage.Height;
            int image_Width = oldImage.Width;
            // 如果你讀的檔案是索引色素(傳統256色)圖片
            // 256 * 256 * 256 = 2的24次方 = 16777216色色彩 = 全彩圖片 = 無索引色圖
            // 256色只有2的8次方，就只有256顏色給你選，諷刺的是，這麼做可以壓縮圖片，而且人眼其實看不太出來差異
            // 回歸正題如果你讀的是索引色素(傳統256色)圖片，請在企圖對既有色塊的色素進行編輯而使用了Bitmap.Clone()時
            // 在()裡面加上:
            // new Rectangle(0, 0, oldImage.Width, oldImage.Height), PixelFormat.Format24bppRgb
            // 消除索引並調整色彩為24位元色彩
            // 不然在這邊newImage.SetPixel時會爆掉，畢竟你企圖拿24位元色彩寫進8位元色彩的圖片像素上。
            Bitmap newImage = oldImage.Clone(new Rectangle(0, 0, oldImage.Width, oldImage.Height), PixelFormat.Format24bppRgb);
            Random r = new Random();

            for (int i = 0; i < image_Width; i++)
            {
                int j = 0;
                while (j < image_Hight)
                {
                    int ratio = r.Next(0, image_Hight);
                    j += ratio / (weight + 1); // 整個方法呼叫越多次(產生越多圖)，可以透過迭代增加weight值創造出每張圖片雜訊密度漸增的目的。
                    int num = r.Next(0, 2) * 255;
                    newImage.SetPixel(i, j % image_Hight, Color.FromArgb(255, num, num, num));
                    j++;
                }
            }

            return newImage;
        }

        /**
         * Median filter
         */
        private Bitmap MedianFilter(Bitmap oldImage)
        {

            Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap processImage = AddBorders(oldImage, true);
            for (int i = 1; i < oldImage.Width + 1; i++)
            {
                for (int j = 1; j < oldImage.Height + 1; j++)
                {
                    /**
                     * 取九宮格色塊
                     */
                    var p1_Color = processImage.GetPixel(i - 1, j - 1);
                    var p2_Color = processImage.GetPixel(i - 1, j);
                    var p3_Color = processImage.GetPixel(i - 1, j + 1);
                    var p4_Color = processImage.GetPixel(i, j - 1);
                    var p5_Color = processImage.GetPixel(i, j);
                    var p6_Color = processImage.GetPixel(i, j + 1);
                    var p7_Color = processImage.GetPixel(i + 1, j - 1);
                    var p8_Color = processImage.GetPixel(i + 1, j);
                    var p9_Color = processImage.GetPixel(i + 1, j + 1);
                    /**
                     * 存成陣列紅'綠'藍'灰
                     */
                    int[] p_Color_R_arr = { p1_Color.R, p2_Color.R, p3_Color.R, p4_Color.R, p5_Color.R, p6_Color.R, p7_Color.R, p8_Color.R, p9_Color.R };
                    int[] p_Color_G_arr = { p1_Color.G, p2_Color.G, p3_Color.G, p4_Color.G, p5_Color.G, p6_Color.G, p7_Color.G, p8_Color.G, p9_Color.G };
                    int[] p_Color_B_arr = { p1_Color.B, p2_Color.B, p3_Color.B, p4_Color.B, p5_Color.B, p6_Color.B, p7_Color.B, p8_Color.B, p9_Color.B };
                    int[] p_Color_Gray_arr = { (int)(p1_Color.R * 0.299 + p1_Color.G * 0.587 + p1_Color.B * 0.114), (int)(p2_Color.R * 0.299 + p2_Color.G * 0.587 + p2_Color.B * 0.114), (int)(p3_Color.R * 0.299 + p3_Color.G * 0.587 + p3_Color.B * 0.114), (int)(p4_Color.R * 0.299 + p4_Color.G * 0.587 + p4_Color.B * 0.114), (int)(p5_Color.R * 0.299 + p5_Color.G * 0.587 + p5_Color.B * 0.114), (int)(p6_Color.R * 0.299 + p6_Color.G * 0.587 + p6_Color.B * 0.114), (int)(p7_Color.R * 0.299 + p7_Color.G * 0.587 + p7_Color.B * 0.114), (int)(p8_Color.R * 0.299 + p8_Color.G * 0.587 + p8_Color.B * 0.114), (int)(p9_Color.R * 0.299 + p9_Color.G * 0.587 + p9_Color.B * 0.114) };
                    /**
                     * 排序陣列，會直接改變原有陣列變數內含成員的順序
                     */
                    Array.Sort(p_Color_R_arr);
                    Array.Sort(p_Color_G_arr);
                    Array.Sort(p_Color_B_arr);
                    Array.Sort(p_Color_Gray_arr);

                    /**
                     * 判斷顯示模式為全彩或是灰階
                     * [4] -> 取中間值 預設一陣列為9成員，故取"位移第4"個成員
                     */
                    newImage.SetPixel(i - 1, j - 1,
                        form1.FULL_COLOR
                            ? Color.FromArgb(p_Color_R_arr[4], p_Color_G_arr[4], p_Color_B_arr[4])
                            : Color.FromArgb(p_Color_Gray_arr[4], p_Color_Gray_arr[4], p_Color_Gray_arr[4]));
                }
            }

            return newImage;
        }

        /**
         * Median filter 用加白框(因為遮罩3*3框框讀到最邊邊色素塊會沒法好好讀)
         */
        public Bitmap AddBorders(Bitmap oldBitmap, Boolean mode)
        {
            Bitmap newBitmap = new Bitmap(oldBitmap.Width + 2, oldBitmap.Height + 2);

            for (int i = 0; i < oldBitmap.Width; i++)
            {
                for (int j = 0; j < oldBitmap.Height; j++)
                {
                    if (i == 0 || i == oldBitmap.Width + 1 ||
                        j == 0 || j == oldBitmap.Height + 1)
                    {
                        if (mode) newBitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                        else newBitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        newBitmap.SetPixel(i + 1, j + 1, oldBitmap.GetPixel(i, j));
                    }

                }
            }

            return newBitmap;
        }

        /**
         * Laplacian
         */
        private Bitmap Laplacian(Bitmap oldImage, int[] maskArray = null)
        {
            /*
             * 預設矩陣內容
             */
            if (maskArray == null) maskArray = new int[9] { -1, -1, -1, -1, 9, -1, -1, -1, -1 };

            Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap processImage = AddBorders(oldImage, true);
            for (int i = 1; i < oldImage.Width + 1; i++)
            {
                for (int j = 1; j < oldImage.Height + 1; j++)
                {
                    /**
                     * 取九宮格色塊
                     */
                    var p1_Color = processImage.GetPixel(i - 1, j - 1);
                    var p2_Color = processImage.GetPixel(i - 1, j);
                    var p3_Color = processImage.GetPixel(i - 1, j + 1);
                    var p4_Color = processImage.GetPixel(i, j - 1);
                    var p5_Color = processImage.GetPixel(i, j);
                    var p6_Color = processImage.GetPixel(i, j + 1);
                    var p7_Color = processImage.GetPixel(i + 1, j - 1);
                    var p8_Color = processImage.GetPixel(i + 1, j);
                    var p9_Color = processImage.GetPixel(i + 1, j + 1);
                    /**
                     * 存成陣列紅'綠'藍'灰
                     */
                    int p_Color_R = p1_Color.R * maskArray[0] + p2_Color.R * maskArray[1] + p3_Color.R * maskArray[2] + p4_Color.R * maskArray[3] + p5_Color.R * maskArray[4] + p6_Color.R * maskArray[5] + p7_Color.R * maskArray[6] + p8_Color.R * maskArray[7] + p9_Color.R * maskArray[8];
                    int p_Color_G = p1_Color.G * maskArray[0] + p2_Color.G * maskArray[1] + p3_Color.G * maskArray[2] + p4_Color.G * maskArray[3] + p5_Color.G * maskArray[4] + p6_Color.G * maskArray[5] + p7_Color.G * maskArray[6] + p8_Color.G * maskArray[7] + p9_Color.G * maskArray[8];
                    int p_Color_B = p1_Color.B * maskArray[0] + p2_Color.B * maskArray[1] + p3_Color.B * maskArray[2] + p4_Color.B * maskArray[3] + p5_Color.B * maskArray[4] + p6_Color.B * maskArray[5] + p7_Color.B * maskArray[6] + p8_Color.B * maskArray[7] + p9_Color.B * maskArray[8];
                    int p_Color_Gray = (int)(p_Color_R * 0.299 + p_Color_G * 0.587 + p_Color_B * 0.114);
                    /*
                     * 將小於0或大於255的數字轉成0或255
                     * 像素介於0<-->255
                     */
                    p_Color_R = p_Color_R < 0 ? 0 : p_Color_R > 255 ? 255 : p_Color_R;
                    p_Color_G = p_Color_G < 0 ? 0 : p_Color_G > 255 ? 255 : p_Color_G;
                    p_Color_B = p_Color_B < 0 ? 0 : p_Color_B > 255 ? 255 : p_Color_B;
                    p_Color_Gray = p_Color_Gray < 0 ? 0 : p_Color_Gray > 255 ? 255 : p_Color_Gray;

                    /**
                     * 判斷顯示模式為全彩或是灰階
                     */
                    newImage.SetPixel(i - 1, j - 1,
                        form1.FULL_COLOR
                            ? Color.FromArgb(p_Color_R, p_Color_G, p_Color_B)
                            : Color.FromArgb(p_Color_Gray, p_Color_Gray, p_Color_Gray));
                }
            }

            return newImage;
        }


        /**
         * Fourier transformation
         * 使用AForge.Net
         * NuGet -> AForge.Imaging
         */
        private Bitmap FourierTransformation(Bitmap oldImage)
        {

            ComplexImage cimage;
            Bitmap pow2Bitmap, bitmap_8bbp, newBitmap;
            // 創建高通濾波器，還不確定數字這樣設定對不對...
            FrequencyFilter height_pass_filter = new FrequencyFilter(new IntRange(64, 256));
            // 創建低通濾波器，同樣還不確定數字這樣設定對不對...
            FrequencyFilter low_pass_filter = new FrequencyFilter(new IntRange(0, 64));
            pow2Bitmap = EnlargeToPow2(oldImage); // 先取正方形圖片(2次方規格)
            bitmap_8bbp = pow2Bitmap.Clone(new Rectangle(0, 0, pow2Bitmap.Width, pow2Bitmap.Height), PixelFormat.Format8bppIndexed); //確保有將圖片轉為8bpp
            SetGrayscalePalette(bitmap_8bbp); // 轉灰階

            /*
             * 以下為AForge.Net函式應用方法我還不太懂..
             */
            // create complex image from bitmap
            cimage = ComplexImage.FromBitmap(bitmap_8bbp);
            // 做傅立葉轉換
            // 原文:perform forward Fourier transformation
            cimage.ForwardFourierTransform();

            //這時候呈現出來的圖片會是黑白很漂亮的十字架(通常是十字架)
            //尚未過濾波器
            newBitmap = cimage.ToBitmap();

            switch (form1.FILTER_CODE)
            {
                case "HighPass":
                    // 過高通濾波器
                    height_pass_filter.Apply(cimage);
                    break;
                case "LowPass":
                    // 過低通濾波器
                    low_pass_filter.Apply(cimage);
                    break;
            }
            //newBitmap = cimage.ToBitmap();
            // 猜測:做傅立葉反轉換回來
            cimage.BackwardFourierTransform();
            // 轉成點陣圖呈現
            newBitmap = cimage.ToBitmap();
            newBitmap = BimapExtract(cimage.ToBitmap(), oldImage.Width, oldImage.Height);
            return newBitmap;
        }

        /*
         * 將8bpp圖片轉換為灰階
         * 直接用AForge.Net的內建dll方法有點問題
         * 推測是出在我的電腦調色盤只轉換出224種顏色
         * 8bit應該要256色，所以我弄了一個靈活點的方法來轉換
         */
        private static void SetGrayscalePalette(Bitmap oldBitmap)
        {
            if (oldBitmap.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new ArgumentException();
            ColorPalette cp = oldBitmap.Palette;
            for (int i = 0; i < oldBitmap.Palette.Entries.Length; i++)
            {
                cp.Entries[i] = Color.FromArgb(i, i, i);
            }
            oldBitmap.Palette = cp;
        }

        /*
         * 將圖片放大到最小能放大的正方形圖片大小(2次方)
         */
        private Bitmap EnlargeToPow2(Bitmap oldBitmap)
        {
            var width = oldBitmap.Width;
            var height = oldBitmap.Height;
            var pow = 0;
            if (Math.Log(width, 2) != Math.Log(height, 2))
            {
                pow = Math.Log(width, 2) > Math.Log(height, 2)
                    ? (int)Math.Log(width, 2) + 1
                    : (int)Math.Log(height, 2) + 1;

            }
            else
            {
                pow = (int)Math.Log(height, 2) + 1;
            }

            //生成放大的新圖
            Bitmap newBitmap = new Bitmap((int)Math.Pow(2, pow), (int)Math.Pow(2, pow));

            //先把舊圖放上去
            for (int i = 0; i < oldBitmap.Width; i++)
            {
                for (int j = 0; j < oldBitmap.Height; j++)
                {
                    newBitmap.SetPixel(i, j, oldBitmap.GetPixel(i, j));
                }
            }

            //放大部分補黑底
            for (int i = oldBitmap.Width; i < newBitmap.Width; i++)
            {
                for (int j = oldBitmap.Height; j < newBitmap.Height; j++)
                {
                    newBitmap.SetPixel(i, j, Color.Black);
                }
            }

            return newBitmap;

        }

        /*
         * 擷取指定範圍的圖像
         */
        private Bitmap BimapExtract(Bitmap oldBitmap, int width, int height)
        {
            Bitmap newBitmap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    newBitmap.SetPixel(i, j, oldBitmap.GetPixel(i, j));
                }
            }
            return newBitmap;
        }


        /*
         * Final Project 
         */
        private Bitmap Thyroid_Segmentation(Bitmap oldBitmap)
        {
            double[] EGL = GetEveragePerRowGrayLevel(oldBitmap);
            int R1 = findMaxGLIndex(EGL);
            int R2 = findMinGLIndex(EGL, R1);
            Bitmap IntensityBtm = IntensityBtmGetter(oldBitmap, R1, R2);//擷取R1,R2切塊圖
            Bitmap MF_IntensityBtm = IntensityBtm.Clone(new Rectangle(0, 0, IntensityBtm.Width, IntensityBtm.Height), PixelFormat.Format24bppRgb);
            for (int i = 0; i < 25; i++)
            {
                MF_IntensityBtm = MedianFilter(MF_IntensityBtm);
            }
            for (int j = 0; j < 4; j++)
            {
                MF_IntensityBtm = Dilate(MF_IntensityBtm);
            }
            //Bitmap MF_IntensityBtm = Dilate(Dilate(MedianFilter(MedianFilter(MedianFilter(MedianFilter(MedianFilter(MedianFilter(IntensityBtm))))))));//膨脹
            Bitmap MF_GLC_IntensityBtm = GLCompensation(MF_IntensityBtm);//根據甲狀腺區域的強度模板補償不同的圖像
            //Bitmap AOP_MF_GLC_IntensityBtm = CutofGLImage(AndOperatorPic(MF_IntensityBtm, MF_GLC_IntensityBtm, 128 - getGLT(oldBitmap), 0), 0, 32);//128 - getGLT(oldBitmap)參數待調整
            Bitmap AOP_MF_GLC_IntensityBtm = CutofGLImage(MF_GLC_IntensityBtm, 32, 64);//128 - getGLT(oldBitmap)參數待調整
            Bitmap Border_AOP_MF_GLC_IntensityBtm = DrawOutLined(MF_GLC_IntensityBtm, AddBorders(MF_GLC_IntensityBtm, false));


            return AddOutLined(oldBitmap, Border_AOP_MF_GLC_IntensityBtm, R1);
            //return Border_AOP_MF_GLC_IntensityBtm;
        }

        /*
         * 取得每列灰階均值
         */
        private double[] GetEveragePerRowGrayLevel(Bitmap oldBitmap)
        {
            double[] EGL = new double[oldBitmap.Height];
            for (int j = 0; j < oldBitmap.Height; j++)
            {
                double sum = 0;
                for (int i = 0; i < oldBitmap.Width; i++)
                {
                    Color pColor = oldBitmap.GetPixel(i, j);

                    int gColor = (int)(0.299 * pColor.R + 0.587 * pColor.G + 0.114 * pColor.B);
                    sum += gColor;
                }
                EGL[j] = sum / oldBitmap.Width;
            }


            return EGL;
        }
        /*
         * 取得最大灰階值列Id
         */
        private int findMaxGLIndex(double[] EGL)
        {
            int index = -1;
            int count = 0;
            double MAX_GL = -1.0;
            foreach (double item in EGL)
            {
                if (item > MAX_GL)
                {
                    index = count;
                    MAX_GL = item;
                }
                count++;
            }
            return index;
        }
        /*
         * 取得最小灰階值列Id
         */
        private int findMinGLIndex(double[] EGL, int Max_Index)
        {
            int index = -1;
            int count = 0;
            double threshold = 0.0;
            while (index == -1)
            {
                count = 0;
                foreach (double item in EGL)
                {
                    if (item <= threshold && count > Max_Index)
                    {
                        index = count;
                        break;
                    }
                    count++;
                }
                if (index == -1) threshold += 10;
            }


            return index;
        }

        /*
         * 擷取R1,R2切塊圖 
         */
        private Bitmap IntensityBtmGetter(Bitmap oldBitmap, int R1, int R2)
        {
            Bitmap newBitmap = new Bitmap(oldBitmap.Width, R2 - R1 - 1);
            for (int i = 0; i < oldBitmap.Width; i++)
            {
                int h_index = 0;
                for (int j = 0; j < oldBitmap.Height; j++)
                {
                    if (j > R1 && j < R2)
                    {
                        newBitmap.SetPixel(i, h_index, oldBitmap.GetPixel(i, j));
                        h_index++;
                    }

                }
            }
            return newBitmap;
        }

        /*
         * 根據甲狀腺區域的強度模板補償不同的圖像
         */
        private Bitmap GLCompensation(Bitmap oldBitmap)
        {
            Bitmap newImage = new Bitmap(oldBitmap.Width, oldBitmap.Height);
            int GLN = 128;
            int GLT = getGLT(oldBitmap);

            int GLD = GLN - GLT;
            for (int i = 0; i < oldBitmap.Width; i++)
            {
                for (int j = 0; j < oldBitmap.Height; j++)
                {
                    Color pColor = oldBitmap.GetPixel(i, j);

                    int gColor = (int)(0.299 * pColor.R + 0.587 * pColor.G + 0.114 * pColor.B);
                    if (gColor - GLD > 255) gColor = 255;
                    else if (gColor - GLD < 0) gColor = 0;
                    else gColor -= GLD;

                    newImage.SetPixel(i, j, Color.FromArgb(gColor, gColor, gColor));

                }
            }
            return newImage;
        }

        /*
         * 取得整張圖n*n個色素前20%高的平均數值
         */
        private int getGLT(Bitmap oldBitmap)
        {
            int GLT = 0;
            int[] p_oldBitmap_Arr = new int[oldBitmap.Width * oldBitmap.Height];
            for (int i = 0; i < oldBitmap.Width; i++)
            {
                for (int j = 0; j < oldBitmap.Height; j++)
                {
                    Color pColor = oldBitmap.GetPixel(i, j);

                    int gColor = (int)(0.299 * pColor.R + 0.587 * pColor.G + 0.114 * pColor.B);
                    p_oldBitmap_Arr[j * oldBitmap.Width + i] = gColor;

                }
            }

            Array.Sort(p_oldBitmap_Arr);
            for (int i = (int)(p_oldBitmap_Arr.Length * 0.8); i < p_oldBitmap_Arr.Length; i++)
            {
                GLT += (int)p_oldBitmap_Arr[i];
            }
            GLT /= (int)(p_oldBitmap_Arr.Length * 0.2);
            return GLT;
        }

        /*
         * 新舊圖做And
         * mode 0 -> 除去
         * mode 1 -> 保留
         */
        private Bitmap AndOperatorPic(Bitmap Bitmap1, Bitmap Bitmap2, int err, int mode)
        {
            Bitmap newBitmap = new Bitmap(Bitmap1.Width, Bitmap1.Height);
            for (int i = 0; i < Bitmap1.Width; i++)
            {
                for (int j = 0; j < Bitmap1.Height; j++)
                {
                    if (Math.Abs(Bitmap1.GetPixel(i, j).R - Bitmap2.GetPixel(i, j).R) < err)
                    {
                        Color pColor = Bitmap2.GetPixel(i, j);
                        int gColor = (int)(0.299 * pColor.R + 0.587 * pColor.G + 0.114 * pColor.B);
                        newBitmap.SetPixel(i, j, Color.FromArgb(gColor * mode, gColor * mode, gColor * mode));
                    }
                    else
                    {
                        newBitmap.SetPixel(i, j, Bitmap2.GetPixel(i, j));
                    }
                }
            }
            return newBitmap;
        }

        /*
         * 畫邊線
         */
        private Bitmap DrawOutLined(Bitmap oldBitmap, Bitmap processImage)
        {

            int[] GL = new int[oldBitmap.Width];

            for (int k = 0; k < oldBitmap.Width; k++)
            {
                Color pColor = oldBitmap.GetPixel(k, 0);

                int gColor = (int)(0.299 * pColor.R + 0.587 * pColor.G + 0.114 * pColor.B);
                GL[k] = gColor;
            }
            Array.Sort(GL);
            int GL_value = GL[0];


            for (int i = 1; i < processImage.Width - 1; i++)
            {
                int Max_X = oldBitmap.Width;//圖片座標初始點在左上，所以這變數要比小
                int Min_X = 0;//比大
                for (int j = 1; j < processImage.Height - 1; j++)
                {
                    /*
                    * 取九宮格色塊
                    */
                    Color[] color_arr = new Color[8];
                    color_arr[0] = processImage.GetPixel(i - 1, j - 1);//左上
                    color_arr[1] = processImage.GetPixel(i - 1, j);//上
                    color_arr[2] = processImage.GetPixel(i - 1, j + 1);//右上
                    color_arr[3] = processImage.GetPixel(i, j - 1);//左
                    Color center_color = processImage.GetPixel(i, j);//中
                    color_arr[4] = processImage.GetPixel(i, j + 1);//右
                    color_arr[5] = processImage.GetPixel(i + 1, j - 1);//左下
                    color_arr[6] = processImage.GetPixel(i + 1, j);//下
                    color_arr[7] = processImage.GetPixel(i + 1, j + 1);//右下
                    //int count = 0;
                    //foreach (Color color in color_arr)
                    //{
                    //    int p_Color_Gray = (int)(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                    //    int p_center_Color_Gray = (int)(center_color.R * 0.299 + center_color.G * 0.587 + center_color.B * 0.114);
                    //    if (p_Color_Gray < 24) count++;//符合條件的算入，作為邊界識別
                    //}
                    //if (count >=3 && count <= 5)
                    //{
                    //    oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                    //}







                    if (color_arr[0].R == color_arr[1].R && color_arr[1].R == color_arr[2].R && color_arr[2].R != center_color.R)
                    {
                        if (j > Min_X && center_color.R == 0)
                        {
                            oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                            Min_X = j;
                        }
                        else if (j < Max_X && center_color.R == GL_value)
                        {
                            oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                            Max_X = j;
                        }
                    }
                    if (color_arr[0].R == color_arr[3].R && color_arr[3].R == color_arr[5].R && color_arr[5].R != center_color.R)
                    {
                        if (j > Min_X && center_color.R == 0)
                        {
                            oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                            Min_X = j;
                        }
                        else if (j < Max_X && center_color.R == GL_value)
                        {
                            oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                            Max_X = j;
                        }
                    }
                    if (color_arr[5].R == color_arr[6].R && color_arr[6].R == color_arr[7].R && color_arr[7].R != center_color.R)
                    {
                        if (j > Min_X && center_color.R == 0)
                        {
                            oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                            Min_X = j;
                        }
                        else if (j < Max_X && center_color.R == GL_value)
                        {
                            oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                            Max_X = j;
                        }
                    }
                    if (color_arr[2].R == color_arr[4].R && color_arr[4].R == color_arr[7].R && color_arr[7].R != center_color.R)
                    {
                        if (j > Min_X && center_color.R == 0)
                        {
                            oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                            Min_X = j;
                        }
                        else if (j < Max_X && center_color.R == GL_value)
                        {
                            oldBitmap.SetPixel(i - 1, j - 1, Color.FromArgb(49, 255, 0));//49, 255, 0
                            Max_X = j;
                        }
                    }

                }
            }

            return oldBitmap;
        }

        /*
         * 侵蝕
         */
        private Bitmap Erosion(Bitmap oldBitmap)
        {
            Bitmap bitmap = oldBitmap.Clone(new Rectangle(0, 0, oldBitmap.Width, oldBitmap.Height), PixelFormat.Format8bppIndexed); //確保有將圖片轉為8bpp
            // create filter
            Erosion filter = new Erosion();
            // apply the filter
            filter.ApplyInPlace(bitmap);
            Bitmap newImage = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format24bppRgb);
            return newImage;
        }

        /*
         * 膨脹
         */
        private Bitmap Dilate(Bitmap oldBitmap)
        {
            Bitmap bitmap = oldBitmap.Clone(new Rectangle(0, 0, oldBitmap.Width, oldBitmap.Height), PixelFormat.Format8bppIndexed); //確保有將圖片轉為8bpp
            // create filter
            Dilatation filter = new Dilatation();
            // apply the filter
            filter.ApplyInPlace(bitmap);
            Bitmap newImage = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format24bppRgb);
            return newImage;
        }

        /*
         * 裁減指定色素範圍內的圖像出來
         */
        private Bitmap CutofGLImage(Bitmap oldBitmap, int range_start, int range_end)
        {
            Bitmap newBitmap = new Bitmap(oldBitmap.Width, oldBitmap.Height);
            for (int i = 0; i < oldBitmap.Width; i++)
            {
                for (int j = 0; j < oldBitmap.Height; j++)
                {
                    Color pColor = oldBitmap.GetPixel(i, j);
                    int gColor = (int)(0.299 * pColor.R + 0.587 * pColor.G + 0.114 * pColor.B);
                    if (gColor >= range_start && gColor <= range_end)
                    {
                        newBitmap.SetPixel(i, j, Color.FromArgb(gColor, gColor, gColor));
                    }
                    else
                    {
                        newBitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                }
            }
            return newBitmap;
        }


        /*
         * 把邊框加回去原本的圖上
         */
        private Bitmap AddOutLined(Bitmap oldBitmap, Bitmap OutLinedBitmap, int R1)
        {
            Bitmap newImage = oldBitmap.Clone(new Rectangle(0, 0, oldBitmap.Width, oldBitmap.Height), PixelFormat.Format24bppRgb);
            for (int i = 0; i < OutLinedBitmap.Width; i++)
            {
                for (int j = 0; j < OutLinedBitmap.Height; j++)
                {
                    Color pColor = OutLinedBitmap.GetPixel(i, j);
                    if (pColor.R == 49 && pColor.G == 255 && pColor.B == 0)
                    {
                        newImage.SetPixel(i, j + R1, Color.FromArgb(49, 255, 0));
                    }
                    else
                    {
                        newImage.SetPixel(i, j + R1, oldBitmap.GetPixel(i, j + R1));
                    }
                }
            }
            return newImage;
        }

    }

}

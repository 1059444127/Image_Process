using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

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

        private Boolean ExcuteFunction(string ExcuteMode)
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
                    form1.BITMAP = newImage ?? form1.BITMAP ;
                    break;
                case "Median filter":
                    newImage = MedianFilter(oldImage);
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
                case "ZOOM":
                    newImage = new Bitmap((oldImage.Width * form1.MUTIPLE - 1), (oldImage.Height * form1.MUTIPLE - 1));
                    for (int i = 0; i < oldImage.Width - 1; i++)
                    {
                        for (int j = 0; j < oldImage.Height - 1; j++)
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
                case "OUT":
                    newImage = new Bitmap((oldImage.Width / form1.MUTIPLE - 1), (oldImage.Height / form1.MUTIPLE - 1));
                    for (int i = 0; i < oldImage.Width / form1.MUTIPLE - 1; i++)
                    {
                        for (int j = 0; j < oldImage.Height / form1.MUTIPLE - 1; j++)
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
                case "ZOOM":
                    newImage = new Bitmap((oldImage.Width * form1.MUTIPLE - 1), (oldImage.Height * form1.MUTIPLE - 1));
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
                case "OUT":
                    newImage = new Bitmap((oldImage.Width / form1.MUTIPLE) - 1, (oldImage.Height / form1.MUTIPLE) - 1);
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

                        // 灰階版本壓縮率疑似更好
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
            int[] p_array ;
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
        public Bitmap GenerateNoise(Bitmap oldImage,int weight)
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
                    int num = r.Next(0, 256);
                    newImage.SetPixel(i, j % image_Hight, Color.FromArgb(255, num, num, num));
                    j++;
                }
            }

            return newImage;
        }

        /**
         * Median filter
         * 
         */
        private Bitmap MedianFilter(Bitmap oldImage)
        {
            
            Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height);
            Bitmap processImage = AddBorders(oldImage);
            for (int i = 1; i < oldImage.Width+1; i++)
            {
                for (int j = 1; j < oldImage.Height+1; j++)
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
        public Bitmap AddBorders(Bitmap oldBitmap)
        {
            Bitmap newBitmap = new Bitmap(oldBitmap.Width+2,oldBitmap.Height+2);

            for (int i = 0; i < oldBitmap.Width; i++)
            {
                for (int j = 0; j < oldBitmap.Height; j++)
                {
                    if (i == 0 || i == oldBitmap.Width + 1 ||
                        j == 0 || j == oldBitmap.Height + 1)
                    {
                        newBitmap.SetPixel(i, j, Color.FromArgb(255,255,255));
                    }
                    else
                    {
                        newBitmap.SetPixel(i+1, j+1, oldBitmap.GetPixel(i, j));
                    }
                    
                }
            }

            return newBitmap;
        }



    }

}

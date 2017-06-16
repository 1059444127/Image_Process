using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Image_Zoom_in_out
{
    public partial class Form1 : Form
    {
        private static Form1 mFrom = null;
        // 建立執行續物件，這並不代表啟動執行續
        private Worker mWorkerObject;
        private Thread mWorkerThread;

        private bool mZoom_in_out_RadioChecked;
        private bool mFilter_RadioChecked;
        private bool mFull_Color;

        private string mPic_Path = "";
        private string mExcuteMode = "";
        private string mZO_Code = "";
        private int mMutiple;
        private double mGamma;
        private string mFilter_Code = "";
        public static Bitmap mBitmap;
        public Bitmap[] mPixelizationImageArray;
        public Bitmap[] mNoiseImageArray = new Bitmap[8];

        public Form1()
        {
            InitializeComponent();
        }

        private void CreateThread()
        {

            // 啟動目標執行續
            mWorkerThread.Start();

            // 等待目標執行續
            while (!mWorkerThread.IsAlive) ;

            // 讓主執行續盡速睡眠一毫秒，好讓目標執行續有機可趁XD
            Thread.Sleep(1);


            // 這句我還不確定它存在意義QAQ
            // Use the Join method to block the current thread 
            // until the object's thread terminates.
            mWorkerThread.Join();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png|bmp files (*.bmp)|*.bmp";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = false;


            DialogResult result = openFileDialog1.ShowDialog(); //顯示選檔視窗
            if (result == DialogResult.OK) // 測試結果
            {
                string file = openFileDialog1.FileName;
                try
                {
                    //在這實作選檔後要執行的動作
                    mBitmap = new Bitmap(file);//pictureBox1藉由路徑開圖檔
                    pictureBox1.Image = mBitmap;
                    pictureBox2.Image = mBitmap;
                    label2.Text = file.Split('\\')[file.Split('\\').Length - 1];
                    PixelLabel.Text = mBitmap.Width + "x" + mBitmap.Height + "Pixel.";
                    OriginalPixelLabel.Text = "原圖 : " + mBitmap.Width + "x" + mBitmap.Height + "Pixel.";
                    mPic_Path = file;



                    if (listBox1.SelectedIndex == 8)
                    {
                        pictureBox2.Visible = false;
                        SubPicBoxVisible(true);
                        mFrom = this;
                        mWorkerObject = new Worker(mFrom);
                        mWorkerThread = new Thread(mWorkerObject.DoPreproccess);
                        //副執行續開工拉
                        CreateThread();
                        SetSubPicBox(mNoiseImageArray);
                    }
                }
                catch (IOException)
                {
                    StatusLabel.Text = "開檔錯誤!";
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "處理中...";
            StatusValidation();
            if (listBox1.SelectedIndex == 6 && BITMAP != null) SetSubPicBox(mPixelizationImageArray);
            StatusLabel.Text = "處理完畢";
            if (listBox1.SelectedIndex == 8 && BITMAP != null)
            {
                NewPictureBoxBox.Visible = true;
                SubPicBoxVisible(false);

            }
            NewPictureBoxBox.Image = BITMAP;



        }

        private void button3_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "處理中...";
            mBitmap = new Bitmap(mPic_Path);//pictureBox2藉由路徑開圖檔
            pictureBox2.Image = mBitmap;
            PixelLabel.Text = mBitmap.Width + "x" + mBitmap.Height + "Pixel.";
            StatusLabel.Text = "處理完畢";

            if (listBox1.SelectedIndex == 6 || listBox1.SelectedIndex == 8)
            {
                NewPictureBoxBox.Visible = false;
                SubPicBoxVisible(true);
                if (listBox1.SelectedIndex == 6) SetSubPicBox(mPixelizationImageArray);
                if (listBox1.SelectedIndex == 8) SetSubPicBox(mNoiseImageArray);
            }
        }

        private Boolean StatusValidation()
        {
            try
            {
                if (listBox1.SelectedIndex > -1 && BITMAP != null)
                {
                    mExcuteMode = listBox1.Text;

                    if ((textBox1.Enabled && int.TryParse(textBox1.Text, out mMutiple) && mMutiple > 0) || (textBox2.Enabled && double.TryParse(textBox2.Text, out mGamma) && mGamma > 0) || (listBox1.SelectedIndex != 0 & listBox1.SelectedIndex != 1 & listBox1.SelectedIndex != 2 & listBox1.SelectedIndex != 5))
                    {


                        if (radioButton1.Checked)
                        {
                            mZO_Code = "ZOOM_IN";
                        }
                        else if (radioButton2.Checked)
                        {
                            mZO_Code = "ZOOM_OUT";
                        }

                        if (radioButton3.Checked)
                        {
                            mFilter_Code = "HighPass";
                            mFilter_RadioChecked = true;
                        }
                        else if (radioButton4.Checked)
                        {
                            mFilter_Code = "LowPass";
                            mFilter_RadioChecked = true;
                        }

                        // 即使模式不提供放大縮小和高低通也要將狀態設定為isChecked
                        if (mZoom_in_out_RadioChecked && mFilter_RadioChecked)
                        {
                            NoticeLabel.Text = "";
                            mFrom = this;
                            mWorkerObject = new Worker(mFrom);
                            mWorkerThread = new Thread(mWorkerObject.DoWork);
                            //副執行續開工拉
                            CreateThread();
                        }
                        else
                        {
                            NoticeLabel.Text = "請先選擇目的或濾波器設定";
                        }

                    }
                    else
                    {
                        NoticeLabel.Text = "倍率/Gamma值不合常理\n或暫不支援";

                    }

                    PixelLabel.Text = BITMAP.Width + "x" + BITMAP.Height + "Pixel.";
                }
                else
                {
                    NoticeLabel.Text = "請選擇模式和圖檔";

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message); //測試用
            }

            return true;
        }

        private void SubPicBoxVisible(bool Bool)
        {
            sub_PictureBox0.Visible = Bool;
            sub_PictureBox1.Visible = Bool;
            sub_PictureBox2.Visible = Bool;
            sub_PictureBox3.Visible = Bool;
            sub_PictureBox4.Visible = Bool;
            sub_PictureBox5.Visible = Bool;
            sub_PictureBox6.Visible = Bool;
            sub_PictureBox7.Visible = Bool;
        }

        public void SetSubPicBox(Bitmap[] sub_PictureBox)
        {
            bool picStatusCheck = true;
            foreach (var VARIABLE in sub_PictureBox)
            {
                if (VARIABLE == null) picStatusCheck = false;
            }
            if (picStatusCheck)
            {
                sub_PictureBox0.Image = sub_PictureBox[0];
                sub_PictureBox1.Image = sub_PictureBox[1];
                sub_PictureBox2.Image = sub_PictureBox[2];
                sub_PictureBox3.Image = sub_PictureBox[3];
                sub_PictureBox4.Image = sub_PictureBox[4];
                sub_PictureBox5.Image = sub_PictureBox[5];
                sub_PictureBox6.Image = sub_PictureBox[6];
                sub_PictureBox7.Image = sub_PictureBox[7];
            }
            else
            {
                StatusLabel.Text = "圖片產生有少，或過程發生錯誤";
            }

        }



        public Button InputImageButton { get { return button1; } }
        public Button ExcuteButton { get { return button2; } }
        public Button RecoveryButton { get { return button3; } }
        public ListBox ExecuteModeList { get { return listBox1; } }
        public RadioButton ZoomButton { get { return radioButton1; } }
        public RadioButton OutButton { get { return radioButton2; } }
        public TextBox MultipleTextBox { get { return textBox1; } }
        public PictureBox OrininalPictureBox { get { return pictureBox1; } }
        public PictureBox NewPictureBoxBox { get { return pictureBox2; } }
        public PictureBox sub_PictureBox0 { get { return pictureBox3; } }
        public PictureBox sub_PictureBox1 { get { return pictureBox4; } }
        public PictureBox sub_PictureBox2 { get { return pictureBox5; } }
        public PictureBox sub_PictureBox3 { get { return pictureBox6; } }
        public PictureBox sub_PictureBox4 { get { return pictureBox7; } }
        public PictureBox sub_PictureBox5 { get { return pictureBox8; } }
        public PictureBox sub_PictureBox6 { get { return pictureBox9; } }
        public PictureBox sub_PictureBox7 { get { return pictureBox10; } }
        public CheckBox FullColorChkBox { get { return checkBox1; } }
        public Label PixelLabel { get { return label4; } }
        public Label StatusLabel { get { return label5; } }
        public Label NoticeLabel { get { return label7; } }
        public Label OriginalPixelLabel { get { return label6; } }
        public Bitmap BITMAP { get { return mBitmap; } set { mBitmap = value; } }
        public String EXCUTEMODE { get { return mExcuteMode; } }
        public String ZO_CODE { get { return mZO_Code; } }
        public String FILTER_CODE { get { return mFilter_Code; } }
        public int MUTIPLE { get { return mMutiple; } }
        public double GAMMA { get { return mGamma; } }
        public Boolean FULL_COLOR { get { return mFull_Color; } }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 這邊就各種開關各種設定，除此外在其他方法內也有一點，都是一些特殊方法
            int index = ExecuteModeList.FindString(ExecuteModeList.SelectedItem.ToString());
            /**
                3.      Negatives
                4.      Log
                5.      Power-Law
                6.      Bit-plane
                7.      Histogram Processing
                8.      Averaging filter
                9.      Median filter
               10.      Laplacian
               11.      Fourier Transformation
               12.      Thyroid Segmentation
             */
            switch (index)
            {
                case 0:
                    groupBox1.Enabled = true;
                    groupBox2.Enabled = false;
                    mZoom_in_out_RadioChecked = false;
                    mFilter_RadioChecked = true;
                    textBox1.Enabled = true;
                    NewPictureBoxBox.Visible = true;
                    FullColorChkBox.Enabled = false;
                    FullColorChkBox.CheckState = CheckState.Checked;
                    SubPicBoxVisible(false);
                    textBox1.Text = "";

                    break;
                case 1:
                    groupBox1.Enabled = true;
                    groupBox2.Enabled = false;
                    mZoom_in_out_RadioChecked = false;
                    mFilter_RadioChecked = true;
                    textBox1.Enabled = true;
                    NewPictureBoxBox.Visible = true;
                    FullColorChkBox.Enabled = false;
                    FullColorChkBox.CheckState = CheckState.Checked;
                    SubPicBoxVisible(false);
                    textBox1.Text = "";
                    break;
                case 2:
                    groupBox1.Enabled = true;
                    groupBox2.Enabled = false;
                    mZoom_in_out_RadioChecked = false;
                    mFilter_RadioChecked = true;
                    textBox1.Enabled = true;
                    NewPictureBoxBox.Visible = true;
                    FullColorChkBox.Enabled = false;
                    FullColorChkBox.CheckState = CheckState.Checked;
                    SubPicBoxVisible(false);
                    textBox1.Text = "";
                    break;
                //case 3:
                //    groupBox1.Enabled = false;
                //    groupBox2.Enabled = false;
                //    mZoom_in_out_RadioChecked = true;
                //    mFilter_RadioChecked = true;
                //    textBox1.Enabled = false;
                //    textBox2.Enabled = false;
                //    NewPictureBoxBox.Visible = true;
                //    FullColorChkBox.Enabled = true;
                //    SubPicBoxVisible(false);
                //    textBox1.Text = "1";
                //    textBox1.Text = "1.0";
                //    mZO_Code = "NONE";
                //    break;
                //case 4:
                //    groupBox1.Enabled = false;
                //    groupBox2.Enabled = false;
                //    mZoom_in_out_RadioChecked = true;
                //    mFilter_RadioChecked = true;
                //    textBox1.Enabled = false;
                //    textBox2.Enabled = false;
                //    NewPictureBoxBox.Visible = true;
                //    FullColorChkBox.Enabled = true;
                //    SubPicBoxVisible(false);
                //    textBox1.Text = "1";
                //    textBox2.Text = "1.0";
                //    mZO_Code = "NONE";
                //    break;
                case 5:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                    mZoom_in_out_RadioChecked = true;
                    mFilter_RadioChecked = true;
                    textBox1.Enabled = false;
                    textBox2.Enabled = true;
                    NewPictureBoxBox.Visible = true;
                    FullColorChkBox.Enabled = true;
                    SubPicBoxVisible(false);
                    textBox1.Text = "1";
                    textBox2.Text = "1.0";
                    //mZO_Code = "NONE";
                    break;
                case 6:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                    mZoom_in_out_RadioChecked = true;
                    mFilter_RadioChecked = true;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    NewPictureBoxBox.Visible = false;
                    FullColorChkBox.Enabled = true;
                    SubPicBoxVisible(true);
                    textBox1.Text = "1";
                    textBox2.Text = "1.0";
                    //mZO_Code = "NONE";
                    break;
                //case 7:
                //    groupBox1.Enabled = false;
                //    groupBox2.Enabled = false;
                //    mZoom_in_out_RadioChecked = true;
                //    mFilter_RadioChecked = true;
                //    textBox1.Enabled = false;
                //    textBox2.Enabled = false;
                //    NewPictureBoxBox.Visible = true;
                //    FullColorChkBox.Enabled = true;
                //    SubPicBoxVisible(false);
                //    textBox1.Text = "1";
                //    textBox1.Text = "1.0";
                //    mZO_Code = "NONE";
                //    break;
                case 8:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                    mZoom_in_out_RadioChecked = true;
                    mFilter_RadioChecked = true;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    NewPictureBoxBox.Visible = false;
                    FullColorChkBox.Enabled = true;
                    SubPicBoxVisible(false);
                    textBox1.Text = "1";
                    textBox1.Text = "1.0";
                    //mZO_Code = "NONE";

                    if (BITMAP != null)
                    {
                        SubPicBoxVisible(true);
                        mFrom = this;
                        mWorkerObject = new Worker(mFrom);
                        mWorkerThread = new Thread(mWorkerObject.DoPreproccess);
                        //副執行續開工拉
                        CreateThread();
                        SetSubPicBox(mNoiseImageArray);
                        NewPictureBoxBox.InitialImage = null;
                    }

                    break;
                //case 9:
                //    groupBox1.Enabled = false;
                //    groupBox2.Enabled = false;
                //    mZoom_in_out_RadioChecked = true;
                //    mFilter_RadioChecked = true;
                //    textBox1.Enabled = false;
                //    textBox2.Enabled = false;
                //    NewPictureBoxBox.Visible = true;
                //    FullColorChkBox.Enabled = true;
                //    SubPicBoxVisible(false);
                //    textBox1.Text = "1";
                //    textBox2.Text = "1.0";
                //    mZO_Code = "NONE";
                //    break;
                //case 10:
                //    groupBox1.Enabled = false;
                //    groupBox2.Enabled = false;
                //    mZoom_in_out_RadioChecked = true;
                //    mFilter_RadioChecked = true;
                //    textBox1.Enabled = false;
                //    textBox2.Enabled = false;
                //    NewPictureBoxBox.Visible = true;
                //    FullColorChkBox.Enabled = true;
                //    SubPicBoxVisible(false);
                //    textBox1.Text = "1";
                //    textBox2.Text = "1.0";
                //    mZO_Code = "NONE";
                //    break;
                case 11:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = true;
                    mZoom_in_out_RadioChecked = true;
                    mFilter_RadioChecked = false;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    NewPictureBoxBox.Visible = true;
                    FullColorChkBox.Enabled = true;
                    SubPicBoxVisible(false);
                    textBox1.Text = "1";
                    textBox2.Text = "1.0";
                    //mZO_Code = "NONE";
                    break;
                case 12:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = true;
                    mZoom_in_out_RadioChecked = true;
                    mFilter_RadioChecked = false;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    NewPictureBoxBox.Visible = true;
                    FullColorChkBox.Enabled = true;
                    SubPicBoxVisible(false);
                    textBox1.Text = "1";
                    textBox2.Text = "1.0";
                    //mZO_Code = "NONE";
                    break;
                default:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                    mZoom_in_out_RadioChecked = true;
                    mFilter_RadioChecked = true;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    NewPictureBoxBox.Visible = true;
                    FullColorChkBox.Enabled = true;
                    SubPicBoxVisible(false);
                    textBox1.Text = "1";
                    textBox2.Text = "1.0";
                    //mZO_Code = "NONE";
                    break;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            mFull_Color = !mFull_Color;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            mZoom_in_out_RadioChecked = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            mZoom_in_out_RadioChecked = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            mFilter_RadioChecked = true;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            mFilter_RadioChecked = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StatusLabel.Text = "待機狀態";
        }


    }
}

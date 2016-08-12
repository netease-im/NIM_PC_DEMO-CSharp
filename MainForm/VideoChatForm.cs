using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NIM;

namespace NIMDemo.MainForm
{
    public partial class VideoChatForm : Form
    {
        private Graphics _peerRegionGraphics;
        private Graphics _mineRegionGraphics;
        //private MultimediaHandler _multimediaHandler;
		private nim_vchat_mp4_record_opt_cb_func _startcb = null;
		private nim_vchat_mp4_record_opt_cb_func _stopcb = null;
		private bool mute = false;
		private bool record = false;
        const int RenderInterval = 60;
        const int MaxFrameCount = 3;
        public VideoChatForm()
        {
            InitializeComponent();
			_startcb = new nim_vchat_mp4_record_opt_cb_func(VChatRecordStartCallback); 
            this.Load += VideoChatForm_Load;
            this.FormClosed += VideoChatForm_FormClosed;
        }

		private void VChatRecordStartCallback(bool ret, int code,string file,Int64 time,string json_extension,IntPtr user_data)
		{
			if(ret)
			{
				MessageBox.Show("开始录制");
			}
			else
			{
				MessageBox.Show("录制失败-错误码:" + code.ToString());
			}
		}

        //public VideoChatForm(MultimediaHandler mh)
        //    : this()
        //{
        //   // _multimediaHandler = mh;
       
        //}

        private void VideoChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //if (_multimediaHandler != null)
            {
                MultimediaHandler.ReceiveVideoFrameHandler -= OnReceivedVideoFrame;
                MultimediaHandler.CapturedVideoFrameHandler -= OnCapturedVideoFrame;
            }
            NIM.VChatAPI.End();
            _peerRegionGraphics.Dispose();
            _mineRegionGraphics.Dispose();
           // _multimediaHandler = null;
        }

        private void VideoChatForm_Load(object sender, EventArgs e)
        {
            _peerRegionGraphics = peerPicBox.CreateGraphics();
            _mineRegionGraphics = minePicBox.CreateGraphics();
            //if (_multimediaHandler != null)
            {
                MultimediaHandler.ReceiveVideoFrameHandler += OnReceivedVideoFrame;
                MultimediaHandler.CapturedVideoFrameHandler += OnCapturedVideoFrame;
            }
        }

        private void OnCapturedVideoFrame(object sender, VideoEventAgrs e)
        {
            ShowVideoFrame(_mineRegionGraphics, minePicBox.Width, minePicBox.Height, e.Frame);
        }

        private void OnReceivedVideoFrame(object sender,VideoEventAgrs args)
        {
            ShowVideoFrame(_peerRegionGraphics, peerPicBox.Width, peerPicBox.Height, args.Frame);
        }

        void ShowVideoFrame(Graphics graphics,int w,int h, VideoFrame frame)
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.Default;
            var stream = frame.GetBmpStream();
            Bitmap img = new Bitmap(stream);
            //等比例缩放
            float sa = (float)w / frame.Width;
            float sb = (float)h / frame.Height;
            var scale = Math.Min(sa, sb);
            var newWidth = frame.Width*scale;
            var newHeight = frame.Height*scale;
            stream.Dispose();
            graphics.DrawImage(img, new Rectangle((int)(w-newWidth)/2, (int)(h-newHeight)/2, (int)newWidth,(int)newHeight), 
                new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
            img.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

		private void btnRecord_Click(object sender, EventArgs e)
		{
			Random random = new Random();
			string path = Application.StartupPath + @"\" + random.Next().ToString() + @".mp4";
			string json_extension = "";
			record = !record;
			if(record)
			{
				btnRecord.Text = "停止录音";
				NIM.VChatAPI.StartRecord(path, json_extension, _startcb, IntPtr.Zero);
			}
			else
			{
				btnRecord.Text = "开始录音";
				NIM.VChatAPI.StopRecord(json_extension, _stopcb, IntPtr.Zero);
			}
			
		}

		private void btnSetMute_Click_1(object sender, EventArgs e)
		{
			if (mute)
				btnSetMute.Text = "取消静音";
			else
				btnSetMute.Text = "设置静音";
			NIM.VChatAPI.SetAudioMute(mute);
			mute = !mute;
		}

        private void bt_setting_Click(object sender, EventArgs e)
        {
            AVChat.AVDevicesSettingForm form = new AVChat.AVDevicesSettingForm();
            form.ShowDialog();
        }
	}
}

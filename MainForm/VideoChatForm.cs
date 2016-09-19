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
using System.Runtime.InteropServices;

namespace NIMDemo.MainForm
{
    public partial class VideoChatForm : Form
    {
		private bool beauty_ = false;//是否已开启美颜
        private Graphics _peerRegionGraphics;
        private Graphics _mineRegionGraphics;
        //private MultimediaHandler _multimediaHandler;
		private nim_vchat_mp4_record_opt_cb_func _startcb = null;
		private nim_vchat_mp4_record_opt_cb_func _stopcb = null;
		private nim_vchat_opt_cb_func _setvideoqualitycb = null;
        private nim_vchat_opt_cb_func _set_custom_videocb = null;

		private bool mute = false;
		private bool record = false;
        const int RenderInterval = 60;
        const int MaxFrameCount = 3;
        public VideoChatForm()
        {
            InitializeComponent();
			InitQuality();
			_startcb = new nim_vchat_mp4_record_opt_cb_func(VChatRecordStartCallback); 

            this.Load += VideoChatForm_Load;
            this.FormClosed += VideoChatForm_FormClosed;
        }

		private void InitQuality()
		{
			foreach(var quality in Enum.GetValues(typeof(NIMVChatVideoQuality)))
			{
				cb_setquality.Items.Add(quality);				
			}
			if (cb_setquality.Items.Count > 0)
				cb_setquality.SelectedIndex = 0;
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
             //如果开启了美颜
			if(beauty_)
			{
                uint size=Convert.ToUInt32(e.Frame.Width*e.Frame.Height*3/2);
                //处理数据
                byte[] i420=NIMDemo.LivingStreamSDK.YUVHelper.ARGBToI420(e.Frame.Data,e.Frame.Width,e.Frame.Height);
                Beauty.Smooth.smooth_process(i420,e.Frame.Width,e.Frame.Height,10,0,200);
                e.Frame.Data = NIMDemo.LivingStreamSDK.YUVHelper.I420ToARGB(i420, e.Frame.Width, e.Frame.Height);
                try
                {
                    //发送自定义数据
                    TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    ulong time = Convert.ToUInt64(ts.TotalMilliseconds);
                    NIMDemo.LivingStreamSDK.YUVHelper.i420Revert(ref i420,e.Frame.Width,e.Frame.Height);
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(i420.Length);
                    Marshal.Copy(i420, 0, unmanagedPointer, i420.Length);
                    NIM.DeviceAPI.CustomVideoData(time, unmanagedPointer, size, (uint)e.Frame.Width, (uint)e.Frame.Height);
                    Marshal.FreeHGlobal(unmanagedPointer);
                }
                catch(Exception ex)
                {

                }
                //本地显示数据 
                ShowVideoFrame(_mineRegionGraphics, minePicBox.Width, minePicBox.Height, e.Frame);
			}
            else
            { 
                ShowVideoFrame(_mineRegionGraphics, minePicBox.Width, minePicBox.Height, e.Frame);
            }
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

		private void cb_setquality_SelectedIndexChanged(object sender, EventArgs e)
		{
			_setvideoqualitycb = new nim_vchat_opt_cb_func((ret, code, json, intptr) =>
			{
				//ret  true
				//设置成功
			});
			NIMVChatVideoQuality quality =(NIMVChatVideoQuality)((ComboBox)sender).SelectedItem;
			NIM.VChatAPI.SetVideoQuality(quality, "", _setvideoqualitycb, IntPtr.Zero);
		}

		private void btn_beauty_Click(object sender, EventArgs e)
		{
            if (_set_custom_videocb == null)
            {
                _set_custom_videocb = new nim_vchat_opt_cb_func((ret, code, json, intptr) =>
                {
                    if (ret)
                    {
                        beauty_ = !beauty_;
                        Action action = () =>
                        {
                            if (beauty_)
                                btn_beauty.Text = "美颜(关)";
                            else
                                btn_beauty.Text = "美颜(开)";
                        };
                        this.Invoke(action);

                    }
                    //ret  true
                    //设置成功
                });
            }
            NIM.VChatAPI.SetCustomData(false, !beauty_, "", _set_custom_videocb, IntPtr.Zero);

            //beauty_ = !beauty_;
            //Action action = () =>
            //{
            //    if (beauty_)
            //        btn_beauty.Text = "美颜(关)";
            //    else
            //        btn_beauty.Text = "美颜(开)";
            //};
            //this.Invoke(action);
            
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NIMDemo.LivingStreamSDK;
using System.Threading;
using NIMDemo.AVChat;
using System.Drawing.Drawing2D;
using NIMDemo.MainForm;
using NIM;

namespace NIMDemo
{
	public partial class LivingStreamForm : Form
	{
		Graphics graphics = null;
		public LivingStreamForm()
		{
			InitializeComponent();
			graphics = pb_livingstream.CreateGraphics();
		}

	

		private void btn_Click(object sender, EventArgs e)
		{
			AVDevicesSettingForm form = new AVDevicesSettingForm();
			form.ShowDialog();
		}

		private void btn_bypass_Click(object sender, EventArgs e)
		{

		}

		public void ShowVideoFrame(VideoFrame frame)
		{
			int w = pb_livingstream.Width;
			int h = pb_livingstream.Height;
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.Default;
			var stream = frame.GetBmpStream();
			Bitmap img = new Bitmap(stream);
			//等比例缩放
			float sa = (float)w / frame.Width;
			float sb = (float)h / frame.Height;
			var scale = Math.Min(sa, sb);
			var newWidth = frame.Width * scale;
			var newHeight = frame.Height * scale;
			stream.Dispose();
			graphics.DrawImage(img, new Rectangle((int)(w - newWidth) / 2, (int)(h - newHeight) / 2, (int)newWidth, (int)newHeight),
				new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
			img.Dispose();
		}
		private void btn_ls_Click(object sender, EventArgs e)
		{
			if(String.IsNullOrEmpty(rt_push_url.Text))
			{
				MessageBox.Show("推流地址不能为空!");
				return;
			}
			NIMDeviceInfoList cameraDeviceinfolist = NIM.DeviceAPI.GetDeviceList(NIM.NIMDeviceType.kNIMDeviceTypeVideo);
			if(cameraDeviceinfolist==null)
			{
				MessageBox.Show("没有摄像头,无法直播！！！");
				return;
			}

			string url = rt_push_url.Text;
			if (nimSDKHelper.session == null)
			{
				nimSDKHelper.form = this;
				nimSDKHelper.session = new LsSession();
				nimSDKHelper.session.InitSession(false, url); 
				nimSDKHelper.session.DoStartLiveStream();
				btn_ls.Text = "结束直播";
			}
			else
			{
				nimSDKHelper.session.DoStopLiveStream();
				nimSDKHelper.session.ClearSession();
				nimSDKHelper.session = null;
				btn_ls.Text = "开始直播";
			}
		}
	}
}

using Newtonsoft.Json.Linq;
using NIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NIMDemo.LivingStreamSDK
{
	class nimSDKHelper
	{
		public static LsSession session;
		public static LivingStreamForm form;
		public static void Init()
		{
			//注册视频采集数据回调
			JObject jo = new JObject();
			jo.Add(new JProperty("subtype", NIMVideoSubType.kNIMVideoSubTypeI420));
			string json_extention = jo.ToString();
			NIM.DeviceAPI.SetVideoCaptureDataCb(VideoDataCaptureHandler,json_extention);
			NIM.DeviceAPI.SetAudioCaptureDataCb(AudioDataCaptureHandler);
		}
		public static void StartDevices()
		{
			NIM.DeviceAPI.StartDeviceResultHandler handle = (type, ret) =>
			{
				System.Diagnostics.Debug.WriteLine(type.ToString() + ":" + ret.ToString());
			};
			NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn, "", 0, handle);//开启麦克风
			NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioOutChat, "", 0, handle);//开启扬声器播放对方语音
			NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeVideo, "", 0, handle);//开启摄像头
		}
		public static void EndDevices()
		{
			NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn);
			NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioOutChat);
			NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeVideo);
		}


		private static void VideoDataCaptureHandler(UInt64 time, IntPtr data, UInt32 size, UInt32 width, UInt32 height, string json_extension, IntPtr user_data)
		{
			if (session != null)
			{
				byte[] yuv = new byte[size];
				int offset = 0;
				while (offset < size)
				{
					var b = Marshal.ReadByte(data, offset);
					yuv[offset++] = b;
				}
				byte[] rgb = YUVHelper.I420ToRGB(yuv, (int)width, (int)height);
				int framesize = (int)(width * height * 4);
				MainForm.VideoFrame frame = new MainForm.VideoFrame(rgb, (int)width, (int)height, framesize, (long)time);
				form.ShowVideoFrame(frame);
				session.SendVideoFrame(data, (int)size);
			}
		}

		private static void AudioDataCaptureHandler(UInt64 time, IntPtr data, uint size, int rate)
		{
			if (session != null)
			{
				session.SendAudioFrame(data,(int)size,rate);
			}
		}


	}
}

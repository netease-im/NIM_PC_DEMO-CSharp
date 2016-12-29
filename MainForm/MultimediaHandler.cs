using NIMDemo.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NIMDemo
{
    public class MultimediaHandler
    {
        private static NIM.NIMVChatSessionStatus _vchatHandlers;
        private static  Form _ownerFriendsListForm = null;
      
        public static void SetFriendsListForm(Form owner)
        {
            _ownerFriendsListForm=owner;
        }


        public MultimediaHandler(Form owner)
        {
            NIMDemo.Helper.VChatHelper.CurrentVChatType = NIMDemo.Helper.VChatType.kP2P;
            _ownerFriendsListForm = owner;
        }
        #region 音视频回调方法
        private static void OnSessionStartRes (long channel_id, int code)
        {
            Action action = () =>
            {
                if (code != 200)
                {
                    MessageBox.Show("发起音视频聊天失败");
                }
            };
            _ownerFriendsListForm.Invoke(action);
        }

        private static void OnSessionInviteNotify(long channel_id, string uid, int mode, long time)
        {
            Action a = () =>
            {
                string test = uid.ToString();
                if (mode == (int)NIM.NIMVideoChatMode.kNIMVideoChatModeAudio)
                {
                    test += "向你发起实时语音";
                }
                else
                {
                    test += "向你发起视频聊天";
                }
                DialogResult ret = MessageBox.Show(test, "音视频邀请", MessageBoxButtons.YesNo);
                NIM.NIMVChatInfo info = new NIM.NIMVChatInfo();
                NIM.VChatAPI.CalleeAck(channel_id, ret == DialogResult.Yes, info);
            };
            _ownerFriendsListForm.BeginInvoke(a);
        }

        private static void OnSessionCalleeAckRes(long channel_id, int code)
        {
            
        }

        private static void OnSessionCalleeAckNotify(long channel_id, string uid, int mode, bool accept)
        {
            if (accept)
            {
                DemoTrace.WriteLine("对方接听");
            }
            else
            {
                Action a = () => { MessageBox.Show("对方拒绝接听"); };
                _ownerFriendsListForm.Invoke(a);
            }
        }

        private static void OnSessionControlRes(long channel_id, int code, int type)
        {

        }

        private static void OnSessionControlNotify(long channel_id, string uid, int type)
        {

        }

        private static void OnSessionConnectNotify(long channel_id, int code, string record_addr, string record_file)
        {
            if (code == 200)
            {
                Action a = () =>
                {
                    if (NIMDemo.Helper.VChatHelper.CurrentVChatType == NIMDemo.Helper.VChatType.kP2P)
                    {
                        MainForm.VideoChatForm vform = MainForm.VideoChatForm.GetInstance();
                        vform.Show();
                    }
                };
                _ownerFriendsListForm.Invoke(a);
                StartDevices();

            }
            else
            {
                NIM.VChatAPI.End();
            }
        }

        private static void OnSessionPeopleStatus(long channel_id, string uid, int status)
        {

        }

        private static void OnSessionNetStatus(long channel_id, int status)
        {

        }

        private static void OnSessionHangupRes(long channel_id, int code)
        {
            EndDevices();
        }

        private static void OnSessionHangupNotify(long channel_id, int code)
        {
            EndDevices();
			if (code == 200)
			{
				Action action = () =>
				{
					MessageBox.Show("已挂断");
                    MainForm.VideoChatForm vform = MainForm.VideoChatForm.GetInstance();
                    if(vform!=null)
                    {
                        vform.Close();
                    }
                  
				};
				_ownerFriendsListForm.Invoke(action);
			}
        }
        #endregion

        public static void InitVChatInfo()
        {
            _vchatHandlers.onSessionStartRes = OnSessionStartRes;
            _vchatHandlers.onSessionInviteNotify = OnSessionInviteNotify;
            _vchatHandlers.onSessionCalleeAckRes = OnSessionCalleeAckRes;
            _vchatHandlers.onSessionCalleeAckNotify = OnSessionCalleeAckNotify;
            _vchatHandlers.onSessionControlRes = OnSessionControlRes;
            _vchatHandlers.onSessionControlNotify = OnSessionControlNotify;
            _vchatHandlers.onSessionConnectNotify = OnSessionConnectNotify;
            _vchatHandlers.onSessionPeopleStatus = OnSessionPeopleStatus;
            _vchatHandlers.onSessionNetStatus = OnSessionNetStatus;
            _vchatHandlers.onSessionHangupRes = OnSessionHangupRes;
            _vchatHandlers.onSessionHangupNotify = OnSessionHangupNotify;

			_vchatHandlers.onSessionSyncAckNotify =(channel_id,code,uid,  mode, accept,  time, client)=>
			{

			};

            //注册音视频会话交互回调
            NIM.VChatAPI.SetSessionStatusCb(_vchatHandlers);
            //注册音频接收数据回调
            NIM.DeviceAPI.SetAudioReceiveDataCb(AudioDataRecHandler);
            //注册视频接收数据回调
            NIM.DeviceAPI.SetVideoReceiveDataCb(VideoDataRecHandler);
            //注册视频采集数据回调
            NIM.DeviceAPI.SetVideoCaptureDataCb(VideoDataCaptureHandler);
        }
       private static void AudioDataRecHandler(UInt64 time, IntPtr data, UInt32 size, Int32 rate)
        {

        }

        public static EventHandler<MainForm.VideoEventAgrs> ReceiveVideoFrameHandler;
        public static EventHandler<MainForm.VideoEventAgrs> CapturedVideoFrameHandler;
        //捕获视频帧回调函数
        private static void VideoDataCaptureHandler(UInt64 time, IntPtr data, UInt32 size, UInt32 width, UInt32 height, string json_extension, IntPtr user_data)
        {
            if (CapturedVideoFrameHandler != null)
            {
                MainForm.VideoFrame frame = new MainForm.VideoFrame(data, (int)width, (int)height, (int)size, (long)time);
                CapturedVideoFrameHandler(_ownerFriendsListForm, new MainForm.VideoEventAgrs(frame));
            }

        }

        //收到视频帧回调函数
       private static void VideoDataRecHandler(UInt64 time, IntPtr data, UInt32 size, UInt32 width, UInt32 height, string json_extension, IntPtr user_data)
        {
            if (ReceiveVideoFrameHandler != null)
            {
                MainForm.VideoFrame frame = new MainForm.VideoFrame(data, (int) width, (int) height, (int) size, (long)time);
                ReceiveVideoFrameHandler(_ownerFriendsListForm, new MainForm.VideoEventAgrs(frame));
            }
        }

        //Stream ParseVedioData(IntPtr data, uint size, uint width, uint height)
        //{
        //    byte[] buffer = new byte[size];
        //    int offset = 0;
        //    while (offset < size)
        //    {
        //        var b = Marshal.ReadByte(data, offset);
        //        buffer[offset++] = b;
        //    }
        //    using (Bitmap resultBitmap = new Bitmap((int) width, (int) height, PixelFormat.Format32bppArgb))
        //    {
        //        MemoryStream curImageStream = new MemoryStream();

        //        resultBitmap.Save(curImageStream, System.Drawing.Imaging.ImageFormat.Bmp);

        //        byte[] tempData = new byte[4];

        //        //bmp format: https://en.wikipedia.org/wiki/BMP_file_format
        //        //读取数据开始位置，写入字节流
        //        curImageStream.Position = 10;

        //        curImageStream.Read(tempData, 0, 4);

        //        var dataOffset = BitConverter.ToInt32(tempData, 0);
        //        curImageStream.Position = dataOffset;
        //        curImageStream.Write(buffer, 0, (int)size);
        //        curImageStream.Flush();
        //        return curImageStream;
        //    }
        //}

        private static void StartDevices()
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
    }
}

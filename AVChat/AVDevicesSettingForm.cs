using NIM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NIMDemo.AVChat
{
  
    public partial class AVDevicesSettingForm : Form
    {
      
        private List<NIMDeviceInfo> _cameraDeviceList = null;
        private List<NIMDeviceInfo> _micphoneDeviceList = null;
        private List<NIMDeviceInfo> _audioOutDeviceList = null;
        public AVDevicesSettingForm()
        {
            InitializeComponent();
            InitDevicesCombolist();
            this.FormClosed += AVDevicesSettingForm_FormClosed;
            this.tb_audioin.Value = NIM.DeviceAPI.AudioCaptureVolumn;
            this.tb_audioout.Value = NIM.DeviceAPI.AudioPlayVolumn;
        }

        void AVDevicesSettingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioOut);
            this.Close();
        }


        //初始化设备列表
        private void InitDevicesCombolist()
        {
			NIMDeviceInfoList cameraDeviceinfolist = NIM.DeviceAPI.GetDeviceList(NIM.NIMDeviceType.kNIMDeviceTypeVideo);
			NIMDeviceInfoList micphoneDeviceinfolist = NIM.DeviceAPI.GetDeviceList(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn);
			NIMDeviceInfoList audioOutDeviceinfolist = NIM.DeviceAPI.GetDeviceList(NIM.NIMDeviceType.kNIMDeviceTypeAudioOut);

			if (cameraDeviceinfolist != null)
				_cameraDeviceList = cameraDeviceinfolist.DeviceList;
			if (micphoneDeviceinfolist != null)
				_micphoneDeviceList = micphoneDeviceinfolist.DeviceList;
			if (audioOutDeviceinfolist != null)
				_audioOutDeviceList = audioOutDeviceinfolist.DeviceList;


            if(_cameraDeviceList!=null)
            {
                foreach(var device in _cameraDeviceList)
                {
                    cb_camera.Items.Add(device.Name);
                }
                cb_camera.SelectedIndex = 0;
            }
            if(_micphoneDeviceList!=null)
            {
               foreach(var device in _micphoneDeviceList)
               {
                   cb_microphone.Items.Add(device.Name);
               }
               cb_microphone.SelectedIndex = 0;
            }
            
            if(_audioOutDeviceList!=null)
            {
                foreach(var device in _audioOutDeviceList)
                {
                    cb_audiooutdevice.Items.Add(device.Name);
                }
                cb_audiooutdevice.SelectedIndex = 0;
            }
            
        }

        private void tb_audioin_ValueChanged(object sender, EventArgs e)
        {
            NIM.DeviceAPI.AudioCaptureVolumn = Convert.ToByte(tb_audioin.Value);
        }

        private void tb_audioout_ValueChanged(object sender, EventArgs e)
        {
            NIM.DeviceAPI.AudioPlayVolumn = Convert.ToByte(tb_audioout.Value);
        }

        private void cb_camera_SelectedIndexChanged(object sender, EventArgs e)
        {
            string camera_device_path = string.Empty;

            if (_cameraDeviceList == null)
                return;

            camera_device_path = _cameraDeviceList[cb_camera.SelectedIndex].Path;
            //if (_cameraDeviceList != null)
            //{
            //    camera_device_path = _cameraDeviceList.Find((device) => { return device.Name == cb_camera.Text; }).Path;
            //}
           

            NIM.DeviceAPI.StartDeviceResultHandler handle = (type, ret) =>
            {
                Action action = () =>
                {
                    if(ret)
                    {
                        
                    }
                    else
                    {
                        MessageBox.Show("启动摄像头设备失败");
                    }

                };
                this.Invoke(action);
            };
            NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeVideo, camera_device_path, 0, handle);//开启摄像头
        }

        private void cb_microphone_SelectedIndexChanged(object sender, EventArgs e)
        {
          
            string microphone_device_path = string.Empty;
            if (_micphoneDeviceList == null)
                return;
            microphone_device_path = _micphoneDeviceList[cb_microphone.SelectedIndex].Path;
  

            //if (_micphoneDeviceList != null)
            //{
            //    microphone_device_path = _micphoneDeviceList.Find((device) => { return device.Name == cb_microphone.Text; }).Path;
            //}

            NIM.DeviceAPI.StartDeviceResultHandler handle = (type, ret) =>
            {
                Action action = () =>
                {
                    if (ret)
                    {

                    }
                    else
                    {
                        MessageBox.Show("启动麦克风设备失败");
                        
                    }

                };
                this.Invoke(action);
            };
      
            NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn, microphone_device_path, 0, handle);//开启麦克风
        }

        private void cb_audiooutdevice_SelectedIndexChanged(object sender, EventArgs e)
        {
       
            string audio_out_device_path = string.Empty;
            if (_audioOutDeviceList == null)
                return;
            audio_out_device_path = _audioOutDeviceList[cb_audiooutdevice.SelectedIndex].Path;
            //if (_audioOutDeviceList != null)
            //{
            //    audio_out_device_path = _audioOutDeviceList.Find((device) => { return device.Name == cb_audiooutdevice.Text; }).Path;
            //}

            NIM.DeviceAPI.StartDeviceResultHandler handle = (type, ret) =>
            {
                Action action = () =>
                {
                    if (ret)
                    {

                    }
                    else
                    {
                        MessageBox.Show("启动播放设备失败");
                    }

                };
                this.Invoke(action);
            };
            NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioOut, audio_out_device_path, 0, handle);
        }

    }


    public delegate void SettingResultEventHandler<EventSettingArgs>(object sender, EventSettingArgs e);

  

}

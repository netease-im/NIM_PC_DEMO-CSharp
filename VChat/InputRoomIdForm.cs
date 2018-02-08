﻿using NIM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NIMDemo
{
    public partial class InputRoomIdForm : Form
    {
        private NIMVChatOpt2Handler _joinroomcb = null;
        private string _room_name;
        public InputRoomIdForm()
        {
            InitializeComponent();
            _joinroomcb = new NIMVChatOpt2Handler(JoinMultiVChatRoomCallback);
        }

        private void JoinMultiVChatRoomCallback(int code, Int64 channel_id, string json_extension)
        {
            if (code == 200)
            {
                //进入房间成功
                Action action = () =>
                {
                    Helper.VChatHelper.CurrentVChatType = Helper.VChatType.kMulti;
                    MultiVChatForm vchat = new MultiVChatForm(_room_name);
                    vchat.Show();
                    this.Close();
                };
                this.Invoke(action);
            }
            else
            {
                Action action = () =>
                {
                    MessageBox.Show("加入房间失败-错误码:" + code.ToString());
					VChatAPI.End();

				};
                this.Invoke(action);

            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
           // string json_extension = "{\"session_id\":\"\"}";
            _room_name = tb_roomid.Text;
			NIM.NIMJoinRoomJsonEx joinRoomJsonEx = new NIMJoinRoomJsonEx();
            joinRoomJsonEx.Layout = new CustomLayout();
            joinRoomJsonEx.Layout.Hostarea = new HostArea();
            joinRoomJsonEx.Layout.Background = new BackGround();
            if (VChatAPI.JoinRoom(NIMVideoChatMode.kNIMVideoChatModeVideo, _room_name, joinRoomJsonEx, _joinroomcb))
            {
                //调用成功
            }
			else
			{
				Action action = () =>
				{
					MessageBox.Show("JoinRoom 调用失败:");
				};
				this.BeginInvoke(action);
			}

        }
    }
}

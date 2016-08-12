using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NIMDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            NIM.SysMessage.NIMSysMessageContent content = new NIM.SysMessage.NIMSysMessageContent()
            {
                Id = 9999,
                MsgType = NIM.SysMessage.NIMSysMsgType.kNIMSysMsgTypeFriendAdd,
                PushContent = "hello world",
                ClientMsgId = "nim_dihfihgfg",
                Status = NIM.SysMessage.NIMSysMsgStatus.kNIMSysMsgStatusInvalid,
                SupportOffline = NIM.NIMMessageSettingStatus.kNIMMessageStatusSetted
            };
            var x = content.Serialize();
            var obj = NIM.SysMessage.NIMSysMessageContent.Deserialize(x);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NIMDemo;

namespace NIMDemo
{
    class RtsHandler
    {
        Form _form;

        public RtsHandler(Form form)
        {
            _form = form;
            RegisterRtsCallback();
        }
        void RegisterRtsCallback()
        {
            NIM.RtsAPI.SetStartNotifyCallback(OnReceiveSessionRequest);
        }

        void OnReceiveSessionRequest(string sessionId, int channelType, string uid, string custom)
        {
            InvokeOnForm(() =>
            {
                var msg = string.Format("收到来自{0}的白板演示请求", uid);
                var ret = MessageBox.Show(msg, "白板演示", MessageBoxButtons.OKCancel);
                NIM.RtsAPI.Ack(sessionId, (NIM.NIMRts.NIMRtsChannelType) channelType, ret == DialogResult.OK, null, Response);
            });
        }

        void Response(int code, string sessionId, int channelType, bool accept)
        {
            if (accept && code == (int)NIM.ResponseCode.kNIMResSuccess)
            {
                InvokeOnForm(() =>
                {
                    var form = new RtsForm(sessionId);
                    form.Show();
                });
            }
            else if(code != (int)NIM.ResponseCode.kNIMResSuccess)
            {
                MessageBox.Show("建立白板会话失败:" + code.ToString());
            }
        }

        void ReceiveData(string sessionId, int channelType, string uid, IntPtr data, int size)
        {
            
        }

        void NotifyHangup(string sessionId, string uid)
        {
            
        }

        void InvokeOnForm(Action action)
        {
            _form.BeginInvoke(action);
        }
    }
}

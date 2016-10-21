using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NIMDemo
{
    public partial class RtsForm : Form
    {
        private readonly string _sessionId;
        private bool _drawing = false;
        private Graphics _peerDrawingGraphics;
        private Graphics _myDrawingGraphics;
        private readonly Pen _myPen = new Pen(Color.Blue,3);
        private readonly Pen _peerPen = new Pen(Color.Brown,3);
        private readonly PaintingRecord _selfPaintingRecord = new PaintingRecord();
        private readonly PaintingRecord _peerPaintingRecord = new PaintingRecord();
        private readonly Timer _sendDataTimer;

        private RtsForm()
        {
            InitializeComponent();
            this.Load += RtsForm_Load;
            this.FormClosed += RtsForm_FormClosed;
            panel1.MouseMove += Panel1_MouseMove;
            panel1.MouseDown += Panel1_MouseDown;
            panel1.MouseUp += Panel1_MouseUp;

        }

        public RtsForm(string id)
            : this()
        {
            _sessionId = id;
            _sendDataTimer = new Timer();
            _sendDataTimer.Interval = 100;
            _sendDataTimer.Tick += _sendDataTimer_Tick;
            _sendDataTimer.Start();
            this.button1.Click += Button1_Click;
            this.button2.Click += Button2_Click;
            this.audioCtrlBtn.Click += AudioCtrlBtn_Click;
        }

        private void AudioCtrlBtn_Click(object sender, EventArgs e)
        {
            var tag = audioCtrlBtn.Tag.ToString();
            string open = "开启音频";
            string close = "关闭音频";
            var i = tag == "0" ? open : close;
            if (tag == "0")
            {
                NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn, "", 0, null); 
            }
            else
            {
                NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn);
            }
            NIM.RtsAPI.Control(_sessionId, i, (int code, string sessionId, string info) =>
            {
                if (code == 200)
                {
                    Func<string, int> func = (ret) =>
                    {
                        if (info == open)
                        {
                            audioCtrlBtn.Text = "关闭麦克风";
                            audioCtrlBtn.Tag = 1;
                        }
                        if (info == close)
                        {
                            audioCtrlBtn.Text = "开启麦克风";
                            audioCtrlBtn.Tag = 0;
                        }
                        return 0;
                    };
                    this.audioCtrlBtn.BeginInvoke(func, info);
                }
            });
        }

        void SendData()
        {
            var cmdStr = _selfPaintingRecord.CreateCommand();
            if (string.IsNullOrEmpty(cmdStr))
                return;
            var ptr = Marshal.StringToHGlobalAnsi(cmdStr);
            NIM.RtsAPI.SendData(_sessionId, NIM.NIMRts.NIMRtsChannelType.kNIMRtsChannelTypeTcp, ptr, cmdStr.Length);
        }

        void SendControlCommand(CommandType type)
        {
            string cmdStr = string.Format("{0}:0,0;", (int) type);
            var ptr = Marshal.StringToHGlobalAnsi(cmdStr);
            NIM.RtsAPI.SendData(_sessionId, NIM.NIMRts.NIMRtsChannelType.kNIMRtsChannelTypeTcp, ptr, cmdStr.Length);
        }

        void ClearGraph()
        {
            _peerDrawingGraphics.Clear(panel1.BackColor);
            _peerPaintingRecord.Clear();
            _myDrawingGraphics.Clear(panel1.BackColor);
            _selfPaintingRecord.Clear();
        }

        private void RtsForm_Load(object sender, EventArgs e)
        {
            NIM.RtsAPI.SetReceiveDataCallback(OnReceiveRtsData);
            _peerDrawingGraphics = panel1.CreateGraphics();
            _myDrawingGraphics = panel1.CreateGraphics();
            _selfPaintingRecord.BaseSize = panel1.Size;
            SetRtsNotifyCallback();
            NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioOutChat, "", 0, null);//开启扬声器播放对方语音
        }

        void OnReceiveRtsData(string sessionId, int channelType, string uid, IntPtr data, int size)
        {
            var content = Marshal.PtrToStringAnsi(data, size);
            var lines = content.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            var s = _peerPaintingRecord.Count;
            foreach (var item in lines)
            {
                var cmd = PaintCommand.Create(item, panel1.Size);
                if (cmd == null || cmd.Type == CommandType.DrawOpPktId)
                    continue;
                _peerPaintingRecord.Add(cmd);
            }
            var c = _peerPaintingRecord.Count;
            if (c <= 0)
                return;
            CommandType lastCmdType = _peerPaintingRecord[c - 1].Type;
            if (lastCmdType == CommandType.DrawOpUndo)
            {
                _peerPaintingRecord.Revert();
                _peerDrawingGraphics.Clear(panel1.BackColor);
                _myDrawingGraphics.Clear(panel1.BackColor);
                DoDraw(_selfPaintingRecord, 0, _selfPaintingRecord.Count - 1, _myDrawingGraphics, _myPen);
                DoDraw(_peerPaintingRecord, 0, _peerPaintingRecord.Count - 1, _peerDrawingGraphics, _peerPen);
            }
            else if (lastCmdType == CommandType.DrawOpClear)
            {
                SendControlCommand(CommandType.DrawOpClearCb);
                ClearGraph();
            }
            else if (lastCmdType == CommandType.DrawOpClearCb)
            {
                ClearGraph();
            }
            else if(lastCmdType == CommandType.DrawOpStart || lastCmdType == CommandType.DrawOpMove || lastCmdType == CommandType.DrawOpEnd)
            {
                DoDraw(_peerPaintingRecord, s, _peerPaintingRecord.Count - 1, _peerDrawingGraphics, _peerPen);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("unknow data type:{0}", content);
            }
        }

        void DoDraw(PaintingRecord record, int startIndex, int endIndex, Graphics graphics, Pen pen)
        {
            Action action = () =>
            {
                Draw(record, startIndex, endIndex, graphics, pen);
            };
            this.BeginInvoke(action);
        }

        byte[] ReadData(IntPtr data, int size)
        {
            byte[] buffer = new byte[size];
            for (int i = 0; i < size; i++)
            {
                buffer[i] = Marshal.ReadByte(data, i);
            }
            return buffer;
        }

        void Draw(PaintingRecord record, int startIndex, int endIndex, Graphics graphics, Pen pen)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            PointF? start = null;
            for (int i = startIndex; i <= endIndex; i++)
            {
                var command = record[i];
                if (i >= 1)
                {
                    var ls = record[i - 1];
                    if (ls.Type != CommandType.DrawOpEnd)
                        start = new PointF(Math.Abs(ls.Coord.X), Math.Abs(ls.Coord.Y));
                    else
                    {
                        graphicsPath.StartFigure();
                        start = null;
                    }
                }
                PointF point = new PointF(Math.Abs(command.Coord.X), Math.Abs(command.Coord.Y));
                if (start != null)
                    graphicsPath.AddLine(start.Value, point);
                else
                {

                }
            }
            graphics.DrawPath(pen, graphicsPath);
        }

        //清空
        private void Button2_Click(object sender, EventArgs e)
        {
            SendControlCommand(CommandType.DrawOpClear);
        }

        //上一步
        private void Button1_Click(object sender, EventArgs e)
        {
            _selfPaintingRecord.Revert();
            _myDrawingGraphics.Clear(panel1.BackColor);
            _peerDrawingGraphics.Clear(panel1.BackColor);
            DoDraw(_selfPaintingRecord, 0, _selfPaintingRecord.Count - 1, _myDrawingGraphics, _myPen);
            DoDraw(_peerPaintingRecord, 0, _peerPaintingRecord.Count - 1, _peerDrawingGraphics, _peerPen);
            SendControlCommand(CommandType.DrawOpUndo);
        }

        private void _sendDataTimer_Tick(object sender, EventArgs e)
        {
            SendData();
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _drawing = false;
                _selfPaintingRecord.Add(CommandType.DrawOpEnd, e.X, e.Y);
                SendData();
            }
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _drawing = true;
                _selfPaintingRecord.Add(CommandType.DrawOpStart, e.X, e.Y);
            }
        }


        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || !_drawing) return;
            _selfPaintingRecord.Add(CommandType.DrawOpMove, e.X, e.Y);
            var count = _selfPaintingRecord.Count;
            if (count >= 2)
            {
                Draw(_selfPaintingRecord, count - 2, count - 1, _myDrawingGraphics, _myPen);
            }
        }

        private void RtsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _sendDataTimer.Stop();
            NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn);
            NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioOutChat);
            NIM.RtsAPI.Hangup(_sessionId, (a, b) =>
            {

            });
        }

        public void SetRtsNotifyCallback()
        {
            NIM.RtsAPI.SetHungupNotify((string sessionId, string uid) =>
            {
                SetPromptTip(uid+" 离开会话");
                MessageBox.Show(uid + " 挂断");
                Invoke(this.Close);
            });

            NIM.RtsAPI.SetConnectionNotifyCallback((string sessionId, int channelType, int code) =>
            {
                
            });

            NIM.RtsAPI.SetControlNotifyCallback((string sessionId, string info, string uid) =>
            {
                if (sessionId != _sessionId) return;
                SetPromptTip(uid + " " + info);
            });
        }

        void SetPromptTip(string tip)
        {
            Action action = () => { this.promptLabel.Text = tip; };
            this.promptLabel.Invoke(action);
        }

        void Invoke(Action action)
        {
            this.BeginInvoke(action);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NIMDemo.Helper
{
    public enum VChatType
    {
        kP2P=0, //点对点音视频
        kMulti, //多人会议
    }

    public class VChatHelper
    {
        public static VChatType CurrentVChatType = VChatType.kP2P;
    }
}

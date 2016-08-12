using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NIM;
using NIM.DataSync;

namespace NIMDemo
{
    class ClientCallbacks
    {
        public static void Register()
        {
            NIM.ClientAPI.RegMultiSpotLoginNotifyCb(OnMultiSpotLogin);
            NIM.ClientAPI.RegKickOtherClientCb(OnKickOtherClient);
            NIM.DataSync.DataSyncAPI.RegCompleteCb(OnDataSyncCompleted);
        }

        private static void OnDataSyncCompleted(NIMDataSyncType syncType, NIMDataSyncStatus status, string jsonAttachment)
        {
            NimUtility.NimLogManager.DefaultLog.InfoFormat("同步完成:{0} {1} {2}", syncType, status, jsonAttachment);
        }

        private static void OnKickOtherClient(NIMKickOtherResult result)
        {
            NimUtility.NimLogManager.DefaultLog.InfoFormat("OnKickOtherClient:{0}-{1}", result.ResCode, result.Serialize());
        }

        private static void OnMultiSpotLogin(NIMMultiSpotLoginNotifyResult result)
        {
            NimUtility.NimLogManager.DefaultLog.InfoFormat("多端登录:{0}-{1}", result.NotifyType, result.Serialize());
        }
    }
}

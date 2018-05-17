using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NIM;
using NIM.SysMessage;
using NIM.SysMessage.Delegate;

namespace NIMDemo
{
    class SysMsgForm : ApiTestForm
    {
        public SysMsgForm()
            :base(typeof(NIM.SysMessage.SysMsgAPI))
        {
            base.Text = "系统消息";
        }

        protected override object GenerateParamerte(Type paramType, string value)
        {
            Object obj = null;
            if (typeof(Delegate).IsAssignableFrom(paramType))
            {
                if (paramType == typeof(QuerySysMsgResult))
                {
                    obj = new QuerySysMsgResult(OnQuerySysMsgCompleted);
                }
                else if (paramType == typeof(CommomOperateResult))
                {
                    obj = new CommomOperateResult(SysMsgCommonOpRet);
                }
                else if (paramType == typeof(OperateSysMsgExternDelegate))
                {
                    obj = new OperateSysMsgExternDelegate(OperateSysMsgExtern);
                }
                else if (paramType == typeof(OperateSysMsgDelegate))
                {
                    obj = new OperateSysMsgDelegate(OnOperateSysMsgCompleted);
                }
            }
            else
                obj = base.GenerateParamerte(paramType, value);
            return obj;
        }

        private void OnOperateSysMsgCompleted(int res_code, int unread_count, string json_extension, IntPtr user_data)
        {
            ShowOperationResult(new { Code = res_code, Unread = unread_count });
        }

        private void OperateSysMsgExtern(int res_code, long msg_id, int unread_count, string json_extension, IntPtr user_data)
        {
            ShowOperationResult(new { Code = res_code, MsgId = msg_id,Unread = unread_count });
        }

        private void SysMsgCommonOpRet(ResponseCode response, int count)
        {
            ShowOperationResult(new {Code = response, Count = count});
        }

        private void OnQuerySysMsgCompleted(NIMSysMsgQueryResult result)
        {
            ShowOperationResult(result);
        }
    }
}

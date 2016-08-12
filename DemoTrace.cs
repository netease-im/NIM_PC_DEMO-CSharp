using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NIMDemo
{
    static class DemoTrace
    {
        //[Conditional("DEBUG")]
        public static void WriteLine(params object[] args)
        {
            StackTrace st = new StackTrace();
            var index = Math.Min(1, st.FrameCount);
            var frame = st.GetFrame(index);
            var m = frame.GetMethod().Name;
            var msg = string.Format("{0} {1} {2}", DateTime.Now.ToLongTimeString(), System.Threading.Thread.CurrentThread.ManagedThreadId, m);
            if (args != null)
            {
                msg += " : ";
                msg = args.Aggregate(msg, (current, arg) => current + arg.ToString() + ',' );
            }
            msg = msg.TrimEnd(',');
            System.Diagnostics.Debug.WriteLine(msg);
            OutputForm.SetText(msg);
        }
    }
}

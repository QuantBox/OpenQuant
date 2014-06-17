using System;

using OpenQuant.API;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace QuantBox.OQ.Demo.Msic
{
    public class QQMsg2_code : Strategy
    {
        public const int WM_PASTE = 0x302;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
        IntPtr hWnd,        // 信息发往的窗口的句柄
        int Msg,            // 消息ID
        int wParam,         // 参数1
        int lParam            // 参数2
        );

        private string GroupName = "OpenQuant";
        private string textToCopy;
        private IntPtr QQHwndSend = new IntPtr(0);

        public override void OnStrategyStart()
        {
            Send("测试剪切板，自动发消息");
        }

        private void SetClipboardTextAndSend()
        {
            try
            {
                Clipboard.SetText(textToCopy + "——消息发自OpenQuant");

                // 粘贴
                PostMessage(QQHwndSend, WM_PASTE, 0, 0);
                // 按下回车，修改QQ为只按回车就可以发送
                PostMessage(QQHwndSend, WM_KEYDOWN, 13, 0);
                PostMessage(QQHwndSend, WM_KEYUP, 13, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Send(string msg)
        {
            QQHwndSend = FindWindow("TXGuiFoundation", GroupName);
            if (QQHwndSend.Equals(IntPtr.Zero))
            {
                return;
            }
            // 此句会报错，所以改用另一线程实现
            //Clipboard.SetDataObject("测试剪切板，自动发消息。", true);

            textToCopy = msg;

            Thread runThread = new Thread(new ThreadStart(SetClipboardTextAndSend));
            runThread.SetApartmentState(ApartmentState.STA);
            runThread.Start();
        }
    }
}
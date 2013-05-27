using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;

using yiwoSDK;

namespace QuantBox.OQ.Demo.Msic
{
    public class Form1 : Form
    {
        public Button button1;
        public Form1()
        {
            button1 = new Button();
            button1.Size = new Size(200, 100);
            button1.Location = new Point(30, 30);

            this.Controls.Add(button1);
        }
    }

    /// <summary>
    /// 简单的使用一公开的第三方API进行登录QQ,并发消息的示例
    /// 
    /// 在运行策略前，先编译下策略，然后在属性中写好QQ用户名与密码
    /// 运行策略后，使用User Command进行操作
    /// LOGIN是登录，如果要输入验证码，再输入LOGIN:XXXX
    /// 登录成功后会收到群列表，复制，记下所有的GID和CODE
    /// 可以用GROUP:XXXX:XXXX来指定当前群号
    /// 直接发送消息即可
    /// 
    /// http://www.smartquant.cn/forum/forum.php?mod=viewthread&tid=566
    /// </summary>
    public class QQMsg_code:Strategy
    {
        [Parameter("QQ号", "LOGIN")]
        public string QQ = "";
        [Parameter("密码", "LOGIN")]
        public string Password = "";

        [Parameter("Gid", "QQ群")]
        public string Gid = "";
        [Parameter("Code", "QQ群")]
        public string Code = "";

        private CWebQQ cw;
        private CEncode ce;
        private CHttpWeb ch;

        private Form1 test;

        public override void OnStrategyStart()
        {
            //必须加，否则一切函数都不好使bbs.yiwowang.com
            CFun.I_LOVE_BBS("yiwowang.com");

            cw = new CWebQQ();
            ce = new CEncode();
            ch = new CHttpWeb();

            cw.RevMsg += cw_RevMsg;
            cw.SetHttpWeb(ch);
        }

        public override void OnStrategyStop()
        {
            cw.RevMsg -= cw_RevMsg;
            cw.Quit();

            cw = null;
            ce = null;
            ch = null;
        }

        public override void OnUserCommand(UserCommand command)
        {
            string cmd = command.Text;
            if (cmd == "LOGIN")
            {
                Login();
            }
            else if (cmd.StartsWith("LOGIN"))
            {
                string[] arr = cmd.Split(':');
                Login(arr[1]);
            }
            else if (cmd.StartsWith("GROUP"))
            {
                string[] arr = cmd.Split(':');
                SetDefaultGroup(arr[1], arr[2]);
            }
            else
            {
                SendMessage(cmd);
            }
        }

        public void SetUser(string qq, string password)
        {
            QQ = qq;
            Password = password;
            Console.WriteLine("设置当前用户:{0},{1}", QQ, Password);
        }

        public void SetDefaultGroup(string gid,string code)
        {
            Gid = gid;
            Code = code;
            Console.WriteLine("设置当前群:{0},{1}", Gid, Code);
        }

        public void Login()
        {
            string verifyCode = cw.GetLoginVC(QQ);
            if (verifyCode.Length != 4)
            {
                ShowVCImage(QQ);
                return;
            }

            Login(verifyCode);
        }

        public void Login(string verifyCode)
        {
            Console.WriteLine("正在登录：{0},{1},{2},{3}", QQ, Password, verifyCode, CFun.Status.online);
            int ret = cw.Login(QQ, Password, verifyCode, CFun.Status.online);
            switch (ret)
            {
                case 0:
                    Console.WriteLine("登录成功");
                    LoadGroupName();
                    break;
                case 1:
                    Console.WriteLine("密码错误");
                    break;
                case 2:
                    Console.WriteLine("验证码错误");
                    ShowVCImage(QQ);
                    break;
                default:
                    Console.WriteLine("输入有误");
                    break;
            }
        }

        public void ShowVCImage(string qq)
        {
            test = new Form1();
            test.button1.Image = cw.GetLoginVCImage(qq);

            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                Application.Run(test);
            });
        }

        public void LoadGroupName()
        {
            try
            {
                string StrJson = cw.GetGroupList();
                JsonGroupList GroupList = new JsonGroupList(StrJson);
                for (int i = 0; i <= GroupList.Result.Gnamelist.GetUpperBound(0); ++i)
                {
                    Console.WriteLine("编号:{0},群名:{1},Gid:{2},Code:{3}",
                        i,
                        GroupList.Result.Gnamelist[i].Name,
                        GroupList.Result.Gnamelist[i].Gid,
                        GroupList.Result.Gnamelist[i].Code);
                }

            }
            catch (Exception e) { Console.WriteLine("您没有群！" + e.ToString()); }
        }

        public void SendMessage(string msg)
        {
            string oq_msg = string.Format("{0}\\r\\n——此消息发自OpenQuant客户端", msg);
            string strResult = cw.SendMsgToGroup(Gid, Code, oq_msg, "", "宋体", "10", "000000", "0,0,0");
            if ((strResult.IndexOf("ok") >= 0))
                Console.WriteLine("消息【" + msg + "】发送成功！");
            else
                Console.WriteLine(strResult);
        }

        void cw_RevMsg(string msg)
        {
            JsonChatMessage ChatMessage = new JsonChatMessage(msg);
            try
            {
                for (int i = 0; i <= ChatMessage.Result.GetUpperBound(0); i++)
                {
                    string poll_type = ChatMessage.Result[0].PollType;
                    if (poll_type == "message")//如果是QQ消息
                    {
                        string FromUin = ChatMessage.Result[i].Value.FromUin.ToString();//消息发送者
                        string Content = ChatMessage.Result[i].Value.Content.Text;//QQ消息

                        Console.WriteLine(Content);
                    }
                    else if (poll_type == "group_message")//如果是群消息
                    {
                        string Content = ChatMessage.Result[i].Value.Content.Text;
                        string SendUin = ChatMessage.Result[i].Value.SendUin.ToString();
                        string Gid = ChatMessage.Result[i].Value.FromUin.ToString();
                        string Code = ChatMessage.Result[i].Value.GroupCode.ToString();

                        Console.WriteLine(Content);
                    }
                    else if (poll_type == "sys_g_msg" || poll_type == "system_message")//如果是系统消息
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

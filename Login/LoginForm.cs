using System;
using System.Windows.Forms;
using NimDemo;
using NimUtility;

namespace NIMDemo
{
    public partial class LoginForm : Form
    {
        private AccountCollection _accounts;
        private string _userName;
        private string _password;
        public static LoginForm LoginFormIstance;

        public LoginForm()
        {
            InitializeComponent();
            this.Load += OnLoginFormLoaded;
            this.VisibleChanged += OnLoginFormVisibleChanged;
            LoginFormIstance = this;
        }

        private void OnLoginFormVisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                InitLoginAccount();
        }

        private void OnLoginFormLoaded(object s, EventArgs e)
        {
            OutputForm.Instance.Show();
            var ps = ProxySettingForm.GetProxySetting();
            checkBox1.Checked = (ps != null && ps.IsValid);
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
        }


        private string _appKey = null;
        private bool InitSdk()
        {
            //读取配置信息,用户可以自己定义配置文件格式和读取方式，使用默认配置项config设置为null即可
            var config = ConfigReader.GetSdkConfig();
            if (config == null)
                config = new NimConfig();
            if (config.CommonSetting == null)
                config.CommonSetting = new SdkCommonSetting();
            //群消息已读功能,如需开启请提前咨询技术支持或销售
            config.CommonSetting.TeamMsgAckEnabled = true;
            if (!NIM.ClientAPI.Init(config.AppKey, "NIMCSharpDemo", null, config))
            {
                MessageBox.Show("NIM init failed!");
                return false;
            }
            _appKey = config.AppKey;
            return true;
        }

        /// <summary>
        /// 执行登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            //必须保证 NIM.ClientAPI.Init 调用成功
            if (!InitSdk())
                return;
            var ps = ProxySettingForm.GetProxySetting();
            if (ps != null && ps.IsValid)
            {
                NIM.GlobalAPI.SetProxy(ps.Type, ps.Host, ps.Port, ps.UserName, ps.Password);
            }
            _userName = UserNameComboBox.Text;
            _password = textBox2.Text;
            //使用明文密码或者其他加密方式请修改此处代码
            var password = NIM.ToolsAPI.GetMd5(_password);
            if (!string.IsNullOrEmpty(_userName) && !string.IsNullOrEmpty(password))
            {
                toolStripProgressBar1.Value = 0;
                label3.Text = "";
                if (string.IsNullOrEmpty(_appKey))
                {
                    MessageBox.Show("请设置app key");
                    return;
                }
                NIM.ClientAPI.Login(_appKey, _userName, password, HandleLoginResult);
            }
        }

        void HandleLogoutResult(NIM.NIMLogoutResult result)
        {
        }

        /// <summary>
        /// 发布登录通知
        /// </summary>
        void PublishLoginEvent()
        {
            NIM.NIMEventInfo info = new NIM.NIMEventInfo();
            info.Value = 200000;
            info.Type = 1;
            info.Sync = 1;
            info.ClientMsgID = Guid.NewGuid().ToString();
            info.BroadcastType = 1;
            info.ValidityPeriod = 24 * 60 * 60;
            NIM.NIMSubscribeApi.Publish(info, (ret, eventInfo) => 
            {
                System.Diagnostics.Debug.Assert(ret == NIM.ResponseCode.kNIMResSuccess);
            });
        }

        /// <summary>
        /// 登录结果处理
        /// </summary>
        /// <param name="result"></param>
        private void HandleLoginResult(NIM.NIMLoginResult result)
        {
            DemoTrace.WriteLine(result.LoginStep.ToString());
            Action action = () =>
            {
                toolStripProgressBar1.PerformStep();

                this.label3.Text = string.Format("{0}  {1}", result.LoginStep, result.Code);
                if (result.LoginStep == NIM.NIMLoginStep.kNIMLoginStepLogin)
                {
                    toolStripProgressBar1.Value = 100;
                    if (result.Code == NIM.ResponseCode.kNIMResSuccess)
                    {
                        this.Hide();
                        new FriendsListForm(_userName).Show();
                        toolStripProgressBar1.Value = 0;
                        this.label3.Text = "";
                        System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                        {
                            SaveLoginAccount();
                        });
                        //PublishLoginEvent();
                        
                    }
                    else
                    {
                        NIM.ClientAPI.Logout(NIM.NIMLogoutType.kNIMLogoutChangeAccout, HandleLogoutResult);
                    }
                }
            };
            this.Invoke(action);
        }

        private void SaveLoginAccount()
        {
            if (_accounts == null)
                _accounts = new AccountCollection();
            var index = _accounts.IndexOf(_userName);
            if (index != -1)
            {
                _accounts.List[index].Password = _password;
                _accounts.LastIndex = index;
            }
            else
            {
                Account account = new Account();
                account.Name = _userName;
                account.Password = _password;
                _accounts.List.Insert(0, account);
                _accounts.LastIndex = 0;
            }
            AccountManager.SaveLoginAccounts(_accounts);
        }

        private void InitLoginAccount()
        {
            _accounts = AccountManager.GetAccountList();
            if (_accounts != null)
            {
                foreach (var item in _accounts.List)
                {
                    if (!UserNameComboBox.Items.Contains(item.Name))
                        UserNameComboBox.Items.Add(item.Name);
                }
                UserNameComboBox.Text = _accounts.List[_accounts.LastIndex].Name;
                textBox2.Text = _accounts.List[_accounts.LastIndex].Password;
            }
        }

        private void UserNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_accounts == null)
                return;
            var text = UserNameComboBox.Text;
            var account = _accounts.Find(text);
            if (account != null)
                textBox2.Text = account.Password;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                new ProxySettingForm().ShowDialog();
                var ps = ProxySettingForm.GetProxySetting();
                if (ps == null || !ps.IsValid)
                {
                    Action action = () => { this.checkBox1.Checked = false; };
                    checkBox1.Invoke(action);
                }
            }
            else
            {
                ProxySettingForm.SetSettingStatus(false);
            }
        }
    }
}

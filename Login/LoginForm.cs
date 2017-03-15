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
            var config = ConfigReader.GetSdkConfig();
            if (!NIM.ClientAPI.Init(config.AppKey, "NIMCSharpDemo", null, config))
            {
               // NimUtility.NimLogManager.NimCoreLog.ErrorFormat("sdk init faild");
                MessageBox.Show("NIM init failed!");
                return false;
            }
            _appKey = config.AppKey;
            return true;
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            if (!InitSdk())
                return;
            var ps = ProxySettingForm.GetProxySetting();
            if (ps != null && ps.IsValid)
            {
                NIM.GlobalAPI.SetProxy(ps.Type, ps.Host, ps.Port, ps.UserName, ps.Password);
            }
            _userName = UserNameComboBox.Text;
            _password = textBox2.Text;
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
                        if (!NIM.VChatAPI.Init())
                        {
                            MessageBox.Show("NIM VChatAPI init failed!");
                        }
                        this.Hide();
                        new FriendsListForm(_userName).Show();
                        toolStripProgressBar1.Value = 0;
                        this.label3.Text = "";
                        System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                        {
                            SaveLoginAccount();
                        });
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

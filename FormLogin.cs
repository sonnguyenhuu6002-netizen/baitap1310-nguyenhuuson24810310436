using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManager
{
    /// <summary>Form đăng nhập đơn giản: chỉ cần nhập tên người dùng.</summary>
    public class FormLogin : Form
    {
        private readonly TextBox txtUser = new TextBox();
        public string UserName => txtUser.Text.Trim();

        public FormLogin()
        {
            Text = "Đăng nhập";
            ClientSize = new Size(340, 130);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label { Text = "Tên người dùng:", Location = new Point(15, 25), AutoSize = true });
            txtUser.Location = new Point(135, 22);
            txtUser.Width = 185;
            txtUser.Text = "admin";
            Controls.Add(txtUser);

            var btnOk = new Button { Text = "Đăng nhập", Location = new Point(135, 70), Size = new Size(100, 34) };
            var btnCancel = new Button { Text = "Hủy", Location = new Point(245, 70), Size = new Size(75, 34), DialogResult = DialogResult.Cancel };
            btnOk.Click += (s, e) =>
            {
                if (UserName.Length == 0)
                {
                    MessageBox.Show("Vui lòng nhập tên người dùng.");
                    return;
                }
                DialogResult = DialogResult.OK;
            };
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}

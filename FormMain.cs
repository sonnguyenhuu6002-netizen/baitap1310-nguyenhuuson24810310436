using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StudentManager
{
    public class FormMain : Form
    {
        private readonly MenuStrip menu = new MenuStrip();
        private readonly ToolStrip tool = new ToolStrip();
        private readonly StatusStrip status = new StatusStrip();
        private readonly ContextMenuStrip ctx = new ContextMenuStrip();
        private readonly ToolStripStatusLabel lblUser = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel lblMessage = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel lblTime = new ToolStripStatusLabel();
        private readonly Timer timer = new Timer { Interval = 1000 };

        private string currentUser;
        private bool forceClose;

        public FormMain(string userName)
        {
            currentUser = userName;
            BuildUI();
            SetUser(userName);
            SetStatus("Sẵn sàng.");

            timer.Tick += (s, e) => lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            timer.Start();
            lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        // ================= DỰNG GIAO DIỆN =================
        private void BuildUI()
        {
            Text = "Hệ thống Quản lý - MDI";
            IsMdiContainer = true;                 // Form cha MDI
            WindowState = FormWindowState.Maximized;
            Font = new Font("Segoe UI", 10F);

            // ---- MenuStrip ----
            var mnuSystem = new ToolStripMenuItem("Hệ thống");
            mnuSystem.DropDownItems.Add(Item("Đăng xuất", (s, e) => Logout(), Keys.Control | Keys.L));
            mnuSystem.DropDownItems.Add(new ToolStripSeparator());
            mnuSystem.DropDownItems.Add(Item("Thoát", (s, e) => Close(), Keys.Alt | Keys.F4));

            var mnuCatalog = new ToolStripMenuItem("Danh mục");
            mnuCatalog.DropDownItems.Add(Item("Quản lý Sinh viên", (s, e) => OpenChild<FormStudent>(), Keys.Control | Keys.D1));
            mnuCatalog.DropDownItems.Add(Item("Quản lý Sản phẩm", (s, e) => OpenChild<FormProduct>(), Keys.Control | Keys.D2));

            var mnuWindow = new ToolStripMenuItem("Cửa sổ");
            mnuWindow.DropDownItems.Add(Item("Cascade", (s, e) => LayoutMdi(MdiLayout.Cascade)));
            mnuWindow.DropDownItems.Add(Item("Tile Horizontal", (s, e) => LayoutMdi(MdiLayout.TileHorizontal)));
            mnuWindow.DropDownItems.Add(Item("Tile Vertical", (s, e) => LayoutMdi(MdiLayout.TileVertical)));
            mnuWindow.DropDownItems.Add(new ToolStripSeparator());
            mnuWindow.DropDownItems.Add(Item("Làm mới form đang mở", (s, e) => RefreshActiveChild(), Keys.F5));
            mnuWindow.DropDownItems.Add(Item("Đóng tất cả", (s, e) => CloseAllChildren()));

            menu.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuCatalog, mnuWindow });
            menu.MdiWindowListItem = mnuWindow;     // tự liệt kê các form con đang mở trong menu Cửa sổ
            MainMenuStrip = menu;

            // ---- ToolStrip (icon lối tắt) ----
            tool.ImageScalingSize = new Size(24, 24);
            tool.Items.Add(ToolBtn("Sinh viên", SystemIcons.Information, (s, e) => OpenChild<FormStudent>()));
            tool.Items.Add(ToolBtn("Sản phẩm", SystemIcons.Question, (s, e) => OpenChild<FormProduct>()));
            tool.Items.Add(new ToolStripSeparator());
            tool.Items.Add(ToolBtn("Cascade", SystemIcons.Application, (s, e) => LayoutMdi(MdiLayout.Cascade)));
            tool.Items.Add(ToolBtn("Thoát", SystemIcons.Error, (s, e) => Close()));

            // ---- StatusStrip ----
            lblUser.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblMessage.Spring = true;
            lblMessage.TextAlign = ContentAlignment.MiddleLeft;
            lblTime.BorderSides = ToolStripStatusLabelBorderSides.Left;
            status.Items.AddRange(new ToolStripItem[] { lblUser, lblMessage, lblTime });

            // ---- ContextMenuStrip (click phải vào vùng nền MDI) ----
            ctx.Items.Add(Item("Cascade", (s, e) => LayoutMdi(MdiLayout.Cascade)));
            ctx.Items.Add(Item("Tile Horizontal", (s, e) => LayoutMdi(MdiLayout.TileHorizontal)));
            ctx.Items.Add(Item("Tile Vertical", (s, e) => LayoutMdi(MdiLayout.TileVertical)));
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(Item("Đóng tất cả", (s, e) => CloseAllChildren()));
            // Vùng nền MDI thực chất là control MdiClient nên phải gán ContextMenuStrip cho nó
            foreach (Control c in Controls)
                if (c is MdiClient) c.ContextMenuStrip = ctx;

            // Thứ tự add: Status/Tool/Menu để docking đúng
            Controls.Add(status);
            Controls.Add(tool);
            Controls.Add(menu);
        }

        private static ToolStripMenuItem Item(string text, EventHandler onClick, Keys shortcut = Keys.None)
        {
            var it = new ToolStripMenuItem(text);
            if (shortcut != Keys.None) it.ShortcutKeys = shortcut;
            it.Click += onClick;
            return it;
        }

        private static ToolStripButton ToolBtn(string text, Icon icon, EventHandler onClick)
        {
            var b = new ToolStripButton(text, icon.ToBitmap(), onClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                TextImageRelation = TextImageRelation.ImageAboveText
            };
            return b;
        }

        // ================= QUẢN LÝ FORM CON =================
        /// <summary>
        /// Mở form con kiểu T. Nếu đã có thì chỉ Activate, nếu chưa thì tạo mới.
        /// </summary>
        private void OpenChild<T>() where T : ChildFormBase, new()
        {
            var existing = MdiChildren.OfType<T>().FirstOrDefault();
            if (existing != null)
            {
                if (existing.WindowState == FormWindowState.Minimized)
                    existing.WindowState = FormWindowState.Normal;
                existing.Activate();
                existing.BringToFront();
                SetStatus($"Form \"{existing.Text}\" đã mở, đã đưa lên trên cùng.");
                return;
            }

            var child = new T { MdiParent = this };
            child.StatusMessage += SetStatus;              // Con -> Cha
            child.FormClosed += (s, e) => SetStatus($"Đã đóng \"{child.Text}\".");
            child.Show();
            SetStatus($"Đã mở \"{child.Text}\".");
        }

        private void RefreshActiveChild()
        {
            if (ActiveMdiChild is ChildFormBase c)
                c.RefreshData();                           // Cha -> Con
            else
                SetStatus("Không có form con nào đang mở.");
        }

        private void CloseAllChildren()
        {
            foreach (var f in MdiChildren) f.Close();
        }

        // ================= TRẠNG THÁI / NGƯỜI DÙNG =================
        public void SetStatus(string message)
        {
            lblMessage.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }

        private void SetUser(string name)
        {
            currentUser = name;
            lblUser.Text = "Người dùng: " + name;
        }

        // ================= ĐĂNG XUẤT / THOÁT =================
        private void Logout()
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            CloseAllChildren();
            Hide();
            using var login = new FormLogin();
            if (login.ShowDialog() == DialogResult.OK)
            {
                SetUser(login.UserName);
                SetStatus("Đăng nhập thành công.");
                Show();
            }
            else
            {
                forceClose = true;
                Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!forceClose && e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("Bạn có chắc muốn thoát chương trình?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnFormClosing(e);
        }
    }
}

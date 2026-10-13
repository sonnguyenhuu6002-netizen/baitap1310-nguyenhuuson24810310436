using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StudentManager
{
    public class FormStudent : ChildFormBase
    {
        private readonly BindingList<Student> students = new BindingList<Student>();
        private readonly BindingSource bs = new BindingSource();

        private DataGridView dgv = new DataGridView();
        private TextBox txtId = new TextBox();
        private TextBox txtName = new TextBox();
        private TextBox txtAge = new TextBox();
        private ComboBox cboGender = new ComboBox();
        private PictureBox pic = new PictureBox();
        private Button btnPick = new Button();
        private Button btnAdd = new Button();
        private Button btnDelete = new Button();
        private Button btnUpdate = new Button();
        private Button btnExport = new Button();
        private ContextMenuStrip ctxGrid = new ContextMenuStrip();

        public FormStudent()
        {
            BuildUI();
            SetupBinding();
            SeedData();
        }

        // ================= GIAO DIỆN =================
        private void BuildUI()
        {
            Text = "Quản lý Sinh viên";
            ClientSize = new Size(900, 600);
            Font = new Font("Segoe UI", 10F);

            dgv.Location = new Point(12, 12);
            dgv.Size = new Size(876, 250);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StudentId", HeaderText = "Mã SV", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ tên", FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Age", HeaderText = "Tuổi", FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Gender", HeaderText = "Giới tính", FillWeight = 12 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AvatarPath", HeaderText = "Đường dẫn ảnh", FillWeight = 35 });
            Controls.Add(dgv);

            // ContextMenuStrip cho DataGridView
            var miDelete = new ToolStripMenuItem("Xóa dòng");
            var miRefresh = new ToolStripMenuItem("Làm mới dữ liệu");
            var miDetail = new ToolStripMenuItem("Xem chi tiết");
            miDelete.Click += (s, e) => DeleteCurrent();
            miRefresh.Click += (s, e) => RefreshData();
            miDetail.Click += (s, e) => ShowDetail();
            ctxGrid.Items.AddRange(new ToolStripItem[] { miDetail, miDelete, new ToolStripSeparator(), miRefresh });
            dgv.ContextMenuStrip = ctxGrid;
            // Click phải: chọn luôn dòng đó để menu thao tác đúng dòng
            dgv.CellMouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                {
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;
                    dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
                }
            };

            var gb = new GroupBox { Text = "Thông tin sinh viên", Location = new Point(12, 275), Size = new Size(876, 230) };
            gb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(gb);

            AddRow(gb, "Mã SV:", txtId, 30);
            AddRow(gb, "Họ tên:", txtName, 75);
            AddRow(gb, "Tuổi:", txtAge, 120);

            gb.Controls.Add(new Label { Text = "Giới tính:", Location = new Point(15, 168), AutoSize = true });
            cboGender.Location = new Point(110, 165);
            cboGender.Width = 300;
            cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGender.Items.AddRange(new object[] { "Nam", "Nữ" });
            gb.Controls.Add(cboGender);

            pic.Location = new Point(560, 25);
            pic.Size = new Size(150, 150);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.BorderStyle = BorderStyle.FixedSingle;
            gb.Controls.Add(pic);

            btnPick.Text = "Chọn ảnh...";
            btnPick.Location = new Point(560, 182);
            btnPick.Size = new Size(150, 32);
            btnPick.Click += BtnPick_Click;
            gb.Controls.Add(btnPick);

            int x = 12;
            foreach (var (btn, text, handler) in new (Button, string, EventHandler)[]
            {
                (btnAdd, "Thêm", BtnAdd_Click),
                (btnDelete, "Xóa", (s, e) => DeleteCurrent()),
                (btnUpdate, "Cập nhật", BtnUpdate_Click),
                (btnExport, "Xuất file CSV", BtnExport_Click)
            })
            {
                btn.Text = text;
                btn.Location = new Point(x, 530);
                btn.Size = new Size(130, 38);
                btn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                btn.Click += handler;
                Controls.Add(btn);
                x += 145;
            }
        }

        private void AddRow(GroupBox gb, string label, TextBox tb, int y)
        {
            gb.Controls.Add(new Label { Text = label, Location = new Point(15, y + 3), AutoSize = true });
            tb.Location = new Point(110, y);
            tb.Width = 300;
            gb.Controls.Add(tb);
        }

        // ================= BINDING =================
        private void SetupBinding()
        {
            bs.DataSource = students;
            dgv.DataSource = bs;

            txtId.DataBindings.Add("Text", bs, nameof(Student.StudentId), true, DataSourceUpdateMode.OnPropertyChanged);
            txtName.DataBindings.Add("Text", bs, nameof(Student.FullName), true, DataSourceUpdateMode.OnPropertyChanged);
            txtAge.DataBindings.Add("Text", bs, nameof(Student.Age), true, DataSourceUpdateMode.OnPropertyChanged);
            cboGender.DataBindings.Add("SelectedItem", bs, nameof(Student.Gender), true, DataSourceUpdateMode.OnPropertyChanged);

            bs.CurrentItemChanged += (s, e) => LoadAvatar();
            LoadAvatar();
        }

        private void SeedData()
        {
            students.Add(new Student { StudentId = "SV001", FullName = "Nguyễn Văn An", Age = 20, Gender = "Nam" });
            students.Add(new Student { StudentId = "SV002", FullName = "Trần Thị Bình", Age = 21, Gender = "Nữ" });
        }

        private void LoadAvatar()
        {
            var old = pic.Image;
            pic.Image = null;
            old?.Dispose();

            if (bs.Current is Student st && !string.IsNullOrWhiteSpace(st.AvatarPath) && File.Exists(st.AvatarPath))
            {
                try
                {
                    using var fs = new FileStream(st.AvatarPath, FileMode.Open, FileAccess.Read);
                    using var img = Image.FromStream(fs);
                    pic.Image = new Bitmap(img);
                }
                catch { pic.Image = null; }
            }
        }

        // Cha gọi hàm này (menu Cửa sổ > Làm mới, phím F5)
        public override void RefreshData()
        {
            bs.ResetBindings(false);
            dgv.Refresh();
            LoadAvatar();
            Report($"Đã làm mới danh sách sinh viên ({students.Count} dòng).");
        }

        // ================= CHỨC NĂNG =================
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var st = (Student)bs.AddNew()!;
            st.StudentId = "SV" + students.Count.ToString("000");
            st.Gender = "Nam";
            txtId.Focus();
            txtId.SelectAll();
            Report("Đã thêm dòng mới, hãy nhập thông tin rồi bấm Cập nhật.");
        }

        private void DeleteCurrent()
        {
            if (bs.Current is not Student st) return;
            if (MessageBox.Show($"Xóa sinh viên {st.StudentId} - {st.FullName}?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            string id = st.StudentId;
            bs.RemoveCurrent();
            Report($"Đã xóa sinh viên {id} thành công.");
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (bs.Current is not Student st) return;
            bs.EndEdit();

            if (string.IsNullOrWhiteSpace(st.StudentId) || string.IsNullOrWhiteSpace(st.FullName))
            {
                MessageBox.Show("Mã SV và Họ tên không được để trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (st.Age <= 0 || st.Age > 120)
            {
                MessageBox.Show("Tuổi không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (students.Count(s => s.StudentId.Trim().Equals(st.StudentId.Trim(), StringComparison.OrdinalIgnoreCase)) > 1)
            {
                MessageBox.Show("Mã SV đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgv.Refresh();
            Report($"Đã cập nhật sinh viên {st.StudentId} thành công.");
        }

        private void ShowDetail()
        {
            if (bs.Current is not Student st) return;
            MessageBox.Show(
                $"Mã SV: {st.StudentId}\nHọ tên: {st.FullName}\nTuổi: {st.Age}\nGiới tính: {st.Gender}\nẢnh: {(string.IsNullOrEmpty(st.AvatarPath) ? "(chưa có)" : st.AvatarPath)}",
                "Chi tiết sinh viên", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Report($"Đã xem chi tiết sinh viên {st.StudentId}.");
        }

        private void BtnPick_Click(object? sender, EventArgs e)
        {
            if (bs.Current is not Student st)
            {
                MessageBox.Show("Hãy chọn hoặc thêm một sinh viên trước.");
                return;
            }
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh đại diện",
                Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                st.AvatarPath = ofd.FileName;
                dgv.Refresh();
                Report($"Đã chọn ảnh cho sinh viên {st.StudentId}.");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Xuất danh sách sinh viên",
                Filter = "CSV (*.csv)|*.csv",
                FileName = "DanhSachSinhVien.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            const string sep = ",";
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(sep, "Mã SV", "Họ tên", "Tuổi", "Giới tính", "Đường dẫn ảnh"));
                foreach (var s in students)
                    sb.AppendLine(string.Join(sep, Csv(s.StudentId), Csv(s.FullName), s.Age.ToString(), Csv(s.Gender), Csv(s.AvatarPath)));
                File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                Report($"Đã xuất {students.Count} sinh viên ra file {Path.GetFileName(sfd.FileName)}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string Csv(string? value)
        {
            value ??= "";
            if (value.Contains(',') || value.Contains(';') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}

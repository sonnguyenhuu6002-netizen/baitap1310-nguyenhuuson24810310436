using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StudentManager
{
    public class MainForm : Form
    {
        // ====== Dữ liệu & BindingSource ======
        private readonly BindingList<Student> students = new BindingList<Student>();
        private readonly BindingSource bs = new BindingSource();

        // ====== Controls ======
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

        public MainForm()
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
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            // DataGridView
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

            // Cụm nhập liệu
            var gb = new GroupBox { Text = "Thông tin sinh viên", Location = new Point(12, 275), Size = new Size(876, 230) };
            gb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(gb);

            AddRow(gb, "Mã SV:", txtId, 30);
            AddRow(gb, "Họ tên:", txtName, 75);
            AddRow(gb, "Tuổi:", txtAge, 120);

            var lblGender = new Label { Text = "Giới tính:", Location = new Point(15, 168), AutoSize = true };
            cboGender.Location = new Point(110, 165);
            cboGender.Width = 300;
            cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGender.Items.AddRange(new object[] { "Nam", "Nữ" });
            gb.Controls.Add(lblGender);
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

            // Cụm chức năng
            int x = 12;
            foreach (var (btn, text, handler) in new (Button, string, EventHandler)[]
            {
                (btnAdd, "Thêm", BtnAdd_Click),
                (btnDelete, "Xóa", BtnDelete_Click),
                (btnUpdate, "Cập nhật", BtnUpdate_Click),
                (btnExport, "Xuất file CSV", BtnExport_Click)
            })
            {
                btn.Text = text;
                btn.Location = new Point(x, 530);
                btn.Size = new Size(130, 38);
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

        // ================= BINDING 2 CHIỀU =================
        private void SetupBinding()
        {
            bs.DataSource = students;
            dgv.DataSource = bs;

            // Control nhập liệu <-> dòng đang chọn (bs.Current)
            txtId.DataBindings.Add("Text", bs, nameof(Student.StudentId), true, DataSourceUpdateMode.OnPropertyChanged);
            txtName.DataBindings.Add("Text", bs, nameof(Student.FullName), true, DataSourceUpdateMode.OnPropertyChanged);
            txtAge.DataBindings.Add("Text", bs, nameof(Student.Age), true, DataSourceUpdateMode.OnPropertyChanged);
            cboGender.DataBindings.Add("SelectedItem", bs, nameof(Student.Gender), true, DataSourceUpdateMode.OnPropertyChanged);

            // Đổi dòng (hoặc đổi AvatarPath) -> nạp lại ảnh
            bs.CurrentItemChanged += (s, e) => LoadAvatar();
            LoadAvatar();
        }

        private void SeedData()
        {
            students.Add(new Student { StudentId = "SV001", FullName = "Nguyễn Văn An", Age = 20, Gender = "Nam" });
            students.Add(new Student { StudentId = "SV002", FullName = "Trần Thị Bình", Age = 21, Gender = "Nữ" });
        }

        // Đọc ảnh qua stream để không khóa file trên đĩa
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

        // ================= CHỨC NĂNG =================
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var st = (Student)bs.AddNew()!;   // thêm dòng mới và tự chọn dòng đó
            st.StudentId = "SV" + (students.Count).ToString("000");
            st.Gender = "Nam";
            txtId.Focus();
            txtId.SelectAll();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (bs.Current == null) return;
            if (MessageBox.Show("Xóa sinh viên đang chọn?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                bs.RemoveCurrent();
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (bs.Current is not Student st) return;
            bs.EndEdit();   // đẩy giá trị từ control vào đối tượng

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
            MessageBox.Show("Đã cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                st.AvatarPath = ofd.FileName;   // CurrentItemChanged sẽ tự nạp ảnh
                dgv.Refresh();
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

            const string sep = ",";   // đổi thành ";" nếu Excel máy bạn dùng dấu chấm phẩy
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(sep, "Mã SV", "Họ tên", "Tuổi", "Giới tính", "Đường dẫn ảnh"));
                foreach (var s in students)
                {
                    sb.AppendLine(string.Join(sep,
                        Csv(s.StudentId), Csv(s.FullName), s.Age.ToString(),
                        Csv(s.Gender), Csv(s.AvatarPath)));
                }
                // UTF-8 có BOM để Excel đọc đúng tiếng Việt
                File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Escape theo chuẩn CSV: bao trong "..." nếu có dấu phẩy, ngoặc kép, xuống dòng
        private static string Csv(string? value)
        {
            value ??= "";
            if (value.Contains(',') || value.Contains(';') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}

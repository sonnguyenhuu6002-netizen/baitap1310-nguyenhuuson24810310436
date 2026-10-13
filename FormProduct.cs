using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManager
{
    public class Product
    {
        public string ProductId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class FormProduct : ChildFormBase
    {
        private readonly BindingList<Product> products = new BindingList<Product>();
        private readonly BindingSource bs = new BindingSource();

        private DataGridView dgv = new DataGridView();
        private TextBox txtId = new TextBox();
        private TextBox txtName = new TextBox();
        private TextBox txtPrice = new TextBox();
        private TextBox txtQty = new TextBox();

        public FormProduct()
        {
            Text = "Quản lý Sản phẩm";
            ClientSize = new Size(700, 450);
            Font = new Font("Segoe UI", 10F);

            dgv.Location = new Point(12, 12);
            dgv.Size = new Size(676, 250);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Mã SP" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Tên SP", FillWeight = 200 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Đơn giá", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số lượng" });
            Controls.Add(dgv);

            int y = 290;
            AddField("Mã SP:", txtId, 12, y);
            AddField("Tên SP:", txtName, 350, y);
            AddField("Đơn giá:", txtPrice, 12, y + 45);
            AddField("Số lượng:", txtQty, 350, y + 45);

            var btnAdd = new Button { Text = "Thêm", Location = new Point(12, y + 100), Size = new Size(110, 36), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            var btnDelete = new Button { Text = "Xóa", Location = new Point(132, y + 100), Size = new Size(110, 36), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
            Controls.Add(btnAdd);
            Controls.Add(btnDelete);

            bs.DataSource = products;
            dgv.DataSource = bs;

            products.Add(new Product { ProductId = "SP001", ProductName = "Bút bi", Price = 5000, Quantity = 100 });
            products.Add(new Product { ProductId = "SP002", ProductName = "Vở 96 trang", Price = 12000, Quantity = 50 });
        }

        private void AddField(string label, TextBox tb, int x, int y)
        {
            var lbl = new Label { Text = label, Location = new Point(x, y + 4), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            tb.Location = new Point(x + 80, y);
            tb.Width = 200;
            tb.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Controls.Add(lbl);
            Controls.Add(tb);
        }

        public override void RefreshData()
        {
            bs.ResetBindings(false);
            Report($"Đã làm mới danh sách sản phẩm ({products.Count} dòng).");
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Nhập Mã SP và Tên SP.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out var price) || price < 0 || !int.TryParse(txtQty.Text, out var qty) || qty < 0)
            {
                MessageBox.Show("Đơn giá và Số lượng phải là số không âm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            products.Add(new Product { ProductId = txtId.Text.Trim(), ProductName = txtName.Text.Trim(), Price = price, Quantity = qty });
            Report($"Đã thêm sản phẩm {txtId.Text.Trim()} thành công.");
            txtId.Clear(); txtName.Clear(); txtPrice.Clear(); txtQty.Clear();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (bs.Current is not Product p) return;
            if (MessageBox.Show($"Xóa sản phẩm {p.ProductName}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            bs.RemoveCurrent();
            Report($"Đã xóa sản phẩm {p.ProductId} thành công.");
        }
    }
}

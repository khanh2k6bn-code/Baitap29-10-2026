using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventoryManagementSystem.BUS;
using InventoryManagementSystem.DTO;

namespace InventoryManagementSystem.GUI
{
    public partial class Form1 : Form
    {
        private VatTuBUS vtBUS = new VatTuBUS();
        private NhaCungCapBUS nccBUS = new NhaCungCapBUS();
        private PhieuKhoBUS pkBUS = new PhieuKhoBUS();

        private TabControl tabControlMain;
        private TabPage tabInventory;
        private TabPage tabImportExport;

        private DataGridView dgvVatTu;
        private TextBox txtMaVatTu, txtTenVatTu, txtDVT, txtSoLuongTon, txtSearchVatTu, txtDonGia;
        private Button btnThemVT, btnSuaVT, btnXoaVT, btnSearchVatTu;
        private Label lblTotalInventoryValue;

        private ComboBox cboLoaiPhieu, cboNhaCungCap, cboVatTu;
        private NumericUpDown numSoLuong;
        private DataGridView dgvChiTietPhieu;
        private Button btnAddToPhieu, btnSavePhieuKho;
        private DataTable dtCurrentPhieu;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadVatTuData();
            LoadNhaCungCapComboBox();
            InitializePhieuTable();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Hệ Thống Quản Lý Kho Nguyên Vật Liệu (3-Tier Architecture)";
            this.Size = new Size(1020, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            tabControlMain = new TabControl { Dock = DockStyle.Fill };
            this.Controls.Add(tabControlMain);

            tabInventory = new TabPage("Quản lý Vật Tư & Tồn Kho");
            tabImportExport = new TabPage("Lập Phiếu Nhập / Xuất Kho");
            tabControlMain.TabPages.Add(tabInventory);
            tabControlMain.TabPages.Add(tabImportExport);

            // --- TAB 1 ---
            GroupBox grpInput = new GroupBox { Text = "Thông tin chi tiết vật tư", Location = new Point(15, 10), Size = new Size(970, 110) };
            tabInventory.Controls.Add(grpInput);

            grpInput.Controls.Add(new Label { Text = "Mã VT:", Location = new Point(15, 30), AutoSize = true });
            txtMaVatTu = new TextBox { Location = new Point(75, 27), Width = 120 };
            grpInput.Controls.Add(txtMaVatTu);

            grpInput.Controls.Add(new Label { Text = "Tên vật tư:", Location = new Point(215, 30), AutoSize = true });
            txtTenVatTu = new TextBox { Location = new Point(290, 27), Width = 220 };
            grpInput.Controls.Add(txtTenVatTu);

            grpInput.Controls.Add(new Label { Text = "Đơn vị tính:", Location = new Point(530, 30), AutoSize = true });
            txtDVT = new TextBox { Location = new Point(610, 27), Width = 100 };
            grpInput.Controls.Add(txtDVT);

            grpInput.Controls.Add(new Label { Text = "SL Tồn:", Location = new Point(730, 30), AutoSize = true });
            txtSoLuongTon = new TextBox { Location = new Point(790, 27), Width = 150, Text = "0" };
            grpInput.Controls.Add(txtSoLuongTon);

            btnThemVT = new Button { Text = "Thêm", Location = new Point(350, 68), Width = 90, Height = 32, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnThemVT.Click += btnThemVT_Click;
            grpInput.Controls.Add(btnThemVT);

            btnSuaVT = new Button { Text = "Sửa", Location = new Point(455, 68), Width = 90, Height = 32, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSuaVT.Click += btnSuaVT_Click;
            grpInput.Controls.Add(btnSuaVT);

            btnXoaVT = new Button { Text = "Xóa", Location = new Point(560, 68), Width = 90, Height = 32, BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnXoaVT.Click += btnXoaVT_Click;
            grpInput.Controls.Add(btnXoaVT);

            tabInventory.Controls.Add(new Label { Text = "Tìm kiếm:", Location = new Point(15, 135), AutoSize = true });
            txtSearchVatTu = new TextBox { Location = new Point(90, 132), Width = 250 };
            tabInventory.Controls.Add(txtSearchVatTu);

            btnSearchVatTu = new Button { Text = "Tìm", Location = new Point(350, 130), Width = 80, FlatStyle = FlatStyle.Flat };
            btnSearchVatTu.Click += (s, e) => LoadVatTuData(txtSearchVatTu.Text.Trim());
            tabInventory.Controls.Add(btnSearchVatTu);

            dgvVatTu = new DataGridView { Location = new Point(15, 175), Size = new Size(970, 370), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            dgvVatTu.CellClick += dgvVatTu_CellClick;
            tabInventory.Controls.Add(dgvVatTu);

            lblTotalInventoryValue = new Label { Text = "Tổng giá trị tồn kho: 0 VNĐ", Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(15, 555), AutoSize = true };
            tabInventory.Controls.Add(lblTotalInventoryValue);

            // --- TAB 2 ---
            tabImportExport.Controls.Add(new Label { Text = "Loại Phiếu:", Location = new Point(15, 20), AutoSize = true });
            cboLoaiPhieu = new ComboBox { Location = new Point(110, 18), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoaiPhieu.Items.AddRange(new string[] { "NHAP", "XUAT" });
            cboLoaiPhieu.SelectedIndex = 0;
            tabImportExport.Controls.Add(cboLoaiPhieu);

            tabImportExport.Controls.Add(new Label { Text = "Nhà Cung Cấp:", Location = new Point(280, 20), AutoSize = true });
            cboNhaCungCap = new ComboBox { Location = new Point(380, 18), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            tabImportExport.Controls.Add(cboNhaCungCap);

            GroupBox grpAddVT = new GroupBox { Text = "Chọn vật tư vào phiếu", Location = new Point(15, 60), Size = new Size(970, 80) };
            tabImportExport.Controls.Add(grpAddVT);

            grpAddVT.Controls.Add(new Label { Text = "Vật tư:", Location = new Point(15, 30), AutoSize = true });
            cboVatTu = new ComboBox { Location = new Point(70, 28), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            grpAddVT.Controls.Add(cboVatTu);

            grpAddVT.Controls.Add(new Label { Text = "Số lượng:", Location = new Point(340, 30), AutoSize = true });
            numSoLuong = new NumericUpDown { Location = new Point(410, 28), Width = 80, Minimum = 1, Maximum = 10000, Value = 1 };
            grpAddVT.Controls.Add(numSoLuong);

            grpAddVT.Controls.Add(new Label { Text = "Đơn giá:", Location = new Point(510, 30), AutoSize = true });
            txtDonGia = new TextBox { Location = new Point(570, 28), Width = 120, Text = "0" };
            grpAddVT.Controls.Add(txtDonGia);

            btnAddToPhieu = new Button { Text = "+ Thêm vào phiếu", Location = new Point(710, 25), Width = 130, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddToPhieu.Click += btnAddToPhieu_Click;
            grpAddVT.Controls.Add(btnAddToPhieu);

            dgvChiTietPhieu = new DataGridView { Location = new Point(15, 150), Size = new Size(970, 340), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            tabImportExport.Controls.Add(dgvChiTietPhieu);

            btnSavePhieuKho = new Button { Text = "Lưu Phiếu Kho (Transaction)", Location = new Point(765, 510), Width = 220, Height = 45, BackColor = Color.FromArgb(204, 51, 0), ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSavePhieuKho.Click += btnSavePhieuKho_Click;
            tabImportExport.Controls.Add(btnSavePhieuKho);
        }

        private void InitializePhieuTable()
        {
            dtCurrentPhieu = new DataTable();
            dtCurrentPhieu.Columns.Add("MaVatTu", typeof(string));
            dtCurrentPhieu.Columns.Add("TenVatTu", typeof(string));
            dtCurrentPhieu.Columns.Add("SoLuong", typeof(decimal));
            dtCurrentPhieu.Columns.Add("DonGia", typeof(decimal));
            dtCurrentPhieu.Columns.Add("Thành tiền", typeof(decimal), "SoLuong * DonGia");
            dgvChiTietPhieu.DataSource = dtCurrentPhieu;
        }

        private void LoadVatTuData(string keyword = "")
        {
            DataTable dt = string.IsNullOrEmpty(keyword) ? vtBUS.GetAllVatTu() : vtBUS.SearchVatTu(keyword);
            dgvVatTu.DataSource = dt;
            lblTotalInventoryValue.Text = $"Tổng giá trị tồn kho (ước tính): {vtBUS.GetTotalInventoryValue():N0} VNĐ";

            cboVatTu.DataSource = dt;
            cboVatTu.DisplayMember = "Tên vật tư";
            cboVatTu.ValueMember = "Mã VT";
        }

        private void LoadNhaCungCapComboBox()
        {
            cboNhaCungCap.DataSource = nccBUS.GetAllNhaCungCap();
            cboNhaCungCap.DisplayMember = "TenNCC";
            cboNhaCungCap.ValueMember = "MaNCC";
        }

        private void dgvVatTu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvVatTu.Rows[e.RowIndex];
                txtMaVatTu.Text = row.Cells["Mã VT"].Value.ToString();
                txtTenVatTu.Text = row.Cells["Tên vật tư"].Value.ToString();
                txtDVT.Text = row.Cells["Đơn vị tính"].Value.ToString();
                txtSoLuongTon.Text = row.Cells["Số lượng tồn"].Value.ToString();
                txtMaVatTu.ReadOnly = true;
            }
        }

        private void btnThemVT_Click(object sender, EventArgs e)
        {
            VatTuDTO vt = new VatTuDTO {
                MaVatTu = txtMaVatTu.Text.Trim(),
                TenVatTu = txtTenVatTu.Text.Trim(),
                DVT = txtDVT.Text.Trim(),
                SoLuongTon = Convert.ToDecimal(txtSoLuongTon.Text)
            };
            if (vtBUS.InsertVatTu(vt)) { MessageBox.Show("Thêm thành công!"); LoadVatTuData(); }
            else MessageBox.Show("Thêm thất bại!");
        }

        private void btnSuaVT_Click(object sender, EventArgs e)
        {
            VatTuDTO vt = new VatTuDTO {
                MaVatTu = txtMaVatTu.Text.Trim(),
                TenVatTu = txtTenVatTu.Text.Trim(),
                DVT = txtDVT.Text.Trim(),
                SoLuongTon = Convert.ToDecimal(txtSoLuongTon.Text)
            };
            if (vtBUS.UpdateVatTu(vt)) { MessageBox.Show("Sửa thành công!"); LoadVatTuData(); }
            else MessageBox.Show("Sửa thất bại!");
        }

        private void btnXoaVT_Click(object sender, EventArgs e)
        {
            if (vtBUS.DeleteVatTu(txtMaVatTu.Text.Trim())) { MessageBox.Show("Xóa thành công!"); LoadVatTuData(); }
            else MessageBox.Show("Xóa thất bại!");
        }

        private void btnAddToPhieu_Click(object sender, EventArgs e)
        {
            if (cboVatTu.SelectedValue == null) return;
            string maVT = cboVatTu.SelectedValue.ToString();
            string tenVT = cboVatTu.Text;
            decimal soLuong = numSoLuong.Value;
            decimal.TryParse(txtDonGia.Text, out decimal donGia);

            foreach (DataRow row in dtCurrentPhieu.Rows)
            {
                if (row["MaVatTu"].ToString() == maVT)
                {
                    row["SoLuong"] = Convert.ToDecimal(row["SoLuong"]) + soLuong;
                    row["DonGia"] = donGia;
                    return;
                }
            }
            dtCurrentPhieu.Rows.Add(maVT, tenVT, soLuong, donGia);
        }

        private void btnSavePhieuKho_Click(object sender, EventArgs e)
        {
            if (dtCurrentPhieu.Rows.Count == 0) return;
            string loaiPhieu = cboLoaiPhieu.SelectedItem.ToString();
            int? maNCC = loaiPhieu == "NHAP" ? (int?)cboNhaCungCap.SelectedValue : null;

            decimal tongTien = 0;
            foreach (DataRow r in dtCurrentPhieu.Rows) tongTien += Convert.ToDecimal(r["Thành tiền"]);

            if (pkBUS.LuuPhieuKho(loaiPhieu, maNCC, tongTien, dtCurrentPhieu, out string err))
            {
                MessageBox.Show("Lưu phiếu kho thành công!");
                dtCurrentPhieu.Clear();
                LoadVatTuData();
                tabControlMain.SelectedTab = tabInventory;
            }
            else
            {
                MessageBox.Show("Lỗi: " + err);
            }
        }
    }
}

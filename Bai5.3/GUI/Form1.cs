using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CafePOSSystem.BUS;

namespace CafePOSSystem.GUI
{
    public partial class Form1 : Form
    {
        private CafeBUS cafeBUS = new CafeBUS();

        private Panel pnlTableList;
        private Panel pnlOrderInfo;
        private DataGridView dgvBillDetails;
        private ComboBox cboCategories;
        private ComboBox cboFoods;
        private NumericUpDown numQuantity;
        private Button btnAddFood;
        private Button btnCheckout;
        private Label lblCurrentTable;
        private Label lblTotalAmount;

        private int selectedTableId = -1;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadTableButtons();
            LoadCategories();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void InitializeCustomComponents()
        {
            this.Text = "Hệ Thống Quản Lý Quán Cafe & Bán Hàng (3-Tier)";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            pnlTableList = new Panel { Location = new Point(12, 12), Size = new Size(420, 535), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlTableList);

            pnlOrderInfo = new Panel { Location = new Point(445, 12), Size = new Size(525, 535), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlOrderInfo);

            lblCurrentTable = new Label { Text = "Vui lòng chọn bàn...", Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.FromArgb(0, 102, 204), Location = new Point(15, 15), AutoSize = true };
            pnlOrderInfo.Controls.Add(lblCurrentTable);

            dgvBillDetails = new DataGridView { Location = new Point(15, 50), Size = new Size(490, 280), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            pnlOrderInfo.Controls.Add(dgvBillDetails);

            cboCategories = new ComboBox { Location = new Point(15, 350), Size = new Size(150, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cboCategories.SelectedIndexChanged += cboCategories_SelectedIndexChanged;
            pnlOrderInfo.Controls.Add(cboCategories);

            cboFoods = new ComboBox { Location = new Point(175, 350), Size = new Size(180, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlOrderInfo.Controls.Add(cboFoods);

            numQuantity = new NumericUpDown { Location = new Point(365, 350), Size = new Size(60, 22), Minimum = 1, Maximum = 99, Value = 1 };
            pnlOrderInfo.Controls.Add(numQuantity);

            btnAddFood = new Button { Text = "Thêm món", Location = new Point(435, 345), Size = new Size(70, 30), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddFood.Click += btnAddFood_Click;
            pnlOrderInfo.Controls.Add(btnAddFood);

            lblTotalAmount = new Label { Text = "Tổng tiền: 0 VNĐ", Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(15, 410), AutoSize = true };
            pnlOrderInfo.Controls.Add(lblTotalAmount);

            btnCheckout = new Button { Text = "Thanh toán", Location = new Point(385, 400), Size = new Size(120, 45), BackColor = Color.FromArgb(204, 51, 0), ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnCheckout.Click += btnCheckout_Click;
            pnlOrderInfo.Controls.Add(btnCheckout);
        }

        private void LoadTableButtons()
        {
            pnlTableList.Controls.Clear();
            pnlTableList.Controls.Add(new Label { Text = "SƠ ĐỒ BÀN ĂN", Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true });

            try
            {
                DataTable dt = cafeBUS.GetBanAnList();
                int startX = 15, startY = 50, width = 120, height = 80, gap = 15, count = 0;

                foreach (DataRow row in dt.Rows)
                {
                    Button btn = new Button
                    {
                        Width = width,
                        Height = height,
                        Left = startX + (count % 3) * (width + gap),
                        Top = startY + (count / 3) * (height + gap)
                    };

                    int maBan = Convert.ToInt32(row["MaBan"]);
                    string tenBan = row["TenBan"].ToString();
                    string trangThai = row["TrangThai"].ToString();

                    btn.Text = $"{tenBan}\n({trangThai})";
                    btn.Tag = maBan;
                    btn.Font = new Font("Arial", 9, FontStyle.Bold);
                    btn.BackColor = (trangThai == "Trống") ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.Click += TableButton_Click;

                    pnlTableList.Controls.Add(btn);
                    count++;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải bàn: " + ex.Message); }
        }

        private void LoadCategories()
        {
            try
            {
                DataTable dt = cafeBUS.GetDanhMucList();
                cboCategories.DataSource = dt;
                cboCategories.DisplayMember = "TenDanhMuc";
                cboCategories.ValueMember = "MaDanhMuc";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh mục: " + ex.Message); }
        }

        private void cboCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCategories.SelectedValue is DataRowView) return;
            try
            {
                if (cboCategories.SelectedValue != null && int.TryParse(cboCategories.SelectedValue.ToString(), out int maDM))
                {
                    DataTable dt = cafeBUS.GetMonAnByDanhMuc(maDM);
                    cboFoods.DataSource = dt;
                    cboFoods.DisplayMember = "TenMon";
                    cboFoods.ValueMember = "MaMon";
                }
            }
            catch { }
        }

        private void TableButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            selectedTableId = (int)btn.Tag;
            lblCurrentTable.Text = $"Đang chọn: {btn.Text.Split('\n')[0]}";
            LoadBillDetails(selectedTableId);
        }

        private void LoadBillDetails(int maBan)
        {
            try
            {
                DataTable dt = cafeBUS.GetChiTietHoaDon(maBan);
                if (dt.Rows.Count > 0)
                {
                    dgvBillDetails.DataSource = dt;
                    decimal total = 0;
                    foreach (DataRow row in dt.Rows) total += Convert.ToDecimal(row["Thành tiền"]);
                    lblTotalAmount.Text = $"Tổng tiền: {total:N0} VNĐ";
                }
                else
                {
                    dgvBillDetails.DataSource = null;
                    lblTotalAmount.Text = "Tổng tiền: 0 VNĐ";
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load hóa đơn: " + ex.Message); }
        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {
            if (selectedTableId == -1) { MessageBox.Show("Vui lòng chọn bàn!"); return; }
            if (cboFoods.SelectedValue == null) { MessageBox.Show("Vui lòng chọn món!"); return; }

            int maMon = Convert.ToInt32(cboFoods.SelectedValue);
            int soLuong = (int)numQuantity.Value;

            try
            {
                cafeBUS.ThemMon(selectedTableId, maMon, soLuong);
                LoadTableButtons();
                LoadBillDetails(selectedTableId);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm món: " + ex.Message); }
        }

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (selectedTableId == -1) { MessageBox.Show("Vui lòng chọn bàn thanh toán!"); return; }

            if (cafeBUS.ThanhToan(selectedTableId, out decimal tongTien, out string err))
            {
                MessageBox.Show($"Thanh toán thành công!\nTổng tiền: {tongTien:N0} VNĐ", "Thông Báo");
                LoadTableButtons();
                LoadBillDetails(selectedTableId);
            }
            else
            {
                MessageBox.Show(err);
            }
        }
    }
}

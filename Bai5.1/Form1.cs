using System;
using System.Data;
using System.Drawing;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace CafePOSSystem
{
    public partial class Form1 : Form
    {
        // Chuỗi kết nối MySQL / Navicat về lại cổng mặc định 3306
        private string connectionString = "Server=localhost;Port=3306;Database=CafePOSDB;Uid=root;Pwd=;";

        // Các Controls giao diện chính
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
            InitializeDatabaseAndDefaultData();
            InitializeCustomComponents();
            LoadTableButtons();
            LoadCategories();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        // 1. Tự động khởi tạo Database CafePOSDB và các bảng dữ liệu sang Navicat
        private void InitializeDatabaseAndDefaultData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Port=3306;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS CafePOSDB;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string script = @"
                        CREATE TABLE IF NOT EXISTS BanAn (
                            MaBan INT AUTO_INCREMENT PRIMARY KEY,
                            TenBan VARCHAR(50) NOT NULL,
                            TrangThai VARCHAR(20) DEFAULT 'Trống'
                        );

                        CREATE TABLE IF NOT EXISTS DanhMuc (
                            MaDanhMuc INT AUTO_INCREMENT PRIMARY KEY,
                            TenDanhMuc VARCHAR(100) NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS MonAn (
                            MaMon INT AUTO_INCREMENT PRIMARY KEY,
                            TenMon VARCHAR(100) NOT NULL,
                            MaDanhMuc INT,
                            DonGia DECIMAL(18,2),
                            FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc)
                        );

                        CREATE TABLE IF NOT EXISTS HoaDon (
                            MaHoaDon INT AUTO_INCREMENT PRIMARY KEY,
                            MaBan INT,
                            GioVao DATETIME,
                            GioRa DATETIME,
                            TongTien DECIMAL(18,2),
                            TrangThai INT DEFAULT 0, 
                            FOREIGN KEY (MaBan) REFERENCES BanAn(MaBan)
                        );

                        CREATE TABLE IF NOT EXISTS ChiTietHoaDon (
                            MaHoaDon INT,
                            MaMon INT,
                            SoLuong INT,
                            DonGia DECIMAL(18,2),
                            PRIMARY KEY (MaHoaDon, MaMon),
                            FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
                            FOREIGN KEY (MaMon) REFERENCES MonAn(MaMon)
                        );
                    ";
                    using (MySqlCommand cmd = new MySqlCommand(script, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    string checkTable = "SELECT COUNT(*) FROM BanAn;";
                    using (MySqlCommand cmd = new MySqlCommand(checkTable, conn))
                    {
                        long count = (long)cmd.ExecuteScalar();
                        if (count == 0)
                        {
                            string seedData = @"
                                INSERT INTO BanAn (TenBan, TrangThai) VALUES ('Bàn 01', 'Trống'), ('Bàn 02', 'Trống'), ('Bàn 03', 'Trống'), ('Bàn 04', 'Trống'), ('Bàn 05', 'Trống'), ('Bàn 06', 'Trống');
                                INSERT INTO DanhMuc (TenDanhMuc) VALUES ('Cà phê'), ('Trà sữa'), ('Đồ ăn nhẹ');
                                INSERT INTO MonAn (TenMon, MaDanhMuc, DonGia) VALUES ('Cà phê đen', 1, 25000), ('Cà phê sữa', 1, 30000), ('Trà sữa trân châu', 2, 45000), ('Trà sữa thái xanh', 2, 40000), ('Hướng dương', 3, 15000), ('Khoai tây chiên', 3, 35000);
                            ";
                            using (MySqlCommand seedCmd = new MySqlCommand(seedData, conn))
                            {
                                seedCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo CSDL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 2. Thiết kế giao diện POS bằng code
        private void InitializeCustomComponents()
        {
            this.Text = "Hệ Thống Quản Lý Quán Cafe & Bán Hàng (POS Cafe)";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            pnlTableList = new Panel();
            pnlTableList.Location = new Point(12, 12);
            pnlTableList.Size = new Size(420, 535);
            pnlTableList.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(pnlTableList);

            pnlOrderInfo = new Panel();
            pnlOrderInfo.Location = new Point(445, 12);
            pnlOrderInfo.Size = new Size(525, 535);
            pnlOrderInfo.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(pnlOrderInfo);

            lblCurrentTable = new Label();
            lblCurrentTable.Text = "Vui lòng chọn bàn...";
            lblCurrentTable.Font = new Font("Arial", 12, FontStyle.Bold);
            lblCurrentTable.ForeColor = Color.FromArgb(0, 102, 204);
            lblCurrentTable.Location = new Point(15, 15);
            lblCurrentTable.AutoSize = true;
            pnlOrderInfo.Controls.Add(lblCurrentTable);

            dgvBillDetails = new DataGridView();
            dgvBillDetails.Location = new Point(15, 50);
            dgvBillDetails.Size = new Size(490, 280);
            dgvBillDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBillDetails.ReadOnly = true;
            pnlOrderInfo.Controls.Add(dgvBillDetails);

            cboCategories = new ComboBox();
            cboCategories.Location = new Point(15, 350);
            cboCategories.Size = new Size(150, 24);
            cboCategories.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategories.SelectedIndexChanged += cboCategories_SelectedIndexChanged;
            pnlOrderInfo.Controls.Add(cboCategories);

            cboFoods = new ComboBox();
            cboFoods.Location = new Point(175, 350);
            cboFoods.Size = new Size(180, 24);
            cboFoods.DropDownStyle = ComboBoxStyle.DropDownList;
            pnlOrderInfo.Controls.Add(cboFoods);

            numQuantity = new NumericUpDown();
            numQuantity.Location = new Point(365, 350);
            numQuantity.Size = new Size(60, 22);
            numQuantity.Minimum = 1;
            numQuantity.Maximum = 99;
            numQuantity.Value = 1;
            pnlOrderInfo.Controls.Add(numQuantity);

            btnAddFood = new Button();
            btnAddFood.Text = "Thêm món";
            btnAddFood.Location = new Point(435, 345);
            btnAddFood.Size = new Size(70, 30);
            btnAddFood.BackColor = Color.FromArgb(40, 167, 69);
            btnAddFood.ForeColor = Color.White;
            btnAddFood.FlatStyle = FlatStyle.Flat;
            btnAddFood.Click += btnAddFood_Click;
            pnlOrderInfo.Controls.Add(btnAddFood);

            lblTotalAmount = new Label();
            lblTotalAmount.Text = "Tổng tiền: 0 VNĐ";
            lblTotalAmount.Font = new Font("Arial", 12, FontStyle.Bold);
            lblTotalAmount.ForeColor = Color.Red;
            lblTotalAmount.Location = new Point(15, 410);
            lblTotalAmount.AutoSize = true;
            pnlOrderInfo.Controls.Add(lblTotalAmount);

            btnCheckout = new Button();
            btnCheckout.Text = "Thanh toán";
            btnCheckout.Location = new Point(385, 400);
            btnCheckout.Size = new Size(120, 45);
            btnCheckout.BackColor = Color.FromArgb(204, 51, 0);
            btnCheckout.ForeColor = Color.White;
            btnCheckout.Font = new Font("Arial", 10, FontStyle.Bold);
            btnCheckout.FlatStyle = FlatStyle.Flat;
            btnCheckout.Click += btnCheckout_Click;
            pnlOrderInfo.Controls.Add(btnCheckout);
        }

        // 3. Hiển thị danh sách bàn dạng Button động
        private void LoadTableButtons()
        {
            pnlTableList.Controls.Clear();
            Label lblTitleTable = new Label() { Text = "SƠ ĐỒ BÀN ĂN", Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            pnlTableList.Controls.Add(lblTitleTable);

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM BanAn";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        int startX = 15;
                        int startY = 50;
                        int width = 120;
                        int height = 80;
                        int gap = 15;
                        int count = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            Button btn = new Button();
                            btn.Width = width;
                            btn.Height = height;
                            btn.Left = startX + (count % 3) * (width + gap);
                            btn.Top = startY + (count / 3) * (height + gap);

                            int maBan = Convert.ToInt32(row["MaBan"]);
                            string tenBan = row["TenBan"].ToString();
                            string trangThai = row["TrangThai"].ToString();

                            btn.Text = $"{tenBan}\n({trangThai})";
                            btn.Tag = maBan;
                            btn.Font = new Font("Arial", 9, FontStyle.Bold);

                            if (trangThai == "Trống")
                            {
                                btn.BackColor = Color.FromArgb(40, 167, 69);
                            }
                            else
                            {
                                btn.BackColor = Color.FromArgb(220, 53, 69);
                            }
                            btn.ForeColor = Color.White;
                            btn.FlatStyle = FlatStyle.Flat;

                            btn.Click += TableButton_Click;
                            pnlTableList.Controls.Add(btn);
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách bàn: " + ex.Message);
            }
        }

        // 4. Load danh mục
        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM DanhMuc";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboCategories.DataSource = dt;
                    cboCategories.DisplayMember = "TenDanhMuc";
                    cboCategories.ValueMember = "MaDanhMuc";
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh mục: " + ex.Message); }
        }

        private void cboCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCategories.SelectedValue is DataRowView) return;
            try
            {
                if (cboCategories.SelectedValue != null && int.TryParse(cboCategories.SelectedValue.ToString(), out int maDanhMuc))
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "SELECT * FROM MonAn WHERE MaDanhMuc = @MaDM";
                        MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                        da.SelectCommand.Parameters.AddWithValue("@MaDM", maDanhMuc);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        cboFoods.DataSource = dt;
                        cboFoods.DisplayMember = "TenMon";
                        cboFoods.ValueMember = "MaMon";
                    }
                }
            }
            catch { }
        }

        private void TableButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            selectedTableId = (int)btn.Tag;
            lblCurrentTable.Text = $"Đang chọn: {btn.Text.Split('\n')[0]}";

            LoadBillDetailsByTable(selectedTableId);
        }

        // 5. Nạp chi tiết hóa đơn bằng mô hình ngắt kết nối
        private void LoadBillDetailsByTable(int maBan)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string getBill = "SELECT MaHoaDon FROM HoaDon WHERE MaBan = @MaBan AND TrangThai = 0";
                    MySqlCommand cmd = new MySqlCommand(getBill, conn);
                    cmd.Parameters.AddWithValue("@MaBan", maBan);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        int maHoaDon = Convert.ToInt32(result);

                        string query = @"
                            SELECT m.TenMon AS 'Tên món', c.SoLuong AS 'Số lượng', c.DonGia AS 'Đơn giá', (c.SoLuong * c.DonGia) AS 'Thành tiền'
                            FROM ChiTietHoaDon c
                            JOIN MonAn m ON c.MaMon = m.MaMon
                            WHERE c.MaHoaDon = @MaHD;
                        ";
                        MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                        da.SelectCommand.Parameters.AddWithValue("@MaHD", maHoaDon);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvBillDetails.DataSource = dt;

                        decimal total = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            total += Convert.ToDecimal(row["Thành tiền"]);
                        }
                        lblTotalAmount.Text = $"Tổng tiền: {total:N0} VNĐ";
                    }
                    else
                    {
                        dgvBillDetails.DataSource = null;
                        lblTotalAmount.Text = "Tổng tiền: 0 VNĐ";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load hóa đơn: " + ex.Message); }
        }

        // 6. Gọi món
        private void btnAddFood_Click(object sender, EventArgs e)
        {
            if (selectedTableId == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi thêm món!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboFoods.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn món ăn!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maMon = Convert.ToInt32(cboFoods.SelectedValue);
            int soLuong = (int)numQuantity.Value;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string checkBill = "SELECT MaHoaDon FROM HoaDon WHERE MaBan = @MaBan AND TrangThai = 0";
                    MySqlCommand cmdBill = new MySqlCommand(checkBill, conn);
                    cmdBill.Parameters.AddWithValue("@MaBan", selectedTableId);
                    object result = cmdBill.ExecuteScalar();

                    int maHoaDon = 0;
                    if (result == null)
                    {
                        string insertBill = "INSERT INTO HoaDon (MaBan, GioVao, TrangThai) VALUES (@MaBan, NOW(), 0); SELECT LAST_INSERT_ID();";
                        MySqlCommand cmdInsertBill = new MySqlCommand(insertBill, conn);
                        cmdInsertBill.Parameters.AddWithValue("@MaBan", selectedTableId);
                        maHoaDon = Convert.ToInt32(cmdInsertBill.ExecuteScalar());

                        string updateTable = "UPDATE BanAn SET TrangThai = 'Có khách' WHERE MaBan = @MaBan";
                        MySqlCommand cmdUpdateTable = new MySqlCommand(updateTable, conn);
                        cmdUpdateTable.Parameters.AddWithValue("@MaBan", selectedTableId);
                        cmdUpdateTable.ExecuteNonQuery();
                    }
                    else
                    {
                        maHoaDon = Convert.ToInt32(result);
                    }

                    string getPrice = "SELECT DonGia FROM MonAn WHERE MaMon = @MaMon";
                    MySqlCommand cmdPrice = new MySqlCommand(getPrice, conn);
                    cmdPrice.Parameters.AddWithValue("@MaMon", maMon);
                    decimal donGia = Convert.ToDecimal(cmdPrice.ExecuteScalar());

                    string checkDetail = "SELECT SoLuong FROM ChiTietHoaDon WHERE MaHoaDon = @MaHD AND MaMon = @MaMon";
                    MySqlCommand cmdCheckDetail = new MySqlCommand(checkDetail, conn);
                    cmdCheckDetail.Parameters.AddWithValue("@MaHD", maHoaDon);
                    cmdCheckDetail.Parameters.AddWithValue("@MaMon", maMon);
                    object detailResult = cmdCheckDetail.ExecuteScalar();

                    if (detailResult != null)
                    {
                        string updateDetail = "UPDATE ChiTietHoaDon SET SoLuong = SoLuong + @SoLuong WHERE MaHoaDon = @MaHD AND MaMon = @MaMon";
                        MySqlCommand cmdUpdateDetail = new MySqlCommand(updateDetail, conn);
                        cmdUpdateDetail.Parameters.AddWithValue("@SoLuong", soLuong);
                        cmdUpdateDetail.Parameters.AddWithValue("@MaHD", maHoaDon);
                        cmdUpdateDetail.Parameters.AddWithValue("@MaMon", maMon);
                        cmdUpdateDetail.ExecuteNonQuery();
                    }
                    else
                    {
                        string insertDetail = "INSERT INTO ChiTietHoaDon (MaHoaDon, MaMon, SoLuong, DonGia) VALUES (@MaHD, @MaMon, @SoLuong, @DonGia)";
                        MySqlCommand cmdInsertDetail = new MySqlCommand(insertDetail, conn);
                        cmdInsertDetail.Parameters.AddWithValue("@MaHD", maHoaDon);
                        cmdInsertDetail.Parameters.AddWithValue("@MaMon", maMon);
                        cmdInsertDetail.Parameters.AddWithValue("@SoLuong", soLuong);
                        cmdInsertDetail.Parameters.AddWithValue("@DonGia", donGia);
                        cmdInsertDetail.ExecuteNonQuery();
                    }
                }

                LoadTableButtons();
                LoadBillDetailsByTable(selectedTableId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm món: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 7. Thanh toán
        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (selectedTableId == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn cần thanh toán!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string getQuery = "SELECT MaHoaDon FROM HoaDon WHERE MaBan = @MaBan AND TrangThai = 0";
                    MySqlCommand cmdBill = new MySqlCommand(getQuery, conn);
                    cmdBill.Parameters.AddWithValue("@MaBan", selectedTableId);
                    object result = cmdBill.ExecuteScalar();

                    if (result == null)
                    {
                        MessageBox.Show("Bàn này hiện không có hóa đơn nào cần thanh toán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    int maHoaDon = Convert.ToInt32(result);

                    string getSum = "SELECT SUM(SoLuong * DonGia) FROM ChiTietHoaDon WHERE MaHoaDon = @MaHD";
                    MySqlCommand cmdSum = new MySqlCommand(getSum, conn);
                    cmdSum.Parameters.AddWithValue("@MaHD", maHoaDon);
                    object sumResult = cmdSum.ExecuteScalar();
                    decimal tongTien = sumResult != DBNull.Value ? Convert.ToDecimal(sumResult) : 0;

                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string updateBill = "UPDATE HoaDon SET GioRa = NOW(), TongTien = @TongTien, TrangThai = 1 WHERE MaHoaDon = @MaHD";
                            using (MySqlCommand cmdUp = new MySqlCommand(updateBill, conn, transaction))
                            {
                                cmdUp.Parameters.AddWithValue("@TongTien", tongTien);
                                cmdUp.Parameters.AddWithValue("@MaHD", maHoaDon);
                                cmdUp.ExecuteNonQuery();
                            }

                            string updateTable = "UPDATE BanAn SET TrangThai = 'Trống' WHERE MaBan = @MaBan";
                            using (MySqlCommand cmdTb = new MySqlCommand(updateTable, conn, transaction))
                            {
                                cmdTb.Parameters.AddWithValue("@MaBan", selectedTableId);
                                cmdTb.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            MessageBox.Show($"Thanh toán thành công!\nTổng tiền: {tongTien:N0} VNĐ", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadTableButtons();
                            LoadBillDetailsByTable(selectedTableId);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Lỗi giao dịch thanh toán: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối khi thanh toán: " + ex.Message);
            }
        }
    }
}

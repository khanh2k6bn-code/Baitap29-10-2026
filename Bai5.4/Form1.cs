using System;
using System.Data;
using System.Drawing;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace HotelManagementSystem
{
    public partial class Form1 : Form
    {
        // Chuỗi kết nối MySQL / Navicat (Cổng chuẩn 3306)
        private string connectionString = "Server=localhost;Port=3306;Database=HotelDB;Uid=root;Pwd=;";

        // Các Controls giao diện chính
        private Panel pnlRoomList;       // Sơ đồ phòng khách sạn
        private Panel pnlControlPanel;   // Khu vực chức năng (Check-in, Dịch vụ, Thanh toán)
        private DataGridView dgvServices;
        private ComboBox cboServiceList;
        private NumericUpDown numQuantity;
        private Button btnAddService;
        private Button btnCheckIn;
        private Button btnCheckout;
        private Label lblRoomInfo;
        private Label lblTotalAmount;

        private string selectedRoomNo = ""; // Lưu số phòng đang chọn

        public Form1()
        {
            InitializeComponent();
            InitializeDatabaseAndDefaultData(); // Tự động tạo CSDL và bảng sang Navicat
            InitializeCustomComponents();       // Thiết kế giao diện WinForms bằng code
            LoadRoomButtons();                  // Hiển thị sơ đồ phòng động
            LoadServiceComboBox();              // Nạp danh mục dịch vụ
        }

        private void Form1_Load(object sender, EventArgs e) { }

        // 1. Tự động khởi tạo Database HotelDB và 5 bảng dữ liệu sang Navicat
        private void InitializeDatabaseAndDefaultData()
        {
            try
            {
                // Tạo Database HotelDB nếu chưa có
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Port=3306;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS HotelDB;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Tạo 5 bảng theo đúng yêu cầu đề bài[cite: 14, 15]
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string script = @"
                        CREATE TABLE IF NOT EXISTS Phong (
                            SoPhong VARCHAR(10) PRIMARY KEY,
                            LoaiPhong VARCHAR(50),
                            GiaTheoGio DECIMAL(18,2),
                            TrangThai VARCHAR(30) DEFAULT 'Trống'
                        );

                        CREATE TABLE IF NOT EXISTS KhachHang (
                            MaKH INT AUTO_INCREMENT PRIMARY KEY,
                            HoTen VARCHAR(100) NOT NULL,
                            CCCD VARCHAR(20) UNIQUE NOT NULL,
                            SoDienThoai VARCHAR(15)
                        );

                        CREATE TABLE IF NOT EXISTS DichVu (
                            MaDV INT AUTO_INCREMENT PRIMARY KEY,
                            TenDV VARCHAR(100),
                            DonGia DECIMAL(18,2)
                        );

                        CREATE TABLE IF NOT EXISTS PhieuDatPhong (
                            MaPhieu INT AUTO_INCREMENT PRIMARY KEY,
                            SoPhong VARCHAR(10),
                            MaKH INT,
                            NgayNhanPhong DATETIME,
                            NgayTraPhong DATETIME NULL,
                            TongTienThanhToan DECIMAL(18,2),
                            FOREIGN KEY (SoPhong) REFERENCES Phong(SoPhong),
                            FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
                        );

                        CREATE TABLE IF NOT EXISTS SuDungDichVu (
                            MaPhieu INT,
                            MaDV INT,
                            SoLuong INT,
                            ThoiGian DATETIME,
                            PRIMARY KEY (MaPhieu, MaDV),
                            FOREIGN KEY (MaPhieu) REFERENCES PhieuDatPhong(MaPhieu),
                            FOREIGN KEY (MaDV) REFERENCES DichVu(MaDV)
                        );
                    ";
                    using (MySqlCommand cmd = new MySqlCommand(script, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Thêm dữ liệu mẫu nếu bảng Phong đang trống
                    string checkPhong = "SELECT COUNT(*) FROM Phong;";
                    using (MySqlCommand cmd = new MySqlCommand(checkPhong, conn))
                    {
                        long count = (long)cmd.ExecuteScalar();
                        if (count == 0)
                        {
                            string seedData = @"
                                INSERT INTO Phong (SoPhong, LoaiPhong, GiaTheoGio, TrangThai) VALUES 
                                ('P101', 'Đơn', 80000, 'Trống'), 
                                ('P102', 'Đơn', 80000, 'Trống'), 
                                ('P201', 'Đôi', 120000, 'Trống'), 
                                ('P202', 'Đôi', 120000, 'Trống'), 
                                ('P301', 'VIP', 200000, 'Trống');

                                INSERT INTO DichVu (TenDV, DonGia) VALUES 
                                ('Nước suối', 10000), 
                                ('Mì tôm ly', 15000), 
                                ('Giặt là ủi', 50000), 
                                ('Bia hơi', 25000);
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

        // 2. Thiết kế giao diện ứng dụng Hotel Management bằng code
        private void InitializeCustomComponents()
        {
            this.Text = "Hệ Thống Quản Lý Khách Sạn (Hotel Management System)";
            this.Size = new Size(1050, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Panel danh sách phòng bên trái
            pnlRoomList = new Panel();
            pnlRoomList.Location = new Point(12, 12);
            pnlRoomList.Size = new Size(420, 555);
            pnlRoomList.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(pnlRoomList);

            // Panel quản lý dịch vụ và check-in/out bên phải
            pnlControlPanel = new Panel();
            pnlControlPanel.Location = new Point(445, 12);
            pnlControlPanel.Size = new Size(575, 555);
            pnlControlPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(pnlControlPanel);

            lblRoomInfo = new Label();
            lblRoomInfo.Text = "Vui lòng chọn phòng...";
            lblRoomInfo.Font = new Font("Arial", 12, FontStyle.Bold);
            lblRoomInfo.ForeColor = Color.FromArgb(0, 102, 204);
            lblRoomInfo.Location = new Point(15, 15);
            lblRoomInfo.AutoSize = true;
            pnlControlPanel.Controls.Add(lblRoomInfo);

            // Nút Check-in Nhận phòng
            btnCheckIn = new Button();
            btnCheckIn.Text = "Check-in Nhận phòng";
            btnCheckIn.Location = new Point(380, 15);
            btnCheckIn.Size = new Size(180, 35);
            btnCheckIn.BackColor = Color.FromArgb(0, 122, 204);
            btnCheckIn.ForeColor = Color.White;
            btnCheckIn.FlatStyle = FlatStyle.Flat;
            btnCheckIn.Click += btnCheckIn_Click;
            pnlControlPanel.Controls.Add(btnCheckIn);

            // DataGridView hiển thị danh sách dịch vụ đã dùng của phòng
            dgvServices = new DataGridView();
            dgvServices.Location = new Point(15, 65);
            dgvServices.Size = new Size(545, 270);
            dgvServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvServices.ReadOnly = true;
            pnlControlPanel.Controls.Add(dgvServices);

            // ComboBox chọn Dịch vụ
            cboServiceList = new ComboBox();
            cboServiceList.Location = new Point(15, 355);
            cboServiceList.Size = new Size(200, 24);
            cboServiceList.DropDownStyle = ComboBoxStyle.DropDownList;
            pnlControlPanel.Controls.Add(cboServiceList);

            // NumericUpDown chọn số lượng dịch vụ
            numQuantity = new NumericUpDown();
            numQuantity.Location = new Point(230, 355);
            numQuantity.Size = new Size(60, 22);
            numQuantity.Minimum = 1;
            numQuantity.Maximum = 99;
            numQuantity.Value = 1;
            pnlControlPanel.Controls.Add(numQuantity);

            // Nút Thêm dịch vụ
            btnAddService = new Button();
            btnAddService.Text = "Đặt dịch vụ";
            btnAddService.Location = new Point(305, 350);
            btnAddService.Size = new Size(100, 32);
            btnAddService.BackColor = Color.FromArgb(40, 167, 69);
            btnAddService.ForeColor = Color.White;
            btnAddService.FlatStyle = FlatStyle.Flat;
            btnAddService.Click += btnAddService_Click;
            pnlControlPanel.Controls.Add(btnAddService);

            // Label tổng tiền thanh toán
            lblTotalAmount = new Label();
            lblTotalAmount.Text = "Tổng tiền tạm tính: 0 VNĐ";
            lblTotalAmount.Font = new Font("Arial", 12, FontStyle.Bold);
            lblTotalAmount.ForeColor = Color.Red;
            lblTotalAmount.Location = new Point(15, 420);
            lblTotalAmount.AutoSize = true;
            pnlControlPanel.Controls.Add(lblTotalAmount);

            // Nút Check-out Thanh toán
            btnCheckout = new Button();
            btnCheckout.Text = "Check-out & Thanh Toán";
            btnCheckout.Location = new Point(345, 410);
            btnCheckout.Size = new Size(215, 45);
            btnCheckout.BackColor = Color.FromArgb(204, 51, 0);
            btnCheckout.ForeColor = Color.White;
            btnCheckout.Font = new Font("Arial", 10, FontStyle.Bold);
            btnCheckout.FlatStyle = FlatStyle.Flat;
            btnCheckout.Click += btnCheckout_Click;
            pnlControlPanel.Controls.Add(btnCheckout);
        }

        // 3. Hiển thị sơ đồ phòng động (Xanh: Trống, Đỏ: Đang ở, Vàng: Dọn dẹp)
        private void LoadRoomButtons()
        {
            pnlRoomList.Controls.Clear();
            Label lblTitle = new Label() { Text = "SƠ ĐỒ PHÒNG KHÁCH SẠN", Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            pnlRoomList.Controls.Add(lblTitle);

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Phong";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        int startX = 15;
                        int startY = 55;
                        int width = 120;
                        int height = 85;
                        int gap = 15;
                        int count = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            Button btn = new Button();
                            btn.Width = width;
                            btn.Height = height;
                            btn.Left = startX + (count % 3) * (width + gap);
                            btn.Top = startY + (count / 3) * (height + gap);

                            string soPhong = row["SoPhong"].ToString();
                            string loaiPhong = row["LoaiPhong"].ToString();
                            string trangThai = row["TrangThai"].ToString();
                            decimal gia = Convert.ToDecimal(row["GiaTheoGio"]);

                            btn.Text = $"{soPhong} ({loaiPhong})\n[{trangThai}]\n{gia:N0}đ/h";
                            btn.Tag = soPhong;
                            btn.Font = new Font("Arial", 8.5F, FontStyle.Bold);

                            if (trangThai == "Trống") btn.BackColor = Color.FromArgb(40, 167, 69); // Xanh lá
                            else if (trangThai == "Đang ở") btn.BackColor = Color.FromArgb(220, 53, 69); // Đỏ
                            else btn.BackColor = Color.FromArgb(255, 193, 7); // Vàng (Đang dọn dẹp)

                            btn.ForeColor = Color.White;
                            btn.FlatStyle = FlatStyle.Flat;
                            btn.Click += RoomButton_Click;

                            pnlRoomList.Controls.Add(btn);
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách phòng: " + ex.Message); }
        }

        // 4. Load danh mục dịch vụ
        private void LoadServiceComboBox()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM DichVu", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboServiceList.DataSource = dt;
                    cboServiceList.DisplayMember = "TenDV";
                    cboServiceList.ValueMember = "MaDV";
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dịch vụ: " + ex.Message); }
        }

        // Khi click chọn phòng trên sơ đồ
        private void RoomButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            selectedRoomNo = btn.Tag.ToString();
            lblRoomInfo.Text = $"Phòng đang chọn: {selectedRoomNo}";

            LoadRoomServiceDetails(selectedRoomNo);
        }

        // Nạp chi tiết dịch vụ và tính tiền tạm tính của phòng đang ở
        private void LoadRoomServiceDetails(string soPhong)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Lấy phiếu đặt phòng chưa thanh toán (NgayTraPhong IS NULL)
                    string getPhieu = "SELECT MaPhieu, NgayNhanPhong FROM PhieuDatPhong WHERE SoPhong = @Phong AND NgayTraPhong IS NULL";
                    MySqlCommand cmd = new MySqlCommand(getPhieu, conn);
                    cmd.Parameters.AddWithValue("@Phong", soPhong);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int maPhieu = reader.GetInt32("MaPhieu");
                            DateTime ngayNhan = reader.GetDateTime("NgayNhanPhong");
                            reader.Close();

                            // Load danh sách dịch vụ đã dùng bằng SqlDataAdapter
                            string queryDV = @"
                                SELECT dv.TenDV AS 'Tên dịch vụ', s.SoLuong AS 'Số lượng', dv.DonGia AS 'Đơn giá', (s.SoLuong * dv.DonGia) AS 'Thành tiền'
                                FROM SuDungDichVu s
                                JOIN DichVu dv ON s.MaDV = dv.MaDV
                                WHERE s.MaPhieu = @MaPhieu;
                            ";
                            MySqlDataAdapter da = new MySqlDataAdapter(queryDV, conn);
                            da.SelectCommand.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dgvServices.DataSource = dt;

                            // Tính tiền dịch vụ
                            decimal tienDV = 0;
                            foreach (DataRow r in dt.Rows) tienDV += Convert.ToDecimal(r["Thành tiền"]);

                            // Tính tiền phòng theo giờ (Thời gian ở x Giá phòng theo giờ)
                            string getGia = "SELECT GiaTheoGio FROM Phong WHERE SoPhong = @Phong";
                            MySqlCommand cmdGia = new MySqlCommand(getGia, conn);
                            cmdGia.Parameters.AddWithValue("@Phong", soPhong);
                            decimal giaTheoGio = Convert.ToDecimal(cmdGia.ExecuteScalar());

                            TimeSpan timeStay = DateTime.Now - ngayNhan;
                            double hours = Math.Max(timeStay.TotalHours, 1.0); // Tối thiểu tính 1 giờ
                            decimal tienPhong = (decimal)hours * giaTheoGio;

                            decimal tongTien = tienPhong + tienDV;
                            lblTotalAmount.Text = $"Tổng tiền tạm tính: {tongTien:N0} VNĐ (Tiền phòng: {tienPhong:N0}đ)";
                        }
                        else
                        {
                            dgvServices.DataSource = null;
                            lblTotalAmount.Text = "Tổng tiền tạm tính: 0 VNĐ (Phòng trống)";
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load thông tin phòng: " + ex.Message); }
        }

        // 5. Chức năng Check-in Nhận phòng (Sử dụng MySqlParameter tra cứu CCCD)[cite: 15]
        private void btnCheckIn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedRoomNo))
            {
                MessageBox.Show("Vui lòng chọn phòng trống trên sơ đồ trước khi nhận phòng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sử dụng Custom InputDialog thuần C# thay thế InputBox[cite: 17]
            string hoTen = PromptDialog.ShowDialog("Nhập họ tên khách hàng:", "Check-in Khách Hàng");
            if (string.IsNullOrWhiteSpace(hoTen)) return;

            string cccd = PromptDialog.ShowDialog("Nhập số CCCD (Mã định danh):", "Check-in Khách Hàng");
            if (string.IsNullOrWhiteSpace(cccd)) return;

            string sdt = PromptDialog.ShowDialog("Nhập số điện thoại khách hàng:", "Check-in Khách Hàng");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Tra cứu khách hàng theo CCCD bằng MySqlParameter[cite: 15]
                    int maKH = 0;
                    string checkKH = "SELECT MaKH FROM KhachHang WHERE CCCD = @CCCD";
                    MySqlCommand cmdCheck = new MySqlCommand(checkKH, conn);
                    cmdCheck.Parameters.AddWithValue("@CCCD", cccd);
                    object result = cmdCheck.ExecuteScalar();

                    if (result != null)
                    {
                        maKH = Convert.ToInt32(result);
                    }
                    else
                    {
                        // Nếu là khách mới -> Thêm thông tin khách hàng vào CSDL[cite: 15]
                        string insertKH = "INSERT INTO KhachHang (HoTen, CCCD, SoDienThoai) VALUES (@HoTen, @CCCD, @SDT); SELECT LAST_INSERT_ID();";
                        MySqlCommand cmdInsertKH = new MySqlCommand(insertKH, conn);
                        cmdInsertKH.Parameters.AddWithValue("@HoTen", hoTen);
                        cmdInsertKH.Parameters.AddWithValue("@CCCD", cccd);
                        cmdInsertKH.Parameters.AddWithValue("@SDT", sdt);
                        maKH = Convert.ToInt32(cmdInsertKH.ExecuteScalar());
                    }

                    // Tạo phiếu đặt phòng mới & chuyển trạng thái phòng thành "Đang ở"[cite: 15]
                    string insertPhieu = "INSERT INTO PhieuDatPhong (SoPhong, MaKH, NgayNhanPhong) VALUES (@SoPhong, @MaKH, NOW());";
                    MySqlCommand cmdPhieu = new MySqlCommand(insertPhieu, conn);
                    cmdPhieu.Parameters.AddWithValue("@SoPhong", selectedRoomNo);
                    cmdPhieu.Parameters.AddWithValue("@MaKH", maKH);
                    cmdPhieu.ExecuteNonQuery();

                    string updateRoom = "UPDATE Phong SET TrangThai = 'Đang ở' WHERE SoPhong = @SoPhong";
                    MySqlCommand cmdRoom = new MySqlCommand(updateRoom, conn);
                    cmdRoom.Parameters.AddWithValue("@SoPhong", selectedRoomNo);
                    cmdRoom.ExecuteNonQuery();

                    MessageBox.Show($"Check-in thành công cho phòng {selectedRoomNo}!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRoomButtons();
                    LoadRoomServiceDetails(selectedRoomNo);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi Check-in: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // 6. Chức năng Gọi Dịch Vụ[cite: 16]
        private void btnAddService_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedRoomNo))
            {
                MessageBox.Show("Vui lòng chọn phòng đang có khách!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maDV = Convert.ToInt32(cboServiceList.SelectedValue);
            int soLuong = (int)numQuantity.Value;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Lấy MaPhieu hiện tại của phòng
                    string getPhieu = "SELECT MaPhieu FROM PhieuDatPhong WHERE SoPhong = @Phong AND NgayTraPhong IS NULL";
                    MySqlCommand cmd = new MySqlCommand(getPhieu, conn);
                    cmd.Parameters.AddWithValue("@Phong", selectedRoomNo);
                    object res = cmd.ExecuteScalar();

                    if (res == null)
                    {
                        MessageBox.Show("Phòng này hiện chưa có khách nhận phòng!", "Thông báo");
                        return;
                    }
                    int maPhieu = Convert.ToInt32(res);

                    // Thêm hoặc cập nhật dịch vụ sử dụng
                    string checkSD = "SELECT SoLuong FROM SuDungDichVu WHERE MaPhieu = @MaPhieu AND MaDV = @MaDV";
                    MySqlCommand cmdCheck = new MySqlCommand(checkSD, conn);
                    cmdCheck.Parameters.AddWithValue("@MaPhieu", maPhieu);
                    cmdCheck.Parameters.AddWithValue("@MaDV", maDV);
                    object exist = cmdCheck.ExecuteScalar();

                    if (exist != null)
                    {
                        string update = "UPDATE SuDungDichVu SET SoLuong = SoLuong + @SL WHERE MaPhieu = @MaPhieu AND MaDV = @MaDV";
                        MySqlCommand cmdUp = new MySqlCommand(update, conn);
                        cmdUp.Parameters.AddWithValue("@SL", soLuong);
                        cmdUp.Parameters.AddWithValue("@MaPhieu", maPhieu);
                        cmdUp.Parameters.AddWithValue("@MaDV", maDV);
                        cmdUp.ExecuteNonQuery();
                    }
                    else
                    {
                        string insert = "INSERT INTO SuDungDichVu (MaPhieu, MaDV, SoLuong, ThoiGian) VALUES (@MaPhieu, @MaDV, @SL, NOW())";
                        MySqlCommand cmdIns = new MySqlCommand(insert, conn);
                        cmdIns.Parameters.AddWithValue("@MaPhieu", maPhieu);
                        cmdIns.Parameters.AddWithValue("@MaDV", maDV);
                        cmdIns.Parameters.AddWithValue("@SL", soLuong);
                        cmdIns.ExecuteNonQuery();
                    }

                    MessageBox.Show("Đặt dịch vụ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRoomServiceDetails(selectedRoomNo);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi đặt dịch vụ: " + ex.Message); }
        }

        // 7. Chức năng Check-out & Xuất Hóa đơn (Thực thi MySqlTransaction)[cite: 16]
        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedRoomNo))
            {
                MessageBox.Show("Vui lòng chọn phòng cần thanh toán!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string getPhieu = "SELECT MaPhieu, NgayNhanPhong FROM PhieuDatPhong WHERE SoPhong = @Phong AND NgayTraPhong IS NULL";
                    MySqlCommand cmd = new MySqlCommand(getPhieu, conn);
                    cmd.Parameters.AddWithValue("@Phong", selectedRoomNo);
                    int maPhieu = 0;
                    DateTime ngayNhan = DateTime.Now;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            maPhieu = reader.GetInt32("MaPhieu");
                            ngayNhan = reader.GetDateTime("NgayNhanPhong");
                        }
                        else
                        {
                            MessageBox.Show("Phòng này hiện không có khách để thanh toán!", "Thông báo");
                            return;
                        }
                    }

                    // Tính tổng tiền dịch vụ
                    string getTienDV = "SELECT SUM(s.SoLuong * dv.DonGia) FROM SuDungDichVu s JOIN DichVu dv ON s.MaDV = dv.MaDV WHERE s.MaPhieu = @MaPhieu";
                    MySqlCommand cmdDV = new MySqlCommand(getTienDV, conn);
                    cmdDV.Parameters.AddWithValue("@MaPhieu", maPhieu);
                    object resDV = cmdDV.ExecuteScalar();
                    decimal tienDV = resDV != DBNull.Value ? Convert.ToDecimal(resDV) : 0;

                    // Tính tiền phòng theo giờ
                    string getGia = "SELECT GiaTheoGio FROM Phong WHERE SoPhong = @Phong";
                    MySqlCommand cmdGia = new MySqlCommand(getGia, conn);
                    cmdGia.Parameters.AddWithValue("@Phong", selectedRoomNo);
                    decimal giaTheoGio = Convert.ToDecimal(cmdGia.ExecuteScalar());

                    TimeSpan timeStay = DateTime.Now - ngayNhan;
                    double hours = Math.Max(timeStay.TotalHours, 1.0);
                    decimal tienPhong = (decimal)hours * giaTheoGio;

                    decimal tongTienThanhToan = tienPhong + tienDV;

                    // Thực thi Transaction đảm bảo đồng bộ dữ liệu[cite: 16]
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Cập nhật PhieuDatPhong: Ngày trả phòng, Tổng tiền thanh toán
                            string updatePhieu = "UPDATE PhieuDatPhong SET NgayTraPhong = NOW(), TongTienThanhToan = @TongTien WHERE MaPhieu = @MaPhieu";
                            using (MySqlCommand cmdUp = new MySqlCommand(updatePhieu, conn, transaction))
                            {
                                cmdUp.Parameters.AddWithValue("@TongTien", tongTienThanhToan);
                                cmdUp.Parameters.AddWithValue("@MaPhieu", maPhieu);
                                cmdUp.ExecuteNonQuery();
                            }

                            // Chuyển trạng thái phòng sang "Đang dọn dẹp"[cite: 16]
                            string updateRoom = "UPDATE Phong SET TrangThai = 'Đang dọn dẹp' WHERE SoPhong = @SoPhong";
                            using (MySqlCommand cmdRm = new MySqlCommand(updateRoom, conn, transaction))
                            {
                                cmdRm.Parameters.AddWithValue("@SoPhong", selectedRoomNo);
                                cmdRm.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            MessageBox.Show($"CHECK-OUT THÀNH CÔNG!\n\n- Tổng thời gian ở: {hours:F1} giờ\n- Tiền phòng: {tienPhong:N0} VNĐ\n- Tiền dịch vụ: {tienDV:N0} VNĐ\n- TỔNG THANH TOÁN: {tongTienThanhToan:N0} VNĐ", "Hóa Đơn Thanh Toán", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadRoomButtons();
                            LoadRoomServiceDetails(selectedRoomNo);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Lỗi giao dịch thanh toán: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi kết nối khi thanh toán: " + ex.Message); }
        }
    }

    // Class tự định nghĩa Hộp thoại Nhập liệu (thay thế InputBox thuần C#)[cite: 17]
    public static class PromptDialog
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
            Button confirmation = new Button() { Text = "Đồng ý", Left = 260, Width = 100, Top = 85, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}

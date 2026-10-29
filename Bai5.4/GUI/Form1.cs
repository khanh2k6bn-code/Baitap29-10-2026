using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelManagementSystem.BUS;

namespace HotelManagementSystem.GUI
{
    public partial class Form1 : Form
    {
        private HotelBUS hotelBUS = new HotelBUS();

        private Panel pnlRoomList;
        private Panel pnlControlPanel;
        private DataGridView dgvServices;
        private ComboBox cboServiceList;
        private NumericUpDown numQuantity;
        private Button btnAddService, btnCheckIn, btnCheckout;
        private Label lblRoomInfo, lblTotalAmount;

        private string selectedRoomNo = "";

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadRoomButtons();
            LoadServiceComboBox();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void InitializeCustomComponents()
        {
            this.Text = "Hệ Thống Quản Lý Khách Sạn (3-Tier Architecture)";
            this.Size = new Size(1050, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            pnlRoomList = new Panel { Location = new Point(12, 12), Size = new Size(420, 555), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlRoomList);

            pnlControlPanel = new Panel { Location = new Point(445, 12), Size = new Size(575, 555), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlControlPanel);

            lblRoomInfo = new Label { Text = "Vui lòng chọn phòng...", Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.FromArgb(0, 102, 204), Location = new Point(15, 15), AutoSize = true };
            pnlControlPanel.Controls.Add(lblRoomInfo);

            btnCheckIn = new Button { Text = "Check-in Nhận phòng", Location = new Point(380, 15), Size = new Size(180, 35), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCheckIn.Click += btnCheckIn_Click;
            pnlControlPanel.Controls.Add(btnCheckIn);

            dgvServices = new DataGridView { Location = new Point(15, 65), Size = new Size(545, 270), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            pnlControlPanel.Controls.Add(dgvServices);

            cboServiceList = new ComboBox { Location = new Point(15, 355), Size = new Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlControlPanel.Controls.Add(cboServiceList);

            numQuantity = new NumericUpDown { Location = new Point(230, 355), Size = new Size(60, 22), Minimum = 1, Maximum = 99, Value = 1 };
            pnlControlPanel.Controls.Add(numQuantity);

            btnAddService = new Button { Text = "Đặt dịch vụ", Location = new Point(305, 350), Size = new Size(100, 32), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddService.Click += btnAddService_Click;
            pnlControlPanel.Controls.Add(btnAddService);

            lblTotalAmount = new Label { Text = "Tổng tiền tạm tính: 0 VNĐ", Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(15, 420), AutoSize = true };
            pnlControlPanel.Controls.Add(lblTotalAmount);

            btnCheckout = new Button { Text = "Check-out & Thanh Toán", Location = new Point(345, 410), Size = new Size(215, 45), BackColor = Color.FromArgb(204, 51, 0), ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnCheckout.Click += btnCheckout_Click;
            pnlControlPanel.Controls.Add(btnCheckout);
        }

        private void LoadRoomButtons()
        {
            pnlRoomList.Controls.Clear();
            pnlRoomList.Controls.Add(new Label { Text = "SƠ ĐỒ PHÒNG KHÁCH SẠN", Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true });

            try
            {
                DataTable dt = hotelBUS.GetPhongList();
                int startX = 15, startY = 55, width = 120, height = 85, gap = 15, count = 0;

                foreach (DataRow row in dt.Rows)
                {
                    Button btn = new Button
                    {
                        Width = width,
                        Height = height,
                        Left = startX + (count % 3) * (width + gap),
                        Top = startY + (count / 3) * (height + gap)
                    };

                    string soPhong = row["SoPhong"].ToString();
                    string loaiPhong = row["LoaiPhong"].ToString();
                    string trangThai = row["TrangThai"].ToString();
                    decimal gia = Convert.ToDecimal(row["GiaTheoGio"]);

                    btn.Text = $"{soPhong} ({loaiPhong})\n[{trangThai}]\n{gia:N0}đ/h";
                    btn.Tag = soPhong;
                    btn.Font = new Font("Arial", 8.5F, FontStyle.Bold);

                    if (trangThai == "Trống") btn.BackColor = Color.FromArgb(40, 167, 69);
                    else if (trangThai == "Đang ở") btn.BackColor = Color.FromArgb(220, 53, 69);
                    else btn.BackColor = Color.FromArgb(255, 193, 7);

                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.Click += RoomButton_Click;

                    pnlRoomList.Controls.Add(btn);
                    count++;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải phòng: " + ex.Message); }
        }

        private void LoadServiceComboBox()
        {
            try
            {
                cboServiceList.DataSource = hotelBUS.GetDichVuList();
                cboServiceList.DisplayMember = "TenDV";
                cboServiceList.ValueMember = "MaDV";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dịch vụ: " + ex.Message); }
        }

        private void RoomButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            selectedRoomNo = btn.Tag.ToString();
            lblRoomInfo.Text = $"Phòng đang chọn: {selectedRoomNo}";
            LoadRoomServiceDetails(selectedRoomNo);
        }

        private void LoadRoomServiceDetails(string soPhong)
        {
            try
            {
                DataTable dt = hotelBUS.GetSuDungDichVuByPhong(soPhong);
                if (dt.Rows.Count > 0)
                {
                    dgvServices.DataSource = dt;
                }
                else
                {
                    dgvServices.DataSource = null;
                }
                lblTotalAmount.Text = "Phòng đang chọn: " + soPhong;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnCheckIn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedRoomNo)) { MessageBox.Show("Vui lòng chọn phòng trống!"); return; }

            string hoTen = PromptDialog.ShowDialog("Nhập họ tên khách hàng:", "Check-in");
            if (string.IsNullOrWhiteSpace(hoTen)) return;
            string cccd = PromptDialog.ShowDialog("Nhập số CCCD:", "Check-in");
            if (string.IsNullOrWhiteSpace(cccd)) return;
            string sdt = PromptDialog.ShowDialog("Nhập số điện thoại:", "Check-in");

            if (hotelBUS.CheckIn(selectedRoomNo, hoTen, cccd, sdt, out string err))
            {
                MessageBox.Show("Check-in thành công!");
                LoadRoomButtons();
                LoadRoomServiceDetails(selectedRoomNo);
            }
            else MessageBox.Show("Lỗi: " + err);
        }

        private void btnAddService_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedRoomNo)) { MessageBox.Show("Vui lòng chọn phòng!"); return; }
            int maDV = Convert.ToInt32(cboServiceList.SelectedValue);
            int sl = (int)numQuantity.Value;

            hotelBUS.DatDichVu(selectedRoomNo, maDV, sl);
            MessageBox.Show("Đặt dịch vụ thành công!");
            LoadRoomServiceDetails(selectedRoomNo);
        }

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedRoomNo)) { MessageBox.Show("Vui lòng chọn phòng!"); return; }

            if (hotelBUS.CheckOut(selectedRoomNo, out decimal tongTien, out double hours, out decimal tienPhong, out decimal tienDV, out string err))
            {
                MessageBox.Show($"CHECK-OUT THÀNH CÔNG!\n- Thời gian ở: {hours:F1}h\n- Tiền phòng: {tienPhong:N0}đ\n- Tiền dịch vụ: {tienDV:N0}đ\n- TỔNG: {tongTien:N0}đ", "Hóa Đơn");
                LoadRoomButtons();
                LoadRoomServiceDetails(selectedRoomNo);
            }
            else MessageBox.Show(err);
        }
    }

    public static class PromptDialog
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form { Width = 400, Height = 180, FormBorderStyle = FormBorderStyle.FixedDialog, Text = caption, StartPosition = FormStartPosition.CenterScreen, MaximizeBox = false, MinimizeBox = false };
            Label textLabel = new Label { Left = 20, Top = 20, Text = text, AutoSize = true };
            TextBox textBox = new TextBox { Left = 20, Top = 50, Width = 340 };
            Button confirmation = new Button { Text = "Đồng ý", Left = 260, Width = 100, Top = 85, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}

using System;
using System.Data;
using MySql.Data.MySqlClient;
using HotelManagementSystem.DAL;

namespace HotelManagementSystem.BUS
{
    public class HotelBUS
    {
        private HotelDAL hotelDAL = new HotelDAL();
        private DataProvider dp = new DataProvider();

        public DataTable GetPhongList() => hotelDAL.GetPhongList();
        public DataTable GetDichVuList() => hotelDAL.GetDichVuList();
        public DataTable GetSuDungDichVuByPhong(string soPhong) => hotelDAL.GetSuDungDichVuByPhong(soPhong);

        public bool CheckIn(string soPhong, string hoTen, string cccd, string sdt, out string err)
        {
            err = "";
            try
            {
                using (MySqlConnection conn = dp.GetConnection())
                {
                    conn.Open();
                    int maKH = 0;
                    string checkKH = "SELECT MaKH FROM KhachHang WHERE CCCD = @CCCD";
                    using (MySqlCommand cmdCheck = new MySqlCommand(checkKH, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@CCCD", cccd);
                        object res = cmdCheck.ExecuteScalar();
                        if (res != null) maKH = Convert.ToInt32(res);
                    }

                    if (maKH == 0)
                    {
                        string insertKH = "INSERT INTO KhachHang (HoTen, CCCD, SoDienThoai) VALUES (@HoTen, @CCCD, @SDT); SELECT LAST_INSERT_ID();";
                        using (MySqlCommand cmdIns = new MySqlCommand(insertKH, conn))
                        {
                            cmdIns.Parameters.AddWithValue("@HoTen", hoTen);
                            cmdIns.Parameters.AddWithValue("@CCCD", cccd);
                            cmdIns.Parameters.AddWithValue("@SDT", sdt);
                            maKH = Convert.ToInt32(cmdIns.ExecuteScalar());
                        }
                    }

                    string insertPhieu = "INSERT INTO PhieuDatPhong (SoPhong, MaKH, NgayNhanPhong) VALUES (@SoPhong, @MaKH, NOW());";
                    using (MySqlCommand cmdPhieu = new MySqlCommand(insertPhieu, conn))
                    {
                        cmdPhieu.Parameters.AddWithValue("@SoPhong", soPhong);
                        cmdPhieu.Parameters.AddWithValue("@MaKH", maKH);
                        cmdPhieu.ExecuteNonQuery();
                    }

                    string updateRoom = "UPDATE Phong SET TrangThai = 'Đang ở' WHERE SoPhong = @SoPhong";
                    using (MySqlCommand cmdRm = new MySqlCommand(updateRoom, conn))
                    {
                        cmdRm.Parameters.AddWithValue("@SoPhong", soPhong);
                        cmdRm.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }

        public void DatDichVu(string soPhong, int maDV, int soLuong)
        {
            using (MySqlConnection conn = dp.GetConnection())
            {
                conn.Open();
                string getPhieu = "SELECT MaPhieu FROM PhieuDatPhong WHERE SoPhong = @Phong AND NgayTraPhong IS NULL";
                MySqlCommand cmd = new MySqlCommand(getPhieu, conn);
                cmd.Parameters.AddWithValue("@Phong", soPhong);
                object res = cmd.ExecuteScalar();
                if (res == null) return;
                int maPhieu = Convert.ToInt32(res);

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
            }
        }

        public bool CheckOut(string soPhong, out decimal tongTienThanhToan, out double hours, out decimal tienPhong, out decimal tienDV, out string err)
        {
            tongTienThanhToan = 0; hours = 0; tienPhong = 0; tienDV = 0; err = "";
            using (MySqlConnection conn = dp.GetConnection())
            {
                conn.Open();
                string getPhieu = "SELECT MaPhieu, NgayNhanPhong FROM PhieuDatPhong WHERE SoPhong = @Phong AND NgayTraPhong IS NULL";
                MySqlCommand cmd = new MySqlCommand(getPhieu, conn);
                cmd.Parameters.AddWithValue("@Phong", soPhong);
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
                        err = "Phòng không có khách!";
                        return false;
                    }
                }

                string getTienDV = "SELECT SUM(s.SoLuong * dv.DonGia) FROM SuDungDichVu s JOIN DichVu dv ON s.MaDV = dv.MaDV WHERE s.MaPhieu = @MaPhieu";
                MySqlCommand cmdDV = new MySqlCommand(getTienDV, conn);
                cmdDV.Parameters.AddWithValue("@MaPhieu", maPhieu);
                object resDV = cmdDV.ExecuteScalar();
                tienDV = resDV != DBNull.Value ? Convert.ToDecimal(resDV) : 0;

                string getGia = "SELECT GiaTheoGio FROM Phong WHERE SoPhong = @Phong";
                MySqlCommand cmdGia = new MySqlCommand(getGia, conn);
                cmdGia.Parameters.AddWithValue("@Phong", soPhong);
                decimal giaTheoGio = Convert.ToDecimal(cmdGia.ExecuteScalar());

                TimeSpan timeStay = DateTime.Now - ngayNhan;
                hours = Math.Max(timeStay.TotalHours, 1.0);
                tienPhong = (decimal)hours * giaTheoGio;
                tongTienThanhToan = tienPhong + tienDV;

                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updatePhieu = "UPDATE PhieuDatPhong SET NgayTraPhong = NOW(), TongTienThanhToan = @TongTien WHERE MaPhieu = @MaPhieu";
                        using (MySqlCommand cmdUp = new MySqlCommand(updatePhieu, conn, transaction))
                        {
                            cmdUp.Parameters.AddWithValue("@TongTien", tongTienThanhToan);
                            cmdUp.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            cmdUp.ExecuteNonQuery();
                        }

                        string updateRoom = "UPDATE Phong SET TrangThai = 'Đang dọn dẹp' WHERE SoPhong = @SoPhong";
                        using (MySqlCommand cmdRm = new MySqlCommand(updateRoom, conn, transaction))
                        {
                            cmdRm.Parameters.AddWithValue("@SoPhong", soPhong);
                            cmdRm.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        err = ex.Message;
                        return false;
                    }
                }
            }
        }
    }
}

using System;
using System.Data;
using MySql.Data.MySqlClient;
using CafePOSSystem.DAL;

namespace CafePOSSystem.BUS
{
    public class CafeBUS
    {
        private CafeDAL cafeDAL = new CafeDAL();
        private DataProvider dp = new DataProvider();

        public DataTable GetBanAnList() => cafeDAL.GetBanAnList();
        public DataTable GetDanhMucList() => cafeDAL.GetDanhMucList();
        public DataTable GetMonAnByDanhMuc(int maDM) => cafeDAL.GetMonAnByDanhMuc(maDM);
        public DataTable GetChiTietHoaDon(int maBan) => cafeDAL.GetChiTietHoaDon(maBan);

        public void ThemMon(int maBan, int maMon, int soLuong)
        {
            using (MySqlConnection conn = dp.GetConnection())
            {
                conn.Open();
                string getBill = "SELECT MaHoaDon FROM HoaDon WHERE MaBan = @MaBan AND TrangThai = 0";
                MySqlCommand cmdBill = new MySqlCommand(getBill, conn);
                cmdBill.Parameters.AddWithValue("@MaBan", maBan);
                object result = cmdBill.ExecuteScalar();

                int maHoaDon = 0;
                if (result == null)
                {
                    string insertBill = "INSERT INTO HoaDon (MaBan, GioVao, TrangThai) VALUES (@MaBan, NOW(), 0); SELECT LAST_INSERT_ID();";
                    MySqlCommand cmdInsertBill = new MySqlCommand(insertBill, conn);
                    cmdInsertBill.Parameters.AddWithValue("@MaBan", maBan);
                    maHoaDon = Convert.ToInt32(cmdInsertBill.ExecuteScalar());

                    string updateTable = "UPDATE BanAn SET TrangThai = 'Có khách' WHERE MaBan = @MaBan";
                    MySqlCommand cmdUpdateTable = new MySqlCommand(updateTable, conn);
                    cmdUpdateTable.Parameters.AddWithValue("@MaBan", maBan);
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
        }

        public bool ThanhToan(int maBan, out decimal tongTien, out string err)
        {
            tongTien = 0;
            err = "";
            using (MySqlConnection conn = dp.GetConnection())
            {
                conn.Open();
                string getQuery = "SELECT MaHoaDon FROM HoaDon WHERE MaBan = @MaBan AND TrangThai = 0";
                MySqlCommand cmdBill = new MySqlCommand(getQuery, conn);
                cmdBill.Parameters.AddWithValue("@MaBan", maBan);
                object result = cmdBill.ExecuteScalar();

                if (result == null)
                {
                    err = "Bàn này không có hóa đơn nào!";
                    return false;
                }

                int maHoaDon = Convert.ToInt32(result);

                string getSum = "SELECT SUM(SoLuong * DonGia) FROM ChiTietHoaDon WHERE MaHoaDon = @MaHD";
                MySqlCommand cmdSum = new MySqlCommand(getSum, conn);
                cmdSum.Parameters.AddWithValue("@MaHD", maHoaDon);
                object sumResult = cmdSum.ExecuteScalar();
                tongTien = sumResult != DBNull.Value ? Convert.ToDecimal(sumResult) : 0;

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
                            cmdTb.Parameters.AddWithValue("@MaBan", maBan);
                            cmdTb.ExecuteNonQuery();
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

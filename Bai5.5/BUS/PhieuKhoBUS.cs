using System;
using System.Data;
using MySql.Data.MySqlClient;
using InventoryManagementSystem.DAL;

namespace InventoryManagementSystem.BUS
{
    public class PhieuKhoBUS
    {
        private DataProvider dp = new DataProvider();

        public bool LuuPhieuKho(string loaiPhieu, int? maNCC, decimal tongTien, DataTable dtChiTiet, out string errorMessage)
        {
            errorMessage = "";
            using (MySqlConnection conn = dp.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string insertPhieu = "INSERT INTO PhieuKho (LoaiPhieu, NgayLap, MaNCC, TongTien) VALUES (@Loai, NOW(), @NCC, @TongTien); SELECT LAST_INSERT_ID();";
                        int maPhieu = 0;
                        using (MySqlCommand cmdPhieu = new MySqlCommand(insertPhieu, conn, transaction))
                        {
                            cmdPhieu.Parameters.AddWithValue("@Loai", loaiPhieu);
                            cmdPhieu.Parameters.AddWithValue("@NCC", (object)maNCC ?? DBNull.Value);
                            cmdPhieu.Parameters.AddWithValue("@TongTien", tongTien);
                            maPhieu = Convert.ToInt32(cmdPhieu.ExecuteScalar());
                        }

                        foreach (DataRow row in dtChiTiet.Rows)
                        {
                            string maVT = row["MaVatTu"].ToString();
                            decimal soLuong = Convert.ToDecimal(row["SoLuong"]);
                            decimal donGia = Convert.ToDecimal(row["DonGia"]);

                            string insertCT = "INSERT INTO ChiTietPhieuKho (MaPhieu, MaVatTu, SoLuong, DonGia) VALUES (@MaPhieu, @MaVT, @SL, @DG)";
                            using (MySqlCommand cmdCT = new MySqlCommand(insertCT, conn, transaction))
                            {
                                cmdCT.Parameters.AddWithValue("@MaPhieu", maPhieu);
                                cmdCT.Parameters.AddWithValue("@MaVT", maVT);
                                cmdCT.Parameters.AddWithValue("@SL", soLuong);
                                cmdCT.Parameters.AddWithValue("@DG", donGia);
                                cmdCT.ExecuteNonQuery();
                            }

                            if (loaiPhieu == "XUAT")
                            {
                                string checkTon = "SELECT SoLuongTon FROM VatTu WHERE MaVatTu = @MaVT";
                                decimal tonHienTai = 0;
                                using (MySqlCommand cmdTon = new MySqlCommand(checkTon, conn, transaction))
                                {
                                    cmdTon.Parameters.AddWithValue("@MaVT", maVT);
                                    tonHienTai = Convert.ToDecimal(cmdTon.ExecuteScalar());
                                }

                                if (tonHienTai < soLuong)
                                {
                                    transaction.Rollback();
                                    errorMessage = $"Vật tư [{maVT}] chỉ còn tồn ({tonHienTai}), không đủ số lượng xuất ({soLuong})!";
                                    return false;
                                }
                            }

                            string updateTon = loaiPhieu == "NHAP" 
                                ? "UPDATE VatTu SET SoLuongTon = SoLuongTon + @SL WHERE MaVatTu = @MaVT" 
                                : "UPDATE VatTu SET SoLuongTon = SoLuongTon - @SL WHERE MaVatTu = @MaVT";

                            using (MySqlCommand cmdUp = new MySqlCommand(updateTon, conn, transaction))
                            {
                                cmdUp.Parameters.AddWithValue("@SL", soLuong);
                                cmdUp.Parameters.AddWithValue("@MaVT", maVT);
                                cmdUp.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        errorMessage = ex.Message;
                        return false;
                    }
                }
            }
        }
    }
}

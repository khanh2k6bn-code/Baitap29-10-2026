using System;
using System.Data;
using MySql.Data.MySqlClient;
using InventoryManagementSystem.DTO;

namespace InventoryManagementSystem.DAL
{
    public class VatTuDAL
    {
        private DataProvider dp = new DataProvider();

        public DataTable GetAllVatTu()
        {
            string query = "SELECT MaVatTu AS 'Mã VT', TenVatTu AS 'Tên vật tư', DVT AS 'Đơn vị tính', SoLuongTon AS 'Số lượng tồn' FROM VatTu";
            return dp.ExecuteQuery(query);
        }

        public DataTable SearchVatTu(string keyword)
        {
            string query = "SELECT MaVatTu AS 'Mã VT', TenVatTu AS 'Tên vật tư', DVT AS 'Đơn vị tính', SoLuongTon AS 'Số lượng tồn' FROM VatTu WHERE MaVatTu LIKE @kw OR TenVatTu LIKE @kw";
            MySqlParameter[] parameters = { new MySqlParameter("@kw", "%" + keyword + "%") };
            return dp.ExecuteQuery(query, parameters);
        }

        public bool InsertVatTu(VatTuDTO vt)
        {
            string query = "INSERT INTO VatTu (MaVatTu, TenVatTu, DVT, SoLuongTon) VALUES (@Ma, @Ten, @Dvt, @SL)";
            MySqlParameter[] parameters = {
                new MySqlParameter("@Ma", vt.MaVatTu),
                new MySqlParameter("@Ten", vt.TenVatTu),
                new MySqlParameter("@Dvt", vt.DVT),
                new MySqlParameter("@SL", vt.SoLuongTon)
            };
            return dp.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateVatTu(VatTuDTO vt)
        {
            string query = "UPDATE VatTu SET TenVatTu = @Ten, DVT = @Dvt, SoLuongTon = @SL WHERE MaVatTu = @Ma";
            MySqlParameter[] parameters = {
                new MySqlParameter("@Ten", vt.TenVatTu),
                new MySqlParameter("@Dvt", vt.DVT),
                new MySqlParameter("@SL", vt.SoLuongTon),
                new MySqlParameter("@Ma", vt.MaVatTu)
            };
            return dp.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteVatTu(string maVatTu)
        {
            string query = "DELETE FROM VatTu WHERE MaVatTu = @Ma";
            MySqlParameter[] parameters = { new MySqlParameter("@Ma", maVatTu) };
            return dp.ExecuteNonQuery(query, parameters) > 0;
        }

        public decimal GetTotalInventoryValue()
        {
            string query = "SELECT SUM(SoLuongTon * 100000) FROM VatTu;";
            object res = dp.ExecuteScalar(query);
            return res != DBNull.Value && res != null ? Convert.ToDecimal(res) : 0;
        }
    }
}

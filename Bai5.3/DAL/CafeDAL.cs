using System.Data;
using MySql.Data.MySqlClient;

namespace CafePOSSystem.DAL
{
    public class CafeDAL
    {
        private DataProvider dp = new DataProvider();

        public DataTable GetBanAnList() => dp.ExecuteQuery("SELECT * FROM BanAn");
        public DataTable GetDanhMucList() => dp.ExecuteQuery("SELECT * FROM DanhMuc");
        
        public DataTable GetMonAnByDanhMuc(int maDM)
        {
            string query = "SELECT * FROM MonAn WHERE MaDanhMuc = @MaDM";
            return dp.ExecuteQuery(query, new MySqlParameter[] { new MySqlParameter("@MaDM", maDM) });
        }

        public DataTable GetChiTietHoaDon(int maBan)
        {
            string query = @"
                SELECT m.TenMon AS 'Tên món', c.SoLuong AS 'Số lượng', c.DonGia AS 'Đơn giá', (c.SoLuong * c.DonGia) AS 'Thành tiền'
                FROM HoaDon h
                JOIN ChiTietHoaDon c ON h.MaHoaDon = c.MaHoaDon
                JOIN MonAn m ON c.MaMon = m.MaMon
                WHERE h.MaBan = @MaBan AND h.TrangThai = 0;
            ";
            return dp.ExecuteQuery(query, new MySqlParameter[] { new MySqlParameter("@MaBan", maBan) });
        }
    }
}

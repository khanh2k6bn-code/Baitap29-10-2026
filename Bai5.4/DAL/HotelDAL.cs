using System.Data;
using MySql.Data.MySqlClient;

namespace HotelManagementSystem.DAL
{
    public class HotelDAL
    {
        private DataProvider dp = new DataProvider();

        public DataTable GetPhongList() => dp.ExecuteQuery("SELECT * FROM Phong");
        public DataTable GetDichVuList() => dp.ExecuteQuery("SELECT * FROM DichVu");

        public DataTable GetSuDungDichVuByPhong(string soPhong)
        {
            string query = @"
                SELECT dv.TenDV AS 'Tên dịch vụ', s.SoLuong AS 'Số lượng', dv.DonGia AS 'Đơn giá', (s.SoLuong * dv.DonGia) AS 'Thành tiền'
                FROM PhieuDatPhong p
                JOIN SuDungDichVu s ON p.MaPhieu = s.MaPhieu
                JOIN DichVu dv ON s.MaDV = dv.MaDV
                WHERE p.SoPhong = @Phong AND p.NgayTraPhong IS NULL;
            ";
            return dp.ExecuteQuery(query, new MySqlParameter[] { new MySqlParameter("@Phong", soPhong) });
        }
    }
}

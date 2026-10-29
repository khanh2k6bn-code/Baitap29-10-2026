using System.Data;

namespace InventoryManagementSystem.DAL
{
    public class NhaCungCapDAL
    {
        private DataProvider dp = new DataProvider();
        public DataTable GetAllNhaCungCap() => dp.ExecuteQuery("SELECT * FROM NhaCungCap");
    }
}

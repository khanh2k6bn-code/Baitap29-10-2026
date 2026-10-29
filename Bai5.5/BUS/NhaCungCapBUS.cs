using System.Data;
using InventoryManagementSystem.DAL;

namespace InventoryManagementSystem.BUS
{
    public class NhaCungCapBUS
    {
        private NhaCungCapDAL nccDAL = new NhaCungCapDAL();
        public DataTable GetAllNhaCungCap() => nccDAL.GetAllNhaCungCap();
    }
}

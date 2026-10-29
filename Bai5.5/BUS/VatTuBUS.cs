using System.Data;
using InventoryManagementSystem.DAL;
using InventoryManagementSystem.DTO;

namespace InventoryManagementSystem.BUS
{
    public class VatTuBUS
    {
        private VatTuDAL vtDAL = new VatTuDAL();

        public DataTable GetAllVatTu() => vtDAL.GetAllVatTu();
        public DataTable SearchVatTu(string keyword) => vtDAL.SearchVatTu(keyword);
        public bool InsertVatTu(VatTuDTO vt) => vtDAL.InsertVatTu(vt);
        public bool UpdateVatTu(VatTuDTO vt) => vtDAL.UpdateVatTu(vt);
        public bool DeleteVatTu(string maVT) => vtDAL.DeleteVatTu(maVT);
        public decimal GetTotalInventoryValue() => vtDAL.GetTotalInventoryValue();
    }
}


namespace CNDM.Models
{
    public class ThemSanPham
    {
       
        public List<SanPham> sanPhams { get; set; }
        public class SanPham
        {
            public string IdSanPham { get; set; }
        }
        
    }
   
}

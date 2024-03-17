namespace CNDM.Models
{
    public class Cay
    {
        public string Id { get; set; }
        public string IdSanPham {  get; set; }
        public int TanSuat { get; set; }
        public List<string > Children { get; set; }
    }
}

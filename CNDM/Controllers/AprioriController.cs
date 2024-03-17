using CNDM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace CNDM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AprioriController : ControllerBase
    {
        private readonly DbDataMiningContext _dbContext;
        public AprioriController(DbDataMiningContext context)
        {
            _dbContext = context;

        }
        [HttpGet("duyet-hoa-don/{smin}")]
        public async Task<IActionResult> DuyetHoaDon(double smin)
        {
            ResponseInfo response = new ResponseInfo();
            try
            {
                if (smin > 1 || smin < 0)
                {
                    return Ok("Vui long nhap tai support_min");
                }
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var hoadons = _dbContext.HoaDons.ToList();
                List<string> dataList = new List<string>();
                foreach (var hoadon in hoadons)
                {
                    string sanPhamJson = hoadon.SanPham;
                    JArray sanPhamArray = JArray.Parse(sanPhamJson);
                    string[] tenSanPhamArray = new string[sanPhamArray.Count];
                    int index = 0;
                    foreach (JObject sanPhamObject in sanPhamArray)
                    {
                        string tenSanPham = sanPhamObject["TenSanPham"].ToString();
                        tenSanPhamArray[index] = tenSanPham;
                        index++;
                    }
                    dataList.Add(string.Join(", ", tenSanPhamArray));
                }

                Apriori apriori_test = new Apriori(smin, 0.8, dataList);
                //apriori_test.ShowOriginData();
                //apriori_test.ShowIteration();
                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;

                response.statusCode = System.Net.HttpStatusCode.OK;
                response.message = elapsed.TotalMilliseconds.ToString();
                response.data = apriori_test.ShowIteration();

                return Ok(response);
            }
            catch(Exception e)
            {
                response.message = e.ToString();
                response.statusCode = System.Net.HttpStatusCode.BadRequest;
                return BadRequest(response);
            }

        }
        [HttpPost("them-hoa-don")]
        public IActionResult ThemHoaDon(ThemSanPham input)
        {
            string id = Guid.NewGuid().ToString();
            HoaDon hoaDon = new HoaDon();
            hoaDon.IdHoaDon = id;
            List<SanPham> sanPhams = new List<SanPham>();
            foreach (var item in input.sanPhams)
            {
                SanPham sp = new SanPham();
                sp.IdSanPham = item.IdSanPham;
                sp.TenSanPham = _dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == item.IdSanPham).TenSanPham;
                sanPhams.Add(sp);
            }
            hoaDon.SanPham = System.Text.Json.JsonSerializer.Serialize(sanPhams);
            _dbContext.HoaDons.Add(hoaDon);
            _dbContext.SaveChanges();
            /*XuLyHoaDon(hoaDon);
            var items = KiemTraToHop(hoaDon);
            if (items.Count == 0)
            {
                return Ok("Khong co to hop moi sinh ra");
            }*/
            return Ok();
        }

    }

}
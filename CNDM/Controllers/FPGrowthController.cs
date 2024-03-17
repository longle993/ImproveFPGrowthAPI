using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using CNDM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CNDM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FPGrowthController : ControllerBase
    {
        private readonly DbDataMiningContext _dbContext;
        public FPGrowthController(DbDataMiningContext context)
        {
            _dbContext = context;
        }
        [HttpGet("nam-moi")]
        public IActionResult NamMoi()
        {
            return Ok("10đ tất cả các môn!");
        }
        [HttpGet("duyet-hoa-don/{smin}")]
        public async Task<IActionResult> DuyetHoaDon(double smin)
        {
            ResponseInfo response = new ResponseInfo();
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var hoadons = _dbContext.HoaDons.ToList();
                double sminhd = smin * hoadons.Count();
                var ts1sps = new List<TanSuatMotSanPham>();
                foreach (var hoadon in hoadons)
                {
                    var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                    foreach (var sp in dssp)
                    {
                        int i = 0;
                        foreach (var ts in ts1sps)
                        {
                            if (ts.IdSanPham == sp.IdSanPham)
                            {
                                ts.TanSuat++;
                                i++;
                            }
                        }
                        if (i == 0)
                        {
                            var ts1sp = new TanSuatMotSanPham();
                            ts1sp.IdSanPham = sp.IdSanPham;
                            ts1sp.TanSuat = 1;
                            ts1sps.Add(ts1sp);
                        }
                    }
                }
                var Ts1sps = ts1sps.OrderByDescending(ts => ts.TanSuat).ToList();
                for (int i = ts1sps.Count - 1; i >= 0; i--)
                {
                    if (Ts1sps[i].TanSuat < sminhd)
                    {
                        Ts1sps.Remove(Ts1sps[i]);
                    }
                    else
                    {
                        break;
                    }
                }
                var cay = new FP_Tree();
                foreach (var hoadon in hoadons)
                {
                    var sortedTs1sps = Ts1sps.ToList();
                    var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                    for (int k = sortedTs1sps.Count - 1; k >= 0; k--)
                    {
                        int i = 0;
                        foreach (var sp in dssp)
                        {
                            if (sp.IdSanPham == sortedTs1sps[k].IdSanPham)
                            {
                                i++;
                                dssp.Remove(sp);
                                break;
                            }
                        }
                        if (dssp.Count == 0)
                        {
                            for (int j = k - 1; j >= 0; j--)
                            {
                                sortedTs1sps.RemoveAt(j);
                            }
                            break;
                        }
                        if (i == 0)
                        {
                            sortedTs1sps.Remove(sortedTs1sps[k]);
                        }

                    }

                    TaoCay(cay, sortedTs1sps);
                }



                var ketqua = new List<List<List<SanPham>>>();
                foreach (var item in Ts1sps)
                {
                    var listtong = new List<List<TanSuatMotSanPham>>();
                    var listphu = new List<TanSuatMotSanPham>();
                    DuyetCay(cay, listphu, listtong, item);
                    listphu = new List<TanSuatMotSanPham>();
                    foreach (var os in listtong)
                    {
                        foreach (var t in os)
                        {
                            int k = 0;
                            foreach (var t2 in listphu)
                            {
                                if (t.IdSanPham == t2.IdSanPham)
                                {
                                    t2.TanSuat = t2.TanSuat + t.TanSuat;
                                    k = 1;
                                    break;
                                }
                            }
                            if (k == 0)
                            {
                                listphu.Add(t);
                            }
                        }
                    }
                    for (int i = listphu.Count - 1; i >= 0; i--)
                    {
                        if (listphu[i].TanSuat < sminhd)
                        {
                            listphu.Remove(listphu[i]);
                        }
                    }
                    List<SanPham> listsp = new List<SanPham>();
                    foreach (var s in listphu)
                    {
                        var sp = new SanPham();
                        sp.IdSanPham = s.IdSanPham;
                        sp.TenSanPham = _dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == s.IdSanPham).TenSanPham;
                        listsp.Add(sp);
                    }
                    for (int i = 1; i <= listsp.Count; i++)
                    {
                        var items = await LayTatCaTongHop(i, listsp);
                        foreach (var ite in items)
                        {
                            ite.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == item.IdSanPham));
                        }

                        ketqua.Add(items);
                    }
                    List<SanPham> list1sp = new List<SanPham>();
                    list1sp.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == item.IdSanPham));
                    List<List<SanPham>> list1sp2 = new List<List<SanPham>>();
                    list1sp2.Add(list1sp);
                    ketqua.Add(list1sp2);
                }

                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                //return Ok(elapsed.TotalMilliseconds);

                

                List<SanPham> spFinal = new List<SanPham>();
                ketqua.ForEach(item =>
                {
                    item.ForEach(item2 =>
                    {
                        item2.ForEach(item3 =>
                        {
                            spFinal.Add(item3);
                        });
                    });
                });

                response.statusCode = System.Net.HttpStatusCode.OK;
                response.data = spFinal;
                response.message = elapsed.TotalMilliseconds.ToString();
                return Ok(response);
            }
            catch(Exception e)
            {
                response.statusCode = System.Net.HttpStatusCode.BadRequest;
                response.message = e.ToString();
                return BadRequest(response);
            }
            
        }

        #region algorithm
        private void DuyetCay(FP_Tree cays, List<TanSuatMotSanPham> list, List<List<TanSuatMotSanPham>> listtong, TanSuatMotSanPham ts1sp)
        {

            foreach (var cay in cays.FPTree)
            {
                if (cay.IdSanPham == ts1sp.IdSanPham)
                {
                    var clonedList = new List<TanSuatMotSanPham>();
                    foreach (var item in list)
                    {
                        var lp = new TanSuatMotSanPham();
                        lp.IdSanPham = item.IdSanPham.ToString();
                        lp.TanSuat = cay.TanSuat.Value;
                        clonedList.Add(lp);
                    }
                    listtong.Add(clonedList.ToList());
                }
                else
                {
                    if (cay.FPTree.Count != 0)
                    {
                        var ts = new TanSuatMotSanPham();
                        ts.IdSanPham = cay.IdSanPham;
                        ts.TanSuat = cay.TanSuat;
                        list.Add(ts);
                        DuyetCay(cay, list, listtong, ts1sp);
                        list.Remove(ts);
                    }
                }

            }
        }

        private void TaoCay(FP_Tree cays, List<TanSuatMotSanPham> ts1sps)
        {
            if (ts1sps.Count > 0)
            {
                bool found = false;
                if (cays.FPTree != null)
                {
                    foreach (var cay in cays.FPTree.ToList())
                    {
                        if (cay.IdSanPham == ts1sps[0].IdSanPham)
                        {
                            cay.TanSuat++;
                            found = true;
                            ts1sps.RemoveAt(0);
                            TaoCay(cay, ts1sps);
                            break;
                        }
                    }
                }

                if (!found)
                {
                    var cay = new FP_Tree();
                    cay.IdSanPham = ts1sps[0].IdSanPham;
                    cay.TanSuat = 1;
                    cay.FPTree = new List<FP_Tree>();
                    if (cays.FPTree == null)
                    {
                        cays.FPTree = new List<FP_Tree>();
                    }
                    cays.FPTree.Add(cay);
                    ts1sps.RemoveAt(0);
                    TaoCay(cay, ts1sps);
                }
            }
        }
        private async Task<List<List<SanPham>>> LayTatCaTongHop(int k, List<SanPham> sanphams)
        {
            List<List<SanPham>> tatCaTongHop = new List<List<SanPham>>();
            List<SanPham> tongHopHienTai = new List<SanPham>();

            await LayTatCaTongHopHelper(k, sanphams, 0, tongHopHienTai, tatCaTongHop);

            return tatCaTongHop;
        }

        private async Task LayTatCaTongHopHelper(int k, List<SanPham> sanphams, int index, List<SanPham> tongHopHienTai, List<List<SanPham>> tatCaTongHop)
        {
            if (tongHopHienTai.Count == k)
            {
                tatCaTongHop.Add(new List<SanPham>(tongHopHienTai));
                return;
            }

            for (int i = index; i < sanphams.Count; i++)
            {
                tongHopHienTai.Add(sanphams[i]);
                await LayTatCaTongHopHelper(k, sanphams, i + 1, tongHopHienTai, tatCaTongHop);
                tongHopHienTai.RemoveAt(tongHopHienTai.Count - 1);
            }
        }
        #endregion
    }
}

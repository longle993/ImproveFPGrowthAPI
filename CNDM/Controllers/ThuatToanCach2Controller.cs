using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CNDM.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CNDM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThuatToanCach2Controller : ControllerBase
    {
        private readonly DbDataMiningContext _dbContext;
        public ThuatToanCach2Controller(DbDataMiningContext context)
        {
            _dbContext = context;
        }

        [HttpGet("duyet-hoa-don-cach-2")]
        public IActionResult DuyetHoaDonCach2()
        {
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            _dbContext.RemoveRange(ts1sp);
            _dbContext.RemoveRange(ts2sp);
            _dbContext.SaveChanges();

            ResponseInfo response = new ResponseInfo();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            XuLyHoaDonCach2();
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            response.statusCode = System.Net.HttpStatusCode.OK;
            response.message = elapsed.TotalMilliseconds.ToString();
            return Ok(response) ;

        }
        [HttpGet("duyet-hoa-don-khong-luu-database/{s_min}")]
        public async Task<IActionResult> DuyetHoaDonKhongLuu(double s_min)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var hoadons = _dbContext.HoaDons.ToList();
            var ldssp = _dbContext.SanPhams.ToList();
            var ts1sp = new List<TanSuatMotSanPham>();
            var ts2sp = new List<TanSuatHaiSanPham>();
            var sohoadon = _dbContext.HoaDons.Count();
            double k = s_min * sohoadon;
            for (int i = 0; i < ldssp.Count; i++)
            {
                var ts1 = new TanSuatMotSanPham();
                ts1.ThuTu = i + 1;
                ts1.TanSuat = 0;
                ts1.IdSanPham = ldssp[i].IdSanPham;
                ts1sp.Add(ts1);
            }
            foreach (var hoadon in hoadons)
            {
                var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                foreach (var sp in dssp)
                {
                    for (int i = 0; i < ts1sp.Count; i++)
                    {
                        if (ts1sp[i].IdSanPham == sp.IdSanPham)
                        {
                            ts1sp[i].TanSuat++;
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < ts1sp.Count - 1; i++)
            {
                for (int j = 0; j < ts1sp.Count - i - 1; j++)
                {
                    if (ts1sp[j].TanSuat < ts1sp[j + 1].TanSuat)
                    {
                        var ts1 = new TanSuatMotSanPham
                        {
                            IdSanPham = ts1sp[j + 1].IdSanPham,
                            TanSuat = ts1sp[j + 1].TanSuat,

                        };

                        ts1sp[j + 1].IdSanPham = ts1sp[j].IdSanPham;
                        ts1sp[j + 1].TanSuat = ts1sp[j].TanSuat;

                        ts1sp[j].IdSanPham = ts1.IdSanPham;
                        ts1sp[j].TanSuat = ts1.TanSuat;
                    }
                }
            }
            for (int i = ts1sp.Count - 1; i >= 0; i--)
            {
                if (ts1sp[i].TanSuat < k)
                {
                    ts1sp.Remove(ts1sp[i]);
                }
                else
                {
                    break;
                }
            }
            for (int i = 0; i < ts1sp.Count; i++)
            {
                var ts2 = new TanSuatHaiSanPham();
                ts2.ThuTu = ts1sp[i].ThuTu;
                ts2.IdHaiSanPham = ts1sp[i].IdSanPham;
                var listtong = new List<TanSuatMotSanPham>();
                for (int j = 0; j < i; j++)
                {
                    var listphu = new TanSuatMotSanPham();
                    listphu.ThuTu = j;
                    listphu.IdSanPham = ts1sp[j].IdSanPham;
                    listphu.TanSuat = 0;
                    listtong.Add(listphu);
                }
                ts2.TanSuat = System.Text.Json.JsonSerializer.Serialize(listtong);
                ts2sp.Add(ts2);
            }
            List<List<TanSuatMotSanPham>> listts2sp = new List<List<TanSuatMotSanPham>>();
            foreach (var ts in ts2sp)
            {
                List<TanSuatMotSanPham> list = new List<TanSuatMotSanPham>();
                list = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts.TanSuat);
                listts2sp.Add(list);
            }
            foreach (var hoadon in hoadons)
            {
                var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);

                for (int i = ts2sp.Count - 1; i >= 0; i--)
                {
                    foreach (var sp in dssp)
                    {
                        if (sp.IdSanPham == ts2sp[i].IdHaiSanPham)
                        {
                            dssp.Remove(sp);
                            foreach (var spp in dssp)
                            {
                                foreach (var dshsp in listts2sp[i])
                                {
                                    if (spp.IdSanPham == dshsp.IdSanPham)
                                    {
                                        dshsp.TanSuat++;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            for (int i = ts1sp.Count - 1; i >= 0; i--)
            {

                if (ts1sp[i].TanSuat < k)
                {
                    ts2sp.Remove(ts2sp[i]);
                }
                else break;
            }
            List<List<List<SanPham>>> listtong2 = new List<List<List<SanPham>>>();
            for (int j = 0; j < ts2sp.Count; j++)
            {
                //var dstss = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts.TanSuat);
                List<SanPham> listsp = new List<SanPham>();
                for (int i = listts2sp[j].Count - 1; i >= 0; i--)
                {
                    if (listts2sp[j][i].TanSuat > k)
                    {
                        listsp.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == listts2sp[j][i].IdSanPham));
                    }
                }
                for (int i = 1; i <= listsp.Count; i++)
                {
                    var items = await LayTatCaTongHop(i, listsp);
                    foreach (var item in items)
                    {
                        item.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == ts2sp[j].IdHaiSanPham));
                    }

                    listtong2.Add(items);
                }
                List<SanPham> list1sp = new List<SanPham>();
                list1sp.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == ts2sp[j].IdHaiSanPham));
                List<List<SanPham>> list1sp2 = new List<List<SanPham>>();
                list1sp2.Add(list1sp);
                listtong2.Add(list1sp2);
            }
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            return Ok(elapsed.TotalMilliseconds);
            //return Ok(listtong2);
        }
        //private void TangTanSuatHoaDon()
        //{
        //    var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
        //    var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
        //    var hoadons = _dbContext.HoaDons.ToList();
        //    List<List<TanSuatMotSanPham>> listts2sp = new List<List<TanSuatMotSanPham>>();
        //    foreach (var ts in ts2sp)
        //    {
        //        List<TanSuatMotSanPham> list = new List<TanSuatMotSanPham>();
        //        list = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts.TanSuat);
        //        listts2sp.Add(list);
        //    }
        //    foreach (var hoadon in hoadons)
        //    {
        //        var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
        //        int ssp = 0;
        //        int ssphd = dssp.Count;
        //        foreach (var sp in dssp)
        //        {
        //            for (int i = 0; i < ts1sp.Count; i++)
        //            {
        //                if (ts1sp[i].IdSanPham == sp.IdSanPham)
        //                {
        //                    ts1sp[i].TanSuat++;
        //                    break;
        //                }
        //            }
        //        }

        //        for (int i = ts2sp.Count - 1; i >= 0; i--)
        //        {
        //            foreach (var sp in dssp)
        //            {
        //                if (sp.IdSanPham == ts2sp[i].IdHaiSanPham)
        //                {
        //                    dssp.Remove(sp);
        //                    foreach (var spp in dssp)
        //                    {
        //                        for (int j = 0; j < listts2sp[i].Count; j++)
        //                        {
        //                            if (spp.IdSanPham == listts2sp[i][j].IdSanPham)
        //                            {
        //                                listts2sp[i][j].TanSuat++;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    ssp++;
        //                    break;
        //                }
        //            }
        //            if (ssp == ssphd)
        //            {
        //                break;
        //            }

        //        }
        //    }
        //    for (int i = 0; i < ts1sp.Count - 1; i++)
        //    {
        //        for (int j = 0; j < ts1sp.Count - i - 1; j++)
        //        {
        //            if (ts1sp[j].TanSuat < ts1sp[j + 1].TanSuat)
        //            {
        //                var doicho = new TanSuatHaiSanPham
        //                {
        //                    IdHaiSanPham = ts2sp[j].IdHaiSanPham,

        //                };
        //                ts2sp[j].IdHaiSanPham = ts2sp[j + 1].IdHaiSanPham;
        //                ts2sp[j + 1].IdHaiSanPham = doicho.IdHaiSanPham;
        //                var doicho2 = new TanSuatMotSanPham
        //                {
        //                    IdSanPham = ts2sp[j + 1].IdHaiSanPham,
        //                    TanSuat = listts2sp[j + 1].FirstOrDefault(c => c.IdSanPham == ts2sp[j + 1].IdHaiSanPham).TanSuat,
        //                    ThuTu = listts2sp[j + 1].FirstOrDefault(c => c.IdSanPham == ts2sp[j + 1].IdHaiSanPham).ThuTu,
        //                };
        //                var listphu = listts2sp[j].ToList();
        //                listphu.Add(doicho2);
        //                listts2sp[j + 1].RemoveAll(c => c.IdSanPham == ts2sp[j + 1].IdHaiSanPham);
        //                listts2sp[j] = listts2sp[j + 1];
        //                listts2sp[j + 1] = listphu.ToList();
        //                var ts1 = new TanSuatMotSanPham
        //                {
        //                    IdSanPham = ts1sp[j + 1].IdSanPham,
        //                    TanSuat = ts1sp[j + 1].TanSuat,

        //                };

        //                ts1sp[j + 1].IdSanPham = ts1sp[j].IdSanPham;
        //                ts1sp[j + 1].TanSuat = ts1sp[j].TanSuat;

        //                ts1sp[j].IdSanPham = ts1.IdSanPham;
        //                ts1sp[j].TanSuat = ts1.TanSuat;
        //            }
        //        }

        //    }
        //    for (int i = 0; i < ts1sp.Count; i++)
        //    {
        //        ts2sp[i].TanSuat = System.Text.Json.JsonSerializer.Serialize(listts2sp[i]);
        //        _dbContext.TanSuatHaiSanPhams.Update(ts2sp[i]);
        //        _dbContext.TanSuatMotSanPhams.Update(ts1sp[i]);

        //    }
        //    _dbContext.SaveChanges();
        //}
        private async Task<List<List<List<SanPham>>>> TangTanSuatMotHoaDon(HoaDon hoadon, double sminhd)
        {
            List<List<List<SanPham>>> listtong = new List<List<List<SanPham>>>();
            List<SanPham> spcanchuy = new List<SanPham>();
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
            foreach (var sp in dssp)
            {
                for (int i = 0; i < ts1sp.Count; i++)
                {
                    if (ts1sp[i].IdSanPham == sp.IdSanPham)
                    {
                        ts1sp[i].TanSuat++;
                        _dbContext.TanSuatMotSanPhams.Update(ts1sp[i]);

                        break;
                    }
                }
            }
            for (int i = ts2sp.Count - 1; i >= 0; i--)
            {
                int k = 0;
                foreach (var sp in dssp)
                {
                    if (sp.IdSanPham == ts2sp[i].IdHaiSanPham)
                    {
                        var dshsps = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts2sp[i].TanSuat);
                        dssp.Remove(sp);
                        foreach (var spp in dssp)
                        {
                            foreach (var dshsp in dshsps)
                            {
                                if (spp.IdSanPham == dshsp.IdSanPham)
                                {
                                    dshsp.TanSuat++;
                                    if (dshsp.TanSuat > sminhd)
                                    {
                                        spcanchuy.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == ts2sp[i].IdHaiSanPham));
                                    }
                                    break;
                                }
                            }
                        }
                        ts2sp[i].TanSuat = System.Text.Json.JsonSerializer.Serialize(dshsps);
                        k++;
                        break;
                    }
                }
                if (k > 0)
                {
                    _dbContext.TanSuatHaiSanPhams.Update(ts2sp[i]);

                }
            }
            if (spcanchuy.Count > 0)
            {

                foreach (var ts in spcanchuy)
                {
                    var ts2 = _dbContext.TanSuatHaiSanPhams.FirstOrDefault(c => c.IdHaiSanPham == ts.IdSanPham);
                    var dstss = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts2.TanSuat);
                    List<SanPham> listsp = new List<SanPham>();
                    for (int i = dstss.Count - 1; i >= 0; i--)
                    {
                        if (dstss[i].TanSuat > sminhd)
                        {
                            listsp.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == dstss[i].IdSanPham));
                        }
                    }
                    for (int i = 1; i <= listsp.Count; i++)
                    {
                        var items = await LayTatCaTongHop(i, listsp);
                        foreach (var item in items)
                        {
                            item.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == ts2.IdHaiSanPham));
                        }

                        listtong.Add(items);
                    }
                    List<SanPham> list1sp = new List<SanPham>();
                    list1sp.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == ts2.IdHaiSanPham));
                    List<List<SanPham>> list1sp2 = new List<List<SanPham>>();
                    list1sp2.Add(list1sp);
                    listtong.Add(list1sp2);
                }
            }
            _dbContext.SaveChanges();
            return listtong;
        }
        private void DoiChoTanSuat()
        {
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            List<List<TanSuatMotSanPham>> listts2sp = new List<List<TanSuatMotSanPham>>();
            foreach (var ts in ts2sp)
            {
                List<TanSuatMotSanPham> list = new List<TanSuatMotSanPham>();
                list = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts.TanSuat);
                listts2sp.Add(list);
            }
            for (int i = 0; i < ts1sp.Count - 1; i++)
            {
                for (int j = 0; j < ts1sp.Count - i - 1; j++)
                {
                    if (ts1sp[j].TanSuat < ts1sp[j + 1].TanSuat)
                    {
                        var doicho = new TanSuatHaiSanPham
                        {
                            IdHaiSanPham = ts2sp[j].IdHaiSanPham,

                        };
                        ts2sp[j].IdHaiSanPham = ts2sp[j + 1].IdHaiSanPham;
                        ts2sp[j + 1].IdHaiSanPham = doicho.IdHaiSanPham;
                        var doicho2 = new TanSuatMotSanPham
                        {
                            IdSanPham = ts2sp[j + 1].IdHaiSanPham,
                            TanSuat = listts2sp[j + 1].FirstOrDefault(c => c.IdSanPham == ts2sp[j + 1].IdHaiSanPham).TanSuat,
                            ThuTu = listts2sp[j + 1].FirstOrDefault(c => c.IdSanPham == ts2sp[j + 1].IdHaiSanPham).ThuTu,
                        };
                        var listphu = listts2sp[j].ToList();
                        listphu.Add(doicho2);
                        listts2sp[j + 1].RemoveAll(c => c.IdSanPham == ts2sp[j + 1].IdHaiSanPham);
                        listts2sp[j] = listts2sp[j + 1];
                        listts2sp[j + 1] = listphu.ToList();
                        var ts1 = new TanSuatMotSanPham
                        {
                            IdSanPham = ts1sp[j + 1].IdSanPham,
                            TanSuat = ts1sp[j + 1].TanSuat,

                        };

                        ts1sp[j + 1].IdSanPham = ts1sp[j].IdSanPham;
                        ts1sp[j + 1].TanSuat = ts1sp[j].TanSuat;

                        ts1sp[j].IdSanPham = ts1.IdSanPham;
                        ts1sp[j].TanSuat = ts1.TanSuat;
                    }
                }

            }
            for (int i = 0; i < ts1sp.Count; i++)
            {
                ts2sp[i].TanSuat = System.Text.Json.JsonSerializer.Serialize(listts2sp[i]);
                _dbContext.TanSuatHaiSanPhams.Update(ts2sp[i]);
                _dbContext.TanSuatMotSanPhams.Update(ts1sp[i]);

            }
            _dbContext.SaveChanges();
        }
        private void XuLyHoaDonCach2()
        {
            var hoadons = _dbContext.HoaDons.ToList();
            var ldssp = _dbContext.SanPhams.ToList();
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            for (int i = 0; i < ldssp.Count; i++)
            {
                var ts1 = new TanSuatMotSanPham();
                ts1.ThuTu = i + 1;
                ts1.TanSuat = 0;
                ts1.IdSanPham = ldssp[i].IdSanPham;
                ts1sp.Add(ts1);
            }
            foreach (var hoadon in hoadons)
            {
                var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);

                foreach (var sp in dssp)
                {
                    for (int i = 0; i < ts1sp.Count; i++)
                    {
                        if (ts1sp[i].IdSanPham == sp.IdSanPham)
                        {
                            ts1sp[i].TanSuat++;
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < ts1sp.Count - 1; i++)
            {
                for (int j = 0; j < ts1sp.Count - i - 1; j++)
                {
                    if (ts1sp[j].TanSuat < ts1sp[j + 1].TanSuat)
                    {
                        var ts1 = new TanSuatMotSanPham
                        {
                            IdSanPham = ts1sp[j + 1].IdSanPham,
                            TanSuat = ts1sp[j + 1].TanSuat,

                        };

                        ts1sp[j + 1].IdSanPham = ts1sp[j].IdSanPham;
                        ts1sp[j + 1].TanSuat = ts1sp[j].TanSuat;

                        ts1sp[j].IdSanPham = ts1.IdSanPham;
                        ts1sp[j].TanSuat = ts1.TanSuat;
                    }
                }
            }
            for (int i = 0; i < ts1sp.Count; i++)
            {
                var ts2 = new TanSuatHaiSanPham();
                ts2.ThuTu = i + 1;
                ts2.IdHaiSanPham = ts1sp[i].IdSanPham;
                var listtong = new List<TanSuatMotSanPham>();
                for (int j = 0; j < i; j++)
                {
                    var listphu = new TanSuatMotSanPham();
                    listphu.ThuTu = j;
                    listphu.IdSanPham = ts1sp[j].IdSanPham;
                    listphu.TanSuat = 0;
                    listtong.Add(listphu);
                }
                ts2.TanSuat = System.Text.Json.JsonSerializer.Serialize(listtong);
                ts2sp.Add(ts2);
            }
            List<List<TanSuatMotSanPham>> listts2sp = new List<List<TanSuatMotSanPham>>();
            foreach (var ts in ts2sp)
            {
                List<TanSuatMotSanPham> list = new List<TanSuatMotSanPham>();
                list = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts.TanSuat);
                listts2sp.Add(list);
            }
            foreach (var hoadon in hoadons)
            {
                var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                int ssp = 0;
                int ssphd = dssp.Count;
                for (int i = ts2sp.Count - 1; i >= 0; i--)
                {
                    foreach (var sp in dssp)
                    {
                        if (sp.IdSanPham == ts2sp[i].IdHaiSanPham)
                        {
                            dssp.Remove(sp);
                            foreach (var spp in dssp)
                            {
                                foreach (var dshsp in listts2sp[i])
                                {
                                    if (spp.IdSanPham == dshsp.IdSanPham)
                                    {
                                        dshsp.TanSuat++;
                                        break;
                                    }
                                }
                            }
                            ssp++;
                            break;
                        }
                    }
                    if (ssp == ssphd)
                    {
                        break;
                    }
                }
            }
            for (int i = 0; i < ts2sp.Count; i++)
            {
                ts2sp[i].TanSuat = System.Text.Json.JsonSerializer.Serialize(listts2sp[i]);
                _dbContext.TanSuatHaiSanPhams.Add(ts2sp[i]);
                _dbContext.TanSuatMotSanPhams.Add(ts1sp[i]);

            }
            _dbContext.SaveChanges();
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

        [HttpGet("bo-san-pham-tiem-nang")]
        public async Task<IActionResult> BoSanPhamTiemNang(double s_min)
        {
            ResponseInfo response = new ResponseInfo();
            try
            {
                if (s_min > 1 || s_min < 0)
                {
                    return Ok("Vui long nhap tai support_min");
                }
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var sohoadon = _dbContext.HoaDons.Count();
                double k = s_min * sohoadon;
                var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
                var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();

                for (int i = ts1sp.Count - 1; i >= 0; i--)
                {

                    if (ts1sp[i].TanSuat < k)
                    {
                        ts2sp.Remove(ts2sp[i]);
                    }
                    else break;
                }
                List<List<List<SanPham>>> listtong = new List<List<List<SanPham>>>();
                foreach (var ts in ts2sp)
                {
                    var dstss = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts.TanSuat);
                    List<SanPham> listsp = new List<SanPham>();
                    for (int i = dstss.Count - 1; i >= 0; i--)
                    {
                        if (dstss[i].TanSuat > k)
                        {
                            listsp.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == dstss[i].IdSanPham));
                        }
                    }
                    for (int i = 1; i <= listsp.Count; i++)
                    {
                        var items = await LayTatCaTongHop(i, listsp);
                        foreach (var item in items)
                        {
                            item.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == ts.IdHaiSanPham));
                        }

                        listtong.Add(items);
                    }
                    List<SanPham> list1sp = new List<SanPham>();
                    list1sp.Add(_dbContext.SanPhams.FirstOrDefault(c => c.IdSanPham == ts.IdHaiSanPham));
                    List<List<SanPham>> list1sp2 = new List<List<SanPham>>();
                    list1sp2.Add(list1sp);
                    listtong.Add(list1sp2);
                }
                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;

                List<SanPham> spFinal = new List<SanPham>();
                listtong.ForEach(item =>
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
            catch (Exception ex)
            {
                response.statusCode = System.Net.HttpStatusCode.BadRequest;
                response.message = ex.ToString();
                return BadRequest(response);
            }

        }
        [HttpPost("them-hoa-don")]
        public async Task<IActionResult> ThemHoaDon([FromForm] double smin, [FromForm] ThemSanPham input)
        {
            string id = Guid.NewGuid().ToString();
            double sminhd = smin * _dbContext.HoaDons.Count();
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
            var items = await TangTanSuatMotHoaDon(hoaDon, sminhd);
            DoiChoTanSuat();
            if (items.Count == 0)
            {
                return Ok("Khong co to hop moi sinh ra");
            }
            return Ok(items);
        }
        [HttpDelete("xoa-bang-tan-suat")]
        public IActionResult XoaBangTanSuat()
        {
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            _dbContext.RemoveRange(ts1sp);
            _dbContext.RemoveRange(ts2sp);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpPost("tan-suat-phoi-hop")]
        public IActionResult Test([FromForm] List<string> IDSanPham)
        {
            var hoadons = _dbContext.HoaDons.ToList();
            int k = 0;
            foreach (var hoadon in hoadons)
            {
                var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                int i = 0;
                foreach (var sp in dssp)
                {
                    foreach (var id in IDSanPham)
                    {
                        if (sp.IdSanPham == id)
                        {
                            i++;
                        }
                    }
                }
                if (i == IDSanPham.Count)
                {
                    k++;
                }
            }
            return Ok(k);
        }

    }

}

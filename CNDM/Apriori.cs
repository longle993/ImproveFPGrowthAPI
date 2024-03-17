using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using CNDM.Models;

namespace CNDM
{
    class Apriori
    {
        private int support;
        private double confidence;
        private List<List<bool>> originDataMap;
        private Hashtable indexMap;
        private List<Dictionary<string, int>> iterationResult;
        private Dictionary<string, double> confidenceResult;

        public Dictionary<string, double> ConfidenceResult { get => confidenceResult;  }
        public List<Dictionary<string, int>> IterationResult { get => iterationResult; }

        public Apriori(double support, double confidence, List<string> data)
        {
            this.support = (int)(support * data.Count);
            this.confidence = confidence;
            this.originDataMap = new List<List<bool>>();
            this.indexMap = new Hashtable();
            this.iterationResult = new List<Dictionary<string, int>>();
            this.confidenceResult = new Dictionary<string, double>();
            List<string> originDataList = new List<string>(data);
            HashSet<string> itemSet = new HashSet<string>();
            for (int i = 0; i < originDataList.Count; i++)
            {
                List<string> temp = new List<string>(originDataList[i].Split(','));
                for (int j = 0; j < temp.Count; j++)
                {
                    temp[j] = temp[j].Trim();
                }
                foreach (string item in temp)
                {
                    itemSet.Add(item);
                }
            }
            int index = 0;
            foreach (string item in itemSet)
            {
                this.indexMap.Add(item, index);
                this.indexMap.Add(index, item);
                index++;
            }
            for (int i = 0; i < originDataList.Count; i++)
            {
                List<string> temp = new List<string>(originDataList[i].Split(','));
                for (int j = 0; j < temp.Count; j++)
                {
                    temp[j] = temp[j].Trim();
                }
                List<bool> tempMap = new List<bool>(itemSet.Count);
                for (int j = 0; j < itemSet.Count; j++)
                {
                    tempMap.Add(false);
                }
                foreach (string item in temp)
                {
                    tempMap[(int)indexMap[item]] = true;
                }
                originDataMap.Add(tempMap);
            }
            this.Run();
        }

        public void ShowOriginData()
        {
            Console.WriteLine("OriginData:");
            for (int i = 0; i < this.originDataMap.Count; i++)
            {
                Console.Write("\t" + (i + 1) + "\t");
                List<string> readyToPrint = new List<string>();
                for (int j = 0; j < this.originDataMap[i].Count; j++)
                {
                    if (this.originDataMap[i][j])
                    {
                        readyToPrint.Add(indexMap[j].ToString());
                    }
                }
                Console.WriteLine(string.Join(',', readyToPrint.ToArray()));
            }
        }

        public List<SanPham> ShowIteration()
        {
            List<SanPham> sanPhams = new List<SanPham>();

            for (int i = 0; i < this.iterationResult.Count; i++)
            {
                List<KeyValuePair<string, int>> dicList = this.iterationResult[i].ToList();
                dicList.Sort((p1, p2) => p1.Key.CompareTo(p2.Key));

                foreach (var pair in dicList)
                {
                    SanPham sanPham = new SanPham
                    {
                        IdSanPham = pair.Value.ToString(),
                        TenSanPham = pair.Key
                    };
                    sanPhams.Add(sanPham);
                }
            }

            return sanPhams;
        }

        public void ShowConfidence()
        {
            Console.WriteLine("Confidence：");
            List<KeyValuePair<string, double>> dicList = this.confidenceResult.ToList();
            dicList.Sort((p1, p2) => p1.Key.CompareTo(p2.Key));
            for (int i = 0; i < dicList.Count; i++)
            {
                Console.WriteLine("\t" + dicList[i].Key + " : " + dicList[i].Value);
            }
            Console.WriteLine();
        }

        private void Iterate()
        {
            int oriCount = this.originDataMap.Count;
            if (oriCount <= 0)
            {
                return;
            }
            Dictionary<string, int> one = new Dictionary<string, int>();
            for (int i = 0; i < oriCount; i++)
            {
                for (int j = 0; j < this.originDataMap[i].Count; j++)
                {
                    if (this.originDataMap[i][j])
                    {
                        if (one.ContainsKey(this.indexMap[j].ToString()))
                        {
                            one[this.indexMap[j].ToString()] += 1;
                        }
                        else
                        {
                            one.Add(this.indexMap[j].ToString(), 1);
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, int> item in one)
            {
                if (item.Value < this.support)
                {
                    one.Remove(item.Key);
                }
            }
            this.iterationResult.Add(one);

            int it = 0;
            while (true)
            {
                if (this.iterationResult[it].Count <= 1)
                {
                    if (this.iterationResult[it].Count == 0)
                    {
                        this.iterationResult.RemoveAt(it);
                    }
                    break;
                }
                List<List<string>> last = new List<List<string>>();
                foreach (string key in this.iterationResult[it].Keys)
                {
                    List<string> temp = new List<string>(key.Split(','));
                    temp.Sort((s1, s2) => s1.CompareTo(s2));
                    last.Add(temp);
                }
                List<List<string>> conjTable = new List<List<string>>();
                for (int i = 0; i < last.Count; i++)
                {
                    List<string> toConj = new List<string>();
                    for (int j = i + 1; j < last.Count; j++)
                    {
                        bool canConj = true;
                        for (int k = 0; k < it; k++)
                        {
                            if (!last[i][k].Equals(last[j][k]))
                            {
                                canConj = false;
                                break;
                            }
                        }
                        if (canConj)
                        {
                            toConj.Add(last[j][it]);
                        }
                    }
                    foreach (string item in toConj)
                    {
                        List<string> conj = new List<string>(last[i]);
                        conj.Add(item);
                        conj.Sort((s1, s2) => s1.CompareTo(s2));
                        conjTable.Add(conj);
                    }
                }
                Dictionary<string, int> freqSet = new Dictionary<string, int>();
                for (int i = 0; i < conjTable.Count; i++)
                {
                    string key = string.Join(',', conjTable[i].ToArray());
                    int num = 0;
                    foreach (List<bool> data in this.originDataMap)
                    {
                        bool canAdd = true;
                        for (int j = 0; j < conjTable[i].Count; j++)
                        {
                            if (!data[(int)this.indexMap[conjTable[i][j]]])
                            {
                                canAdd = false;
                                break;
                            }
                        }
                        if (canAdd)
                        {
                            num++;
                        }
                    }
                    if (num >= this.support)
                    {
                        freqSet.Add(key, num);
                    }
                }
                this.iterationResult.Add(freqSet);

                it++;
            }
        }

        private void ConfidenceCal()
        {
            if (iterationResult.Count == 1) return;
            for (int i = 1; i < iterationResult.Count; i++)
            {
                foreach (var key in iterationResult[i].Keys)
                {
                    List<List<string>> subsets = GetRealSubsets(new List<string>(key.Split(',')));
                    for (int j = 0; j < subsets.Count; j++)
                    {
                        if (subsets[j].Count == 0 || subsets[j].Count == i + 1) continue;
                        for (int k = 0; k < subsets.Count; k++)
                        {
                            List<string> fullSet = new List<string>(subsets[j]);
                            fullSet.AddRange(subsets[k]);
                            fullSet.Sort((s1, s2) => s1.CompareTo(s2));
                            if (!string.Join(',', fullSet.ToArray()).Equals(key)) continue;
                            subsets[j].Sort((s1, s2) => s1.CompareTo(s2));
                            subsets[k].Sort((s1, s2) => s1.CompareTo(s2));
                            string theKey = string.Join(',', subsets[j].ToArray());
                            int conditionNum = iterationResult[subsets[j].Count - 1][theKey];
                            int totalNum = iterationResult[key.Split(',').Length - 1][key];
                            double conf = totalNum * 1.0 / conditionNum;
                            if (conf > this.confidence)
                            {
                                this.confidenceResult.Add("{" + theKey + "} ==> {" + string.Join(',', subsets[k].ToArray()) + "}", conf);
                            }
                        }
                    }
                }
            }
        }

        private List<List<string>> GetRealSubsets(List<string> data)
        {
            int n = data.Count;
            List<List<string>> res = new List<List<string>>();
            List<string> t = new List<string>();
            for (int mask = 0; mask < (1 << n); ++mask)
            {
                t.Clear();
                for (int i = 0; i < n; ++i)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        t.Add(data[i]);
                    }
                }
                res.Add(new List<string>(t));
            }
            return res;
        }

        private void Run()
        {
            this.Iterate();
            this.ConfidenceCal();
        }
    }
}
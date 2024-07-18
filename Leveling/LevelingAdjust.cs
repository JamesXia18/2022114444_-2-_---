using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Leveling.LevelingPoint;

namespace Leveling
{
    internal class LevelingAdjust
    {
        public static List<LevelingPoint> LevelingPoints = new List<LevelingPoint>();//水准点的集合，包括已知点和未知点。
        public static List<LevelingLine> LevelingLines = new List<LevelingLine>();//观测的路线的集合。
        public static List<string> StrLoopClosure = new List<string>();//返回闭合差计算后的闭合差计算结果的字符信息。
        public static int MPnumber;//水准网总点数，包括已知点和未知点。
        public static int MKownPnumber;//已知点个数。
        public static double Alpha;//验前单位权中误差。
        public static int MLnumber;//高差观测值个数。
        public static int UnknowPointNum;//已经计算出的未知点高程的点数。
        public static List<string> StrAdjust = new List<string>();//严密平差报告文件字符信息

        /// <summary>
        /// 搜索最短路径
        /// </summary>
        /// <param name="p">目标点的点号</param>
        /// <param name="exclude">编号等于exclude的观测值不得参加最短路线的链接。</param>
        /// <param name="neighbor">邻接点点号数组，数组长度等于总点数</param>
        /// <param name="diff">高差累加值数组，存储目标点沿最短路线到每点的高差之和，数组长度等于总点数,数组内容待计算。</param>
        /// <param name="s">路线长度累加值数组，存储目标点沿最短路线到每点的路线长度，数组长度等于总点数，数组内容待计算。</param>
        public static void FindShortPath(int p, int exclude, int[] neighbor, double[] diff, double[] s)
        {
            for (int i = 0; i < MPnumber; i++)
            {
                neighbor[i] = -1;//给每个点的邻接点号数组赋值-1，表示还没有邻接点。
                s[i] = Math.Pow(10, 30);//每点到目标点的初始路线长度等于无穷大。
            }

            s[p] = 0.0;
            diff[p] = 0.0;
            neighbor[p] = p;//目标点的邻接点点号就是自己的点号。

            for (int j = 0; ; j++)
            {
                bool unchanged = true;//表示所有点都找到其邻接点的点号了。
                for (int k = 0; k < MLnumber; k++)
                {
                    if (k == exclude)
                    {
                        continue;//表示跳出当前这一次循环。
                    }
                    int p1 = LevelingLines[k].StartPoint.PointNum;//该观测路线起点的编号。
                    int p2 = LevelingLines[k].EndPoint.PointNum;//该观测路线的终点的编号。
                    double s12 = LevelingLines[k].LeveingRoadLength;//该观测路线的长度。
                    if (neighbor[p1] < 0 && neighbor[p2] < 0)//若起点和终点邻接点点号都没有找到，则跳出本次循环。
                    {
                        continue;//表示跳出当前这一次循环。
                    }
                    if (s[p2] > (s[p1] + s12))
                    {
                        neighbor[p2] = p1;
                        s[p2] = s[p1] + s12;
                        diff[p2] = diff[p1] + LevelingLines[k].LeveingHeightDifferent;
                        unchanged = false;
                    }

                    if (s[p1] > (s[p2] + s12))
                    {
                        neighbor[p1] = p2;
                        s[p1] = s[p2] + s12;
                        diff[p1] = diff[p2] - LevelingLines[k].LeveingHeightDifferent;
                        unchanged = false;
                    }

                }
                if (unchanged)
                {
                    break;
                }

            }
            return;
        }

        /// <summary>
        /// 环闭合差计算
        /// </summary>
        public static void  LoopClosure()
        {

            string str1 = "\t=========环闭合差计算=============";
            StrLoopClosure.Add(str1 + "\r\n");
            int num = MLnumber - MPnumber + 1;//独立闭合环的个数。
            if (num < 1)
            {
                MessageBox.Show("该水准网无闭合环！");
                return;
            }
            int[] neighbor = new int[MPnumber];//邻接点号数组。
            int[] used = new int[MLnumber];//观测值是否已经用于闭合差计算。
            double[] diff = new double[MPnumber];//高差累加值数组。
            double[] s = new double[MPnumber];//每点到目标点的路线长数组。

            for (int i = 0; i < MLnumber; i++)
            {
                used[i] = 0;//初始化所有观测值都没有参加闭合差计算。
            }
            for (int j = 0; j < MLnumber; j++)
            {
                int k1 = 0;
                int k2 = 0;
                if (LevelingLines.Count != 0)
                {
                    k1 = LevelingLines[j].StartPoint.PointNum;//得到该观测值的起点编号。
                    k2 = LevelingLines[j].EndPoint.PointNum;//得到该观测值的终点编号。
                    if (used[j] == 1)
                    {
                        continue;//若该观测值已经参加过闭合差计算，则跳出本次循环。
                    }
                }
                else
                {
                    MessageBox.Show("请先输入或导入数据，再进行闭合差计算！");
                    return;
                }
                FindShortPath(k2, j, neighbor, diff, s);
                if (neighbor[k1] < 0)
                {
                    //显示结果的字符串。
                    string str2 = string.Format("观测值{0}-{1}与任何观测边不构成闭合环" + "\r\n", LevelingLines[j].StartPoint.PointName, LevelingLines[j].EndPoint.PointName);
                    StrLoopClosure.Add(str2);
                }
                else
                {
                    used[j] = 1;//该观测值已参加闭合差计算。
                    string str3 = "闭合环：" + "\r\n";
                    StrLoopClosure.Add(str3);
                    int p1 = k1;
                    while (true)
                    {
                        int p2 = neighbor[p1];
                        string str4 = string.Format("{0}--", LevelingPoints[p1].PointName);
                        StrLoopClosure.Add(str4);
                        //将用过的边标定。
                        for (int r = 0; r < MLnumber; r++)
                        {
                            if (LevelingLines[r].StartPoint.PointNum == p1 && LevelingLines[r].EndPoint.PointNum == p2)
                            {
                                used[r] = 1;
                                break;
                            }
                            if (LevelingLines[r].StartPoint.PointNum == p2 && LevelingLines[r].EndPoint.PointNum == p1)
                            {
                                used[r] = 1;
                                break;
                            }
                        }
                        if (p2 == k2)
                        {
                            break;
                        }
                        else
                        {
                            p1 = p2;//继续寻找邻接点，直到邻接点点号是K2。
                        }
                    }
                    string str5 = string.Format("{0}--{1}" + "\r\n", LevelingPoints[k2].PointName, LevelingPoints[k1].PointName);
                    StrLoopClosure.Add(str5);
                    double w = LevelingLines[j].LeveingHeightDifferent + diff[k1];//闭合差。
                    double ss = s[k1] + LevelingLines[j].LeveingRoadLength;//环的长度。
                    string str6 = string.Format("闭合差:W ={0} " + "\r\n" + "\r\n", -Math.Round(w, 5));
                    StrLoopClosure.Add(str6);

                }

            }
        }

        /// <summary>
        /// 附合路线闭合差计算。
        /// </summary>
        public static void LineClosure()
        {
            string str1 = "\t=====附合路线闭合差计算=========";
            StrLoopClosure.Add(str1 + "\r\n");
            if (MKownPnumber < 2)//已知点数小于2，则表明无符合路线。
            {
                return;
            }
            int[] neighbor = new int[MPnumber];//l邻接点号数组。
            double[] diff = new double[MPnumber];//高差累加数组。
            double[] s = new double[MPnumber];//路线长累加数组。

            for (int i = 0; i < MPnumber; i++)
            {
                if (LevelingPoints[i].Nature == PointNature.Known)//如果该点是已知点
                {
                    //该方法的第二个参数-1表示每个观测值都参加最短路径的搜索。
                    FindShortPath(i, -1, neighbor, diff, s);//搜索到i号点的最短路线，用所有观测值。
                    for (int j = 0; j < MPnumber; j++)
                    {
                        if ((LevelingPoints[j].Nature == PointNature.Known) && (LevelingPoints[j].PointNum != LevelingPoints[i].PointNum))//如果该点是已知点同时要求该点和上一个已知点不是同一个点。
                        {
                            if (neighbor[j] < 0)
                            {
                                string str2 = string.Format("{0}--{1}之间找不到最短路径。" + "\r\n", LevelingPoints[i].PointName, LevelingPoints[j].PointName);
                                StrLoopClosure.Add(str2);
                                continue;
                            }
                            string str3 = "附合路线：" + "\r\n";
                            StrLoopClosure.Add(str3);
                            int k = j;
                            while (true)
                            {
                                string str4 = string.Format("{0}--", LevelingPoints[k].PointName);
                                StrLoopClosure.Add(str4);
                                k = neighbor[k];
                                if (k == i)
                                {
                                    break;
                                }
                            }
                            string str5 = string.Format("{0}" + "\r\n", LevelingPoints[i].PointName);
                            StrLoopClosure.Add(str5);
                            double w = LevelingPoints[i].Height + diff[j] - LevelingPoints[j].Height;
                            string str6 = string.Format("闭合差W ={0} " + "\r\n" + "\r\n", -Math.Round(w, 5));
                            StrLoopClosure.Add(str6);
                        }
                    }

                }
            }
        }
        /// <summary>
        /// 高程近似值计算。
        /// </summary>
        public static void Cal_H0()
        {
            //计算每个点的近似高程
            for (int i = 0; ; i++)
            {
                for (int j = 0; j < MLnumber; j++)
                {
                    //如果某观测边起点高程已知，同时终点高程未知的情况。
                    if ((LevelingLines[j].StartPoint.Height != 0) && (LevelingLines[j].EndPoint.Height == 0))
                    {
                        LevelingLines[j].EndPoint.Height = LevelingLines[j].StartPoint.Height + LevelingLines[j].LeveingHeightDifferent;
                        UnknowPointNum++;
                    }
                    //如果某观测边终点高程已知，同时起点高程未知的情况。
                    if ((LevelingLines[j].StartPoint.Height == 0) && (LevelingLines[j].EndPoint.Height != 0))
                    {
                        LevelingLines[j].StartPoint.Height = LevelingLines[j].EndPoint.Height - LevelingLines[j].LeveingHeightDifferent;
                        UnknowPointNum++;
                    }

                    if (UnknowPointNum == (MPnumber - MKownPnumber))
                    {
                        MessageBox.Show("近似高程计算成功！");
                        return;
                    }
                    if (i > (MPnumber - MKownPnumber))
                    {
                        //  string str1 = "下列点无法计算出概略高程：   ";
                        for (int k = 0; k < MPnumber; k++)
                        {
                            if (LevelingPoints[k].Height == 0)
                            {
                                string str2 = string.Format("{0}", LevelingPoints[k].PointName);
                            }
                            MessageBox.Show("近似高程计算失败！");
                            return;
                        }
                    }
                }
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leveling
{
    internal class LevelingLine
    {   
     /// <summary>
     /// 水准线路的编号
     /// </summary>
     public int LeveingLineNum {set; get;}

    /// <summary>
    /// 水准线路的起点
    /// </summary>
     public LevelingPoint? StartPoint {set; get;}
     
     /// <summary>
     /// 水准线路的终点
     /// </summary>
     public LevelingPoint? EndPoint {set; get;}
     
     /// <summary>
     /// 水准线路的高差
     /// </summary>
     public double LeveingHeightDifferent { set; get; }

    /// <summary>
    /// 水准线路的长度
    /// </summary>
     public double LeveingRoadLength { set; get; } 
  }
}

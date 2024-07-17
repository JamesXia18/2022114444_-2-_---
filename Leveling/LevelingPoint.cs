using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leveling
{
    internal class LevelingPoint
    {
        /// <summary>
        /// 点的性质:已知点和未知点
        /// </summary>
        public enum PointNature 
        {
            Known,
            UnKnown
        }

        /// <summary>
        /// 水准点的名称
        /// </summary>
        public string? PointName { get; set; }

        /// <summary>
        /// 水准点的编号
        /// </summary>
        public int PointNum { get; set; }

        /// <summary>
        /// 未知点的编号,若为已知则为-1
        /// </summary>
        public int UKnownPointNum { get; set; }

        /// <summary>
        /// 水准点的高程
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 水准点的性质
        /// </summary>
        public PointNature Nature { get; set; }

        // 重写 Equals 方法
        public override bool Equals(object obj)
        {

            if (this.PointName == (obj as LevelingPoint).PointName) {
                return true;
            }
            return false;
        }

    }
}

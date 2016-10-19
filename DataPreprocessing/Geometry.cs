using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LorryDataAnalysis
{
    //点类
    [Serializable]
    public class Point
    {
        private int _ID;
        private int _FromId;
        private int _ToId;
        private double _x;
        private double _y;

        public int FromID { get { return _FromId; } set { _FromId = value; } }
        public int ToID { get { return _ToId; } set { _ToId = value; } }
        public int ID { get { return _ID; } set { _ID = value; } }
        public double x { get { return _x; } set { _x = value; } }
        public double y { get { return _y; } set { _y = value; } }

        //判断一个点属于一个矩形
        public bool Within(Envelope r)
        {
            if (this._x <= r.XMax && this._x >= r.XMin && this._y <= r.YMax && this._y >= r.YMin)

                return true;

            else

                return false;
        }
    }

    public class Envelope
    {
        private double _XMin;
        private double _XMax;
        private double _YMin;
        private double _YMax;

        public double XMin { get { return _XMin; } set { _XMin = value; } }
        public double XMax { get { return _XMax; } set { _XMax = value; } }
        public double YMin { get { return _YMin; } set { _YMin = value; } }
        public double YMax { get { return _YMax; } set { _YMax = value; } }

        public void PutCoords(double xmin, double xmax, double ymin, double ymax)
        {
            XMin = xmin;
            XMax = xmax;
            YMin = ymin;
            YMax = ymax;
        }

        //判断一个矩形是否与另一个矩形相交
        public bool Intersect(Envelope r)
        {
            if ((r.XMin >= this._XMin && r.XMin <= this._XMax && r.YMax <= this._YMax && r.YMax >= this._YMin) ||
                 (r.XMax >= this._XMin && r.XMax <= this._XMax && r.YMax <= this._YMax && r.YMax >= this._YMin) ||
                 (r.XMax >= this._XMin && r.XMax <= this._XMax && r.YMin <= this._YMax && r.YMin >= this._YMin) ||
                 (r.XMin >= this._XMin && r.XMin <= this._XMax && r.YMin <= this._YMax && r.YMin >= this._YMin))

                return true;

            else

                return false;
        }

        //判断一个矩形是否与另一个矩形分离
        public bool Disjoint(Envelope r)
        {
            if (r.XMax <= this._XMin || r.XMin >= this._XMax || r.YMax <= this._YMin || r.YMin >= this._YMax)

                return true;

            else

                return false;
        }
    }
}

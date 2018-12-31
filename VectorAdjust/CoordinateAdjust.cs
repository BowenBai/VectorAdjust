using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAdjust
{
    //几何纠偏算法

    public delegate double[] DelegateCoordsTransform(double lat, double lon);
    public class CoordinateAdjust
    {
        //常量字段
        public const double PI = 3.14159265358979324; //PI
        public const double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        public const double EarthRadius = 6378137;

        /**
         * -----------------
         * 字段
         * -----------------
         */
        public double MinLatitude // 
        {
            set;
            get;
        }
        public double MaxLatitude  //
        {
            set;
            get;
        }
        public double MinLongitude //
        {
            set;
            get;
        }
        public double MaxLongitude //
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public CoordinateAdjust()
        {
            MinLatitude = -85.05112878;
            MaxLatitude = 85.05112878;
            MinLongitude = -180.0;
            MaxLongitude = 180;

        }

        /// <summary>返回两个坐标之间的距离
        /// 返回两个坐标之间的距离
        /// </summary>
        /// <param name="latA">纬度A</param>
        /// <param name="lonA">经度A</param>
        /// <param name="latB">纬度B</param>
        /// <param name="lonB">经度B</param>
        /// <returns>两个坐标点（经纬度）直接的距离</returns>
        public double GetDistance(double latA, double lonA, double latB, double lonB)
        {
            double earthR = 6371000.0;
            double x = Math.Cos(latA * PI / 180.0) * Math.Cos(latB * PI / 180.0) * Math.Cos((lonA - lonB) * PI / 180.0);
            double y = Math.Sin(latA * PI / 180.0) * Math.Sin(latB * PI / 180.0);
            double s = x + y;
            if (s > 1)
            {
                s = 1;
            }
            else if (s < -1)
            {
                s = -1;
            }

            double alpha = Math.Acos(s);
            double distance = alpha * earthR;
            return distance;
        }

        /// <summary>变换纬度
        /// 变换纬度
        /// </summary>
        /// <param name="x">纬度</param>
        /// <param name="y">经度</param>
        /// <returns>变换后的纬度</returns>
        public double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>变换经度
        /// 变换经度
        /// </summary>
        /// <param name="x">纬度</param>
        /// <param name="y">精度</param>
        /// <returns>变换后的经度</returns>
        public double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0 * PI)) * 2.0 / 3.0;
            return ret;
        }


        /// <summary>变换坐标
        /// 变换坐标
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns>包含转换后的经度和纬度的数组</returns>
        public double[] delta(double lat, double lon)
        {
            double[] coords = new double[2];
            const double a = 6378245.0;
            const double ee = 0.00669342162296594323;
            double dLat = this.transformLat(lon - 105.0, lat - 35.0);
            double dLon = this.transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * PI;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * PI);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * PI);
            coords[0] = dLat;
            coords[1] = dLon;
            return coords;
        }
        /// <summary>判断是否超出中国范围
        /// 判断是否超出中国范围
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns>真假</returns>
        public bool outofChina(double lat, double lon)
        {
            if ((lon < 72.004) || (lon > 137.8347))
                return true;
            else if ((lat < 0.8293) || (lat > 55.8271))
                return true;
            return false;
        }


        /// <summary>WGS84转国测局02
        /// WGS84转国测局02
        /// </summary>
        /// <param name="wgsLat">WGS84纬度</param>
        /// <param name="wgsLon">WGS84经度</param>
        /// <returns>GCJ纬度 经度</returns>
        public double[] WGS84ToGCJ02(double wgsLat, double wgsLon)
        {
            double[] coords = new double[2];
            if (outofChina(wgsLat, wgsLon))
            {
                coords[0] = wgsLat;
                coords[1] = wgsLon;
                return coords;
            }
            else
            {
                coords[0] = wgsLat + delta(wgsLat, wgsLon)[0];
                coords[1] = wgsLon + delta(wgsLat, wgsLon)[1];
                return coords;
            }


        }

        /// <summary>国测局GCJ02转wgs84坐标
        /// 国测局GCJ02转wgs84坐标
        /// </summary>
        /// <param name="gcjLat">国测局纬度</param>
        /// <param name="gcjLon">国测局经度</param>
        /// <returns></returns>
        public double[] GCJ02ToWGS84(double gcjLat, double gcjLon)
        {
            double[] coords = new double[2];
            const double threshold = 0.000000001;
            const double initDelta = 0.01;
            double dLat = initDelta;
            double dLon = initDelta;
            double mLat = gcjLat - dLat;
            double mLon = gcjLon - dLon;
            double pLat = gcjLat + dLat;
            double pLon = gcjLon + dLon;
            double wgsLat = 0;
            double wgsLon = 0;
            int i = 10000;
            while (true)
            {
                wgsLat = (mLat + pLat) / 2;
                wgsLon = (mLon + pLon) / 2;
                dLat = WGS84ToGCJ02(wgsLat, wgsLon)[0] - gcjLat;
                dLon = WGS84ToGCJ02(wgsLat, wgsLon)[1] - gcjLon;
                if (((Math.Abs(dLat) < threshold) && (Math.Abs(dLon) < threshold)))
                {
                    break;
                }

                if (dLat > 0)
                {
                    pLat = wgsLat;
                }

                else
                {
                    mLat = wgsLat;
                }
                if (dLon > 0)
                { pLon = wgsLon; }
                else
                { mLon = wgsLon; }
                i -= 1;
                if (i <= 0) { break; }

            }
            coords[0] = wgsLat;
            coords[1] = wgsLon;
            return coords;
        }

        /// <summary>国测局坐标转百度BD09坐标
        /// 国测局坐标转百度BD09坐标
        /// </summary>
        /// <param name="gcjLat">国测局纬度</param>
        /// <param name="gcjLon">国测局经度</param>
        /// <returns>百度BD09坐标纬度和经度</returns>
        public double[] GCJ02ToBD09(double gcjLat, double gcjLon)
        {
            double[] coords = new double[2];
            double x = gcjLon;
            double y = gcjLat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            double bdLon = z * Math.Cos(theta) + 0.0065;
            double bdLat = z * Math.Sin(theta) + 0.006;
            coords[0] = bdLat;
            coords[1] = bdLon;
            return coords;
        }

        /// <summary>百度坐标BD09坐标转国测局GJC02坐标
        /// 百度坐标BD09坐标转国测局GJC02坐标
        /// </summary>
        /// <param name="bdLat">百度纬度坐标</param>
        /// <param name="bdLon">百度经度坐标</param>
        /// <returns>国测局GCJ02坐标</returns>
        public double[] BD09ToGCJ02(double bdLat, double bdLon)
        {
            double[] coords = new double[2];
            double x = bdLon - 0.0065;
            double y = bdLat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            coords[1] = z * Math.Cos(theta);
            coords[0] = z * Math.Sin(theta);
            return coords;
        }

        /// <summary>WGS84坐标转WebMercator投影坐标
        /// WGS84坐标转WebMercator投影坐标
        /// </summary>
        /// <param name="wgsLat">WGS-84纬度</param>
        /// <param name="wgsLon">WGS-84经度</param>
        /// <returns>WebMercator坐标x y</returns>
        public double[] WGS84ToMercator(double wgsLat, double wgsLon)
        {
            double[] coords = new double[2];
            coords[1] = wgsLon * 20037508.34 / 180.0;
            double y = Math.Log(Math.Tan((90.0 + wgsLat) * PI / 360.0)) / (PI / 180.0);
            coords[0] = y * 20037508.34 / 180.0;
            return coords;
        }

        /// <summary>WebMerctor转WGS-84坐标
        /// WebMerctor转WGS-84坐标
        /// </summary>
        /// <param name="mercatorLat">WebMerctor 纬度 y</param>
        /// <param name="mercatorLon">WebMerctor 经度 x</param>
        /// <returns>WGS84 经度 纬度</returns>
        public double[] MercatorToWGS84(double mercatorLat, double mercatorLon)
        {
            double[] coords = new double[2];
            coords[1] = mercatorLon / 20037508.34 * 180.0;
            double y = mercatorLat / 20037508.34 * 180.0;
            coords[0] = 180 / PI * (2 * Math.Atan(Math.Exp(y * PI / 180.0)) - PI / 2);
            return coords;

        }

        /// <summary>WGS84坐标转BD09坐标
        /// WGS84坐标转BD09坐标
        /// </summary>
        /// <param name="wgsLat">WGS84纬度</param>
        /// <param name="wgsLon">WGS84经度</param>
        /// <returns>BD09坐标 纬度 经度</returns>
        public double[] WGS84ToBD09(double wgsLat, double wgsLon)
        {
            double[] gcjcoords = WGS84ToGCJ02(wgsLat, wgsLon);
            return GCJ02ToBD09(gcjcoords[0], gcjcoords[1]);
        }

        /// <summary>百度BD09坐标转WGS84坐标
        /// 百度BD09坐标转WGS84坐标
        /// </summary>
        /// <param name="bdLat">百度DB09纬度</param>
        /// <param name="bdLon">百度BD09经度</param>
        /// <returns>WGS84纬度 经度</returns>
        public double[] BD09ToWGS84(double bdLat, double bdLon)
        {
            double[] gcjcoords = this.BD09ToGCJ02(bdLat, bdLon);
            return GCJ02ToWGS84(gcjcoords[0], gcjcoords[1]);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSGeo.OGR;
using OSGeo.OSR;
using OSGeo.GDAL;
using System.Text.RegularExpressions;
using System.Data;


namespace VectorAdjust
{
    //几何对象纠偏
    public class VectorCoordsAdjust
    {
        public Geometry GeometryCoordTransfrom2D(Geometry geomS, DelegateCoordsTransform dele)
        {
            string wktGeo;
            geomS.FlattenTo2D();
            geomS.ExportToWkt(out wktGeo);

            Regex xy = new Regex(@"\d+ \d+|\d+\.\d+ \d+\.\d+");
            //Match matchex = xy.Match(wktGeo);

            MatchCollection mc = xy.Matches(wktGeo);
            foreach (Match item in mc)
            {
                string str = item.Value;
                string[] x_y = item.Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                double lat = Convert.ToDouble(x_y[1]);
                double lon = Convert.ToDouble(x_y[0]);
                double[] lat_lon = dele(lat, lon);
                string replStr = String.Format("{0} {1}", lat_lon[1], lat_lon[0]);

                wktGeo = wktGeo.Replace(str, replStr);
            }
            Geometry newGeo = Geometry.CreateFromWkt(wktGeo);
            return newGeo;
        }

        [Obsolete]
        public Geometry GeometryCoordTransfrom(Geometry geomS, DelegateCoordsTransform dele)
        {
            wkbGeometryType geotype = geomS.GetGeometryType();
            geomS.FlattenTo2D();
            //CoordinateTransformation tran;
            //geomS.Transform();

            Geometry geomT;
            switch (geotype)
            {

                case wkbGeometryType.wkbPoint:
                    string wktCoord_p;
                    geomS.ExportToWkt(out wktCoord_p);
                    //解析点文件
                    int istart_p = wktCoord_p.IndexOf("(");
                    int iend_p = wktCoord_p.LastIndexOf(")");
                    int len_p = iend_p - istart_p - 1;
                    string strCoord_p = wktCoord_p.Substring(istart_p + 1, len_p);
                    string[] strXY_p = strCoord_p.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    double lat_p = Convert.ToDouble(strXY_p[1]);
                    double lon_p = Convert.ToDouble(strXY_p[0]);

                    double[] yx_p = dele(lat_p, lon_p);
                    string x_y_p = String.Format("POINT({0} {1})", yx_p[1], yx_p[0]);
                    geomT = Geometry.CreateFromWkt(x_y_p);

                    return geomT;

                case wkbGeometryType.wkbLineString:
                    StringBuilder sb_l = new StringBuilder("LineString (");
                    string wktCoord_1;
                    geomS.ExportToWkt(out wktCoord_1);
                    int istar_l = wktCoord_1.IndexOf("(");
                    int iend_l = wktCoord_1.IndexOf(")");
                    int len_l = iend_l - istar_l - 1;
                    string[] strArrxy_l = wktCoord_1.Substring(istar_l + 1, len_l).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < strArrxy_l.Length; i++)
                    {
                        string strxy_l = strArrxy_l[i];
                        string[] xy_l = strxy_l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        double lat_l = Convert.ToDouble(xy_l[1]);
                        double lon_l = Convert.ToDouble(xy_l[0]);
                        double[] dxy_l = dele(lat_l, lon_l);

                        sb_l.AppendFormat("{0} {1}", dxy_l[1], dxy_l[0]);
                        if (i != strArrxy_l.Length - 1)
                        {
                            sb_l.Append(",");
                        }

                    }
                    sb_l.Append(")");
                    geomT = Geometry.CreateFromWkt(sb_l.ToString());
                    return geomT;

                case wkbGeometryType.wkbPolygon:
                    StringBuilder sb_pl = new StringBuilder("Polygon ((");
                    string wktCoord_pl;
                    geomS.ExportToWkt(out wktCoord_pl);
                    int istar_pl = wktCoord_pl.IndexOf("(");
                    int iend_pl = wktCoord_pl.IndexOf(")");
                    int len_pl = iend_pl - istar_pl;
                    string[] strArrxy_pl = wktCoord_pl.Substring(istar_pl + 1, len_pl).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < strArrxy_pl.Length; i++)
                    {
                        string strxy_pl = strArrxy_pl[i];
                        string[] xy_pl = strxy_pl.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        double lat_pl = Convert.ToDouble(xy_pl[1]);
                        double lon_pl = Convert.ToDouble(xy_pl[0]);
                        double[] dxy_pl = dele(lat_pl, lon_pl);

                        sb_pl.AppendFormat("{0} {1}", dxy_pl[1], dxy_pl[0]);
                        if (i != strArrxy_pl.Length - 1)
                        {
                            sb_pl.Append(",");
                        }

                    }
                    sb_pl.Append(")");
                    geomT = Geometry.CreateFromWkt(sb_pl.ToString());

                    return geomT;

                case wkbGeometryType.wkbMultiPoint:
                    StringBuilder sb_mp = new StringBuilder("Polygon ((");
                    string wktCoord_mp;
                    geomS.ExportToWkt(out wktCoord_mp);
                    int istar_mp = wktCoord_mp.IndexOf("(");
                    int iend_mp = wktCoord_mp.IndexOf(")");
                    int len_mp = iend_mp - istar_mp;
                    string[] strArrxy_mp = wktCoord_mp.Substring(istar_mp + 1, len_mp).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < strArrxy_mp.Length; i++)
                    {
                        string strxy_mp = strArrxy_mp[i];
                        string[] xy_mp = strxy_mp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        double lat_mp = Convert.ToDouble(xy_mp[1]);
                        double lon_mp = Convert.ToDouble(xy_mp[0]);
                        double[] dxy_mp = dele(lat_mp, lon_mp);

                        sb_mp.AppendFormat("{0} {1}", dxy_mp[0], dxy_mp[1]);
                        if (i != strArrxy_mp.Length - 1)
                        {
                            sb_mp.Append(",");
                        }

                    }
                    sb_mp.Append(")");
                    geomT = Geometry.CreateFromWkt(sb_mp.ToString());
                    return geomT;

                case wkbGeometryType.wkbMultiLineString:
                    return null;

                case wkbGeometryType.wkbMultiPolygon:
                    return null;


                case wkbGeometryType.wkbGeometryCollection:
                    return null;
                default:
                    return null;
            }


        }


        public bool VectorFileCoordTransfrom(string fileName, string saveFileName, DelegateCoordsTransform deleCoordsTransfrom)
        {
            //注册所有驱动
            Ogr.RegisterAll();
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            //为支持属性表字段支持中文
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");

            //开启日志
            Gdal.SetConfigOption("CPL_DEBUG", "ON");
            Gdal.SetConfigOption("CPL_LOG", System.IO.Directory.GetCurrentDirectory() + "\\gdallog.txt");



            /**
             * 打开数据源
             */
            //---------------------------------------
            //目标
            string strDriverName = null;
            if (System.IO.Path.GetExtension(saveFileName).ToUpper().Equals(".TAB"))
            {
                strDriverName = "MapInfo File";
            }
            if (System.IO.Path.GetExtension(saveFileName).ToUpper().Equals(".SHP"))
            {
                strDriverName = "ESRI Shapefile";
            }
            OSGeo.OGR.Driver Tdriver = Ogr.GetDriverByName(strDriverName);
            if (Tdriver == null)
            {
                return false;
            }
            string dir = System.IO.Path.GetDirectoryName(saveFileName);
            DataSource dsT = Tdriver.CreateDataSource(dir, null);
            if (dsT == null)
            {
                return false;
            }
            //---------------------------------------
            //源
            DataSource dsS = Ogr.Open(fileName, 0);

            if (dsS == null)
            {
                return false;
            }
            OSGeo.OGR.Driver Sdriver = dsS.GetDriver();
            if (Sdriver == null)
            {
                return false;
            }
            //-------------------------------------------------
            try
            {

                for (int iLayer = 0; iLayer < dsS.GetLayerCount(); iLayer++)
                {
                    //获得图层信息
                    Layer layerS = dsS.GetLayerByIndex(iLayer);
                    //创建图层
                    //图层名称
                    string saveName = System.IO.Path.GetFileNameWithoutExtension(saveFileName);
                    Layer layerT = dsT.CreateLayer(saveName, layerS.GetSpatialRef(), layerS.GetGeomType(), null);
                    if (layerS != null)
                    {

                        Feature pFeatureS;
                        Feature pFeatureT;
                        Geometry geomS, geomT;
                        //地理要素
                        long countFeature = layerS.GetFeatureCount(0);
                        FeatureDefn fd = layerS.GetLayerDefn();
                        //对目标图层进行属性定义
                        for (int i = 0; i < fd.GetFieldCount(); i++)
                        {
                            layerT.CreateField(fd.GetFieldDefn(i), 1);
                        }
                        fd.Dispose(); //释放资源

                        for (int i = 0; i < countFeature; i++)
                        {

                            pFeatureS = layerS.GetFeature(i);
                            if (pFeatureS != null)
                            {
                                geomS = pFeatureS.GetGeometryRef();
                                geomT = GeometryCoordTransfrom2D(geomS, deleCoordsTransfrom);
                                pFeatureT = pFeatureS.Clone();
                                pFeatureT.SetFrom(pFeatureS, 1);


                                pFeatureT.SetGeometryDirectly(geomT);

                                //newGeotry.Dispose();
                                layerT.CreateFeature(pFeatureT);

                                geomS.Dispose();
                                geomT.Dispose();
                                pFeatureS.Dispose();
                                pFeatureT.Dispose();
                                //layerT.SetFeature(pFeatureT);
                                //pFeature.Dispose();
                            }

                        }//endfor
                    }//endif
                    layerS.Dispose();
                    layerT.Dispose();
                }//endfor
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            dsS.Dispose();
            dsT.Dispose();
            return true;
        }


        [Obsolete]
        public bool VectorFileCoordTransfrom(string fileName, DelegateCoordsTransform deleCoordsTransfrom)
        {
            Ogr.RegisterAll();
            /**
             * 打开数据源
             */
            DataSource ds = Ogr.Open(fileName, 1);

            if (ds == null)
            {
                return false;
            }
            OSGeo.OGR.Driver drv = ds.GetDriver();
            if (drv == null)
            {
                return false;
            }

            // try
            //{

            for (int iLayer = 0; iLayer < ds.GetLayerCount(); iLayer++)
            {
                //获得图层信息
                Layer layer = ds.GetLayerByIndex(iLayer);

                if (layer != null)
                {
                    Feature pFeature;
                    Geometry geom;
                    //地理要素
                    long countFeature = layer.GetFeatureCount(0);


                    for (int i = 0; i < countFeature; i++)
                    {

                        pFeature = layer.GetFeature(i);
                        if (pFeature != null)
                        {

                            geom = pFeature.GetGeometryRef();
                            string wktgeo;
                            geom.ExportToWkt(out wktgeo);
                            wkbGeometryType geotype = geom.GetGeometryType();
                            StringBuilder sb = new StringBuilder();
                            switch (geotype)
                            {
                                case wkbGeometryType.wkbLineString:
                                    sb.Append("LineString(");
                                    int zk = wktgeo.LastIndexOf("(");
                                    int yk = wktgeo.IndexOf(")");
                                    int lenth = yk - zk - 1;
                                    string coords = wktgeo.Substring(zk + 1, lenth);
                                    string[] coordxy = coords.Split(',');
                                    for (int k = 0; k < coordxy.Length; k++)
                                    {
                                        string strxy = coordxy[k];
                                        string[] sxsy = strxy.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        double lat = Convert.ToDouble(sxsy[1]);
                                        double lon = Convert.ToDouble(sxsy[0]);

                                        double[] yx = deleCoordsTransfrom(lat, lon);
                                        string x_y = String.Format("{0} {1}", yx[1], yx[0]);
                                        sb.Append(x_y);
                                        if (k != coordxy.Length - 1)
                                        {
                                            sb.Append(",");
                                        }
                                    }
                                    sb.Append(")");
                                    //Geometry newgeom = Geometry.CreateFromWkt(sb.ToString());
                                    break;
                                case wkbGeometryType.wkbPoint:
                                    sb.Append("POINT(");
                                    int zkp = wktgeo.LastIndexOf("(");
                                    int ykp = wktgeo.IndexOf(")");
                                    int lenthp = ykp - zkp - 1;
                                    string coordsp = wktgeo.Substring(zkp + 1, lenthp);
                                    string[] sxsyp = coordsp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    double latp = Convert.ToDouble(sxsyp[1]);
                                    double lonp = Convert.ToDouble(sxsyp[0]);

                                    double[] yxp = deleCoordsTransfrom(latp, lonp);
                                    string x_yp = String.Format("{0} {1}", yxp[1], yxp[0]);
                                    sb.Append(x_yp);
                                    sb.Append(")");

                                    break;
                                case wkbGeometryType.wkbPolygon:
                                    sb.Append("POLYGON((");
                                    int zkm = wktgeo.LastIndexOf("(");
                                    int ykm = wktgeo.IndexOf(")");
                                    int lenthm = ykm - zkm - 1;
                                    string coordsm = wktgeo.Substring(zkm + 1, lenthm);
                                    string[] coordxym = coordsm.Split(',');
                                    for (int k = 0; k < coordxym.Length; k++)
                                    {
                                        string strxy = coordxym[k];
                                        string[] sxsy = strxy.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        double lat = Convert.ToDouble(sxsy[1]);
                                        double lon = Convert.ToDouble(sxsy[0]);

                                        double[] yx = deleCoordsTransfrom(lat, lon);
                                        string x_y = String.Format("{0} {1}", yx[1], yx[0]);
                                        sb.Append(x_y);
                                        if (k != coordxym.Length - 1)
                                        {
                                            sb.Append(",");
                                        }
                                    }
                                    sb.Append("))");

                                    break;
                                default:
                                    break;
                            }

                            Geometry newGeotry = Geometry.CreateFromWkt(sb.ToString());
                            pFeature.SetGeometryDirectly(newGeotry);

                            //newGeotry.Dispose();



                            layer.SetFeature(pFeature);
                            //pFeature.Dispose();
                        }

                    }

                }
            }
            //}
            //catch (Exception ex)
            //{

            //    throw new Exception(ex.Message);
            //}

            return true;
        }


        public void DataTableWithXYCoordTransfrom(DataTable dt, string lat, string lon, DelegateCoordsTransform deleCoordsTransform)
        {
            if (!dt.Columns.Contains(lat) || !dt.Columns.Contains(lon))
            {
                throw new Exception("内存数据库中不包含指定的：" + lat + "或" + lon + "列");
            }
            //坐标转换
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string strLat = dt.Rows[i][lat].ToString();
                    string strLon = dt.Rows[i][lon].ToString();
                    double vlat, vlon;
                    if (!double.TryParse(strLat, out vlat))
                    {
                        continue;
                    }
                    if (!double.TryParse(strLon, out vlon))
                    {
                        continue;
                    }
                    //double vlat = Convert.ToDouble(dt.Rows[i][lat]);
                    //double vlon = Convert.ToDouble(dt.Rows[i][lon]);
                    double[] values = deleCoordsTransform(vlat, vlon);

                    DataRow dr = dt.Rows[i];
                    dr.BeginEdit();
                    dr[lat] = values[0];
                    dr[lon] = values[1];
                    dr.EndEdit();

                }
            }
            catch (InvalidCastException caseEx)
            {
                throw new Exception(caseEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

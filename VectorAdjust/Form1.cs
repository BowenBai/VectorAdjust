using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSGeo.OGR;
using OSGeo.GDAL;
using OSGeo.OSR;
using System.Text.RegularExpressions;


namespace VectorAdjust
{
    public partial class Form1 : Form
    {
        string[] files = null;
        public Form1()
        {
            InitializeComponent();
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 1;
        }

        public static string TransferEncoding(Encoding srcEncoding, Encoding dstEncoding, string srcStr)
        {
            byte[] srcBytes = srcEncoding.GetBytes(srcStr);
            byte[] bytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            return dstEncoding.GetString(bytes);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofg = new OpenFileDialog();
            ofg.Title = "选择Shapefile文件";
            ofg.Multiselect = true;
            ofg.CheckFileExists = true;
            ofg.Filter = "Shapefile文件(*.shp)|*.shp";
            if (ofg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.files = ofg.FileNames;
                this.textBox1.Text = string.Join("|", ofg.SafeFileNames);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
            //string str = files[0];
            ////string encodingstring = TransferEncoding(Encoding.UTF8, Encoding.GetEncoding("GBK"), str);
            ////MessageBox.Show(encodingstring);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择要输出的文件目录";
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox2.Text = fbd.SelectedPath;
            }
        }

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

        private void button4_Click(object sender, EventArgs e)
        {
            string dir_gdal_data = System.Environment.CurrentDirectory+"\\"+ "gdal-data\\";
            
            Environment.SetEnvironmentVariable("GDAL_DATA", dir_gdal_data, EnvironmentVariableTarget.Process);
            DelegateCoordsTransform dele = null;
            VectorAdjust.CoordinateAdjust ca = new CoordinateAdjust();
            //------------WGS84--GCJ02--------------
            if (comboBox1.SelectedIndex ==0 && comboBox2.SelectedIndex == 1)
            {
                dele = new DelegateCoordsTransform(ca.WGS84ToGCJ02);
            }
            //------------GCJ02--WGS84--------------
            if (comboBox1.SelectedIndex == 1 && comboBox2.SelectedIndex == 0)
            {
                dele = new DelegateCoordsTransform(ca.GCJ02ToWGS84);
            }
            //------------WGS84--BD09--------------
            if (comboBox1.SelectedIndex == 0 && comboBox2.SelectedIndex == 2)
            {
                dele = new DelegateCoordsTransform(ca.WGS84ToBD09);
            }
            //------------BD09---WGS84--------------
            if (comboBox1.SelectedIndex == 2 && comboBox2.SelectedIndex == 0)
            {
                dele = new DelegateCoordsTransform(ca.BD09ToWGS84);                
            }
            //------------BD09---GCJ02--------------
            if (comboBox1.SelectedIndex == 2 && comboBox2.SelectedIndex == 1)
            {
                dele = new DelegateCoordsTransform(ca.BD09ToGCJ02);
            }
            //------------GCJ02--BD09--------------
            if (comboBox1.SelectedIndex == 1 && comboBox2.SelectedIndex == 2)
            {
                dele = new DelegateCoordsTransform(ca.GCJ02ToBD09);
            }

            if (files == null || files.Length == 0)
            {
                MessageBox.Show("请选择要处理的数据");
                this.button1.Focus();
                return;
            }
            if (comboBox1.SelectedIndex == comboBox2.SelectedIndex)
            {
                MessageBox.Show("目标坐标系和原始坐标系相同，请重新选择");
                return;
            }
            if (String.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("请先选择保存数据目录");
                this.button2.Focus();
                return;
            }

            //初始化驱动信息
            //注册所有驱动
            Ogr.RegisterAll();
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            //为支持属性表字段支持中文
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");

            //开启日志
            Gdal.SetConfigOption("CPL_DEBUG", "ON");
            Gdal.SetConfigOption("CPL_LOG", System.IO.Directory.GetCurrentDirectory() + "\\gdallog.txt");
            //驱动名称
            string strDriverName = "ESRI Shapefile";
            OSGeo.OGR.Driver Tdriver = Ogr.GetDriverByName(strDriverName);
            if (Tdriver == null)
            {
                MessageBox.Show("获得驱动失败！程序退出");
                return;
            }
            //遍历某个图层
            this.progressBar1.Maximum = files.Length;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Value = 0;

            for (int i = 0; i < files.Length; i++)//遍历每个图层
            {
                string shp_file = files[i];
                string shp_dir = System.IO.Path.GetDirectoryName(shp_file);
                string shp_fileSaveName = System.IO.Path.GetFileNameWithoutExtension(shp_file); 
                //打开源数据
                DataSource dsS = Ogr.Open(shp_file,0);
                if (dsS == null)
                {
                    MessageBox.Show("打开" + shp_file + "文件失败", "提示");
                    this.progressBar1.Value++;
                    continue;
                }
                //创建目标数据
                string saveDir = this.textBox2.Text;
                
                DataSource dsT = Tdriver.CreateDataSource(saveDir,null);
                if (dsT == null)
                {
                    MessageBox.Show("创建" + saveDir+"\\"+ shp_fileSaveName + "文件失败");
                    this.progressBar1.Value++;

                    continue;
                }
                Layer layerS = dsS.GetLayerByIndex(0);
                if (layerS == null)
                {
                    MessageBox.Show("获得"+ shp_file +"图层失败");
                    this.progressBar1.Value++;
                    continue;
                }
                //string shp_fileSaveName_utf8 = TransferEncoding(Encoding.BigEndianUnicode,Encoding.UTF8,shp_fileSaveName);
                OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
                Layer LayerT = dsT.CreateLayer(shp_fileSaveName,layerS.GetSpatialRef(), layerS.GetGeomType(), null);//layerS.GetSpatialRef()
                if (LayerT == null)
                {
                    MessageBox.Show("创建" +  saveDir + "\\"+shp_fileSaveName +"图层失败！","提示");
                    this.progressBar1.Value++;
                    continue;
                }
                Feature pFeatureS, pFeatureT;
                Geometry geomS, geomT;

                long countFeature = layerS.GetFeatureCount(0);
                FeatureDefn fd = layerS.GetLayerDefn();
                //对目标图层进行属性定义
                for (int ii = 0; ii < fd.GetFieldCount(); ii++)
                {
                    LayerT.CreateField(fd.GetFieldDefn(ii), 1);
                    
                }
                fd.Dispose();
                this.progressBar2.Minimum = 0;
                this.progressBar2.Maximum = (int)countFeature;
                this.progressBar2.Value = 0;
                for (int k = 0; k < countFeature; k++)
                {
                    pFeatureS = layerS.GetFeature(k);
                    if (pFeatureS != null)
                    {
                        geomS = pFeatureS.GetGeometryRef();
                        geomT = GeometryCoordTransfrom2D(geomS, dele);
                        pFeatureT = pFeatureS.Clone();
                        pFeatureT.SetFrom(pFeatureS,1);
                        pFeatureT.SetGeometryDirectly(geomT);
                        LayerT.CreateFeature(pFeatureT);

                        geomS.Dispose();
                        geomT.Dispose();
                        pFeatureS.Dispose();
                        pFeatureT.Dispose();
                        
                    }
                    this.progressBar2.Value++;
                }
                layerS.Dispose();
                LayerT.Dispose();


                dsS.Dispose();
                dsT.Dispose();
                this.progressBar1.Value++;
            }

            MessageBox.Show("处理完毕","提示");


 
        }
    }
}

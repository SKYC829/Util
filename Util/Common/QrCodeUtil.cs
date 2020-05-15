using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;

namespace Util.Common
{
    /// <summary>
    /// 二维码，条形码帮助类
    /// <para>可以生产或读取二维码，条形码</para>
    /// </summary>
    public class QrCodeUtil
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="content">二维码的内容</param>
        /// <param name="logo">二维码图标，如果没有则不会生成</param>
        /// <param name="codeMargin">二维码边距</param>
        /// <param name="codeWidth">二维码宽度</param>
        /// <param name="codeHeight">二维码长度</param>
        /// <returns></returns>
        public static Bitmap GenerateQrCode(string content,Bitmap logo,int codeMargin=1,int codeWidth=250,int codeHeight = 250)
        {
            //声明二维码生成器
            MultiFormatWriter writer = new MultiFormatWriter();
            //设置生成器的部分参数
            Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>()
            {
                {EncodeHintType.CHARACTER_SET,"UTF-8" },//定义二维码字符串编码为UTF-8
                {EncodeHintType.ERROR_CORRECTION,ErrorCorrectionLevel.H },//好像是设置生成器的误差值
                {EncodeHintType.MARGIN,codeMargin },//设置二维码边距
                {EncodeHintType.DISABLE_ECI,true },//只有使用ISO-8859-1规范才可以不禁用ECI
            };
            //生成二维码
            BitMatrix matrix = writer.encode(content, BarcodeFormat.QR_CODE, codeWidth, codeHeight, hints);
            //声明二维码写入器，将二维码写入到bitmap里
            BarcodeWriter barWriter = new BarcodeWriter();
            Bitmap result = barWriter.Write(matrix);//如果没有特殊参数声明，直接用barWriter.Write(content)好像也是可以的
            //插入Logo
            if(logo != null)
            {
                //计算二维码尺寸
                // 第0位:左边距
                // 第1位:上边距
                // 第2位:二维码宽
                // 第3位:二维码高

                //获取二维码矩形区域
                int[] rectangle = matrix.getEnclosingRectangle();
                int logoWidth = Math.Min((int)(rectangle[2] / 3), logo.Width); //计算logo宽度，并且限制logo最小宽度不能小于二维码宽度的1/3
                int logoHeight = Math.Min((int)(rectangle[3] / 3), logo.Height); //计算logo高度，并且限制logo最小高度不能小于二维码高度的1/3
                int logoLeft = result.Width - logoWidth; //计算logo的左边距
                int logoTop = result.Height - logoHeight; //计算logo的上边距
                //新建一个bitmap用于合并二维码和logo
                Bitmap qrCode = new Bitmap(result.Width, result.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //声明一个GDI+绘图类用来绘制二维码
                using (Graphics graphics = Graphics.FromImage(qrCode))
                {
                    //设置图片的清晰度
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; //高清晰度
                    //设置图片抗锯齿
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//高品质的抗锯齿
                    //把二维码画上去作为底色
                    graphics.DrawImage(result, 0, 0, codeWidth, codeHeight); //左边距，上边距为0
                    //在二维码上面再覆盖一层logo
                    //绘制logo的矩形区域
                    graphics.FillRectangle(Brushes.White, logoLeft, logoTop, logoWidth, logoHeight); //矩形填充色为白色，既logo的背景色是白色。
                    //绘制logo
                    graphics.DrawImage(logo, logoLeft, logoTop, logoWidth, logoHeight);
                }
                //覆盖原没有logo的二维码
                result = qrCode;
            }
            return result;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="content">二维码的内容</param>
        /// <param name="logoPath">二维码图标，如果没有则不会生成</param>
        /// <returns></returns>
        public static Bitmap GenerateQrCode(string content,string logoPath)
        {
            Bitmap logo = null;
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                //如果logo存在于路径下，则读取logo，否则不创建logo
                logo = new Bitmap(logoPath);
            }
            return GenerateQrCode(content, logo);
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="content">二维码的内容</param>
        /// <returns></returns>
        public static Bitmap GenerateQrCode(string content)
        {
            return GenerateQrCode(content, logoPath:null);
        }

        /// <summary>
        /// 生成条形码
        /// </summary>
        /// <param name="content">条形码内容</param>
        /// <param name="codeMargin">条码边距</param>
        /// <param name="codeWidth">条码宽度</param>
        /// <param name="codeHeight">条码高度</param>
        /// <returns></returns>
        public static Bitmap GenerateBarCode(string content,int codeMargin=1,int codeWidth=100,int codeHeight = 40)
        {
            //声明条码生成器
            BarcodeWriter writer = new BarcodeWriter();
            //设置条码格式
            writer.Format = BarcodeFormat.CODE_128; //只有Code_128格式能同时被支付宝和微信识别
            //设置宽和高以及边距的参数
            writer.Options = new EncodingOptions()
            {
                Height = codeHeight,
                Width = codeWidth,
                Margin = codeMargin
            };
            //生成条码
            Bitmap result = writer.Write(content);
            return result;
        }

        /// <summary>
        /// 读取二维码，条形码
        /// </summary>
        /// <param name="codeImage">二维码，条形码图片</param>
        /// <returns></returns>
        public static string ReadCode(Bitmap codeImage)
        {
            if(codeImage == null)
            {
                return string.Empty;
            }
            //声明解码器
            BarcodeReader reader = new BarcodeReader();
            //设置解码器的参数
            reader.Options = new DecodingOptions()
            {
                CharacterSet = "UTF-8",//设置字符串编码为UTF-8
            };
            //对图片进行解码并获取图片上的码的信息
            Result result = reader.Decode(codeImage);
            return result.Text;
        }

        /// <summary>
        /// 读取二维码，条形码
        /// </summary>
        /// <param name="codePath">二维码，条形码路径</param>
        /// <returns></returns>
        public static string ReadCode(string codePath)
        {
            if(string.IsNullOrEmpty(codePath) || !File.Exists(codePath))
            {
                return string.Empty;
            }
            //将图片读取出来
            Bitmap codeImage = new Bitmap(codePath);
            return ReadCode(codeImage: codeImage);
        }
    }
}

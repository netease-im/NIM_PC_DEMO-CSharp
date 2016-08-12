using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NIMDemo.LivingStreamSDK
{
	class YUVHelper
	{
		private static int A = 0;
		private static int R = 3;
		private static int G = 2;
		private static int B = 1;
// 		public static int ARGBToI420( byte[] src_argb,int src_stride_argb,
// 									 byte[] dst_y,int dst_stride_y,
// 									 byte[] dst_u,int dst_stride_u,
// 									 byte[] dst_v,int dst_stride_v,
// 									 int width,int height)
// 		{
// 			int index_src = 0;
// 			int index_y = 0;
// 			int index_u = 0;
// 			int index_v = 0;
// 			int y;
// 			if(src_argb!=null||dst_y!=null||dst_u!=null||dst_v!=null||width<=0||height==0)
// 			{
// 				return -1;
// 			}
// 			if(height<0)
// 			{
// 				height = -height;
// 
// 				index_src = (height - 1) * src_stride_argb;
// 				src_stride_argb = -src_stride_argb;
// 			}
// 
// 			for(y=0;y<height-1;y+=2)
// 			{
// 				ARGBToUVRow(src_argb, src_stride_argb, dst_u, dst_v, width, index_src,index_y,index_u,index_v);
// 				ARGBToYRow(src_argb, dst_y, width,0,0);
// 				ARGBToYRow(src_argb , dst_y, width, index_src + src_stride_argb,index_y+dst_stride_y);
// 				index_src += src_stride_argb * 2;
// 				index_y += dst_stride_y * 2;
// 				index_u += dst_stride_u;
// 				index_v += dst_stride_v;
// 			}
// 
// 			if(Convert.ToBoolean(height&1))
// 			{
// 				ARGBToUVRow(src_argb, 0, dst_u, dst_v, width,index_src,index_y,index_u,index_v);
// 				ARGBToYRow(src_argb, dst_y, width,index_src,index_y);
// 			}
// 			return 0;
// 		}



		public static byte[] I420ToRGB(byte[] src,int width,int height)
		{
			int numOfPixel = width * height;
			int positionOfV = numOfPixel;
			int positionOfU = numOfPixel / 4 + numOfPixel;
			byte[] rgb = new byte[numOfPixel * 4];

			for(int i=0;i<height;i++)
			{
				int startY = i * width;
				int step = (i / 2) * (width / 2);
				int startV = positionOfV + step;
				int startU = positionOfU + step;
				for(int j=0;j<width;j++)
				{
					int Y = startY + j;
					int V = startV + j / 2;
					int U = startU + j / 2;
					int index = Y * 4;
					RGB tmp = yuvTorgb(src[Y], src[U], src[V]);
					rgb[index + R] = tmp.r;
					rgb[index + G] = tmp.g;
					rgb[index + B] = tmp.b;
				}
			}
			rgb = rgb.Reverse().ToArray();
			return rgb;
		}

		private  class RGB
		{
			public byte r;
			public byte g;
			public byte b;
		}

		private static RGB yuvTorgb(byte Y, byte U, byte V)
		{
			RGB rgb = new RGB();
			int r = (int)((Y & 0xff) + 1.4075 * ((V & 0xff) - 128));
			int g = (int)((Y & 0xff) - 0.3455 * ((U & 0xff) - 128) - 0.7169 * ((V & 0xff) - 128));
			int b = (int)((Y & 0xff) + 1.779 * ((U & 0xff) - 128));
			rgb.r = Convert.ToByte((r < 0 ? 0 : r > 255 ? 255 : r));
			rgb.g = Convert.ToByte((g < 0 ? 0 : g > 255 ? 255 : g));
			rgb.b = Convert.ToByte((b < 0 ? 0 : b > 255 ? 255 : b));
			return rgb;
		}


		//Y = (0.257 * R) + (0.504 * G) + (0.098 * B) + 16
		//Cr = V = (0.439 * R) - (0.368 * G) - (0.071 * B) + 128
		//Cb = U = -(0.148 * R) - (0.291 * G) + (0.439 * B) + 128

		private static void ARGBToUVRow(byte[] src_argb, int src_stride_argb, byte[] dst_u, byte[] dst_v, int width, int index_src, int index_y, int index_u, int index_v)
		{
			int index = 0;
			int index_dst_uv = 0;
			for (; index < width*2; index += 8)
			{
				dst_v[index_v+ index_dst_uv] = Convert.ToByte(src_argb[index_src + index + 1] * 0.439 - 0.368 * src_argb[index_src + index + 2] - src_argb[index_src + index + 3] * 0.071 + 128);
				dst_u[index_u+ index_dst_uv] = Convert.ToByte(-src_argb[index_src + index + 1] * 0.148 - 0.291 * src_argb[index_src + index + 2] + src_argb[index_src + index + 3] * 0.439 + 128);
				index_dst_uv += 1;
			}
		}
		private static void ARGBToYRow(byte[] src_argb, byte[] dst_y, int width, int index_src, int index_y)
		{
			int index = 0;
			int index_dst_y = 0;
			for (; index < width; index++)
			{
				dst_y[index_y+index_dst_y] = Convert.ToByte(src_argb[index_src + index + 1] * 0.257 + 0.504 * src_argb[index_src + index + 2] + src_argb[index_src + index + 3] * 0.098 + 16);
				index_dst_y += 1;
			}
		}


	}
}

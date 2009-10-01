/*
* CVS identifier:
*
* $Id: HeaderInfo.java,v 1.3 2001/10/26 16:30:33 grosbois Exp $
*
* Class:                   HeaderInfo
*
* Description:             Holds information found in main and tile-part
*                          headers 
*
*
*
* COPYRIGHT:
* 
* This software module was originally developed by Raphaël Grosbois and
* Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
* Askelöf (Ericsson Radio Systems AB); and Bertrand Berthelot, David
* Bouchard, Félix Henry, Gerard Mozelle and Patrice Onno (Canon Research
* Centre France S.A) in the course of development of the JPEG2000
* standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
* software module is an implementation of a part of the JPEG 2000
* Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
* Systems AB and Canon Research Centre France S.A (collectively JJ2000
* Partners) agree not to assert against ISO/IEC and users of the JPEG
* 2000 Standard (Users) any of their rights under the copyright, not
* including other intellectual property rights, for this software module
* with respect to the usage by ISO/IEC and Users of this software module
* or modifications thereof for use in hardware or software products
* claiming conformance to the JPEG 2000 Standard. Those intending to use
* this software module in hardware or software products are advised that
* their use may infringe existing patents. The original developers of
* this software module, JJ2000 Partners and ISO/IEC assume no liability
* for use of this software module or modifications thereof. No license
* or right to this software module is granted for non JPEG 2000 Standard
* conforming products. JJ2000 Partners have full right to use this
* software module for his/her own purpose, assign or donate this
* software module to any third party and to inhibit third parties from
* using this software module for non JPEG 2000 Standard conforming
* products. This copyright notice must be included in all copies or
* derivative works of this software module.
* 
* Copyright (c) 1999/2000 JJ2000 Partners.
* */
using System;
using CSJ2K.j2k.wavelet;
namespace CSJ2K.j2k.codestream
{
	
	/// <summary> Classe that holds information found in the marker segments of the main and
	/// tile-part headers. There is one inner-class per marker segment type found
	/// in these headers.
	/// 
	/// </summary>
	public class HeaderInfo : FilterTypes, System.ICloneable
	{
		/// <summary>Returns a new instance of SIZ </summary>
		virtual public SIZ NewSIZ
		{
			get
			{
				return new SIZ(this);
			}
			
		}
		/// <summary>Returns a new instance of SOT </summary>
		virtual public SOT NewSOT
		{
			get
			{
				return new SOT(this);
			}
			
		}
		/// <summary>Returns a new instance of COD </summary>
		virtual public COD NewCOD
		{
			get
			{
				return new COD(this);
			}
			
		}
		/// <summary>Returns a new instance of COC </summary>
		virtual public COC NewCOC
		{
			get
			{
				return new COC(this);
			}
			
		}
		/// <summary>Returns a new instance of RGN </summary>
		virtual public RGN NewRGN
		{
			get
			{
				return new RGN(this);
			}
			
		}
		/// <summary>Returns a new instance of QCD </summary>
		virtual public QCD NewQCD
		{
			get
			{
				return new QCD(this);
			}
			
		}
		/// <summary>Returns a new instance of QCC </summary>
		virtual public QCC NewQCC
		{
			get
			{
				return new QCC(this);
			}
			
		}
		/// <summary>Returns a new instance of POC </summary>
		virtual public POC NewPOC
		{
			get
			{
				return new POC(this);
			}
			
		}
		/// <summary>Returns a new instance of CRG </summary>
		virtual public CRG NewCRG
		{
			get
			{
				return new CRG(this);
			}
			
		}
		/// <summary>Returns a new instance of COM </summary>
		virtual public COM NewCOM
		{
			get
			{
				ncom++; return new COM(this);
			}
			
		}
		/// <summary>Returns the number of found COM marker segments </summary>
		virtual public int NumCOM
		{
			get
			{
				return ncom;
			}
			
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'SIZ' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the SIZ marker segment </summary>
		public class SIZ : System.ICloneable
		{
			public SIZ(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			virtual public int MaxCompWidth
			{
				get
				{
					if (compWidth == null)
					{
						compWidth = new int[csiz];
						for (int cc = 0; cc < csiz; cc++)
						{
							//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
							compWidth[cc] = (int) (System.Math.Ceiling((xsiz) / (double) xrsiz[cc]) - System.Math.Ceiling(x0siz / (double) xrsiz[cc]));
						}
					}
					if (maxCompWidth == - 1)
					{
						for (int c = 0; c < csiz; c++)
						{
							if (compWidth[c] > maxCompWidth)
							{
								maxCompWidth = compWidth[c];
							}
						}
					}
					return maxCompWidth;
				}
				
			}
			virtual public int MaxCompHeight
			{
				get
				{
					if (compHeight == null)
					{
						compHeight = new int[csiz];
						for (int cc = 0; cc < csiz; cc++)
						{
							//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
							compHeight[cc] = (int) (System.Math.Ceiling((ysiz) / (double) yrsiz[cc]) - System.Math.Ceiling(y0siz / (double) yrsiz[cc]));
						}
					}
					if (maxCompHeight == - 1)
					{
						for (int c = 0; c < csiz; c++)
						{
							if (compHeight[c] != maxCompHeight)
							{
								maxCompHeight = compHeight[c];
							}
						}
					}
					return maxCompHeight;
				}
				
			}
			virtual public int NumTiles
			{
				get
				{
					if (numTiles == - 1)
					{
						numTiles = ((xsiz - xt0siz + xtsiz - 1) / xtsiz) * ((ysiz - yt0siz + ytsiz - 1) / ytsiz);
					}
					return numTiles;
				}
				
			}
			virtual public SIZ Copy
			{
				get
				{
					SIZ ms = null;
					try
					{
						ms = (SIZ) this.Clone();
					}
					//UPGRADE_NOTE: Exception 'java.lang.CloneNotSupportedException' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
					catch (System.Exception)
					{
						throw new System.ApplicationException("Cannot clone SIZ marker segment");
					}
					return ms;
				}
				
			}
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lsiz;
			public int rsiz;
			public int xsiz;
			public int ysiz;
			public int x0siz;
			public int y0siz;
			public int xtsiz;
			public int ytsiz;
			public int xt0siz;
			public int yt0siz;
			public int csiz;
			public int[] ssiz;
			public int[] xrsiz;
			public int[] yrsiz;
			
			/// <summary>Component widths </summary>
			private int[] compWidth = null;
			/// <summary>Maximum width among all components </summary>
			private int maxCompWidth = - 1;
			/// <summary>Component heights </summary>
			private int[] compHeight = null;
			/// <summary>Maximum height among all components </summary>
			private int maxCompHeight = - 1;
			/// <summary> Width of the specified tile-component
			/// 
			/// </summary>
			/// <param name="t">Tile index
			/// 
			/// </param>
			/// <param name="c">Component index
			/// 
			/// </param>
			public virtual int getCompImgWidth(int c)
			{
				if (compWidth == null)
				{
					compWidth = new int[csiz];
					for (int cc = 0; cc < csiz; cc++)
					{
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						compWidth[cc] = (int) (System.Math.Ceiling((xsiz) / (double) xrsiz[cc]) - System.Math.Ceiling(x0siz / (double) xrsiz[cc]));
					}
				}
				return compWidth[c];
			}
			public virtual int getCompImgHeight(int c)
			{
				if (compHeight == null)
				{
					compHeight = new int[csiz];
					for (int cc = 0; cc < csiz; cc++)
					{
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						compHeight[cc] = (int) (System.Math.Ceiling((ysiz) / (double) yrsiz[cc]) - System.Math.Ceiling(y0siz / (double) yrsiz[cc]));
					}
				}
				return compHeight[c];
			}
			private int numTiles = - 1;
			private bool[] origSigned = null;
			public virtual bool isOrigSigned(int c)
			{
				if (origSigned == null)
				{
					origSigned = new bool[csiz];
					for (int cc = 0; cc < csiz; cc++)
					{
						origSigned[cc] = ((SupportClass.URShift(ssiz[cc], CSJ2K.j2k.codestream.Markers.SSIZ_DEPTH_BITS)) == 1);
					}
				}
				return origSigned[c];
			}
			private int[] origBitDepth = null;
			public virtual int getOrigBitDepth(int c)
			{
				if (origBitDepth == null)
				{
					origBitDepth = new int[csiz];
					for (int cc = 0; cc < csiz; cc++)
					{
						origBitDepth[cc] = (ssiz[cc] & ((1 << CSJ2K.j2k.codestream.Markers.SSIZ_DEPTH_BITS) - 1)) + 1;
					}
				}
				return origBitDepth[c];
			}
			
			/// <summary>Display information found in SIZ marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- SIZ (" + lsiz + " bytes) ---\n";
				str += (" Capabilities : " + rsiz + "\n");
				str += (" Image dim.   : " + (xsiz - x0siz) + "x" + (ysiz - y0siz) + ", (off=" + x0siz + "," + y0siz + ")\n");
				str += (" Tile dim.    : " + xtsiz + "x" + ytsiz + ", (off=" + xt0siz + "," + yt0siz + ")\n");
				str += (" Component(s) : " + csiz + "\n");
				str += " Orig. depth  : ";
				for (int i = 0; i < csiz; i++)
				{
					str += (getOrigBitDepth(i) + " ");
				}
				str += "\n";
				str += " Orig. signed : ";
				for (int i = 0; i < csiz; i++)
				{
					str += (isOrigSigned(i) + " ");
				}
				str += "\n";
				str += " Subs. factor : ";
				for (int i = 0; i < csiz; i++)
				{
					str += (xrsiz[i] + "," + yrsiz[i] + " ");
				}
				str += "\n";
				return str;
			}
			//UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1232'"
			virtual public System.Object Clone()
			{
				return null;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'SOT' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the SOt marker segments </summary>
		public class SOT
		{
			public SOT(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lsot;
			public int isot;
			public int psot;
			public int tpsot;
			public int tnsot;
			
			/// <summary>Display information found in this SOT marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- SOT (" + lsot + " bytes) ---\n";
				str += ("Tile index         : " + isot + "\n");
				str += ("Tile-part length   : " + psot + " bytes\n");
				str += ("Tile-part index    : " + tpsot + "\n");
				str += ("Num. of tile-parts : " + tnsot + "\n");
				str += "\n";
				return str;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'COD' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the COD marker segments </summary>
		public class COD : System.ICloneable
		{
			public COD(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			virtual public COD Copy
			{
				get
				{
					COD ms = null;
					try
					{
						ms = (COD) this.Clone();
					}
					//UPGRADE_NOTE: Exception 'java.lang.CloneNotSupportedException' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
					catch (System.Exception)
					{
						throw new System.ApplicationException("Cannot clone SIZ marker segment");
					}
					return ms;
				}
				
			}
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lcod;
			public int scod;
			public int sgcod_po; // Progression order
			public int sgcod_nl; // Number of layers
			public int sgcod_mct; // Multiple component transformation
			public int spcod_ndl; // Number of decomposition levels
			public int spcod_cw; // Code-blocks width
			public int spcod_ch; // Code-blocks height
			public int spcod_cs; // Code-blocks style
			public int[] spcod_t = new int[1]; // Transformation
			public int[] spcod_ps; // Precinct size
			/// <summary>Display information found in this COD marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- COD (" + lcod + " bytes) ---\n";
				str += " Coding style   : ";
				if (scod == 0)
				{
					str += "Default";
				}
				else
				{
					if ((scod & CSJ2K.j2k.codestream.Markers.SCOX_PRECINCT_PARTITION) != 0)
						str += "Precints ";
					if ((scod & CSJ2K.j2k.codestream.Markers.SCOX_USE_SOP) != 0)
						str += "SOP ";
					if ((scod & CSJ2K.j2k.codestream.Markers.SCOX_USE_EPH) != 0)
						str += "EPH ";
					int cb0x = ((scod & CSJ2K.j2k.codestream.Markers.SCOX_HOR_CB_PART) != 0)?1:0;
					int cb0y = ((scod & CSJ2K.j2k.codestream.Markers.SCOX_VER_CB_PART) != 0)?1:0;
					if (cb0x != 0 || cb0y != 0)
					{
						str += "Code-blocks offset";
						str += ("\n Cblk partition : " + cb0x + "," + cb0y);
					}
				}
				str += "\n";
				str += " Cblk style     : ";
				if (spcod_cs == 0)
				{
					str += "Default";
				}
				else
				{
					if ((spcod_cs & 0x1) != 0)
						str += "Bypass ";
					if ((spcod_cs & 0x2) != 0)
						str += "Reset ";
					if ((spcod_cs & 0x4) != 0)
						str += "Terminate ";
					if ((spcod_cs & 0x8) != 0)
						str += "Vert_causal ";
					if ((spcod_cs & 0x10) != 0)
						str += "Predict ";
					if ((spcod_cs & 0x20) != 0)
						str += "Seg_symb ";
				}
				str += "\n";
				str += (" Num. of levels : " + spcod_ndl + "\n");
				switch (sgcod_po)
				{
					
					case CSJ2K.j2k.codestream.ProgressionType.LY_RES_COMP_POS_PROG: 
						str += " Progress. type : LY_RES_COMP_POS_PROG\n";
						break;
					
					case CSJ2K.j2k.codestream.ProgressionType.RES_LY_COMP_POS_PROG: 
						str += " Progress. type : RES_LY_COMP_POS_PROG\n";
						break;
					
					case CSJ2K.j2k.codestream.ProgressionType.RES_POS_COMP_LY_PROG: 
						str += " Progress. type : RES_POS_COMP_LY_PROG\n";
						break;
					
					case CSJ2K.j2k.codestream.ProgressionType.POS_COMP_RES_LY_PROG: 
						str += " Progress. type : POS_COMP_RES_LY_PROG\n";
						break;
					
					case CSJ2K.j2k.codestream.ProgressionType.COMP_POS_RES_LY_PROG: 
						str += " Progress. type : COMP_POS_RES_LY_PROG\n";
						break;
					}
				str += (" Num. of layers : " + sgcod_nl + "\n");
				str += (" Cblk dimension : " + (1 << (spcod_cw + 2)) + "x" + (1 << (spcod_ch + 2)) + "\n");
				switch (spcod_t[0])
				{
					
					case CSJ2K.j2k.wavelet.FilterTypes_Fields.W9X7: 
						str += " Filter         : 9-7 irreversible\n";
						break;
					
					case CSJ2K.j2k.wavelet.FilterTypes_Fields.W5X3: 
						str += " Filter         : 5-3 reversible\n";
						break;
					}
				str += (" Multi comp tr. : " + (sgcod_mct == 1) + "\n");
				if (spcod_ps != null)
				{
					str += " Precincts      : ";
					for (int i = 0; i < spcod_ps.Length; i++)
					{
						str += ((1 << (spcod_ps[i] & 0x000F)) + "x" + (1 << (((spcod_ps[i] & 0x00F0) >> 4))) + " ");
					}
				}
				str += "\n";
				return str;
			}
			//UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1232'"
			virtual public System.Object Clone()
			{
				return null;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'COC' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the COC marker segments </summary>
		public class COC
		{
			public COC(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lcoc;
			public int ccoc;
			public int scoc;
			public int spcoc_ndl; // Number of decomposition levels
			public int spcoc_cw;
			public int spcoc_ch;
			public int spcoc_cs;
			public int[] spcoc_t = new int[1];
			public int[] spcoc_ps;
			/// <summary>Display information found in this COC marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- COC (" + lcoc + " bytes) ---\n";
				str += (" Component      : " + ccoc + "\n");
				str += " Coding style   : ";
				if (scoc == 0)
				{
					str += "Default";
				}
				else
				{
					if ((scoc & 0x1) != 0)
						str += "Precints ";
					if ((scoc & 0x2) != 0)
						str += "SOP ";
					if ((scoc & 0x4) != 0)
						str += "EPH ";
				}
				str += "\n";
				str += " Cblk style     : ";
				if (spcoc_cs == 0)
				{
					str += "Default";
				}
				else
				{
					if ((spcoc_cs & 0x1) != 0)
						str += "Bypass ";
					if ((spcoc_cs & 0x2) != 0)
						str += "Reset ";
					if ((spcoc_cs & 0x4) != 0)
						str += "Terminate ";
					if ((spcoc_cs & 0x8) != 0)
						str += "Vert_causal ";
					if ((spcoc_cs & 0x10) != 0)
						str += "Predict ";
					if ((spcoc_cs & 0x20) != 0)
						str += "Seg_symb ";
				}
				str += "\n";
				str += (" Num. of levels : " + spcoc_ndl + "\n");
				str += (" Cblk dimension : " + (1 << (spcoc_cw + 2)) + "x" + (1 << (spcoc_ch + 2)) + "\n");
				switch (spcoc_t[0])
				{
					
					case CSJ2K.j2k.wavelet.FilterTypes_Fields.W9X7: 
						str += " Filter         : 9-7 irreversible\n";
						break;
					
					case CSJ2K.j2k.wavelet.FilterTypes_Fields.W5X3: 
						str += " Filter         : 5-3 reversible\n";
						break;
					}
				if (spcoc_ps != null)
				{
					str += " Precincts      : ";
					for (int i = 0; i < spcoc_ps.Length; i++)
					{
						str += ((1 << (spcoc_ps[i] & 0x000F)) + "x" + (1 << (((spcoc_ps[i] & 0x00F0) >> 4))) + " ");
					}
				}
				str += "\n";
				return str;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'RGN' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the RGN marker segments </summary>
		public class RGN
		{
			public RGN(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lrgn;
			public int crgn;
			public int srgn;
			public int sprgn;
			/// <summary>Display information found in this RGN marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- RGN (" + lrgn + " bytes) ---\n";
				str += (" Component : " + crgn + "\n");
				if (srgn == 0)
				{
					str += " ROI style : Implicit\n";
				}
				else
				{
					str += " ROI style : Unsupported\n";
				}
				str += (" ROI shift : " + sprgn + "\n");
				str += "\n";
				return str;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'QCD' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the QCD marker segments </summary>
		public class QCD
		{
			public QCD(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			virtual public int QuantType
			{
				get
				{
					if (qType == - 1)
					{
						qType = sqcd & ~ (CSJ2K.j2k.codestream.Markers.SQCX_GB_MSK << CSJ2K.j2k.codestream.Markers.SQCX_GB_SHIFT);
					}
					return qType;
				}
				
			}
			virtual public int NumGuardBits
			{
				get
				{
					if (gb == - 1)
					{
						gb = (sqcd >> CSJ2K.j2k.codestream.Markers.SQCX_GB_SHIFT) & CSJ2K.j2k.codestream.Markers.SQCX_GB_MSK;
					}
					return gb;
				}
				
			}
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lqcd;
			public int sqcd;
			public int[][] spqcd;
			
			private int qType = - 1;
			private int gb = - 1;
			
			/// <summary>Display information found in this QCD marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- QCD (" + lqcd + " bytes) ---\n";
				str += " Quant. type    : ";
				int qt = QuantType;
				if (qt == CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION)
					str += "No quantization \n";
				else if (qt == CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED)
					str += "Scalar derived\n";
				else if (qt == CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED)
					str += "Scalar expounded\n";
				str += (" Guard bits     : " + NumGuardBits + "\n");
				if (qt == CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION)
				{
					str += " Exponents   :\n";
					int exp;
					for (int i = 0; i < spqcd.Length; i++)
					{
						for (int j = 0; j < spqcd[i].Length; j++)
						{
							if (i == 0 && j == 0)
							{
								exp = (spqcd[0][0] >> CSJ2K.j2k.codestream.Markers.SQCX_EXP_SHIFT) & CSJ2K.j2k.codestream.Markers.SQCX_EXP_MASK;
								str += ("\tr=0 : " + exp + "\n");
							}
							else if (i != 0 && j > 0)
							{
								exp = (spqcd[i][j] >> CSJ2K.j2k.codestream.Markers.SQCX_EXP_SHIFT) & CSJ2K.j2k.codestream.Markers.SQCX_EXP_MASK;
								str += ("\tr=" + i + ",s=" + j + " : " + exp + "\n");
							}
						}
					}
				}
				else
				{
					str += " Exp / Mantissa : \n";
					int exp;
					double mantissa;
					for (int i = 0; i < spqcd.Length; i++)
					{
						for (int j = 0; j < spqcd[i].Length; j++)
						{
							if (i == 0 && j == 0)
							{
								exp = (spqcd[0][0] >> 11) & 0x1f;
								//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
								mantissa = (- 1f - ((float) (spqcd[0][0] & 0x07ff)) / (1 << 11)) / (- 1 << exp);
								str += ("\tr=0 : " + exp + " / " + mantissa + "\n");
							}
							else if (i != 0 && j > 0)
							{
								exp = (spqcd[i][j] >> 11) & 0x1f;
								//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
								mantissa = (- 1f - ((float) (spqcd[i][j] & 0x07ff)) / (1 << 11)) / (- 1 << exp);
								str += ("\tr=" + i + ",s=" + j + " : " + exp + " / " + mantissa + "\n");
							}
						}
					}
				}
				str += "\n";
				return str;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'QCC' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the QCC marker segments </summary>
		public class QCC
		{
			public QCC(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			virtual public int QuantType
			{
				get
				{
					if (qType == - 1)
					{
						qType = sqcc & ~ (CSJ2K.j2k.codestream.Markers.SQCX_GB_MSK << CSJ2K.j2k.codestream.Markers.SQCX_GB_SHIFT);
					}
					return qType;
				}
				
			}
			virtual public int NumGuardBits
			{
				get
				{
					if (gb == - 1)
					{
						gb = (sqcc >> CSJ2K.j2k.codestream.Markers.SQCX_GB_SHIFT) & CSJ2K.j2k.codestream.Markers.SQCX_GB_MSK;
					}
					return gb;
				}
				
			}
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lqcc;
			public int cqcc;
			public int sqcc;
			public int[][] spqcc;
			
			private int qType = - 1;
			private int gb = - 1;
			
			/// <summary>Display information found in this QCC marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- QCC (" + lqcc + " bytes) ---\n";
				str += (" Component      : " + cqcc + "\n");
				str += " Quant. type    : ";
				int qt = QuantType;
				if (qt == CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION)
					str += "No quantization \n";
				else if (qt == CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED)
					str += "Scalar derived\n";
				else if (qt == CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED)
					str += "Scalar expounded\n";
				str += (" Guard bits     : " + NumGuardBits + "\n");
				if (qt == CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION)
				{
					str += " Exponents   :\n";
					int exp;
					for (int i = 0; i < spqcc.Length; i++)
					{
						for (int j = 0; j < spqcc[i].Length; j++)
						{
							if (i == 0 && j == 0)
							{
								exp = (spqcc[0][0] >> CSJ2K.j2k.codestream.Markers.SQCX_EXP_SHIFT) & CSJ2K.j2k.codestream.Markers.SQCX_EXP_MASK;
								str += ("\tr=0 : " + exp + "\n");
							}
							else if (i != 0 && j > 0)
							{
								exp = (spqcc[i][j] >> CSJ2K.j2k.codestream.Markers.SQCX_EXP_SHIFT) & CSJ2K.j2k.codestream.Markers.SQCX_EXP_MASK;
								str += ("\tr=" + i + ",s=" + j + " : " + exp + "\n");
							}
						}
					}
				}
				else
				{
					str += " Exp / Mantissa : \n";
					int exp;
					double mantissa;
					for (int i = 0; i < spqcc.Length; i++)
					{
						for (int j = 0; j < spqcc[i].Length; j++)
						{
							if (i == 0 && j == 0)
							{
								exp = (spqcc[0][0] >> 11) & 0x1f;
								//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
								mantissa = (- 1f - ((float) (spqcc[0][0] & 0x07ff)) / (1 << 11)) / (- 1 << exp);
								str += ("\tr=0 : " + exp + " / " + mantissa + "\n");
							}
							else if (i != 0 && j > 0)
							{
								exp = (spqcc[i][j] >> 11) & 0x1f;
								//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
								mantissa = (- 1f - ((float) (spqcc[i][j] & 0x07ff)) / (1 << 11)) / (- 1 << exp);
								str += ("\tr=" + i + ",s=" + j + " : " + exp + " / " + mantissa + "\n");
							}
						}
					}
				}
				str += "\n";
				return str;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'POC' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the POC marker segments </summary>
		public class POC
		{
			public POC(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lpoc;
			public int[] rspoc;
			public int[] cspoc;
			public int[] lyepoc;
			public int[] repoc;
			public int[] cepoc;
			public int[] ppoc;
			/// <summary>Display information found in this POC marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- POC (" + lpoc + " bytes) ---\n";
				str += " Chg_idx RSpoc CSpoc LYEpoc REpoc CEpoc Ppoc\n";
				for (int chg = 0; chg < rspoc.Length; chg++)
				{
					str += ("   " + chg + "      " + rspoc[chg] + "     " + cspoc[chg] + "     " + lyepoc[chg] + "      " + repoc[chg] + "     " + cepoc[chg]);
					switch (ppoc[chg])
					{
						
						case CSJ2K.j2k.codestream.ProgressionType.LY_RES_COMP_POS_PROG: 
							str += "  LY_RES_COMP_POS_PROG\n";
							break;
						
						case CSJ2K.j2k.codestream.ProgressionType.RES_LY_COMP_POS_PROG: 
							str += "  RES_LY_COMP_POS_PROG\n";
							break;
						
						case CSJ2K.j2k.codestream.ProgressionType.RES_POS_COMP_LY_PROG: 
							str += "  RES_POS_COMP_LY_PROG\n";
							break;
						
						case CSJ2K.j2k.codestream.ProgressionType.POS_COMP_RES_LY_PROG: 
							str += "  POS_COMP_RES_LY_PROG\n";
							break;
						
						case CSJ2K.j2k.codestream.ProgressionType.COMP_POS_RES_LY_PROG: 
							str += "  COMP_POS_RES_LY_PROG\n";
							break;
						}
				}
				str += "\n";
				return str;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'CRG' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the CRG marker segment </summary>
		public class CRG
		{
			public CRG(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lcrg;
			public int[] xcrg;
			public int[] ycrg;
			/// <summary>Display information found in the CRG marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- CRG (" + lcrg + " bytes) ---\n";
				for (int c = 0; c < xcrg.Length; c++)
				{
					str += (" Component " + c + " offset : " + xcrg[c] + "," + ycrg[c] + "\n");
				}
				str += "\n";
				return str;
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'COM' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary>Internal class holding information found in the COM marker segments </summary>
		public class COM
		{
			public COM(HeaderInfo enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(HeaderInfo enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private HeaderInfo enclosingInstance;
			public HeaderInfo Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public int lcom;
			public int rcom;
			public byte[] ccom;
			/// <summary>Display information found in the COM marker segment </summary>
			public override System.String ToString()
			{
				System.String str = "\n --- COM (" + lcom + " bytes) ---\n";
				if (rcom == 0)
				{
					str += " Registration : General use (binary values)\n";
				}
				else if (rcom == 1)
				{
					str += (" Registration : General use (IS 8859-15:1999 " + "(Latin) values)\n");
					str += (" Text         : " + System.Text.ASCIIEncoding.ASCII.GetString(ccom) + "\n");
				}
				else
				{
					str += " Registration : Unknown\n";
				}
				str += "\n";
				return str;
			}
		}
		
		/// <summary>Reference to the SIZ marker segment found in main header </summary>
		public SIZ sizValue;
		
		/// <summary>Reference to the SOT marker segments found in tile-part headers. The
		/// kwy is given by "t"+tileIdx"_tp"+tilepartIndex. 
		/// </summary>
		public System.Collections.Hashtable sotValue = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Reference to the COD marker segments found in main and first tile-part
		/// header. The key is either "main" or "t"+tileIdx.
		/// </summary>
		public System.Collections.Hashtable codValue = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Reference to the COC marker segments found in main and first tile-part
		/// header. The key is either "main_c"+componentIndex or
		/// "t"+tileIdx+"_c"+component_index. 
		/// </summary>
		public System.Collections.Hashtable cocValue = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Reference to the RGN marker segments found in main and first tile-part
		/// header. The key is either "main_c"+componentIndex or
		/// "t"+tileIdx+"_c"+component_index. 
		/// </summary>
		public System.Collections.Hashtable rgnValue = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Reference to the QCD marker segments found in main and first tile-part
		/// header. The key is either "main" or "t"+tileIdx. 
		/// </summary>
		public System.Collections.Hashtable qcdValue = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Reference to the QCC marker segments found in main and first tile-part
		/// header. They key is either "main_c"+componentIndex or
		/// "t"+tileIdx+"_c"+component_index. 
		/// </summary>
		public System.Collections.Hashtable qccValue = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Reference to the POC marker segments found in main and first tile-part
		/// header. They key is either "main" or "t"+tileIdx. 
		/// </summary>
		public System.Collections.Hashtable pocValue = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Reference to the CRG marker segment found in main header </summary>
		public CRG crgValue;
		
		/// <summary>Reference to the COM marker segments found in main and tile-part
		/// headers. The key is either "main_"+comIdx or "t"+tileIdx+"_"+comIdx. 
		/// </summary>
		public System.Collections.Hashtable comValue = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>Number of found COM marker segment </summary>
		private int ncom = 0;
		
		/// <summary>Display information found in the different marker segments of the main
		/// header 
		/// </summary>
		public virtual System.String toStringMainHeader()
		{
			int nc = sizValue.csiz;
			// SIZ
			System.String str = "" + sizValue;
			// COD
			if (codValue["main"] != null)
			{
				str += ("" + ((COD) codValue["main"]));
			}
			// COCs
			for (int c = 0; c < nc; c++)
			{
				if (cocValue["main_c" + c] != null)
				{
					str += ("" + ((COC) cocValue["main_c" + c]));
				}
			}
			// QCD
			if (qcdValue["main"] != null)
			{
				str += ("" + ((QCD) qcdValue["main"]));
			}
			// QCCs
			for (int c = 0; c < nc; c++)
			{
				if (qccValue["main_c" + c] != null)
				{
					str += ("" + ((QCC) qccValue["main_c" + c]));
				}
			}
			// RGN
			for (int c = 0; c < nc; c++)
			{
				if (rgnValue["main_c" + c] != null)
				{
					str += ("" + ((RGN) rgnValue["main_c" + c]));
				}
			}
			// POC
			if (pocValue["main"] != null)
			{
				str += ("" + ((POC) pocValue["main"]));
			}
			// CRG
			if (crgValue != null)
			{
				str += ("" + crgValue);
			}
			// COM
			for (int i = 0; i < ncom; i++)
			{
				if (comValue["main_" + i] != null)
				{
					str += ("" + ((COM) comValue["main_" + i]));
				}
			}
			return str;
		}
		
		/// <summary> Returns information found in the tile-part headers of a given tile.
		/// 
		/// </summary>
		/// <param name="t">index of the tile
		/// 
		/// </param>
		/// <param name="tp">Number of tile-parts
		/// 
		/// </param>
		public virtual System.String toStringTileHeader(int t, int ntp)
		{
			int nc = sizValue.csiz;
			System.String str = "";
			// SOT
			for (int i = 0; i < ntp; i++)
			{
				str += ("Tile-part " + i + ", tile " + t + ":\n");
				str += ("" + ((SOT) sotValue["t" + t + "_tp" + i]));
			}
			// COD
			if (codValue["t" + t] != null)
			{
				str += ("" + ((COD) codValue["t" + t]));
			}
			// COCs
			for (int c = 0; c < nc; c++)
			{
				if (cocValue["t" + t + "_c" + c] != null)
				{
					str += ("" + ((COC) cocValue["t" + t + "_c" + c]));
				}
			}
			// QCD
			if (qcdValue["t" + t] != null)
			{
				str += ("" + ((QCD) qcdValue["t" + t]));
			}
			// QCCs
			for (int c = 0; c < nc; c++)
			{
				if (qccValue["t" + t + "_c" + c] != null)
				{
					str += ("" + ((QCC) qccValue["t" + t + "_c" + c]));
				}
			}
			// RGN
			for (int c = 0; c < nc; c++)
			{
				if (rgnValue["t" + t + "_c" + c] != null)
				{
					str += ("" + ((RGN) rgnValue["t" + t + "_c" + c]));
				}
			}
			// POC
			if (pocValue["t" + t] != null)
			{
				str += ("" + ((POC) pocValue["t" + t]));
			}
			return str;
		}
		
		/// <summary> Returns information found in the tile-part headers of a given tile
		/// exception the SOT marker segment.
		/// 
		/// </summary>
		/// <param name="t">index of the tile
		/// 
		/// </param>
		/// <param name="tp">Number of tile-parts
		/// 
		/// </param>
		public virtual System.String toStringThNoSOT(int t, int ntp)
		{
			int nc = sizValue.csiz;
			System.String str = "";
			// COD
			if (codValue["t" + t] != null)
			{
				str += ("" + ((COD) codValue["t" + t]));
			}
			// COCs
			for (int c = 0; c < nc; c++)
			{
				if (cocValue["t" + t + "_c" + c] != null)
				{
					str += ("" + ((COC) cocValue["t" + t + "_c" + c]));
				}
			}
			// QCD
			if (qcdValue["t" + t] != null)
			{
				str += ("" + ((QCD) qcdValue["t" + t]));
			}
			// QCCs
			for (int c = 0; c < nc; c++)
			{
				if (qccValue["t" + t + "_c" + c] != null)
				{
					str += ("" + ((QCC) qccValue["t" + t + "_c" + c]));
				}
			}
			// RGN
			for (int c = 0; c < nc; c++)
			{
				if (rgnValue["t" + t + "_c" + c] != null)
				{
					str += ("" + ((RGN) rgnValue["t" + t + "_c" + c]));
				}
			}
			// POC
			if (pocValue["t" + t] != null)
			{
				str += ("" + ((POC) pocValue["t" + t]));
			}
			return str;
		}
		
		/// <summary>Returns a copy of this object </summary>
		public virtual HeaderInfo getCopy(int nt)
		{
			HeaderInfo nhi = null;
			// SIZ
			try
			{
				nhi = (HeaderInfo) Clone();
			}
			//UPGRADE_NOTE: Exception 'java.lang.CloneNotSupportedException' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
			catch (System.Exception)
			{
				throw new System.ApplicationException("Cannot clone HeaderInfo instance");
			}
			nhi.sizValue = sizValue.Copy;
			// COD
			if (codValue["main"] != null)
			{
				COD ms = (COD) codValue["main"];
				nhi.codValue["main"] = ms.Copy;
			}
			for (int t = 0; t < nt; t++)
			{
				if (codValue["t" + t] != null)
				{
					COD ms = (COD) codValue["t" + t];
					nhi.codValue["t" + t] = ms.Copy;
				}
			}
			return nhi;
		}
		//UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1232'"
		virtual public System.Object Clone()
		{
			return null;
		}
	}
}
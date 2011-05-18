/*
*
* Class:                   ImgWriterGDI
*
* Description:             Image writer for System.Drawing.GDI streams
*
* */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CSJ2K.j2k.image;
using CSJ2K.j2k.io;
using CSJ2K.j2k;

namespace CSJ2K.j2k.image.input
{
    public class ImgReaderGDI : ImgReader
    {
        /// <summary>The number of bits that determine the nominal dynamic range </summary>
        private int rb;

        /// <summary>Buffer for the components of each pixel(in the current block) </summary>
        private int[][] barr;

        /// <summary>Data block used only to store coordinates of the buffered blocks </summary>
        private DataBlkInt dbi = new DataBlkInt();

        /// <summary>Temporary DataBlkInt object (needed when encoder uses floating-point
        /// filters). This avoid allocating new DataBlk at each time 
        /// </summary>
        private DataBlkInt intBlk;

        private Image image;

        public ImgReaderGDI(Image image)
        {
            if (image.PixelFormat != PixelFormat.Format24bppRgb)
                throw new ArgumentException("Only 24bpp RGB images are currently supported");

            this.image = image;

            w = image.Width;
            h = image.Height;

            nc = 3;
            rb = 8;
        }

        /// <summary> Closes the underlying file from where the image data is being read. No
        /// operations are possible after a call to this method.
        /// 
        /// </summary>
        /// <exception cref="IOException">If an I/O error occurs.
        /// 
        /// </exception>
        public override void close()
        {
            image.Dispose();
            image = null;

            barr = null;
        }

        /// <summary> Returns the number of bits corresponding to the nominal range of the
        /// data in the specified component. This is the value rb (range bits) that
        /// was specified in the constructor, which normally is 8 for non bilevel
        /// data, and 1 for bilevel data.
        /// 
        /// <P>If this number is <i>b</b> then the nominal range is between
        /// -2^(b-1) and 2^(b-1)-1, since unsigned data is level shifted to have a
        /// nominal avergae of 0.
        /// 
        /// </summary>
        /// <param name="c">The index of the component.
        /// 
        /// </param>
        /// <returns> The number of bits corresponding to the nominal range of the
        /// data. For floating-point data this value is not applicable and the
        /// return value is undefined.
        /// 
        /// </returns>
        public override int getNomRangeBits(int c)
        {
            // Check component index
            if (c < 0 || c > 2)
                throw new System.ArgumentException();

            return rb;
        }

        /// <summary> Returns the position of the fixed point in the specified component
        /// (i.e. the number of fractional bits), which is always 0 for this
        /// ImgReader.
        /// 
        /// </summary>
        /// <param name="c">The index of the component.
        /// 
        /// </param>
        /// <returns> The position of the fixed-point (i.e. the number of fractional
        /// bits). Always 0 for this ImgReader.
        /// 
        /// </returns>
        public override int getFixedPoint(int c)
        {
            // Check component index
            if (c < 0 || c > 2)
                throw new System.ArgumentException();
            return 0;
        }

        /// <summary> Returns, in the blk argument, the block of image data containing the
        /// specifed rectangular area, in the specified component. The data is
        /// returned, as a reference to the internal data, if any, instead of as a
        /// copy, therefore the returned data should not be modified.
        /// 
        /// <P> After being read the coefficients are level shifted by subtracting
        /// 2^(nominal bit range - 1)
        /// 
        /// <P>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
        /// and 'h' members of the 'blk' argument, relative to the current
        /// tile. These members are not modified by this method. The 'offset' and
        /// 'scanw' of the returned data can be arbitrary. See the 'DataBlk' class.
        /// 
        /// <P>If the data array in <tt>blk</tt> is <tt>null</tt>, then a new one
        /// is created if necessary. The implementation of this interface may
        /// choose to return the same array or a new one, depending on what is more
        /// efficient. Therefore, the data array in <tt>blk</tt> prior to the
        /// method call should not be considered to contain the returned data, a
        /// new array may have been created. Instead, get the array from
        /// <tt>blk</tt> after the method has returned.
        /// 
        /// <P>The returned data always has its 'progressive' attribute unset
        /// (i.e. false).
        /// 
        /// <P>When an I/O exception is encountered the JJ2KExceptionHandler is
        /// used. The exception is passed to its handleException method. The action
        /// that is taken depends on the action that has been registered in
        /// JJ2KExceptionHandler. See JJ2KExceptionHandler for details.
        /// 
        /// <P>This method implements buffering for the 3 components: When the
        /// first one is asked, all the 3 components are read and stored until they
        /// are needed.
        /// 
        /// </summary>
        /// <param name="blk">Its coordinates and dimensions specify the area to
        /// return. Some fields in this object are modified to return the data.
        /// 
        /// </param>
        /// <param name="c">The index of the component from which to get the data. Only 0,
        /// 1 and 3 are valid.
        /// 
        /// </param>
        /// <returns> The requested DataBlk
        /// 
        /// </returns>
        /// <seealso cref="getCompData">
        /// 
        /// </seealso>
        /// <seealso cref="JJ2KExceptionHandler">
        /// 
        /// </seealso>
        public override DataBlk getInternCompData(DataBlk blk, int c)
        {
            // Check component index
            if (c < 0 || c > 2)
                throw new System.ArgumentException();

            // Check type of block provided as an argument
            if (blk.DataType != DataBlk.TYPE_INT)
            {
                if (intBlk == null)
                    intBlk = new DataBlkInt(blk.ulx, blk.uly, blk.w, blk.h);
                else
                {
                    intBlk.ulx = blk.ulx;
                    intBlk.uly = blk.uly;
                    intBlk.w = blk.w;
                    intBlk.h = blk.h;
                }
                blk = intBlk;
            }

            // If asking a component for the first time for this block, read all of the components
            if ((barr == null) || (dbi.ulx > blk.ulx) || (dbi.uly > blk.uly) || (dbi.ulx + dbi.w < blk.ulx + blk.w) || (dbi.uly + dbi.h < blk.uly + blk.h))
            {
                int i;
                int[] red, green, blue, alpha;
                int componentCount = (image.PixelFormat == PixelFormat.Format32bppArgb) ? 4 : 3;

                barr = new int[componentCount][];

                // Reset data arrays if needed
                if (barr[c] == null || barr[c].Length < blk.w * blk.h)
                {
                    barr[c] = new int[blk.w * blk.h];
                }
                blk.Data = barr[c];

                i = (c + 1) % 3;
                if (barr[i] == null || barr[i].Length < blk.w * blk.h)
                {
                    barr[i] = new int[blk.w * blk.h];
                }
                i = (c + 2) % 3;
                if (barr[i] == null || barr[i].Length < blk.w * blk.h)
                {
                    barr[i] = new int[blk.w * blk.h];
                }

                if (componentCount == 4)
                {
                    if (barr[3] == null || barr[3].Length < blk.w * blk.h)
                        barr[3] = new int[blk.w * blk.h];
                }

                // set attributes of the DataBlk used for buffering
                dbi.ulx = blk.ulx;
                dbi.uly = blk.uly;
                dbi.w = blk.w;
                dbi.h = blk.h;

                red = barr[0];
                green = barr[1];
                blue = barr[2];
                alpha = (componentCount == 4) ? barr[3] : null;

                Bitmap bitmap = (Bitmap)image;
                BitmapData data = bitmap.LockBits(new Rectangle(blk.ulx, blk.uly, blk.w, blk.h), ImageLockMode.ReadOnly,
                    (componentCount == 3) ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0.ToPointer();
                    
                    int k = 0;
                    for (int j = 0; j < blk.w * blk.h; j++)
                    {
                        blue[k] = ((byte)*(ptr + 0) & 0xFF) - 128;
                        green[k] = ((byte)*(ptr + 1) & 0xFF) - 128;
                        red[k] = ((byte)*(ptr + 2) & 0xFF) - 128;
                        if (componentCount == 4)
                            alpha[k] = ((byte)*(ptr + 3) & 0xFF) - 128;

                        ++k;
                        ptr += 3;
                    }
                }
                bitmap.UnlockBits(data);

                barr[0] = red;
                barr[1] = green;
                barr[2] = blue;
                if (componentCount == 4)
                    barr[3] = alpha;

                // Set buffer attributes
                blk.Data = barr[c];
                blk.offset = 0;
                blk.scanw = blk.w;
            }
            else
            {
                //Asking for the 2nd or 3rd (or 4th) block component
                blk.Data = barr[c];
                blk.offset = (blk.ulx - dbi.ulx) * dbi.w + blk.ulx - dbi.ulx;
                blk.scanw = dbi.scanw;
            }

            // Turn off the progressive attribute
            blk.progressive = false;
            return blk;
        }

        /// <summary> Returns, in the blk argument, a block of image data containing the
        /// specifed rectangular area, in the specified component. The data is
        /// returned, as a copy of the internal data, therefore the returned data
        /// can be modified "in place".
        /// 
        /// <P> After being read the coefficients are level shifted by subtracting
        /// 2^(nominal bit range - 1)
        /// 
        /// <P>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
        /// and 'h' members of the 'blk' argument, relative to the current
        /// tile. These members are not modified by this method. The 'offset' of
        /// the returned data is 0, and the 'scanw' is the same as the block's
        /// width. See the 'DataBlk' class.
        /// 
        /// <P>If the data array in 'blk' is 'null', then a new one is created. If
        /// the data array is not 'null' then it is reused, and it must be large
        /// enough to contain the block's data. Otherwise an 'ArrayStoreException'
        /// or an 'IndexOutOfBoundsException' is thrown by the Java system.
        /// 
        /// <P>The returned data has its 'progressive' attribute unset
        /// (i.e. false).
        /// 
        /// <P>When an I/O exception is encountered the JJ2KExceptionHandler is
        /// used. The exception is passed to its handleException method. The action
        /// that is taken depends on the action that has been registered in
        /// JJ2KExceptionHandler. See JJ2KExceptionHandler for details.
        /// 
        /// </summary>
        /// <param name="blk">Its coordinates and dimensions specify the area to
        /// return. If it contains a non-null data array, then it must have the
        /// correct dimensions. If it contains a null data array a new one is
        /// created. The fields in this object are modified to return the data.
        /// 
        /// </param>
        /// <param name="c">The index of the component from which to get the data. Only
        /// 0,1 and 2 are valid.
        /// 
        /// </param>
        /// <returns> The requested DataBlk
        /// 
        /// </returns>
        /// <seealso cref="getInternCompData">
        /// 
        /// </seealso>
        /// <seealso cref="JJ2KExceptionHandler">
        /// 
        /// </seealso>
        public override DataBlk getCompData(DataBlk blk, int c)
        {
            // NOTE: can not directly call getInterCompData since that returns
            // internally buffered data.
            int ulx, uly, w, h;

            // Check type of block provided as an argument
            if (blk.DataType != DataBlk.TYPE_INT)
            {
                DataBlkInt tmp = new DataBlkInt(blk.ulx, blk.uly, blk.w, blk.h);
                blk = tmp;
            }

            int[] bakarr = (int[])blk.Data;
            // Save requested block size
            ulx = blk.ulx;
            uly = blk.uly;
            w = blk.w;
            h = blk.h;
            // Force internal data buffer to be different from external
            blk.Data = null;
            getInternCompData(blk, c);
            // Copy the data
            if (bakarr == null)
            {
                bakarr = new int[w * h];
            }
            if (blk.offset == 0 && blk.scanw == w)
            {
                // Requested and returned block buffer are the same size
                // CONVERSION PROBLEM?
                Array.Copy((System.Array)blk.Data, 0, (System.Array)bakarr, 0, w * h);
            }
            else
            {
                // Requested and returned block are different
                for (int i = h - 1; i >= 0; i--)
                {
                    // copy line by line
                    // CONVERSION PROBLEM?
                    Array.Copy((System.Array)blk.Data, blk.offset + i * blk.scanw, (System.Array)bakarr, i * w, w);
                }
            }
            blk.Data = bakarr;
            blk.offset = 0;
            blk.scanw = blk.w;
            return blk;
        }

        /// <summary> Returns true if the data read was originally signed in the specified
        /// component, false if not. This method always returns false since PPM
        /// data is always unsigned.
        /// 
        /// </summary>
        /// <param name="c">The index of the component, from 0 to N-1.
        /// 
        /// </param>
        /// <returns> always false, since PPM data is always unsigned.
        /// 
        /// </returns>
        public override bool isOrigSigned(int c)
        {
            // Check component index
            if (c < 0 || c > 2)
                throw new System.ArgumentException();
            return false;
        }

        /// <summary> Returns a string of information about the object, more than 1 line
        /// long. The information string includes information from the underlying
        /// RandomAccessFile (its toString() method is called in turn).
        /// 
        /// </summary>
        /// <returns> A string of information about the object.  
        /// 
        /// </returns>
        public override System.String ToString()
        {
            return "ImgReaderGDI: WxH = " + w + "x" + h + ", Component = 0,1,2";
        }
    }
}
/* 
* CVS identifier:
* 
* $Id: StdEntropyCoderOptions.java,v 1.10 2001/03/27 09:57:20 grosbois Exp $
* 
* Class:                   StdEntropyCoderOptions
* 
* Description:             Entropy coding engine of stripes in
*                          code-blocks options
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
namespace CSJ2K.j2k.entropy
{
	
	/// <summary> This interface define the constants that identify the possible options for
	/// the entropy coder, as well some fixed parameters of the JPEG 2000 entropy
	/// coder.
	/// 
	/// </summary>
	public struct StdEntropyCoderOptions{
		/// <summary>The flag bit to indicate that selective arithmetic coding bypass
		/// should be used.  In this mode, the significance propagation and
		/// magnitude refinement passes bypass the arithmetic encoder in the fourth
		/// bit-plane and latter ones (but not the cleanup pass). Note that the
		/// transition between raw and AC segments needs terminations (whether or
		/// not OPT_TERM_PASS is used). 
		/// </summary>
		public readonly static int OPT_BYPASS = 1;
		/// <summary>The flag bit to indicate that the MQ states for all contexts should be 
		/// reset at the end of each (non-bypassed) coding pass. 
		/// </summary>
		public readonly static int OPT_RESET_MQ = 1 << 1;
		/// <summary>The flag bit to indicate that a termination should be performed after
		/// each coding pass.  Note that terminations are applied to both * *
		/// arithmetically coded and bypassed (i.e. raw) passes . 
		/// </summary>
		public readonly static int OPT_TERM_PASS = 1 << 2;
		/// <summary>The flag bit to indicate the vertically stripe-causal context
		/// formation should be used. 
		/// </summary>
		public readonly static int OPT_VERT_STR_CAUSAL = 1 << 3;
		/// <summary>The flag bit to indicate that error resilience info is embedded on MQ
		/// termination. This corresponds to the predictable termination described
		/// in Annex D.4.2 of the FDIS 
		/// </summary>
		public readonly static int OPT_PRED_TERM = 1 << 4;
		/// <summary>The flag bit to indicate that an error resilience segmentation symbol
		/// is to be inserted at the end of each cleanup coding pass. The
		/// segmentation symbol is the four symbol sequence 1010 that are sent
		/// through the MQ coder using the UNIFORM context (as explained in annex
		/// D.5 of FDIS). 
		/// </summary>
		public readonly static int OPT_SEG_SYMBOLS = 1 << 5;
		/// <summary>The minimum code-block dimension. The nominal width or height of a
		/// code-block must never be less than this. It is 4. 
		/// </summary>
		public readonly static int MIN_CB_DIM = 4;
		/// <summary>The maximum code-block dimension. No code-block should be larger,
		/// either in width or height, than this value. It is 1024. 
		/// </summary>
		public readonly static int MAX_CB_DIM = 1024;
		/// <summary>The maximum code-block area (width x height). The surface covered by
		/// a nominal size block should never be larger than this. It is 4096 
		/// </summary>
		public readonly static int MAX_CB_AREA = 4096;
		/// <summary>The stripe height. This is the nominal value of the stripe height. It
		/// is 4. 
		/// </summary>
		public readonly static int STRIPE_HEIGHT = 4;
		/// <summary>The number of coding passes per bit-plane. This is the number of
		/// passes per bit-plane. It is 3. 
		/// </summary>
		public readonly static int NUM_PASSES = 3;
		/// <summary>The number of most significant bit-planes where bypass mode is not to
		/// be used, even if bypass mode is on: 4. 
		/// </summary>
		public readonly static int NUM_NON_BYPASS_MS_BP = 4;
		/// <summary>The number of empty passes in the most significant bit-plane. It is
		/// 2. 
		/// </summary>
		public readonly static int NUM_EMPTY_PASSES_IN_MS_BP = 2;
		/// <summary>The index of the first "raw" pass, if bypass mode is on. </summary>
		public readonly static int FIRST_BYPASS_PASS_IDX;
		static StdEntropyCoderOptions()
		{
			FIRST_BYPASS_PASS_IDX = CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_PASSES * CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_NON_BYPASS_MS_BP - CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP;
		}
	}
}
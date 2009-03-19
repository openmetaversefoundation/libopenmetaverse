
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenMetaverse;

/*
 * [14:09]	<jhurliman>	the onnewprim function will add missing texture uuids to the download queue, 
 * and a separate thread will pull entries off that queue. 
 * if they exist in the cache it will add that texture to a dictionary that the rendering loop accesses, 
 * otherwise it will start the download. 
 * the ondownloaded function will put the new texture in the same dictionary
 * 
 * 
 * Easy Start:
 * subscribe to OnImageRenderReady event
 * send request with RequestTexture()
 * 
 * when OnImageRenderReady fires:
 * request image data with GetTextureToRender() using key returned in OnImageRenderReady event
 * (optionally) use RemoveFromPipeline() with key to cleanup dictionary
 */
namespace PrimWorkshop
{
    
}

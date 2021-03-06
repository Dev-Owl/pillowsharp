
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RestSharp;

namespace PillowSharp.Helper
{
    public static class MimeMapping
    {

        private static Dictionary<string,string> mappings = new Dictionary<string,string> ();
       
         static MimeMapping()
         {
                mappings.Add(".323", "text/h323");
                mappings.Add(".aaf", "application/octet-stream");
                mappings.Add(".aca", "application/octet-stream");
                mappings.Add(".accdb", "application/msaccess");
                mappings.Add(".accde", "application/msaccess");
                mappings.Add(".accdt", "application/msaccess");
                mappings.Add(".acx", "application/internet-property-stream");
                mappings.Add(".afm", "application/octet-stream");
                mappings.Add(".ai", "application/postscript");
                mappings.Add(".aif", "audio/x-aiff");
                mappings.Add(".aifc", "audio/aiff");
                mappings.Add(".aiff", "audio/aiff");
                mappings.Add(".application", "application/x-ms-application");
                mappings.Add(".art", "image/x-jg");
                mappings.Add(".asd", "application/octet-stream");
                mappings.Add(".asf", "video/x-ms-asf");
                mappings.Add(".asi", "application/octet-stream");
                mappings.Add(".asm", "text/plain");
                mappings.Add(".asr", "video/x-ms-asf");
                mappings.Add(".asx", "video/x-ms-asf");
                mappings.Add(".atom", "application/atom+xml");
                mappings.Add(".au", "audio/basic");
                mappings.Add(".avi", "video/x-msvideo");
                mappings.Add(".axs", "application/olescript");
                mappings.Add(".bas", "text/plain");
                mappings.Add(".bcpio", "application/x-bcpio");
                mappings.Add(".bin", "application/octet-stream");
                mappings.Add(".bmp", "image/bmp");
                mappings.Add(".c", "text/plain");
                mappings.Add(".cab", "application/octet-stream");
                mappings.Add(".calx", "application/vnd.ms-office.calx");
                mappings.Add(".cat", "application/vnd.ms-pki.seccat");
                mappings.Add(".cdf", "application/x-cdf");
                mappings.Add(".chm", "application/octet-stream");
                mappings.Add(".class", "application/x-java-applet");
                mappings.Add(".clp", "application/x-msclip");
                mappings.Add(".cmx", "image/x-cmx");
                mappings.Add(".cnf", "text/plain");
                mappings.Add(".cod", "image/cis-cod");
                mappings.Add(".cpio", "application/x-cpio");
                mappings.Add(".cpp", "text/plain");
                mappings.Add(".crd", "application/x-mscardfile");
                mappings.Add(".crl", "application/pkix-crl");
                mappings.Add(".crt", "application/x-x509-ca-cert");
                mappings.Add(".csh", "application/x-csh");
                mappings.Add(".css", "text/css");
                mappings.Add(".csv", "application/octet-stream");
                mappings.Add(".cur", "application/octet-stream");
                mappings.Add(".dcr", "application/x-director");
                mappings.Add(".deploy", "application/octet-stream");
                mappings.Add(".der", "application/x-x509-ca-cert");
                mappings.Add(".dib", "image/bmp");
                mappings.Add(".dir", "application/x-director");
                mappings.Add(".disco", "text/xml");
                mappings.Add(".dll", "application/x-msdownload");
                mappings.Add(".dll.config", "text/xml");
                mappings.Add(".dlm", "text/dlm");
                mappings.Add(".doc", "application/msword");
                mappings.Add(".docm", "application/vnd.ms-word.document.macroEnabled.12");
                mappings.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                mappings.Add(".dot", "application/msword");
                mappings.Add(".dotm", "application/vnd.ms-word.template.macroEnabled.12");
                mappings.Add(".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
                mappings.Add(".dsp", "application/octet-stream");
                mappings.Add(".dtd", "text/xml");
                mappings.Add(".dvi", "application/x-dvi");
                mappings.Add(".dwf", "drawing/x-dwf");
                mappings.Add(".dwp", "application/octet-stream");
                mappings.Add(".dxr", "application/x-director");
                mappings.Add(".eml", "message/rfc822");
                mappings.Add(".emz", "application/octet-stream");
                mappings.Add(".eot", "application/octet-stream");
                mappings.Add(".eps", "application/postscript");
                mappings.Add(".etx", "text/x-setext");
                mappings.Add(".evy", "application/envoy");
                mappings.Add(".exe", "application/octet-stream");
                mappings.Add(".exe.config", "text/xml");
                mappings.Add(".fdf", "application/vnd.fdf");
                mappings.Add(".fif", "application/fractals");
                mappings.Add(".fla", "application/octet-stream");
                mappings.Add(".flr", "x-world/x-vrml");
                mappings.Add(".flv", "video/x-flv");
                mappings.Add(".gif", "image/gif");
                mappings.Add(".gtar", "application/x-gtar");
                mappings.Add(".gz", "application/x-gzip");
                mappings.Add(".h", "text/plain");
                mappings.Add(".hdf", "application/x-hdf");
                mappings.Add(".hdml", "text/x-hdml");
                mappings.Add(".hhc", "application/x-oleobject");
                mappings.Add(".hhk", "application/octet-stream");
                mappings.Add(".hhp", "application/octet-stream");
                mappings.Add(".hlp", "application/winhlp");
                mappings.Add(".hqx", "application/mac-binhex40");
                mappings.Add(".hta", "application/hta");
                mappings.Add(".htc", "text/x-component");
                mappings.Add(".htm", "text/html");
                mappings.Add(".html", "text/html");
                mappings.Add(".htt", "text/webviewhtml");
                mappings.Add(".hxt", "text/html");
                mappings.Add(".ico", "image/x-icon");
                mappings.Add(".ics", "application/octet-stream");
                mappings.Add(".ief", "image/ief");
                mappings.Add(".iii", "application/x-iphone");
                mappings.Add(".inf", "application/octet-stream");
                mappings.Add(".ins", "application/x-internet-signup");
                mappings.Add(".isp", "application/x-internet-signup");
                mappings.Add(".IVF", "video/x-ivf");
                mappings.Add(".jar", "application/java-archive");
                mappings.Add(".java", "application/octet-stream");
                mappings.Add(".jck", "application/liquidmotion");
                mappings.Add(".jcz", "application/liquidmotion");
                mappings.Add(".jfif", "image/pjpeg");
                mappings.Add(".jpb", "application/octet-stream");
                mappings.Add(".jpe", "image/jpeg");
                mappings.Add(".jpeg", "image/jpeg");
                mappings.Add(".jpg", "image/jpeg");
                mappings.Add(".js", "application/x-javascript");
                mappings.Add(".jsx", "text/jscript");
                mappings.Add(".latex", "application/x-latex");
                mappings.Add(".lit", "application/x-ms-reader");
                mappings.Add(".lpk", "application/octet-stream");
                mappings.Add(".lsf", "video/x-la-asf");
                mappings.Add(".lsx", "video/x-la-asf");
                mappings.Add(".lzh", "application/octet-stream");
                mappings.Add(".m13", "application/x-msmediaview");
                mappings.Add(".m14", "application/x-msmediaview");
                mappings.Add(".m1v", "video/mpeg");
                mappings.Add(".m3u", "audio/x-mpegurl");
                mappings.Add(".man", "application/x-troff-man");
                mappings.Add(".manifest", "application/x-ms-manifest");
                mappings.Add(".map", "text/plain");
                mappings.Add(".mdb", "application/x-msaccess");
                mappings.Add(".mdp", "application/octet-stream");
                mappings.Add(".me", "application/x-troff-me");
                mappings.Add(".mht", "message/rfc822");
                mappings.Add(".mhtml", "message/rfc822");
                mappings.Add(".mid", "audio/mid");
                mappings.Add(".midi", "audio/mid");
                mappings.Add(".mix", "application/octet-stream");
                mappings.Add(".mmf", "application/x-smaf");
                mappings.Add(".mno", "text/xml");
                mappings.Add(".mny", "application/x-msmoney");
                mappings.Add(".mov", "video/quicktime");
                mappings.Add(".movie", "video/x-sgi-movie");
                mappings.Add(".mp2", "video/mpeg");
                mappings.Add(".mp3", "audio/mpeg");
                mappings.Add(".mpa", "video/mpeg");
                mappings.Add(".mpe", "video/mpeg");
                mappings.Add(".mpeg", "video/mpeg");
                mappings.Add(".mpg", "video/mpeg");
                mappings.Add(".mpp", "application/vnd.ms-project");
                mappings.Add(".mpv2", "video/mpeg");
                mappings.Add(".ms", "application/x-troff-ms");
                mappings.Add(".msi", "application/octet-stream");
                mappings.Add(".mso", "application/octet-stream");
                mappings.Add(".mvb", "application/x-msmediaview");
                mappings.Add(".mvc", "application/x-miva-compiled");
                mappings.Add(".nc", "application/x-netcdf");
                mappings.Add(".nsc", "video/x-ms-asf");
                mappings.Add(".nws", "message/rfc822");
                mappings.Add(".ocx", "application/octet-stream");
                mappings.Add(".oda", "application/oda");
                mappings.Add(".odc", "text/x-ms-odc");
                mappings.Add(".ods", "application/oleobject");
                mappings.Add(".one", "application/onenote");
                mappings.Add(".onea", "application/onenote");
                mappings.Add(".onetoc", "application/onenote");
                mappings.Add(".onetoc2", "application/onenote");
                mappings.Add(".onetmp", "application/onenote");
                mappings.Add(".onepkg", "application/onenote");
                mappings.Add(".osdx", "application/opensearchdescription+xml");
                mappings.Add(".p10", "application/pkcs10");
                mappings.Add(".p12", "application/x-pkcs12");
                mappings.Add(".p7b", "application/x-pkcs7-certificates");
                mappings.Add(".p7c", "application/pkcs7-mime");
                mappings.Add(".p7m", "application/pkcs7-mime");
                mappings.Add(".p7r", "application/x-pkcs7-certreqresp");
                mappings.Add(".p7s", "application/pkcs7-signature");
                mappings.Add(".pbm", "image/x-portable-bitmap");
                mappings.Add(".pcx", "application/octet-stream");
                mappings.Add(".pcz", "application/octet-stream");
                mappings.Add(".pdf", "application/pdf");
                mappings.Add(".pfb", "application/octet-stream");
                mappings.Add(".pfm", "application/octet-stream");
                mappings.Add(".pfx", "application/x-pkcs12");
                mappings.Add(".pgm", "image/x-portable-graymap");
                mappings.Add(".pko", "application/vnd.ms-pki.pko");
                mappings.Add(".pma", "application/x-perfmon");
                mappings.Add(".pmc", "application/x-perfmon");
                mappings.Add(".pml", "application/x-perfmon");
                mappings.Add(".pmr", "application/x-perfmon");
                mappings.Add(".pmw", "application/x-perfmon");
                mappings.Add(".png", "image/png");
                mappings.Add(".pnm", "image/x-portable-anymap");
                mappings.Add(".pnz", "image/png");
                mappings.Add(".pot", "application/vnd.ms-powerpoint");
                mappings.Add(".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12");
                mappings.Add(".potx", "application/vnd.openxmlformats-officedocument.presentationml.template");
                mappings.Add(".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12");
                mappings.Add(".ppm", "image/x-portable-pixmap");
                mappings.Add(".pps", "application/vnd.ms-powerpoint");
                mappings.Add(".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
                mappings.Add(".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");
                mappings.Add(".ppt", "application/vnd.ms-powerpoint");
                mappings.Add(".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12");
                mappings.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
                mappings.Add(".prf", "application/pics-rules");
                mappings.Add(".prm", "application/octet-stream");
                mappings.Add(".prx", "application/octet-stream");
                mappings.Add(".ps", "application/postscript");
                mappings.Add(".psd", "application/octet-stream");
                mappings.Add(".psm", "application/octet-stream");
                mappings.Add(".psp", "application/octet-stream");
                mappings.Add(".pub", "application/x-mspublisher");
                mappings.Add(".qt", "video/quicktime");
                mappings.Add(".qtl", "application/x-quicktimeplayer");
                mappings.Add(".qxd", "application/octet-stream");
                mappings.Add(".ra", "audio/x-pn-realaudio");
                mappings.Add(".ram", "audio/x-pn-realaudio");
                mappings.Add(".rar", "application/octet-stream");
                mappings.Add(".ras", "image/x-cmu-raster");
                mappings.Add(".rf", "image/vnd.rn-realflash");
                mappings.Add(".rgb", "image/x-rgb");
                mappings.Add(".rm", "application/vnd.rn-realmedia");
                mappings.Add(".rmi", "audio/mid");
                mappings.Add(".roff", "application/x-troff");
                mappings.Add(".rpm", "audio/x-pn-realaudio-plugin");
                mappings.Add(".rtf", "application/rtf");
                mappings.Add(".rtx", "text/richtext");
                mappings.Add(".scd", "application/x-msschedule");
                mappings.Add(".sct", "text/scriptlet");
                mappings.Add(".sea", "application/octet-stream");
                mappings.Add(".setpay", "application/set-payment-initiation");
                mappings.Add(".setreg", "application/set-registration-initiation");
                mappings.Add(".sgml", "text/sgml");
                mappings.Add(".sh", "application/x-sh");
                mappings.Add(".shar", "application/x-shar");
                mappings.Add(".sit", "application/x-stuffit");
                mappings.Add(".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12");
                mappings.Add(".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide");
                mappings.Add(".smd", "audio/x-smd");
                mappings.Add(".smi", "application/octet-stream");
                mappings.Add(".smx", "audio/x-smd");
                mappings.Add(".smz", "audio/x-smd");
                mappings.Add(".snd", "audio/basic");
                mappings.Add(".snp", "application/octet-stream");
                mappings.Add(".spc", "application/x-pkcs7-certificates");
                mappings.Add(".spl", "application/futuresplash");
                mappings.Add(".src", "application/x-wais-source");
                mappings.Add(".ssm", "application/streamingmedia");
                mappings.Add(".sst", "application/vnd.ms-pki.certstore");
                mappings.Add(".stl", "application/vnd.ms-pki.stl");
                mappings.Add(".sv4cpio", "application/x-sv4cpio");
                mappings.Add(".sv4crc", "application/x-sv4crc");
                mappings.Add(".swf", "application/x-shockwave-flash");
                mappings.Add(".t", "application/x-troff");
                mappings.Add(".tar", "application/x-tar");
                mappings.Add(".tcl", "application/x-tcl");
                mappings.Add(".tex", "application/x-tex");
                mappings.Add(".texi", "application/x-texinfo");
                mappings.Add(".texinfo", "application/x-texinfo");
                mappings.Add(".tgz", "application/x-compressed");
                mappings.Add(".thmx", "application/vnd.ms-officetheme");
                mappings.Add(".thn", "application/octet-stream");
                mappings.Add(".tif", "image/tiff");
                mappings.Add(".tiff", "image/tiff");
                mappings.Add(".toc", "application/octet-stream");
                mappings.Add(".tr", "application/x-troff");
                mappings.Add(".trm", "application/x-msterminal");
                mappings.Add(".tsv", "text/tab-separated-values");
                mappings.Add(".ttf", "application/octet-stream");
                mappings.Add(".txt", "text/plain");
                mappings.Add(".u32", "application/octet-stream");
                mappings.Add(".uls", "text/iuls");
                mappings.Add(".ustar", "application/x-ustar");
                mappings.Add(".vbs", "text/vbscript");
                mappings.Add(".vcf", "text/x-vcard");
                mappings.Add(".vcs", "text/plain");
                mappings.Add(".vdx", "application/vnd.ms-visio.viewer");
                mappings.Add(".vml", "text/xml");
                mappings.Add(".vsd", "application/vnd.visio");
                mappings.Add(".vss", "application/vnd.visio");
                mappings.Add(".vst", "application/vnd.visio");
                mappings.Add(".vsto", "application/x-ms-vsto");
                mappings.Add(".vsw", "application/vnd.visio");
                mappings.Add(".vsx", "application/vnd.visio");
                mappings.Add(".vtx", "application/vnd.visio");
                mappings.Add(".wav", "audio/wav");
                mappings.Add(".wax", "audio/x-ms-wax");
                mappings.Add(".wbmp", "image/vnd.wap.wbmp");
                mappings.Add(".wcm", "application/vnd.ms-works");
                mappings.Add(".wdb", "application/vnd.ms-works");
                mappings.Add(".wks", "application/vnd.ms-works");
                mappings.Add(".wm", "video/x-ms-wm");
                mappings.Add(".wma", "audio/x-ms-wma");
                mappings.Add(".wmd", "application/x-ms-wmd");
                mappings.Add(".wmf", "application/x-msmetafile");
                mappings.Add(".wml", "text/vnd.wap.wml");
                mappings.Add(".wmlc", "application/vnd.wap.wmlc");
                mappings.Add(".wmls", "text/vnd.wap.wmlscript");
                mappings.Add(".wmlsc", "application/vnd.wap.wmlscriptc");
                mappings.Add(".wmp", "video/x-ms-wmp");
                mappings.Add(".wmv", "video/x-ms-wmv");
                mappings.Add(".wmx", "video/x-ms-wmx");
                mappings.Add(".wmz", "application/x-ms-wmz");
                mappings.Add(".wps", "application/vnd.ms-works");
                mappings.Add(".wri", "application/x-mswrite");
                mappings.Add(".wrl", "x-world/x-vrml");
                mappings.Add(".wrz", "x-world/x-vrml");
                mappings.Add(".wsdl", "text/xml");
                mappings.Add(".wvx", "video/x-ms-wvx");
                mappings.Add(".x", "application/directx");
                mappings.Add(".xaf", "x-world/x-vrml");
                mappings.Add(".xaml", "application/xaml+xml");
                mappings.Add(".xap", "application/x-silverlight-app");
                mappings.Add(".xbap", "application/x-ms-xbap");
                mappings.Add(".xbm", "image/x-xbitmap");
                mappings.Add(".xdr", "text/plain");
                mappings.Add(".xla", "application/vnd.ms-excel");
                mappings.Add(".xlam", "application/vnd.ms-excel.addin.macroEnabled.12");
                mappings.Add(".xlc", "application/vnd.ms-excel");
                mappings.Add(".xlm", "application/vnd.ms-excel");
                mappings.Add(".xls", "application/vnd.ms-excel");
                mappings.Add(".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12");
                mappings.Add(".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12");
                mappings.Add(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                mappings.Add(".xlt", "application/vnd.ms-excel");
                mappings.Add(".xltm", "application/vnd.ms-excel.template.macroEnabled.12");
                mappings.Add(".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template");
                mappings.Add(".xlw", "application/vnd.ms-excel");
                mappings.Add(".xml", "text/xml");
                mappings.Add(".xof", "x-world/x-vrml");
                mappings.Add(".xpm", "image/x-xpixmap");
                mappings.Add(".xps", "application/vnd.ms-xpsdocument");
                mappings.Add(".xsd", "text/xml");
                mappings.Add(".xsf", "text/xml");
                mappings.Add(".xsl", "text/xml");
                mappings.Add(".xslt", "text/xml");
                mappings.Add(".xsn", "application/octet-stream");
                mappings.Add(".xtp", "application/octet-stream");
                mappings.Add(".xwd", "image/x-xwindowdump");
                mappings.Add(".z", "application/x-compress");
                mappings.Add(".zip", "application/x-zip-compressed");
            }

            public static string GetMimeType(string FilePath,string Default="text/xml")
            {
                var ending = Path.GetExtension(FilePath).ToLower();
                if(!mappings.ContainsKey(ending))
                    return Default;
                else
                    return mappings[ending];
            }

    }
}
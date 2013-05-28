using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using NBrightCore.providers;



namespace NBrightCore.common
{

    public class UtilsEmail
    {
        bool _invalid = false;

        public bool IsValidEmail(string strIn)
        {
            _invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper);
            if (_invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn,
                   @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                   RegexOptions.IgnoreCase);
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            var idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                _invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }

    public class Utils
    {

        public static void CreateFolder(string folderMapPath)
        {
            if (!Directory.Exists(folderMapPath))
            {
                Directory.CreateDirectory(folderMapPath);
            }
        }

        public static void DeleteFolder(string folderMapPath, bool recursive = false)
        {
            if (Directory.Exists(folderMapPath))
            {
                Directory.Delete(folderMapPath,recursive);
            }
        }

        public static string GetCurrentCulture()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
        }

        public static string GetCurrentCountryCode()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(2, 2);
        }

        public static string RequestParam(HttpContext context, string paramName)
        {
            string result = null;

            if (context.Request.Form.Count != 0)
            {
                result = Convert.ToString(context.Request.Form[paramName]);
            }

            if (result == null)
            {
                if (context.Request.QueryString.Count != 0)
                {
                    result = Convert.ToString(context.Request.QueryString[paramName]);
                }                
            }

            return (result == null) ? String.Empty : result.Trim();
        }

        public static string RequestQueryStringParam(System.Web.HttpRequest Request, string paramName)
        {
            var result = String.Empty;

            if (Request.QueryString.Count != 0)
            {
                result = Convert.ToString(Request.QueryString[paramName]);
            }

            return (result == null) ? String.Empty : result.Trim();
        }

        public static string RequestQueryStringParam(HttpContext context, string paramName)
        {
            var result = String.Empty;

            if (context.Request.QueryString.Count != 0)
            {
                result = Convert.ToString(context.Request.QueryString[paramName]);
            }

            return (result == null) ? String.Empty : result.Trim();
        }


        public static void ForceDocDownload(string docFilePath, string fileName, HttpResponse response)
        {
            if (File.Exists(docFilePath) & !string.IsNullOrEmpty(fileName))
            {
                response.AppendHeader("content-disposition", "attachment; filename=" + fileName);
                response.ContentType = "application/octet-stream";
                response.WriteFile(docFilePath);
                response.End();
            }

        }

        public static void ForceStringDownload(HttpResponse response, string fileName, string fileData)
        {
            response.AppendHeader("content-disposition", "attachment; filename=" + fileName);
            response.ContentType = "application/octet-stream";
            response.Write(fileData);
            response.End();
        }

        public static string FormatToSave(string inpData)
        {
            return FormatToSave(inpData, TypeCode.String);
        }

        public static string FormatToSave(string inpData, TypeCode dataTyp)
        {
            if (string.IsNullOrEmpty(inpData))
                return inpData;
            switch (dataTyp)
            {
                case TypeCode.Double:
                    //always save CultureInfo.InvariantCulture format to the XML
                    if (IsNumeric(inpData,GetCurrentCulture()))
                    {
                        var cultureInfo = new CultureInfo(GetCurrentCulture(), true);
                        var num = Convert.ToDouble(inpData, cultureInfo);
                        return num.ToString(CultureInfo.InvariantCulture);
                    }
                    if (IsNumeric(inpData)) // just check if we have a Invariant double
                    {
                        var num = Convert.ToDouble(inpData,CultureInfo.InvariantCulture);
                        return num.ToString(CultureInfo.InvariantCulture);
                    }
                    return "0";
                case TypeCode.DateTime:
                    if (Utils.IsDate(inpData))
                    {
                        var dte = Convert.ToDateTime(inpData);
                        return dte.ToString("s");
                    }
                    return inpData;
                default:
                    return inpData;
            }
        }

        public static string FormatToDisplay(string inpData, TypeCode dataTyp, string formatCode = "")
        {
            return FormatToDisplay(inpData, GetCurrentCulture(), dataTyp, formatCode);
        }

        public static string FormatToDisplay(string inpData, string cultureCode, TypeCode dataTyp, string formatCode = "")
        {
            if (string.IsNullOrEmpty(inpData))
            {
                if (dataTyp == TypeCode.Double)
                {
                    return "0";
                }
                return inpData;
            }
            var outCulture = new CultureInfo(cultureCode, false);
            switch (dataTyp)
            {
                case TypeCode.Double:
                    if (IsNumeric(inpData))
                    {
                            return double.Parse(inpData, CultureInfo.InvariantCulture).ToString(formatCode);                            
                    }
                    return "0";
                case TypeCode.DateTime:
                    if (Utils.IsDate(inpData))
                    {
                        if (formatCode == "") formatCode = "d";
                        return DateTime.Parse(inpData).ToString(formatCode, outCulture);
                    }
                    return inpData;
                default:
                    return inpData;
            }
        }


        /// <summary>
        ///  IsEmail function checks for a valid email format         
        /// </summary>
        public static bool IsEmail(string emailaddress)
        {
            var e = new UtilsEmail();
            return e.IsValidEmail(emailaddress);
        }

        /// <summary>
        ///  IsNumeric function check if a given value is numeric, based on the culture code passed.  If no culture code is passed then a test on InvariantCulture is done.
        /// </summary>
        public static bool IsNumeric(object expression, string cultureCode = "")
        {
            if (expression == null) return false;

            double retNum;
            bool isNum = false;
            if (cultureCode != "")
            {
                var cultureInfo = new CultureInfo(cultureCode, true);
                isNum = Double.TryParse(Convert.ToString(expression), NumberStyles.Any, cultureInfo.NumberFormat, out retNum);                
            }
            else
            {
                isNum = Double.TryParse(Convert.ToString(expression), NumberStyles.Any, CultureInfo.InvariantCulture, out retNum);                
            }

            return isNum;
        }

        // IsDate culture Function
        public static bool IsDate(object expression, string cultureCode)
        {
            DateTime rtnD;
            return DateTime.TryParse(Convert.ToString(expression), CultureInfo.CreateSpecificCulture(cultureCode), DateTimeStyles.None, out rtnD);
        }

        public static bool IsDate(object expression)
        {
            return IsDate(expression, GetCurrentCulture());
        }

        public static void SaveFile(string fullFileName, string data)
        {
            var buffer = StrToByteArray(data);
            SaveFile(fullFileName,buffer);
        }

        public static void SaveFile(string fullFileName, byte[] buffer)
        {
            if (File.Exists(fullFileName))
            {
                File.SetAttributes(fullFileName, FileAttributes.Normal);
            }
            FileStream fs = null;
            try
            {
                fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
                fs.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        public static string ReadFile(string filePath)
        {
            StreamReader reader = null;
            string fileContent;
            try
            {
                reader = File.OpenText(filePath);
                fileContent = reader.ReadToEnd();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return fileContent;
        }

        public static string FormatFolderPath(string folderPath)
        {
            if (String.IsNullOrEmpty(folderPath) || String.IsNullOrEmpty(folderPath.Trim()))
            {
                return "";
            }

            return folderPath.EndsWith("/") ? folderPath : folderPath + "/";
        }

        public static byte[] StrToByteArray(string str)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// Convert input stream to UTF8 string, can be used for text files.
        /// </summary>
        /// <param name="InpStream"></param>
        /// <returns></returns>
        public static string InputStreamToString(System.IO.Stream InpStream)
        {
            // Create a Stream object.
            // Find number of bytes in stream.
            var strLen = Convert.ToInt32(InpStream.Length);
            // Create a byte array.
            var strArr = new byte[strLen];
            // Read stream into byte array.
            InpStream.Read(strArr, 0, strLen);
            // Convert byte array to a text string.
            var strmContents = System.Text.Encoding.UTF8.GetString(strArr);
            return strmContents;
        }

        /// <summary>
        /// Convert input stream to base-64 string, can be used for image/binary files.
        /// </summary>
        /// <param name="InpStream"></param>
        /// <returns></returns>
        public static string Base64StreamToString(System.IO.Stream InpStream)
        {
            // Create a Stream object.
            // Find number of bytes in stream.
            var strLen = Convert.ToInt32(InpStream.Length);
            // Create a byte array.
            var strArr = new byte[strLen];
            // Read stream into byte array.
            InpStream.Read(strArr, 0, strLen);
            var strmContents = Convert.ToBase64String(strArr);
            return strmContents;
        }

        public static MemoryStream Base64StringToStream(string inputStr)
        {
            var myByte = System.Convert.FromBase64String(inputStr);
            var theMemStream = new MemoryStream();
            theMemStream.Write(myByte, 0, myByte.Length);
            return theMemStream;
        }

        public static Image SaveImgBase64ToFile(string FileMapPath, string strBase64Img)
        {
            // Save the image to a file.
            var mem = Utils.Base64StringToStream(strBase64Img);
            Image pic = Image.FromStream(mem);
            var fPath = FileMapPath;
            if (fPath.ToLower().EndsWith(".gif"))
            {
                pic.Save(fPath, System.Drawing.Imaging.ImageFormat.Gif);
            }
            else if (fPath.ToLower().EndsWith(".jpg") | fPath.ToLower().EndsWith(".jpeg"))
            {
                pic.Save(fPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else if (fPath.ToLower().EndsWith(".png"))
            {
                pic.Save(fPath, System.Drawing.Imaging.ImageFormat.Png);
            }
            else
            {
                pic.Save(fPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            return pic;
        }


        public static void SaveBase64ToFile(string FileMapPath, string strBase64)
        {
            // Save the image to a file.
            var mem = Utils.Base64StringToStream(strBase64);

            FileStream outStream = File.OpenWrite(FileMapPath);
            mem.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
        }



        public static string ReplaceFileExt(string fileName, string newExt)
        {
            var strOut = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + newExt;
            return strOut;
        }

        public static string FormatAsMailTo(string email)
        {
            var functionReturnValue = "";

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(email.Trim(Convert.ToChar(" "))))
            {
                if (email.IndexOf(Convert.ToChar("@")) != -1)
                {
                    functionReturnValue = "<a href=\"mailto:" + email + "\">" + email + "</a>";
                }
                else
                {
                    functionReturnValue = email;
                }
            }

            return CloakText(functionReturnValue);

        }


        // obfuscate sensitive data to prevent collection by robots and spiders and crawlers
        public static string CloakText(string personalInfo)
        {
            return CloakText(personalInfo, true);
        }

        public static string CloakText(string personalInfo, bool addScriptTag)
        {
            if (personalInfo != null)
            {
                var sb = new StringBuilder();
                var chars = personalInfo.ToCharArray();
                foreach (char chr in chars)
                {
                    sb.Append(((int) chr).ToString(CultureInfo.InvariantCulture));
                    sb.Append(',');
                }
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                if (addScriptTag)
                {
                    var sbScript = new StringBuilder();
                    sbScript.Append("<script type=\"text/javascript\">");
                    sbScript.Append("document.write(String.fromCharCode(" + sb + "))");
                    sbScript.Append("</script>");
                    return sbScript.ToString();
                }
                return string.Format("document.write(String.fromCharCode({0}))", sb);
            }
            return "";
        }

        public static void DeleteSysFile(string filePathName)
        {
            try
            {
                File.Delete(filePathName);
            }
            catch (Exception)
            {
                //ignore file could be locked.
                // should only be called if not important to remove file.                
            }
        }

        public static object GetCache(string strCacheKey)
        {
            return CmsProviderManager.Default.GetCache(strCacheKey);
        }

        public static void SetCache(string CacheKey, object objObject)
        {
            CmsProviderManager.Default.SetCache(CacheKey, objObject, DateTime.Now + new TimeSpan(0, 1, 0, 0));
        }

        public static void SetCache(string CacheKey, object objObject, DateTime AbsoluteExpiration)
        {
            CmsProviderManager.Default.SetCache(CacheKey, objObject, AbsoluteExpiration);
        }

        public static void RemoveCache(string strCacheKey)
        {
            CmsProviderManager.Default.RemoveCache(strCacheKey);
        }

        public static string[] ParseTemplateText(string templText)
        {
            char[] paramAry = { Convert.ToChar("["), Convert.ToChar("]") };

            //use double sqr brqckets as escape char.
            var foundEscapeChar = false;
            if (templText.IndexOf("[[", StringComparison.Ordinal) > 0 | templText.IndexOf("]]", StringComparison.Ordinal) > 0)
            {
                templText = templText.Replace("[[", "**SQROPEN**");
                templText = templText.Replace("]]", "**SQRCLOSE**");
                foundEscapeChar = true;
            }

            var strOut = templText.Split(paramAry);

            if (foundEscapeChar)
            {
                for (var lp = 0; lp <= strOut.GetUpperBound(0); lp++)
                {
                    if (strOut[lp].Contains("**SQROPEN**"))
                    {
                        strOut[lp] = strOut[lp].Replace("**SQROPEN**", "[");
                    }
                    if (strOut[lp].Contains("**SQRCLOSE**"))
                    {
                        strOut[lp] = strOut[lp].Replace("**SQRCLOSE**", "]");
                    }
                }
            }

            return strOut;
        }

        /// <summary>
        /// CleanInput strips out all nonalphanumeric characters except periods (.), at symbols (@), and hyphens (-), and returns the remaining string. However, you can modify the regular expression pattern so that it strips out any characters that should not be included in an input string.
        /// </summary>
        /// <param name="strIn">Dirty String</param>
        /// <returns>Clean String</returns>
        public static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings. 
            return Regex.Replace(strIn, @"[^\w\.@-]", "",RegexOptions.None);
        }

        /// <summary>
        /// Strip accents from string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>String without accents</returns>
        public static string StripAccents(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s.Normalize(NormalizationForm.FormKD))
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        break;

                    default:
                        sb.Append(c);
                        break;
                }
            return sb.ToString();
        }

        /// <summary>
        /// Get Azure Authentication for Translator.
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="ClientSecret"></param>
        /// <returns></returns>
        public static AdmAccessToken GetAzureAccessToken(String ClientId, String ClientSecret)
        {
            var admAuth = new AdmAuthentication(ClientId, ClientSecret);
            var token = admAuth.GetAccessToken();
            return token;
        }

    	/// <summary>
    	/// Create dictionary of config setting from files
    	/// </summary>
    	/// <param name="DefaultConfigMapPath"></param>
    	/// <param name="SecondaryConfigMapPath"></param>
    	/// <param name="configNameCSV">CSV list of sectiuonnames to be returned, "" for all</param>
    	/// <param name="AdvancedFlag">Flag to select advanced settings "1"=Advanved Only, "0"=Simple Only,""=All </param>
    	/// <returns>Dictionary of all config settings</returns>
    	public static Dictionary<String,NBrightSetting> ConfigBuildDictionary(String DefaultConfigMapPath, String SecondaryConfigMapPath,String configNameCSV = "", String AdvancedFlag = "")
        {
            var outDict = new Dictionary<String, NBrightSetting>();

            if (File.Exists(DefaultConfigMapPath))
            {

                var xmlConfigDoc = new System.Xml.XmlDataDocument();
                System.Xml.XmlNodeList xmlNodList = null;
                xmlConfigDoc.Load(DefaultConfigMapPath);

                if (configNameCSV == "")
                {
                    xmlNodList = xmlConfigDoc.SelectNodes("root/*");
                    if (xmlNodList != null)
                    {
                        foreach (XmlNode xNod in xmlNodList)
                        {
                            configNameCSV += xNod.Name + ",";
                        }
                        configNameCSV = configNameCSV.TrimEnd(',');
                    }
                }

                foreach (var configName in configNameCSV.Split(','))
                {
                    xmlNodList = xmlConfigDoc.SelectNodes("root/" + configName + "/*");
                    if (xmlNodList != null)
                    {
                        foreach (XmlNode xNod in xmlNodList)
                        {
                            if (xNod.Attributes != null && xNod.Attributes["value"] != null)
                            {
								if ((AdvancedFlag == "") || (AdvancedFlag == "0" && (xNod.Attributes["advanced"] == null || xNod.Attributes["advanced"].Value == "0")) || (AdvancedFlag == "1" && xNod.Attributes["advanced"] != null && xNod.Attributes["advanced"].Value == "1"))
								{
									var obj = new NBrightSetting();
									obj.Key = configName + "." + xNod.Name;
									obj.Name = xNod.Name;
									obj.Type = "";
									if (xNod.Attributes["type"] != null) obj.Type = xNod.Attributes["type"].Value;
									obj.Value = "";
									if (xNod.Attributes["value"] != null) obj.Value = xNod.Attributes["value"].Value;

									if (outDict.ContainsKey(configName + "." + xNod.Name))
									{
										outDict[configName + "." + xNod.Name] = obj;
									}
									else
									{
										outDict.Add(configName + "." + xNod.Name, obj);
									}
								}
                            }
                        }
                    }

                }

                //overwrite with secondary file data
                if (File.Exists(SecondaryConfigMapPath))
                {
                    xmlConfigDoc = new System.Xml.XmlDataDocument();
                    xmlConfigDoc.Load(SecondaryConfigMapPath);
                    foreach (var configName in configNameCSV.Split(','))
                    {
                        xmlNodList = xmlConfigDoc.SelectNodes("root/" + configName + "/*");
                        if (xmlNodList != null)
                        {
                            foreach (XmlNode xNod in xmlNodList)
                            {
                                if (xNod.Attributes != null && xNod.Attributes["value"] != null)
                                {
									if ((AdvancedFlag == "") || (AdvancedFlag == "0" && (xNod.Attributes["advanced"] == null || xNod.Attributes["advanced"].Value == "0")) || (AdvancedFlag == "1" && xNod.Attributes["advanced"] != null && xNod.Attributes["advanced"].Value == "1"))
									{
										var obj = new NBrightSetting();
										obj.Key = configName + "." + xNod.Name;
										obj.Name = xNod.Name;
										obj.Type = "";
										if (xNod.Attributes["type"] != null) obj.Type = xNod.Attributes["type"].Value;
										obj.Value = "";
										if (xNod.Attributes["value"] != null) obj.Value = xNod.Attributes["value"].Value;

										if (outDict.ContainsKey(configName + "." + xNod.Name))
										{
											outDict[configName + "." + xNod.Name] = obj;
										}
										else
										{
											outDict.Add(configName + "." + xNod.Name, obj);
										}
									}
                                }
                            }
                        }

                    }


                }
            }

            return outDict;

        }

		/// <summary>
		/// Create Dictionary of config sections  i.e. all "root/*" nodes
		/// </summary>
		/// <param name="DefaultConfigDictionary">Dictionary of all Config settings. (Created by ConfigBuildDictionary function)</param>
		/// <returns></returns>
		public static  List<String> ConfigBuildSectionList(Dictionary<String, NBrightSetting> DefaultConfigDictionary)
		{
			var outL = new List<String>();

			foreach (var i in DefaultConfigDictionary)
			{
				var secName = i.Key.Split('.')[0];
				if (secName != null)
				{
					if (!outL.Contains(secName)) outL.Add(secName);					
				}
			}

			return outL;
		}


		/// <summary>
		/// Take the xml config file and convert it to a Template.
		/// </summary>
        /// <param name="settingDict">Dictionary of Settings</param>
		/// <param name="sectionName">Name of the config section to edit (e.g. "products")</param>
		/// <returns>String html nbright template for displaying settings options</returns>
        public static String ConfigConvertToTemplate(Dictionary<String, NBrightSetting> settingDict,String sectionName)
		{
			var strTempl = "";

            if (settingDict != null) 
			{
				strTempl += "<table><th></th><th></th>";
				foreach (var i in settingDict)
                    {
						if (i.Key.ToLower().StartsWith(sectionName.ToLower()) | sectionName == "")
						{
							strTempl += "<tr><td>";
							strTempl += i.Key + " : ";
							strTempl += "</td><td>";
							var obj = i.Value;
							switch (obj.Type)
							{
								case "decimal":
									strTempl += "[<tag id='config" + i.Key + "' type='textbox' text='" + obj.Value + "' width='100px' maxlength='50'  />]";
									break;
								case "int":
									strTempl += "[<tag id='config" + i.Key + "' type='textbox' text='" + obj.Value + "' width='100px' maxlength='50'  />]";
									break;
								case "bool":
									strTempl += "[<tag id='config" + i.Key + "' type='checkbox' checked='" + obj.Value + "' />]";
									break;
								default:
									strTempl += "[<tag id='config" + i.Key + "' type='textbox' text='" + obj.Value + "' width='600px' maxlength='250'  />]";
									break;
							}
							strTempl += "</td>";
						}
                    }
				strTempl += "</table>";
			}
			return strTempl;
		}

		/// <summary>
		/// Take a Repeater and convert it to XML config data.
		/// </summary>
		/// <param name="xmlConfig"></param>
		/// <returns></returns>
		public static System.Xml.XmlDataDocument ConfigConvertToXml()
		{
			var xmlDoc = new XmlDataDocument();

			return xmlDoc;
		}


    }
}

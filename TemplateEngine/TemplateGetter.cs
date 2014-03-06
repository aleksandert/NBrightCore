using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using NBrightCore.common;

namespace NBrightCore.TemplateEngine
{
    public class TemplateGetter
    {

        private TemplateController TemplCtrl1;
        private TemplateController TemplCtrl2;
        private TemplateController TemplCtrl3;
        private TemplateController TemplCtrl4;

        /// <summary>
        /// Initialize the template getter 
        /// </summary>
        /// <param name="primaryMapPath">folder to look for a themes (On a multiple portal system this will usually be the portal root)</param>
        /// <param name="secondaryMapPath">fallback folder to look for a themes if not found in primary (Usually the module admin folder, where default installed templates are saved)</param>
        /// <param name="defaultThemeFolder">Default theme folder name to look for</param>
        /// <param name="themeFolder">custom theme folder name to look for, if no template is found here the default theme will be searched.</param>
        public TemplateGetter(string primaryMapPath, string secondaryMapPath, string defaultThemeFolder = "NBrightTemplates", string themeFolder = "")
        {
            if (themeFolder != "")
            {
                TemplCtrl1 = new TemplateController(primaryMapPath, themeFolder);
                TemplCtrl2 = new TemplateController(secondaryMapPath, themeFolder);                
            }
            TemplCtrl3 = new TemplateController(primaryMapPath, defaultThemeFolder);
            TemplCtrl4 = new TemplateController(secondaryMapPath, defaultThemeFolder);
        }

        /// <summary>
        /// Get template from the filesytem, search primary mappath (both themes), if not found search socendary mappath (both themes)
        /// </summary>
        /// <param name="templatename">template file anme</param>
        /// <param name="lang">langauge to get</param>
        /// <param name="replaceTemplateTokens">replace the [Template:*] tokens</param>
        /// <param name="replaceStringTokens">replace the [String:*] tokens</param>
        /// <returns></returns>
        public string GetTemplateData(string templatename, string lang, bool replaceTemplateTokens = true, bool replaceStringTokens = true)
        {
            var templateData = "";
            var objT = new Template("");
            if (TemplCtrl1 != null)
            {
                // search custom themefolders
                objT = TemplCtrl1.GetTemplate(templatename, lang);
                templateData = objT.TemplateData;
                if (!objT.IsTemplateFound)
                {
                    objT = TemplCtrl2.GetTemplate(templatename, lang);
                    templateData = objT.TemplateData;
                }                
            }
            if (!objT.IsTemplateFound)
            {
                // search default themefolders
                objT = TemplCtrl3.GetTemplate(templatename, lang);
                templateData = objT.TemplateData;
                if (!objT.IsTemplateFound)
                {
                    objT = TemplCtrl4.GetTemplate(templatename, lang);
                    templateData = objT.TemplateData;
                }                                
            }


            if (replaceTemplateTokens) templateData = ReplaceTemplateTokens(templateData, lang);

            if (replaceStringTokens) templateData = ReplaceResourceString(templateData);

            return templateData;
        }

        public string ReplaceTemplateTokens(string templText, string lang, int recursiveCount = 0)
        {
            var strOut = templText;
            if (TemplCtrl1 != null)
            {
                strOut = TemplCtrl1.ReplaceTemplateTokens(strOut, lang, recursiveCount);
                strOut = TemplCtrl2.ReplaceTemplateTokens(strOut, lang, recursiveCount);
            }
            strOut = TemplCtrl3.ReplaceTemplateTokens(strOut, lang, recursiveCount);
            strOut = TemplCtrl4.ReplaceTemplateTokens(strOut, lang, recursiveCount);
            return strOut;
        }

        public string ReplaceResourceString(string templText)
        {
            var strOut = templText;
            if (TemplCtrl1 != null)
            {
                strOut = TemplCtrl1.ReplaceResourceString(strOut);
            }
            strOut = TemplCtrl3.ReplaceResourceString(strOut);
            return strOut;

        }

        public void SaveTemplate(string templatename, string lang, string templatedata)
        {
            // save the template on secondary folder (usually portal in multiportal system)
            if (TemplCtrl1 != null)
                TemplCtrl2.SaveTemplate(templatename,lang,templatedata); // save in custom theme
            else
                TemplCtrl4.SaveTemplate(templatename, lang, templatedata); // save in default theme
        }

        public void SaveTemplate(string templatename, string templatedata)
        {
            SaveTemplate(templatename, "Default", templatedata);
        }


    }
}

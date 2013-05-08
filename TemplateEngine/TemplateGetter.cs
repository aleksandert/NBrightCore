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

        public TemplateGetter(string primaryMapPath, string secondaryMapPath, string themeFolder = "NBrightTemplates")
        {
            TemplCtrl1 = new TemplateController(primaryMapPath, themeFolder);
            TemplCtrl2 = new TemplateController(secondaryMapPath, themeFolder);
        }

        public string GetTemplateData(string templatename, string lang, bool replaceTemplateTokens = true)
        {
            var objT = TemplCtrl1.GetTemplate(templatename, lang);
            var templateData = objT.TemplateData;
            if (!objT.IsTemplateFound)
            {
                objT = TemplCtrl2.GetTemplate(templatename, lang);
                templateData = objT.TemplateData;                
            }

            if (replaceTemplateTokens)
            {
                templateData = ReplaceTemplateTokens(templateData, lang);
            }

            templateData = ReplaceResourceString(templateData);

            return templateData;
        }

        public string ReplaceTemplateTokens(string templText, string lang, int recursiveCount = 0)
        {
            var strOut = TemplCtrl1.ReplaceTemplateTokens(templText, lang, recursiveCount);
            strOut = TemplCtrl2.ReplaceTemplateTokens(strOut, lang, recursiveCount);
            return strOut;
        }

        public string ReplaceResourceString(string templText)
        {
            var strOut = TemplCtrl1.ReplaceResourceString(templText);
            return strOut;
        }

    }
}

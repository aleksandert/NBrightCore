using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace NBrightCore.providers
{
    public abstract class GenXProvider : ProviderBase
    {

        public abstract bool CreateGenControl(string ctrltype, Control container, XmlNode xmlNod, string rootname = "genxml", string databindColum = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null, List<Boolean> visibleStatusIn = null);

        public abstract string GetField(Control ctrl);

        public abstract void SetField(Control ctrl, string newValue);

        public abstract string GetGenXml(List<Control> genCtrls, XmlDataDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml");

        public abstract string GetGenXmlTextBox(List<Control> genCtrls, XmlDataDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml");

        public abstract object PopulateGenObject(List<Control> genCtrls, object obj);

        public abstract String TestOfDataBinding(object sender, EventArgs e, Boolean currentVisibleStatus);

    }
}

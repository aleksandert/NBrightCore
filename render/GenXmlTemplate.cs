﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using NBrightCore.common;

namespace NBrightCore.render
{
    public class GenXmlTemplate : ITemplate
    {

        protected string[] AryTempl;
        public List<string> MetaTags;
        protected string Rootname = "genxml";
        protected string DatabindColumn = "XMLData";
        protected Page CurrentPage;
        protected string EditCultureCode = "";
        private Dictionary<string, string> hiddenFields;
		private string _ResourcePath = "";

        public String GetHiddenFieldValue(string Key)
        {
            if (hiddenFields.ContainsKey(Key.ToLower()))
            {
                return hiddenFields[Key.ToLower()];
            }
            return "";
        }

        public String SortItemId { get; set; }

        public void AddProvider()
        {
            
        }

        public GenXmlTemplate(string templateText): this(templateText, "genxml", "XMLData")
        {
        }

        public GenXmlTemplate(string templateText, string xmlRootName): this(templateText, xmlRootName, "XMLData")
        {
        }

        public GenXmlTemplate(string templateText, string xmlRootName, string dataBindXmlColumn)
        {
            //set the rootname of the xml, this allows for compatiblity with legacy xml structure
            Rootname = xmlRootName;
            AryTempl = Utils.ParseTemplateText(templateText);
            DatabindColumn = dataBindXmlColumn;
            MetaTags = new List<string>();
            hiddenFields = new Dictionary<string, string>();

            // find any meta tags
            var xmlDoc = new XmlDocument();
            string ctrltype = "";
            foreach (var s in AryTempl)
            {
                var htmlDecode = System.Web.HttpUtility.HtmlDecode(s);
                    if (htmlDecode != null && htmlDecode.ToLower().StartsWith("<tag"))
                    {
                        var strXml = System.Web.HttpUtility.HtmlDecode(s);
                        strXml = "<root>" + strXml + "</root>";

                        xmlDoc.LoadXml(strXml);
                        var xmlNod = xmlDoc.SelectSingleNode("root/tag");
                        if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["type"] != null)))
                        {
                            ctrltype = xmlNod.Attributes["type"].InnerXml.ToLower();
                        }

                        if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["ctrltype"] != null)))
                        {
                            ctrltype = xmlNod.Attributes["ctrltype"].InnerXml.ToLower();
                        }

                        if (ctrltype=="meta")
                        {
                            MetaTags.Add(s);
							if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null)))
							{
								if (xmlNod.Attributes["id"].Value.ToLower() == "resourcepath")
								{
									_ResourcePath = xmlNod.Attributes["value"].Value;
								}
							}
						}

                    }
            }

        }


        /// <summary>
        /// tag types: 
        /// <para>fileupload : file upload control</para>
        /// <para>linkbutton : link button to post back a response</para>
        /// <para>translatebutton : ?</para>
        /// <para>valueof : Display value of a field.</para>
        /// <para>breakof : Display value of a field and change newline to &gt;br/&lt;</para>
        /// <para>testof : Test for values and display text/html based on true or false.</para>
        /// <para>htmlof : Display encoded html, saved as html in the xml, mainly for the richtext editor.</para>
        /// <para>label : label control</para>
        /// <para>hidden : hidden field.</para>
        /// <para>const : hidden field, but without ability to change values on save.</para>
        /// <para>textbox : Textbox control.</para>
        /// <para>dropdownlist : dropdownlist control.</para>
        /// <para>checkboxlist : checkbox list control.</para>
        /// <para>checkbox : checkbox control.</para>
        /// <para>radiobuttonlist : radiobuttonlist control.</para>
        /// <para>rvalidator : field validator control.</para>
        /// <para>rfvalidator : required field validator.</para>
        /// <para>revalidator: RegExpr validator.</para>
        /// <para>cvalidator : custom validator.</para>
        /// <para>validationsummary : validation summary control.</para> 
        /// </summary>
        public void InstantiateIn(Control container)
        {
            if (CurrentPage != null)
            {
                container.Page = CurrentPage;                
            }

            var xmlDoc = new XmlDocument();
            string ctrltype = "";

            for (var lp = 0; lp <= AryTempl.GetUpperBound(0); lp++)
            {

                if ((AryTempl[lp] != null))
                {
                    var htmlDecode = System.Web.HttpUtility.HtmlDecode(AryTempl[lp]);
                    if (htmlDecode != null && htmlDecode.ToLower().StartsWith("<tag"))
                    {
                        var strXml = System.Web.HttpUtility.HtmlDecode(AryTempl[lp]);
                        strXml = "<root>" + strXml + "</root>";

                        xmlDoc.LoadXml(strXml);
                        var xmlNod = xmlDoc.SelectSingleNode("root/tag");
                        if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["type"] != null)))
                        {
                            ctrltype = xmlNod.Attributes["type"].InnerXml.ToLower();
                        }

                        if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["ctrltype"] != null)))
                        {
                            ctrltype = xmlNod.Attributes["ctrltype"].InnerXml.ToLower();
                        }


                        if (!string.IsNullOrEmpty(ctrltype))
                        {
							// get any Langauge Resource Data from CMS
							if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["resourcekey"] != null)))
							{
								xmlNod = GetCMSResourceData(xmlDoc);
							}

                            switch (ctrltype)
                            {
                                case "fileupload":
                                    CreateFileUpload(container, xmlNod);
                                    break;
                                case "linkbutton":
                                    CreateLinkButton(container, xmlNod);
                                    break;
                                case "translatebutton":
                                    CreateTransButton(container, xmlNod);
                                    break;
                                case "valueof":
                                    CreateValueOf(container, xmlNod);
                                    break;
                                case "breakof":
                                    CreateBreakOf(container, xmlNod);
                                    break;
                                case "checkboxlistof":
                                    CreateCheckBoxListOf(container, xmlNod);
                                    break;
                                case "testof":
                                    CreateTestOf(container, xmlNod);
                                    break;
                                case "htmlof":
                                    CreateHtmlOf(container, xmlNod);
                                    break;
                                case "label":
                                    CreateLabel(container, xmlNod);
                                    break;
                                case "hidden":
                                    CreateHidden(container, xmlNod);
                                    if ((xmlNod != null) && (xmlNod.Attributes != null) && (xmlNod.Attributes["id"] != null) && (xmlNod.Attributes["value"] != null))
                                    {
                                            if (!hiddenFields.ContainsKey(xmlNod.Attributes["id"].InnerXml.ToLower())) hiddenFields.Add(xmlNod.Attributes["id"].InnerXml.ToLower(), xmlNod.Attributes["value"].InnerXml);
                                    }
                                    break;
                                case "const":
                                    CreateConst(container, xmlNod);
                                    if ((xmlNod != null) && (xmlNod.Attributes != null) && (xmlNod.Attributes["id"] != null) && (xmlNod.Attributes["value"] != null))
                                    {
                                        if (!hiddenFields.ContainsKey(xmlNod.Attributes["id"].InnerXml.ToLower())) hiddenFields.Add(xmlNod.Attributes["id"].InnerXml.ToLower(), xmlNod.Attributes["value"].InnerXml);
                                    }
                                    break;
                                case "textbox":
                                    CreateTextbox(container, xmlNod);
                                    break;
                                case "dropdownlist":
                                    CreateDropDownList(container, xmlNod);
                                    break;
                                case "checkboxlist":
                                    CreateCheckBoxList(container, xmlNod);
                                    break;
                                case "checkbox":
                                    CreateCheckBox(container, xmlNod);
                                    break;
                                case "radiobuttonlist":
                                    CreateRadioButtonList(container, xmlNod);
                                    break;
                                case "rvalidator":
                                    CreateRangeValidator(container, xmlNod);
                                    break;
                                case "rfvalidator":
                                    CreateRequiredFieldValidator(container, xmlNod);
                                    break;
                                case "revalidator":
                                    CreateRegExValidator(container, xmlNod);
                                    break;
                                case "cvalidator":
                                    CreateCustomValidator(container, xmlNod);
                                    break;
                                case "validationsummary":
                                    CreateValidationSummary(container, xmlNod);
                                    break;
                                case "meta":
                                    if ((xmlNod != null) && (xmlNod.Attributes != null) && (xmlNod.Attributes["id"] != null) && (xmlNod.Attributes["value"] != null))
                                    {
                                        if (!hiddenFields.ContainsKey(xmlNod.Attributes["id"].InnerXml.ToLower())) hiddenFields.Add(xmlNod.Attributes["id"].InnerXml.ToLower(), xmlNod.Attributes["value"].InnerXml);
                                    }
                                    // meta tags are for passing data to the system only and should not be displayed. (e.g. orderby field)
                                    break;
                                default:

                                    var providerCtrl = false;

                                    //check for any template providers.
                                    var providerList = providers.GenXProviderManager.ProviderList;
                                    if (providerList != null)
                                    {                                    
                                        foreach (var prov in providerList)
                                        {
                                            providerCtrl = prov.Value.CreateGenControl(ctrltype, container, xmlNod, Rootname, DatabindColumn);
                                            if (providerCtrl)
                                            {
                                                break;
                                            }
                                        }   
                                    }


                                    if (providerCtrl == false)
                                    {
                                        var lc = new Literal();
                                        xmlDoc.LoadXml(strXml);
                                        xmlNod = xmlDoc.SelectSingleNode("root/tag");
                                        if ((xmlNod != null) && (xmlNod.Attributes != null))
                                        {
                                            lc.Text = xmlNod.Attributes[0].InnerXml;
                                        }
                                        lc.DataBinding += GeneralDataBinding;
                                        container.Controls.Add(lc);
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        var lc = new Literal {Text = AryTempl[lp]};
                        container.Controls.Add(lc);
                    }
                }
            }

        }

        #region "create controls"


        private void CreateValueOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                lc.Text = "databind:" + xmlNod.Attributes["databind"].InnerXml;
            }

            // pass structured string to format data
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                var strFormat = "";
                if (xmlNod.Attributes != null && (xmlNod.Attributes["format"] != null))
                {
                    strFormat = xmlNod.Attributes["format"].InnerXml.Replace(":", "**COLON**");
                }
                if (xmlNod.Attributes["datatype"].InnerText.ToLower() == "date")
                    {
                        if (strFormat == "") strFormat = "d";
                        lc.Text = "date:" + strFormat + ":" + lc.Text;
                    }
                    if (xmlNod.Attributes["datatype"].InnerText.ToLower() == "double")
                    {
                        lc.Text = "double:" + strFormat + ":" + lc.Text;
                    }                

            }

            lc.DataBinding += ValueOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateBreakOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                lc.Text = "databind:" + xmlNod.Attributes["databind"].InnerXml;
            }

            lc.DataBinding += BreakOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateCheckBoxListOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }

            lc.DataBinding += ChkBoxListOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateTestOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal {Text = xmlNod.OuterXml};
            lc.DataBinding += TestOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateHtmlOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                lc.Text = "databind:" + xmlNod.Attributes["databind"].InnerXml;
            }
            lc.DataBinding += HtmlOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateRadioButtonList(Control container, XmlNode xmlNod)
        {
            var rbl = new RadioButtonList();
            rbl = (RadioButtonList) GenXmlFunctions.AssignByReflection(rbl, xmlNod);

            var dataTyp = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                dataTyp = xmlNod.Attributes["datatype"].InnerXml;
                rbl.Attributes.Add("datatype", dataTyp);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                rbl.Attributes.Add("searchindex", "1");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                rbl.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["data"] != null))
            {
                string[] strListValue;
                if ((xmlNod.Attributes["datavalue"] != null))
                {
                    strListValue = xmlNod.Attributes["datavalue"].InnerXml.Split(';');
                }
                else
                {
                    strListValue = xmlNod.Attributes["data"].InnerXml.Split(';');
                }
                var strList = xmlNod.Attributes["data"].InnerXml.Split(';');
                for (var lp = 0; lp <= strList.GetUpperBound(0); lp++)
                {
                    var li = new ListItem();
                    switch (dataTyp.ToLower())
                    {
                        case "double":
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                        default:
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                    }
                    rbl.Items.Add(li);
                }

            }

            rbl.Visible = GetRoleVisible(xmlNod.OuterXml);
            rbl.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            rbl.DataBinding += RblDataBinding;
            container.Controls.Add(rbl);
        }

        private void CreateCheckBoxList(Control container, XmlNode xmlNod)
        {
            var chk = new CheckBoxList();
            chk = (CheckBoxList)GenXmlFunctions.AssignByReflection(chk, xmlNod);

            var dataTyp = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                dataTyp = xmlNod.Attributes["datatype"].InnerXml;
                chk.Attributes.Add("datatype", dataTyp);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                chk.Attributes.Add("searchindex", "1");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["data"] != null))
            {
                string[] strListValue;
                if ((xmlNod.Attributes["datavalue"] != null))
                {
                    strListValue = xmlNod.Attributes["datavalue"].InnerXml.Split(';');
                }
                else
                {
                    strListValue = xmlNod.Attributes["data"].InnerXml.Split(';');
                }
                string[] strList = xmlNod.Attributes["data"].InnerXml.Split(';');
                for (int lp = 0; lp <= strList.GetUpperBound(0); lp++)
                {
                    var li = new ListItem();
                    li.Attributes.Add("datavalue", strListValue[lp]);                        
                    switch (dataTyp.ToLower())
                    {
                        case "double":
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                        default:
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                    }
                    if (li.Value != "") chk.Items.Add(li);
                }
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatabs"] != null))
            {
                var tList = providers.CmsProviderManager.Default.GetTabList(Utils.GetCurrentCulture());

                foreach (var tItem in tList)
                {
                    var li2 = new ListItem();
                    li2.Text = tItem.Value;
                    li2.Value = tItem.Key.ToString("");
                    if (li2.Value != "") chk.Items.Add(li2);
                }
            }

            chk.Visible = GetRoleVisible(xmlNod.OuterXml);
            chk.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            chk.DataBinding += ChkBDataBinding;
            if (chk.Items.Count > 0)
            { // only display if values exist.
                container.Controls.Add(chk);                
            }
        }

        private void CreateCheckBox(Control container, XmlNode xmlNod)
        {
            var chk = new CheckBox();
            chk = (CheckBox)GenXmlFunctions.AssignByReflection(chk, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                chk.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                chk.Attributes.Add("searchindex", "1");
            }

			if (xmlNod.Attributes != null && (xmlNod.Attributes["checked"] != null))
			{
				if (xmlNod.Attributes["checked"].Value == "1" | xmlNod.Attributes["checked"].Value == "True")
				{
					chk.Checked = true;
				}
			}


            chk.Visible = GetRoleVisible(xmlNod.OuterXml);
            chk.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            chk.DataBinding += ChkBoxDataBinding;
            container.Controls.Add(chk);
        }

        private void CreateDropDownList(Control container, XmlNode xmlNod)
        {
            var ddl = new DropDownList();
            ddl = (DropDownList)GenXmlFunctions.AssignByReflection(ddl, xmlNod);

            var dataTyp = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                dataTyp = xmlNod.Attributes["datatype"].InnerXml;
                ddl.Attributes.Add("datatype", dataTyp);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                ddl.Attributes.Add("searchindex", "1");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                ddl.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatabs"] != null))
            {
                ddl.Attributes.Add("datatabs", xmlNod.Attributes["datatabs"].InnerXml);
                var tList = providers.CmsProviderManager.Default.GetTabList(Utils.GetCurrentCulture());

                if (xmlNod.Attributes["datatabs"].InnerXml.ToLower() == "blank")
                {
                    var li = new ListItem();
                    li.Text = "";
                    li.Value = "";
                    ddl.Items.Add(li);                    
                }

                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value ;
                    li.Value = tItem.Key.ToString("");
                    ddl.Items.Add(li);
                }
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["dataculture"] != null))
            {
                ddl.Attributes.Add("dataculture", xmlNod.Attributes["dataculture"].InnerXml);

                if (xmlNod.Attributes["dataculturevalue"] != null)
                {
                    var li = new ListItem();
                    li.Text = xmlNod.Attributes["dataculture"].InnerXml;
                    li.Value = xmlNod.Attributes["dataculturevalue"].InnerXml;
                    ddl.Items.Add(li);
                }
                var cList = providers.CmsProviderManager.Default.GetCultureCodeList();

                foreach (var cItem in cList)
                {
                    var li = new ListItem();
                    li.Text = cItem;
                    li.Value = cItem;
                    ddl.Items.Add(li);
                }
            }


            if (xmlNod.Attributes != null && (xmlNod.Attributes["data"] != null))
            {
                string[] strListValue;
                if ((xmlNod.Attributes["datavalue"] != null))
                {
                    strListValue = xmlNod.Attributes["datavalue"].InnerXml.Split(';');
                }
                else
                {
                    strListValue = xmlNod.Attributes["data"].InnerXml.Split(';');
                }
                var strList = xmlNod.Attributes["data"].InnerXml.Split(';');
                for (var lp = 0; lp <= strList.GetUpperBound(0); lp++)
                {
                    var li = new ListItem();
                    switch (dataTyp.ToLower())
                    {
                        case "double":
                            li.Text = Utils.FormatToDisplay(strList[lp],TypeCode.Double);
                            li.Value = Utils.FormatToDisplay(strListValue[lp], TypeCode.Double,"N");
                            break;
                        default:
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                    }
                    ddl.Items.Add(li);
                }
            }

            ddl.Visible = GetRoleVisible(xmlNod.OuterXml);
            ddl.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            ddl.DataBinding += DdListDataBinding;
            container.Controls.Add(ddl);
        }

        private static void CreateRangeValidator(Control container, XmlNode xmlNod)
        {
            var rfv = new RangeValidator {Text = "*"};
            rfv = (RangeValidator)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }

        private static void CreateRegExValidator(Control container, XmlNode xmlNod)
        {

            var rfv = new RegularExpressionValidator { Text = "*" };
            rfv = (RegularExpressionValidator)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }

        private static void CreateValidationSummary(Control container, XmlNode xmlNod)
        {
            var rfv = new ValidationSummary();
            rfv = (ValidationSummary)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }

        private static void CreateCustomValidator(Control container, XmlNode xmlNod)
        {
            var rfv = new CustomValidator { Text = "*" };
            rfv = (CustomValidator)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }

        private static void CreateRequiredFieldValidator(Control container, XmlNode xmlNod)
        {
            var rfv = new RequiredFieldValidator { Text = "*" };
            rfv = (RequiredFieldValidator)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }
        /// <summary title="Textbox Control">
        /// <para class="example">
        /// [<tag id="txtModuleKey" type="textbox" width="150" maxlength="50" />]
        /// [<tag id="txtSummary" type="textbox" height="100" width="500" maxlength="200" textmode="MultiLine"/>]
        /// </para>
        /// </summary>
        private TextBox GetCreateTextbox(XmlNode xmlNod)
        {
            var txt = new TextBox {Text = ""};

            txt = (TextBox)GenXmlFunctions.AssignByReflection(txt, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["text"] != null))
            {
                txt.Text = xmlNod.Attributes["text"].InnerXml;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                txt.Attributes.Add("datatype", xmlNod.Attributes["datatype"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                txt.Attributes.Add("searchindex", "1");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                txt.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["format"] != null))
            {
                txt.Attributes.Add("format", xmlNod.Attributes["format"].InnerXml);
            }

            txt.Visible = GetRoleVisible(xmlNod.OuterXml);
            txt.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            txt.DataBinding += TextDataBinding;
            return txt;
        }

        private void CreateTextbox(Control container, XmlNode xmlNod)
        {
            var txt = GetCreateTextbox(xmlNod);
            container.Controls.Add(txt);
        }

        private void CreateFileUpload(Control container, XmlNode xmlNod)
        {
            var txt = new TextBox();
            var fup = new FileUpload();
            var hid = new HtmlGenericControl();
            var hidInfo = new HtmlGenericControl();
            hid.Attributes["value"] = "";
            hidInfo.Attributes["value"] = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["cssclass"] != null))
            {
                fup.CssClass = xmlNod.Attributes["cssclass"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                fup.ID = xmlNod.Attributes["id"].InnerXml;
                hid.ID = "hid" + xmlNod.Attributes["id"].InnerXml;
                txt.ID = "txt" + xmlNod.Attributes["id"].InnerXml;
                hidInfo.ID = "hidInfo" + xmlNod.Attributes["id"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["tooltip"] != null))
            {
                fup.ToolTip = xmlNod.Attributes["tooltip"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["textwidth"] != null))
            {
                txt.Width = Convert.ToInt16(xmlNod.Attributes["textwidth"].InnerXml);
            }
            else
            {
                txt.Width = 150;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["maxlength"] != null))
            {
                txt.MaxLength = Convert.ToInt16(xmlNod.Attributes["maxlength"].InnerXml);
            }
            else
            {
                txt.MaxLength = 50;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["width"] != null))
            {
                fup.Width = Convert.ToInt16(xmlNod.Attributes["width"].InnerXml);
            }

            txt.Visible = GetRoleVisible(xmlNod.OuterXml);
            txt.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            if (txt.Width == 0 )
            {
                txt.Visible = false;
            }

            fup.Visible = GetRoleVisible(xmlNod.OuterXml);
            fup.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            txt.DataBinding += TextDataBinding;
            container.Controls.Add(txt);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["separator"] != null) & fup.Visible)
            {
                var lc = new Literal {Text = xmlNod.Attributes["separator"].InnerXml};
                if (lc.Text == "br")
                {
                    lc.Text = "<br />";
                }
                container.Controls.Add(lc);
            }

            container.Controls.Add(fup);
            hid.DataBinding += HiddenDataBinding;
            container.Controls.Add(hid);
            hidInfo.DataBinding += HiddenDataBinding;
            container.Controls.Add(hidInfo);
        }

        private static HtmlGenericControl GetHiddenFieldCtrl(XmlNode xmlNod)
        {
            var hid = new HtmlGenericControl("input");
            hid.Attributes.Add("type", "hidden");
            if (xmlNod.Attributes != null)
            {
                var dataType = "";
                if ((xmlNod.Attributes["datatype"] != null))
                {
                    dataType = xmlNod.Attributes["datatype"].InnerXml;
                    hid.Attributes.Add("datatype", dataType);
                }

                if ((xmlNod.Attributes["id"] != null))
                {
                    hid.ID = xmlNod.Attributes["id"].InnerXml.ToLower();
                    // check for legacy datatype naming convension
                    if (xmlNod.Attributes["id"].InnerXml.ToLower().StartsWith("dbl"))
                    {
                        dataType = "double";
                    }
                    if (xmlNod.Attributes["id"].InnerXml.ToLower().StartsWith("dte"))
                    {
                        dataType = "date";
                    }
                    if (xmlNod.Attributes["id"].InnerXml.ToLower().StartsWith("date"))
                    {
                        dataType = "date";
                    }
                }

                if ((xmlNod.Attributes["const"] != null))
                {
                    dataType = xmlNod.Attributes["const"].InnerXml;
                    hid.Attributes.Add("const", dataType);
                }

                if ((xmlNod.Attributes["value"] != null))
                {
                    if (dataType.ToLower() == "double")
                    {
                        hid.Attributes.Add("value",xmlNod.Attributes["value"].InnerXml);
                    }
                    else if (dataType.ToLower() == "date")
                    {
                        hid.Attributes.Add("value",xmlNod.Attributes["value"].InnerXml);
                    }
                    else
                    {
                        hid.Attributes.Add("value",xmlNod.Attributes["value"].InnerXml);
                    }
                }
                if ((xmlNod.Attributes["class"] != null))
                {
                    hid.Attributes.Add("class", xmlNod.Attributes["class"].InnerXml);
                }
                if ((xmlNod.Attributes["cssclass"] != null))
                {//just cover the asp ccsclass 
                    hid.Attributes.Add("class", xmlNod.Attributes["cssclass"].InnerXml);
                }
                if ((xmlNod.Attributes["databind"] != null))
                {
                    hid.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
                }
            }

            return hid;
        }

        private LinkButton GetLinkButtonCtrl(XmlNode xmlNod)
        {
            var cmd = new LinkButton();
            cmd = (LinkButton)GenXmlFunctions.AssignByReflection(cmd, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["src"] != null))
            {
                cmd.Text = "<img src=\"" + xmlNod.Attributes["src"].InnerXml + "\" border=\"0\" />" + cmd.Text;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["confirm"] != null))
            {
                if (!string.IsNullOrEmpty(xmlNod.Attributes["confirm"].InnerXml))
                {
                    cmd.Attributes.Add("onClick", "javascript:return confirm('" + xmlNod.Attributes["confirm"].InnerXml + "');");
                }
            }

            cmd.Visible = GetRoleVisible(xmlNod.OuterXml);
            cmd.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            return cmd;
        }

        private void CreateLinkButton(Control container, XmlNode xmlNod)
        {
            var cmd = GetLinkButtonCtrl(xmlNod);

            cmd.DataBinding += LinkButtonDataBinding;
            container.Controls.Add(cmd);
        }

        private void CreateLabel(Control container, XmlNode xmlNod)
        {
            var hid = new Label {Text = ""};

            hid = (Label)GenXmlFunctions.AssignByReflection(hid, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                hid.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["ctrltype"] != null))
            {
                hid.Text = xmlNod.Attributes["ctrltype"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["length"] != null))
            {
                hid.Attributes.Add("length", xmlNod.Attributes["length"].InnerXml);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["substring"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["substring"].InnerXml))
                {
                    if (hid.Text.Length > Convert.ToInt32(xmlNod.Attributes["substring"].InnerXml))
                    {
                        hid.Text = hid.Text.Substring(0, Convert.ToInt32(xmlNod.Attributes["substring"].InnerXml));
                    }
                }
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["format"] != null))
            {
                hid.Attributes.Add("format", xmlNod.Attributes["format"].InnerXml);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                hid.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }

            hid.Visible = GetRoleVisible(xmlNod.OuterXml);
            hid.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            hid.DataBinding += LabelDataBinding;
            container.Controls.Add(hid);
        }

        /// <summary>
        /// Constant value for hidden field.
        /// <para>Special processing applys for a field with an id of "editlangauges".  This field will restructure the XMLData field in the DB to be a multiple langauge structure, use a CSV list of cultureCode specify specific cultures or use "*" to use all valid CMS lanaguges.</para>
        /// <para>A const with the id of "staticlangfields" will make the field the same across all langauges. Enter a CSV list of xpath values to specify the fields.</para>
        /// <para>[<tag id="Thumbsize" type="const" value="82,50" />]</para>
        /// <para>[<tag id="staticlangfields" type="const" value="/genxml/textbox/txtculturecode,/genxml/textbox/txtmobiurl" />]</para>
        /// <para>[<tag id="editlangauges" type="const" value="*" />]</para>
        /// </summary>
        private static void CreateConst(Control container, XmlNode xmlNod)
        {
            var hid = GetHiddenFieldCtrl(xmlNod);
            container.Controls.Add(hid);
        }

        private void CreateHidden(Control container, XmlNode xmlNod)
        {
            var hid = GetHiddenFieldCtrl(xmlNod);
            hid.DataBinding += HiddenDataBinding;
            container.Controls.Add(hid);
        }

        private void CreateTransButton(Control container, XmlNode xmlNod)
        {
            var cmd = GetLinkButtonCtrl(xmlNod);
            cmd.DataBinding += LinkButtonDataBinding;
            container.Controls.Add(cmd);

            var hid = GetHiddenFieldCtrl(xmlNod);
            hid.DataBinding += HiddenDataBinding;
            container.Controls.Add(hid);
        }


        #endregion

        #region "databind controls"

        /// <summary>
        /// Hidden field tag
        /// <para>A hidden field with the id of "lang" will be used as the langauge being edited.</para>
        /// <para>[<tag id="" type="hidden" const="true|false" value="" databind="" datatype="double|date" format="" class="" />]</para>
        /// <para>id : Unique id require for the page.</para>
        /// <para>value : default value.  the value is used as a varible that is saved in the data, if const property is used then the value will always be used.</para>        
        /// <para>const: Optional "true|false", uses this as the canstant value for the value property, the DB field will not replace the value field.</para>
        /// <para>databind: Optional, specify the data column of the repeater control.  "const" property will be ignored if this property is set. </para>
        /// <para>datatype: Optional, datatype of the data "double|date" </para>
        /// <para>format: Optional, format code of datatype. default is used if not specified. </para>
        /// <para>class: Optional, class name so field can be used easily vis jQuery. </para>
        /// <para>[<tag id="ItemID" class="itemid" type="hidden" value="" />]</para>
        /// <para>[<tag id="Thumbsize" const="true" type="hidden" value="82,50" />]</para>
        /// <para>[<tag id="ImageResize" const="true" type="hidden" value="600" />]</para>
        /// <para>[<tag id="lang" type="hidden" value="" />]</para>
        /// </summary>
        private void HiddenDataBinding(object sender, EventArgs e)
        {
            var hid = (HtmlGenericControl)sender;
            var container = (IDataItemContainer)hid.NamingContainer;
            try
            {
                if (hid.Attributes["databind"] != null)
                {
                    hid.Attributes["value"] = Convert.ToString(DataBinder.Eval(container.DataItem, hid.Attributes["databind"]));                    
                }
                else if (hid.Attributes["value"].ToLower().StartsWith("databind:"))
                {
                    // check for legacy databind method on value
                    hid.Attributes["value"] = Convert.ToString(DataBinder.Eval(container.DataItem, hid.Attributes["value"].ToLower().Replace("databind:", "")));
                }
                else
                {
                    if (hid.Attributes["const"] == null | hid.Attributes["const"] == "false")
                    {
                        hid.Attributes["value"] = GenXmlFunctions.GetGenXmLnode(hid.ID, "hidden", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn))).InnerText;
                    }
                }


                if (hid.Attributes["datatype"] != null)
                {
                    var strFormat = "";
                    if (hid.Attributes["datatype"] == "double")
                    {
                        if (Utils.IsNumeric(hid.Attributes["value"]))
                        {
                            strFormat = "N";
                            if (hid.Attributes["format"] != null)
                            {
                                strFormat = hid.Attributes["format"];
                            }
                            //hid.Attributes["value"] = Convert.ToDouble(hid.Attributes["value"]).ToString(strFormat);                            
                            hid.Attributes["value"] = Utils.FormatToDisplay(hid.Attributes["value"], Utils.GetCurrentCulture(), TypeCode.Double, strFormat);
                        }
                    }
                    else if (hid.Attributes["datatype"] == "date")
                    {
                        if (Utils.IsDate(hid.Attributes["value"]))
                        {
                            strFormat = "d";
                            if (hid.Attributes["format"] != null)
                            {
                                strFormat = hid.Attributes["format"];
                            }
                            hid.Attributes["value"] = Convert.ToDateTime(hid.Attributes["value"]).ToString(strFormat);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Label control tag
        /// <para>[<tag type="label" xpath="" length="" databind="" datatype="" format="" />]</para>
        /// <para>xpath: Optional, specify the xpath in the genxml structure.  Must be specified if databind is not used.</para>
        /// <para>databind: Optional, specify the data column of the repeater control.  Must be specified if xpath is not used. </para>
        /// <para>length: Optional, Length of display field.</para>
        /// <para>datatype: Optional, datatype of the data "double|date" </para>
        /// <para>format: Optional, format code of datatype. default is used if not specified. </para>
        /// <para>Asp : Asp.net properties created by reflection. </para>
        /// <para>[<tag type="label" xpath="genxml/textbox/txtclientname" length="20" />]</para>
        /// </summary>
        private void LabelDataBinding(object sender, EventArgs e)
        {
            var hid = (Label)sender;
            var container = (IDataItemContainer)hid.NamingContainer;
            try
            {
                if ((hid.Attributes["databind"] != null))
                {
                    hid.Text = Convert.ToString(DataBinder.Eval(container.DataItem, hid.Attributes["databind"]));
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(hid.ID, hid.Text, Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                    if ((nod != null))
                    {
                        hid.Text = nod.InnerText;
                    }
                    else
                    {
                        nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), hid.Text);
                        if (nod != null)
                        {
                            hid.Text = nod.InnerText;
                        }
                        else
                        {
                            hid.Text = "";
                        }
                    }


                    if (hid.Attributes["datatype"] != null)
                    {
                        var strFormat = "";
                        if (hid.Attributes["datatype"] == "double")
                        {
                            if (Utils.IsNumeric(hid.Attributes["value"]))
                            {
                                if (hid.Attributes["format"] != null)
                                {
                                    strFormat = hid.Attributes["format"];
                                }
                                hid.Text = Utils.FormatToDisplay(hid.Text, Utils.GetCurrentCulture(), TypeCode.Double, strFormat);
                            }
                        }
                        else if (hid.Attributes["datatype"] == "date")
                        {
                            if (Utils.IsDate(hid.Attributes["value"]))
                            {
                                strFormat = "d";
                                if (hid.Attributes["format"] != null)
                                {
                                    strFormat = hid.Attributes["format"];
                                }
                                hid.Text = Utils.FormatToDisplay(hid.Text, Utils.GetCurrentCulture(), TypeCode.DateTime, strFormat);
                            }
                        }
                    }

                    if ((hid.Attributes["length"] != null))
                    {
                        hid.Text = hid.Text.Substring(0, Convert.ToInt32(hid.Attributes["length"]));
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        private void ValueOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                // check if we have any formatting to do
                var strFormat = "";
                var strFormatType = "";
                var xPath = lc.Text;
                if (lc.Text.ToLower().StartsWith("date:"))
                {
                    var strF = lc.Text.Split(':');
                    if (strF.Length == 3)
                    {
                        strFormat = strF[1].Replace("**COLON**",":"); 
                        strFormatType = "date";
                        xPath = strF[2];                        
                    }
                }
                if (lc.Text.ToLower().StartsWith("double:"))
                {
                    var strF = lc.Text.Split(':');
                    if (strF.Length == 3)
                    {
                        strFormat = strF[1].Replace("**COLON**", ":");
                        strFormatType = "double";
                        xPath = strF[2];
                    }
                }

                //Get Data
                if (lc.Text.ToLower().StartsWith("databind:"))
                {
                    lc.Text = Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text.ToLower().Replace("databind:", "")));
                }
                else
                {
                    XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), xPath);
                    if ((nod != null))
                    {
                        lc.Text = XmlConvert.DecodeName(nod.InnerText);
                    }
                    else
                    {
                        lc.Text = "";
                    }
                }

                //Do the formatting
                if (strFormatType == "date")
                {
                    lc.Text = Utils.FormatToDisplay(lc.Text, Utils.GetCurrentCulture(), TypeCode.DateTime, strFormat);
                }
                if (strFormatType == "double")
                {
                    lc.Text = Utils.FormatToDisplay(lc.Text, Utils.GetCurrentCulture(), TypeCode.Double, strFormat);                    
                }

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void ChkBoxListOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;

            try
            {
                var xmlNod = GenXmlFunctions.GetGenXmLnode((string)DataBinder.Eval(container.DataItem, DatabindColumn), lc.Text);
                lc.Text = "";
                var xmlNodeList = xmlNod.SelectNodes("./chk");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode xmlNoda in xmlNodeList)
                    {
                        if (xmlNoda.Attributes != null && xmlNoda.Attributes["value"] != null)
                        {
                            if (xmlNoda.Attributes["value"].Value.ToLower() == "true")
                            {
                                lc.Text  += "[X] " + xmlNoda.InnerText + "<br/>"; 
                            }
                            else
                            {
                                lc.Text += "[_] " + xmlNoda.InnerText + "<br/>";                                 
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }


        private void BreakOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;

            try
            {
                if (lc.Text.ToLower().StartsWith("databind:"))
                {
                    lc.Text =Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text.ToLower().Replace("databind:", "")));
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), lc.Text);
                    if ((nod != null))
                    {
                        lc.Text = nod.InnerText;
                    }
                    else
                    {
                        lc.Text = "";
                    }
                }
                lc.Text = System.Web.HttpUtility.HtmlEncode(lc.Text);
                lc.Text = lc.Text.Replace(Environment.NewLine, "<br/>");

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void TestOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var xmlDoc = new XmlDataDocument();
                string testValue = "";
                string display = "";
                string displayElse = "";
                string dataValue = "";
                var roleValid = true;

                xmlDoc.LoadXml("<root>" + lc.Text + "</root>");
                var xmlNod = xmlDoc.SelectSingleNode("root/tag");

                if (xmlNod != null && (xmlNod.Attributes != null &&  (xmlNod.Attributes["testvalue"] != null)))
                {
                    testValue = xmlNod.Attributes["testvalue"].InnerXml;
                }
                if (xmlNod != null && (xmlNod.Attributes != null &&  (xmlNod.Attributes["testinrole"] != null)))
                {
                    var testRole = xmlNod.Attributes["testinrole"].InnerXml;
                    //do test for user rolew
                    if (!providers.CmsProviderManager.Default.IsInRole(testRole))
                    {
                        roleValid = false;
                    }
                }

                if (xmlNod != null && (xmlNod.Attributes != null &&  (xmlNod.Attributes["display"] != null)))
                {
                    display = xmlNod.Attributes["display"].InnerXml;
                }
                if (xmlNod != null && (xmlNod.Attributes != null &&  (xmlNod.Attributes["displayelse"] != null)))
                {
                    displayElse = xmlNod.Attributes["displayelse"].InnerXml;
                }

				if (container.DataItem != null && xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null)))
				{
					var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), xmlNod.Attributes["xpath"].InnerXml);
					if (nod != null)
					{
						dataValue = nod.InnerText;
					}
				}

				if (container.DataItem != null && xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null)))
                {
                    dataValue =Convert.ToString(DataBinder.Eval(container.DataItem, xmlNod.Attributes["databind"].InnerXml));
                }

                // special check to see if a sort item has been selected.
                if (testValue.ToLower() == "sortselected")
                {
                    if (Utils.IsNumeric(SortItemId))
                    {
                        dataValue = "sortselected";
                    }                    
                }

                //get test value 
                string output;
				if (dataValue == testValue & roleValid)
                {
                    output = display;
                }
                else
                {
                    output = displayElse;
                }

                lc.Text = output;

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void HtmlOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;

            try
            {
                if (lc.Text.ToLower().StartsWith("databind:"))
                {
                    lc.Text = System.Web.HttpUtility.HtmlDecode(Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text.ToLower().Replace("databind:", ""))));
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), lc.Text);
                    if ((nod != null))
                    {
                        lc.Text = System.Web.HttpUtility.HtmlDecode(nod.InnerText);
                    }
                    else
                    {
                        lc.Text = "";
                    }
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void LinkButtonDataBinding(object sender, EventArgs e)
        {
            var cmd = (LinkButton)sender;
            var container = (IDataItemContainer)cmd.NamingContainer;
            try
            {

                if (cmd.Text.ToLower().StartsWith("databind:"))
                {
                    // If using for repeated linkbutton, we can datbind the text (e.g. for paging) 
                    if ((DataBinder.Eval(container.DataItem, cmd.Text.Replace("databind:","")) != null))
                    {
                        //dataitem value matching commandarg name 
                        cmd.Text = Convert.ToString(DataBinder.Eval(container.DataItem, cmd.Text.Replace("databind:", "")));
                    }                    
                }

                if ((DataBinder.Eval(container.DataItem, cmd.CommandArgument) != null))
                {
                    //dataitem value matching commandarg name 
                    cmd.CommandArgument =Convert.ToString(DataBinder.Eval(container.DataItem, cmd.CommandArgument));
                }
                else
                {
                    //no value in dataitem matching commandarg name so search xml values
                    var nod = GenXmlFunctions.GetGenXmLnode(cmd.ID, cmd.Text,Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                    if ((nod != null))
                    {
                        cmd.CommandArgument = nod.InnerXml;
                    }
                    else
                    {
                        nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), cmd.Text);
                        if (nod != null)
                        {
                            cmd.CommandArgument = nod.InnerXml;
                        }
                        else
                        {
                            cmd.CommandArgument = "";
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private static void GeneralDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;

            try
            {
                lc.Text =Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text));
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void RblDataBinding(object sender, EventArgs e)
        {

            var rbl = (RadioButtonList)sender;
            var container = (IDataItemContainer)rbl.NamingContainer;

            try
            {
                string strValue;
                if ((rbl.Attributes["databind"] != null))
                {
                    strValue = Convert.ToString(DataBinder.Eval(container.DataItem, rbl.Attributes["databind"]));
                }
                else
                {
                    strValue = GenXmlFunctions.GetGenXmLnode(rbl.ID, "radiobuttonlist",Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn))).InnerText;
                }
                if ((rbl.Items.FindByValue(strValue) != null))
                {
                    rbl.SelectedValue = strValue;
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void ChkBDataBinding(object sender, EventArgs e)
        {
            var chk = (CheckBoxList)sender;
            var container = (IDataItemContainer) chk.NamingContainer;

            try
            {
                var xmlNod = GenXmlFunctions.GetGenXmLnode(chk.ID, "checkboxlist",(string) DataBinder.Eval(container.DataItem, DatabindColumn));
                var xmlNodeList = xmlNod.SelectNodes("./chk");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode xmlNoda in xmlNodeList)
                    {
                        if (xmlNoda.Attributes != null)
                        {
                            if (xmlNoda.Attributes.GetNamedItem("data") != null)
                            {
                                var datavalue = xmlNoda.Attributes["data"].Value;
                                //use the data attribute if there
                                if ((chk.Items.FindByValue(datavalue).Value != null))
                                {
                                    chk.Items.FindByValue(datavalue).Selected =
                                        Convert.ToBoolean(xmlNoda.Attributes["value"].Value);
                                }
                            }
                            else
                            {
                                // use the text or value, if no data att exists (backward compatibility)
                                var findName = xmlNoda.Value;
                                if (string.IsNullOrEmpty(findName))
                                {
                                    findName = xmlNoda.InnerText;
                                    if ((chk.Items.FindByText(findName).Value != null))
                                    {
                                        chk.Items.FindByText(findName).Selected =
                                            Convert.ToBoolean(xmlNoda.Attributes["value"].Value);
                                    }
                                }
                                else
                                {
                                    if ((chk.Items.FindByValue(findName).Value != null))
                                    {
                                        chk.Items.FindByValue(findName).Selected =
                                            Convert.ToBoolean(xmlNoda.Attributes["value"].Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void ChkBoxDataBinding(object sender, EventArgs e)
        {
            var chk = (CheckBox)sender;
            var container = (IDataItemContainer)chk.NamingContainer;

            try
            {
                if ((chk.Attributes["databind"] != null))
                {
                    chk.Checked = Convert.ToBoolean(Convert.ToString(DataBinder.Eval(container.DataItem, chk.Attributes["databind"])));
                }
                else
                {
                    chk.Checked = Convert.ToBoolean(GenXmlFunctions.GetGenXmlValue(chk.ID, "checkbox",Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn))));
                }

            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void DdListDataBinding(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;

            try
            {
                string strValue;
                if ((ddl.Attributes["databind"] != null))
                {
                    strValue = Convert.ToString(Convert.ToString(DataBinder.Eval(container.DataItem, ddl.Attributes["databind"])));
                }
                else
                {
                    strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                }

                if ((ddl.Items.FindByValue(strValue) != null))
                {
                        ddl.SelectedValue = strValue;                        
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                    if ((nod.Attributes != null) && (nod.Attributes["selectedtext"] != null))
                    {
                            strValue = XmlConvert.DecodeName(nod.Attributes["selectedtext"].Value);                            
                            if ((ddl.Items.FindByValue(strValue) != null))
                            {
                                ddl.SelectedValue = strValue;
                            }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void TextDataBinding(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            var container = (IDataItemContainer)txt.NamingContainer;

            try
            {
                if ((txt.Attributes["databind"] != null))
                {
                    txt.Text = Convert.ToString(DataBinder.Eval(container.DataItem, txt.Attributes["databind"]));
                    if (txt.Text.Contains("**CDATASTART**"))
                    {
                        //convert back cdata marks converted so it saves OK into XML 
                        txt.Text = txt.Text.Replace("**CDATASTART**", "<![CDATA[");
                        txt.Text = txt.Text.Replace("**CDATAEND**", "]]>");
                    }
                }
                else
                {
                    var strData = GenXmlFunctions.GetGenXmlValue(txt.ID, "textbox", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                    if (txt.Text == "")
                    {
                        txt.Text = strData;
                    }
                    else
                    {
                        if (strData != "") txt.Text = strData;
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        #endregion

        #region "General Methods"


        private static bool GetRoleEnabled(string xmLproperties)
        {
            var genprop = GenXmlFunctions.GetGenControlPropety(xmLproperties, "editinrole");
            if (genprop == "" | genprop == null)
            {
                return true;
            }

            // Call CMS Security Interface 
            return providers.CmsProviderManager.Default.IsInRole(genprop);
        }

        private static bool GetRoleVisible(string xmLproperties)
        {
            var genprop = GenXmlFunctions.GetGenControlPropety(xmLproperties, "viewinrole");
            if (genprop == "" | genprop == null)
            {
                return true;
            }

            // Call CMS Security Interface 
            return providers.CmsProviderManager.Default.IsInRole(genprop);
            
        }

		private XmlNode GetCMSResourceData(XmlDocument xmlDoc)
		{
			var xmlNod = xmlDoc.SelectSingleNode("root/tag");                        
			if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["resourcekey"] != null)))
			{
				if (_ResourcePath != "")
				{
					//add resource attribuutes to tag xml node.
					var rList = providers.CmsProviderManager.Default.GetResourceData(_ResourcePath, xmlNod.Attributes["resourcekey"].Value);
					foreach (var i in rList)
					{
						var aNod = xmlDoc.CreateAttribute(i.Key);
						aNod.Value = i.Value;
						xmlNod.Attributes.Append(aNod);
					}
					var rNod = xmlNod.Attributes["resourcekey"];
					xmlNod.Attributes.Remove(rNod);
				}
			}
			return xmlNod;
		}

        #endregion

    }

}
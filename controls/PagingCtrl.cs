using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using NBrightCore.common;
using NBrightCore.render;

namespace NBrightCore.controls 
{
    public class PagingCtrl:  WebControl
    {

        #region "setup"

        protected Repeater RpData;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public string CssPagingDiv { get; set; }
        public string CssPositionDiv { get; set; }
        public string CssSelectedPage { get; set; }
        public string CssNormalPage { get; set; }
        public string CssFirstPage { get; set; }
        public string CssLastPage { get; set; }
        public string CssPrevPage { get; set; }
        public string CssNextPage { get; set; }
        public string CssPrevSection { get; set; }
        public string CssNextSection { get; set; }
        public string TextFirst { get; set; }
        public string TextLast { get; set; }
        public string TextPrev { get; set; }
        public string TextNext { get; set; }
        public string TextPrevSection { get; set; }
        public string TextNextSection { get; set; }

        /// <summary>
        /// Use a html href link link for hte paging buttons, this is so SEO robots can follow them easily (Only needed for Front Office Display)
        /// </summary>
        public bool UseHrefLink
        {
            set
            {
                if (value)
                {
                    //SEO page link
                    var modparam = "";
                    if (ModuleId != "") modparam = "&pagemid=" + ModuleId;
                    RpData.ItemTemplate = new GenXmlTemplate("<a href=\"?page=[<tag type='valueof' databind='PageNumber' />]" + modparam + "\">[<tag type='valueof' databind='Text' />]</a>");
                }
            }
        }

        /// <summary>
        /// Use to add a moduleid onto the pg param, so multiple modules can use paging on 1 page.
        /// </summary>
        public String ModuleId { get; set; }

        public PagingCtrl()
        {
            UseHrefLink = false;
            ModuleId = "";
            CurrentPage = 1;
            PageSize = 10;
            TotalRecords = 0;
            CssPagingDiv = "NBrightPagingDiv";
            CssPositionDiv = "NBrightPositionPgDiv";
            CssSelectedPage = "NBrightSelectPg";
            CssNormalPage = "NBrightNormalPg";
            CssFirstPage = "NBrightFirstPg";
            CssLastPage = "NBrightLastPg";
            CssPrevPage = "NBrightPrevPg";
            CssNextPage = "NBrightNextPg";
            CssPrevSection = "NBrightPrevSection";
            CssNextSection = "NBrightNextSection";            
            TextFirst = "<<";
            TextLast = ">>";
            TextPrev = "<";
            TextNext = ">";
            TextNext = ">";
            TextPrevSection = "...";
            TextNextSection = "...";

        }

        #endregion

        #region "events"


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            RpData = new Repeater();
            RpData.ItemCommand += new RepeaterCommandEventHandler(ClientItemCommand);

            RpData.ItemTemplate = new GenXmlTemplate("[<tag id='cmdPg' type='linkbutton' Text='databind:Text' commandname='Page' commandargument='PageNumber' />]");

            this.Controls.AddAt(0, new LiteralControl("</div>"));
            this.Controls.AddAt(0, RpData);
            this.Controls.AddAt(0, new LiteralControl("<div class='" + CssPagingDiv + "'>"));
        }

        public event RepeaterCommandEventHandler PageChanged;

        protected void ClientItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();

            switch (e.CommandName.ToLower())
            {
                case "page":
                    if (PageChanged != null)
                    {
                        PageChanged(this, e);                        
                    }
                    break;
            }

        }



        #endregion


        #region "methods"

        public void BindPageLinks()
        {

            var pageL = new List<NBrightEspacePaging>();

            var lastPage = Convert.ToInt32(TotalRecords / PageSize);
            if (TotalRecords != (lastPage * PageSize))
            {
                lastPage = lastPage + 1;
            }

            //if only one page, don;t process
            if (lastPage == 1)
            {
                return;
            }

            if (CurrentPage <= 0)
            {
                CurrentPage = 1;
            }

            NBrightEspacePaging p;

            const int pageLinksPerPage = 10;

            var rangebase = Convert.ToInt32((CurrentPage - 1)/pageLinksPerPage);

            var lowNum = (rangebase * pageLinksPerPage) + 1;
            var highNum = lowNum + (pageLinksPerPage -1);

            if (highNum > Convert.ToInt32(lastPage))
            {
                highNum = Convert.ToInt32(lastPage);
            }
            if (lowNum < 1)
            {
                lowNum = 1;
            }

            if ((lowNum != 1) && (CurrentPage > 1) && (TextFirst != ""))
            {
                p = new NBrightEspacePaging {PageNumber = "1", Text = "<span class='" + CssFirstPage + "'>" + TextFirst + "</span>"};
                pageL.Add(p);
            }

            if ((CurrentPage > 1) && (TextPrev != ""))
            {
                p = new NBrightEspacePaging { PageNumber = Convert.ToString(CurrentPage - 1), Text = "<span class='" + CssPrevPage + "'>" + TextPrev + "</span>" };
                pageL.Add(p);                
            }

            if ((lowNum > 1) && (TextPrevSection != ""))
            {
                p = new NBrightEspacePaging { PageNumber = Convert.ToString(lowNum - 1), Text = "<span class='" + CssPrevSection + "'>" + TextPrevSection + "</span>" };
                pageL.Add(p);
            }


            for (int i = lowNum; i <= highNum; i++)
            {
                
                if (i == CurrentPage)
                {
                    p = new NBrightEspacePaging { PageNumber = Convert.ToString(i), Text = "<span class='" + CssSelectedPage + "'>" + Convert.ToString(i) + "</span>" };
                }
                else
                {
                    p = new NBrightEspacePaging { PageNumber = Convert.ToString(i), Text = "<span class='" + CssNormalPage + "'>" + Convert.ToString(i) + "</span>" };
                }
                pageL.Add(p);

            }


            if ((lastPage > highNum) && (TextNextSection != ""))
            {
                p = new NBrightEspacePaging { PageNumber = Convert.ToString(highNum + 1), Text = "<span class='" + CssNextSection + "'>" + TextNextSection + "</span>" };
                pageL.Add(p);
            }


            if ((lastPage > CurrentPage) && (TextNext != ""))
            {
                p = new NBrightEspacePaging { PageNumber = Convert.ToString(CurrentPage + 1), Text = "<span class='" + CssNextPage + "'>" + TextNext + "</span>" };
                pageL.Add(p);
            }

            if ((lastPage != highNum) && (lastPage > CurrentPage) && (TextLast != ""))
            {
                p = new NBrightEspacePaging { PageNumber = Convert.ToString(lastPage), Text = "<span class='" + CssLastPage + "'>" + TextLast + "</span>" };
                pageL.Add(p);                
            }

            RpData.DataSource = pageL;
            RpData.DataBind();

        }

        #endregion


        #region "data classes"

        private class NBrightEspacePaging
        {
            public string PageNumber { get; set; }
            public string Text { get; set; }
        }

        #endregion

    }
}

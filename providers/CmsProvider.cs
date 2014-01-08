using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Web.UI.WebControls;
using System.Xml;

namespace NBrightCore.providers
{
    public abstract class CmsProvider : ProviderBase
    {

        public abstract int GetCurrentUserId();

        public abstract string GetCurrentUserName();

        public abstract bool IsInRole(string testRole);

        public abstract string HomeMapPath();

        public abstract void SetCache(string CacheKey, object objObject, DateTime AbsoluteExpiration);

        public abstract object GetCache(string CacheKey);

        public abstract void RemoveCache(string CacheKey);

        public abstract Dictionary<int, string> GetTabList(string CultureCode);

        public abstract List<string> GetCultureCodeList();

		// This method is designed to return a list of resource keys and values that can be used for localization.
		public abstract Dictionary<String, String> GetResourceData(String ResourcePath, String ResourceKey);

    }
}

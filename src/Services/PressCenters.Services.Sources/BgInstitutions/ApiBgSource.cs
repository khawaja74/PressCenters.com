﻿namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;

    public class ApiBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.api.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}index.php/tools/blocks/news_list/rss?bID=606&cID=186&arHandle=Main";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll("item > link").Select(x => x.InnerHtml).ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override string ExtractIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            var uri = new Uri(url);
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                       ? uri.Segments[uri.Segments.Length - 1]
                       : uri.Segments[uri.Segments.Length - 2];
            id = id.Replace("/", string.Empty);
            return id;
        }

        protected override RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var title = document.QuerySelector(".box h1").TextContent?.Trim();

            var contentElement = document.QuerySelector(".news-article");

            var timeNode = contentElement.ChildNodes[0];
            var time = DateTime.ParseExact(timeNode.TextContent, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture); // 24.01.2016 09:59

            var imageElement = document.QuerySelector(".news-article img");
            var imageUrl = imageElement?.GetAttribute("src");

            contentElement.RemoveChild(timeNode);
            this.RemoveRecursively(contentElement, imageElement);
            var content = contentElement.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}

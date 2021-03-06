﻿using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga.Domain;
using HtmlAgilityPack;
using Microsoft.Practices.ServiceLocation;
using ProjectX.Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ProjectX.Common.Extensions;

namespace BlueWind.Crawler.Manga.Site.Manga24h
{
    public class Manga24hSeries : MangaSeries
    {
        public Manga24hSeries()
        {
        }

        public Manga24hSeries(string siteUri, string name, string tag, MangaSite site, ProgressStatus status)
        {
            base.SiteUri = siteUri;
            base.Tag = tag;
            base.Name = Utility.RemoveInvalidChars(name.Trim());
            this.FullText = Name.RemoveVietnameseSign() + " " + this.Tag;
            this.HomeSite = site;
            base.Status = (byte)status;
            base.SeriesFolderPath = @"Sites\" + this.HomeSite.Name + @"\" + base.Name + @"\";
            base.Discriminator = "Manga24hSeries";
        }

        public override bool CheckDate(string last, string current)
        {
            try
            {
                return (DateTime.ParseExact(last.Trim(), "yyyy-MM-dd", null) < DateTime.ParseExact(current.Trim(), "yyyy-MM-dd", null));
            }
            catch
            {
                return false;
            }
        }

        public override bool GetChapters()
        {
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            var isSuccess = true;
            using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<MangaDataContext>())
            {
                if (!(!crawlerParameter.UseDb || context == null))
                    context.Set<MangaSeries>().Attach(this);
                if ((this.MangaChapters == null) || (this.MangaChapters.Count() < 1))
                {
                    this.MangaChapters = new List<MangaChapter>();
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(HttpUtility.GetResponseString(base.SiteUri));
                    string chapterListQueryPath = this.HomeSite.ChapterListQueryPath;
                    HtmlNodeCollection nodes = document.DocumentNode.SelectNodes(chapterListQueryPath);
                    if (nodes != null)
                    {
                        try
                        {
                            foreach (HtmlNode node in nodes)
                            {
                                List<HtmlNode> list = (from n in node.ChildNodes
                                                       where n.Name == "td"
                                                       select n).ToList();
                                MangaChapter chapter = new Manga24hChapter(this.HomeSite.SiteUri + "/" + list[0].ChildNodes[0].Attributes["href"].Value, list[0].ChildNodes[0].InnerText, this, list[2].InnerText, list[1].InnerText);
                                if (context != null)
                                    context.Set<MangaChapter>().Add(chapter);
                                this.MangaChapters.Add(chapter);
                            }
                            if (!(!crawlerParameter.UseDb || context == null))
                            {
                                if (context.SaveChanges()<1)
                                    isSuccess = false;
                            }
                        }
                        catch (Exception exception)
                        {
                            Logger.Write(exception);
                            isSuccess = false;
                        }
                    }
                }
            }
            return isSuccess;
        }

        public override DateTime GetDate(string dateString)
        {
            DateTime time;
            DateTime.TryParseExact(dateString.Trim(), "yyyy-MM-dd", null, DateTimeStyles.None, out time);
            return time;
        }

        public override bool UpdateChapters()
        {
            HtmlDocument document = new HtmlDocument();
            bool isUpdated = false;
            document.LoadHtml(HttpUtility.GetResponseString(base.SiteUri));
            using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<MangaDataContext>())
            {
                context.Set<MangaSeries>().Attach(this);
                context.Entry(this).Reference<MangaSite>(n => n.HomeSite).Load();
                context.Set<MangaSite>().Attach(this.HomeSite);
                HtmlNodeCollection nodeCollection = document.DocumentNode.SelectNodes(this.HomeSite.ChapterListQueryPath);
                MangaChapter chapter;

                List<HtmlNode> nodes;
                if (nodeCollection != null)
                {
                    foreach (HtmlNodeCollection childNodeCollection in from n in nodeCollection select n.ChildNodes)
                    {
                        try
                        {
                            nodes = new List<HtmlNode>(from n in childNodeCollection
                                                       where n.Name == "td"
                                                       select n);
                            if (!this.CheckDate(this.LastUpdatedSource, nodes[2].InnerText))
                            {
                                break;
                            }
                            isUpdated = true;
                            chapter = new Manga24hChapter(this.HomeSite.SiteUri + "/" + nodes[0].ChildNodes[0].Attributes["href"].Value, nodes[0].ChildNodes[0].InnerText, this, nodes[2].InnerText, nodes[1].InnerText);
                            context.Set<MangaChapter>().Add(chapter);
                            context.SaveChanges();
                            if (chapter.Scan())
                            { }
                        }
                        catch (Exception exception)
                        {
                            Logger.Write(exception);
                        }
                    }
                    this.LastUpdatedSource = context.Entry(this).Collection(n=>n.MangaChapters).Query().Max(m => m.UpdatedDate);
                    context.SaveChanges();
                    Logger.Write(this.Name + " Updated. " + this.LastUpdatedSource);
                }
            }
            return isUpdated;
        }

        public override void UpdateViewCount(Func<object, bool> Callback)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(HttpUtility.GetResponseString(base.SiteUri));
            string chapterListQueryPath = this.HomeSite.ChapterListQueryPath;
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes(chapterListQueryPath);
            MangaChapter chapter = null;
            int result = 0;
            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    try
                    {
                        List<HtmlNode> list = new List<HtmlNode>(from n in node.ChildNodes
                                                                 where n.Name == "td"
                                                                 select n);
                        chapter = this.MangaChapters.SingleOrDefault<MangaChapter>(n => n.Name == Utility.RemoveInvalidChars(list[0].ChildNodes[0].InnerText.Trim()));
                        if ((chapter != null) && int.TryParse(list[1].InnerText, out result))
                        {
                            chapter.SourceViewCount = result;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Write(exception);
                    }
                }
            }
            if (Callback != null)
            {
                Callback(null);
            }
        }

        public override void UpdateInfo()
        {
            List<string> removeStrings = new List<string>(File.ReadAllLines("RemoveStrings.txt"));
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<MangaDataContext>())
            {
                if (!(!crawlerParameter.UseDb || context == null))
                context.Set<MangaSeries>().Attach(this);
                HtmlDocument doc = new HtmlDocument();
                HtmlNode singleNode;
                var xpath = "";
                doc.LoadHtml(HttpUtility.GetResponseString(this.SiteUri));
                if (doc.DocumentNode != null)
                {
                    if (this.ThumbnailUrl == null || this.ThumbnailUrl == "")
                    {
                        xpath = "//div[@class='col-md-3']//img[contains(@class,'img-rounded')][1]";
                        singleNode = doc.DocumentNode.SelectSingleNode(xpath);
                        if (singleNode != null)
                        {
                            this.ThumbnailUrl = singleNode.Attributes["src"].Value;
                            if (!(!crawlerParameter.UseDb || context == null))
                                context.SaveChanges();
                        }
                        else
                        {
                            Logger.Write(LogLevel.Trace, this.SiteUri + "- thumbnail - xpath's wrong.", null);
                        }
                    }

                    if (this.Status != (byte)ProgressStatus.Completed)
                    {
                        xpath = "//div[@class='col-md-9']//ul[@class='mangainfo']//span[@class='info_tinhtrang'][1]";
                        singleNode = doc.DocumentNode.SelectSingleNode(xpath);
                        if (singleNode != null)
                        {
                            switch (singleNode.InnerText)
                            {
                                case "Hoàn Thành":
                                    this.Status = (byte)ProgressStatus.Completed;
                                    break;
                                case "Đang Tiến Hành":
                                    this.Status = (byte)ProgressStatus.Ongoing;
                                    break;
                                case "Tạm Ngưng":
                                    this.Status = (byte)ProgressStatus.Suspended;
                                    break;
                                default:
                                    this.Status = (byte)ProgressStatus.Unknown;
                                    break;
                            }
                            if (!(!crawlerParameter.UseDb || context == null))
                                context.SaveChanges();
                        }
                        else
                        {
                            Logger.Write(LogLevel.Trace, this.SiteUri+"- info_tinhtrang - xpath's wrong.", null);
                        }
                    }
                    if (this.Overview == null || this.Overview == "")
                    {
                        xpath = "//div[@class='col-md-9'][1]//..//div[5]";
                        singleNode = doc.DocumentNode.SelectSingleNode(xpath);

                        if (singleNode != null)
                        {
                            var builder = new StringBuilder(System.Web.HttpUtility.HtmlDecode(singleNode.ChildNodes.Select(n => n.InnerText).Aggregate((m, n) => m.Trim('\r', '\n') + "\r\n" + n.Trim('\r', '\n'))));
                            foreach (string item in removeStrings)
                            {
                                builder.Replace(item, "");
                            }
                            this.Overview = builder.ToString();
                            if (!(!crawlerParameter.UseDb || context == null))
                                context.SaveChanges();

                        }
                        else
                        {
                            Logger.Write(LogLevel.Trace, this.SiteUri + "- overview - xpath's wrong.", null);
                        }
                    }
                }
            }
        }
    }
}
﻿namespace AutoTagger.Crawler.Standard.V1
{
    using System;
    using System.Collections.Generic;
    using AutoTagger.Contract;
    using AutoTagger.Crawler.Standard.V1.Crawler;

    public class CrawlerV1 : ICrawler
    {
        private readonly HashtagQueue<IHumanoidTag> hashtagQueue;
        private readonly RandomTagsCrawler randomTagsCrawler;
        private readonly ExploreTagsCrawler exploreTagsPageCrawler;
        private readonly ImageDetailCrawler imageDetailPageCrawler;
        private readonly UserCrawler userCrawler;

        public event Action<IHumanoidTag> OnHashtagFound;
        private readonly Dictionary<string, int> conditions;

        public CrawlerV1()
        {
            this.conditions = new Dictionary<string, int>();
            this.conditions.Add("MinPostsForHashtags", 1 * 1000 * 1000);

            this.hashtagQueue = new HashtagQueue<IHumanoidTag>();
            this.randomTagsCrawler      = new RandomTagsCrawler();
            this.exploreTagsPageCrawler = new ExploreTagsCrawler(this);
            this.imageDetailPageCrawler = new ImageDetailCrawler();
            this.userCrawler            = new UserCrawler();
        }

        public void AddHTags(List<IHumanoidTag> preexistingHTags)
        {
            if (this.hashtagQueue == null)
                throw new Exception("hashTagQueue not set");

            this.hashtagQueue.AddProcessed(preexistingHTags);

        }

        public IEnumerable<IImage> DoCrawling(int limit, params string[] customTags)
        {
            this.BuildTags(customTags);
            this.hashtagQueue.SetLimit(limit);
            this.hashtagQueue.OnHashtagFound += HashtagFound;
            return this.hashtagQueue.Process(
                this.ExploreTagsCrawlerFunc,
                this.ImagePageCrawlerFunc,
                this.UserCrawlerFunc
                );
        }

        private void HashtagFound(IHumanoidTag tag)
        {
            this.OnHashtagFound?.Invoke(tag);
        }

        public void BuildTags(string[] customTags)
        {
            var tags = customTags.Length == 0 ? this.randomTagsCrawler.Parse() : customTags;
            var hTags = new List<IHumanoidTag>();
            foreach (var name in tags)
            {
                hTags.Add(new HumanoidTag { Name = name });
            }
            this.hashtagQueue.Build(hTags);
        }

        private (int, List<string>) ExploreTagsCrawlerFunc(IHumanoidTag tag)
        {
            var url = $"https://www.instagram.com/explore/tags/{tag.Name}/";
            (var amountPosts, var images) = this.exploreTagsPageCrawler.Parse(url);
            var shortcodes = new List<string>();
            if (images != null)
            {
                foreach (var image in images)
                {
                    shortcodes.Add(image.Shortcode);
                }
            }
            return (amountPosts, shortcodes);
        }

        private string ImagePageCrawlerFunc(string shortcode)
        {
            var url = $"https://www.instagram.com/p/{shortcode}/?hl=en";
            return this.imageDetailPageCrawler.Parse(url);
        }

        private IEnumerable<IImage> UserCrawlerFunc(string user)
        {
            var url = $"https://www.instagram.com/{user}/?hl=en";
            var images = this.userCrawler.Parse(url);
            foreach (var image in images)
            {
                image.User = user;
                yield return image;
            }
        }

        public bool OverrideCondition(string key, int value)
        {
            if (this.conditions.ContainsKey(key))
            {
                this.conditions[key] = value;
                return true;
            }

            return false;
        }

        public int GetCondition(string key)
        {
            if (this.conditions.ContainsKey(key))
            {
                return this.conditions[key];
            }
            return 0;
        }

        public void DisableFurtherEnqueue()
        {
            this.hashtagQueue.AllowEnqueue = false;
        }
    }
}

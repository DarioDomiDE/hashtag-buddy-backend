﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace AutoTagger.Crawler.Standard
{
    public class Crawler
    {
        public IEnumerable<ICrawlerImage> GetImagesFromHashTag(string hashTag, int amount)
        {
            var images = new Dictionary<string, ICrawlerImage>();
            var processedHashTags = new HashSet<string>();
            var hashTagQueue = new ConcurrentQueue<string>();
            hashTagQueue.Enqueue(hashTag);

            while (hashTagQueue.TryDequeue(out var currentHashTag))
            {
                if (processedHashTags.Contains(currentHashTag))
                {
                    continue;
                }

                processedHashTags.Add(currentHashTag);

                var shortCodes = GetShortCodesFromHashTag(currentHashTag);
                var imageData = shortCodes.Select(GetImageDataFromShortCode);
                foreach (var crawlerImage in imageData)
                {
                    foreach (var tag in crawlerImage.HumanoidTags
                        .Where(t=>!processedHashTags.Contains(t)&&!hashTagQueue.Contains(t)))
                    {
                        hashTagQueue.Enqueue(tag);
                    }

                    if (images.ContainsKey(crawlerImage.ImageId))
                    {
                        continue;
                    }

                    images[crawlerImage.ImageId] = crawlerImage;
                    if (images.Count >= amount)
                    {
                        return images.Values;
                    }
                }
            }


            return images.Values;
        }

        public ICrawlerImage GetImageDataFromShortCode(string shortCode)
        {
            Console.WriteLine("Processing ShortCode " + shortCode);
            HttpClient hc = new HttpClient();
            HttpResponseMessage result = hc.GetAsync($"https://www.instagram.com/p/{shortCode}/").Result;
            var document = new HtmlDocument();
            document.Load(result.Content.ReadAsStreamAsync().Result);
            var imageUrl = document.DocumentNode.SelectNodes("//meta[@property='og:image']").FirstOrDefault().Attributes["content"].Value;
            var hashTags = document.DocumentNode.SelectNodes("//meta[@property='instapp:hashtags']").Select(x => x.Attributes["content"].Value);
            //Console.WriteLine("URL: " + imageUrl);
            //Console.WriteLine("Tags: " + string.Join(", ", hashTags));
            return new CrawlerImage
            {
                HumanoidTags = hashTags,
                ImageId = shortCode,
                ImageUrl = imageUrl,
            };

            /// Bgsth_jAPup
            /// <meta property="og:description" content="Gefällt 46 Mal, 1 Kommentare - Christian Seidlitz (@seidchr) auf Instagram: „silent alster 3 incredible calm alsterwasser and an awesome littelbit of fog in the athmosphere…“" />
            /// <meta property="instapp:hashtags" content="wearehamburg" /><meta property="instapp:hashtags" content="welovehh" /><meta property="instapp:hashtags" content="hambourg" /><meta property="instapp:hashtags" content="iamatraveler" />
        }

        public IEnumerable<string> GetShortCodesFromHashTag(string hashTag)
        {
            Console.WriteLine("Processing HashTag " + hashTag);

            HttpClient hc = new HttpClient();
            var x = hc.GetStringAsync($"https://www.instagram.com/explore/tags/{hashTag}/").Result;
            var matches = Regex.Matches(x, @"\""shortcode\""\:\""([^\""]+)""");
            var shortcodes = matches.OfType<Match>().Select(m => m.Groups[1].Value);

            //Console.WriteLine("ShortCodes: " + string.Join(", ", shortcodes));
            return shortcodes;
            /// Bgsth_jAPup
            /// <meta property="og:description" content="Gefällt 46 Mal, 1 Kommentare - Christian Seidlitz (@seidchr) auf Instagram: „silent alster 3 incredible calm alsterwasser and an awesome littelbit of fog in the athmosphere…“" />
            /// <meta property="instapp:hashtags" content="wearehamburg" /><meta property="instapp:hashtags" content="welovehh" /><meta property="instapp:hashtags" content="hambourg" /><meta property="instapp:hashtags" content="iamatraveler" />
        }
    }

    public class CrawlerImage : ICrawlerImage
    {
        public string ImageId { get; set; }
        public string ImageUrl { get; set; }
        public IEnumerable<string> HumanoidTags { get; set; }
    }

    public interface ICrawlerImage
    {
        string ImageId { get; }

        string ImageUrl { get; }

        IEnumerable<string> HumanoidTags { get; }
    }
}
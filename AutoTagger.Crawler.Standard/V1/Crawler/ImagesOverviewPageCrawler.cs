﻿namespace AutoTagger.Crawler.Standard.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using AutoTagger.Contract;

    class ImagesOverviewPageCrawler : HttpCrawler
    {
        public enum PageType
        {
            None,
            ExploreTags,
            Profile
        }

        private const int MinHashTagCount = 5;
        private const int MinLikes = 100;
        private const int MinFollowerCount = 500;
        private const int MinPostsForHashtags = 1000000;
        private static readonly Regex FindHashTagsRegex = new Regex(@"#\w+", RegexOptions.Compiled);

        public IEnumerable<IImage> Parse(string url, PageType currentPageType)
        {
            var document = this.FetchDocument(url);
            var data = GetScriptNodeData(document);

            if (currentPageType == PageType.ExploreTags && !HashtagHasEnoughPosts(data))
            {
                yield break;
            }

            var nodes = GetImageNodes(data, currentPageType);
            var images = GetImages(nodes);

            foreach (IImage image in images)
            {
                if (currentPageType == PageType.Profile)
                {
                    var followerCount = Convert.ToInt32(data?.entry_data?.ProfilePage?[0]?.graphql?.user?.edge_followed_by?.count.ToString());
                    image.Follower    = followerCount;
                    if (followerCount < MinFollowerCount)
                    {
                        continue;
                    }
                }
                yield return image;
            }
        }

        private static bool HashtagHasEnoughPosts(dynamic data)
        {
            if (data == null)
            {
                return false;
            }

            dynamic node = data.entry_data?.TagPage?[0]?.graphql?.hashtag?.edge_hashtag_to_media;
            var amountOfPosts = Convert.ToInt32(node?.count.ToString());
            if (amountOfPosts < MinPostsForHashtags)
            {
                return false;
            }

            return true;
        }

        private static dynamic GetImageNodes(dynamic data, PageType currentPageType)
        {
            if (data == null)
            {
                return null;
            }

            dynamic nodes = null;
            switch (currentPageType)
            {
                case PageType.ExploreTags:
                    nodes = data.entry_data?.TagPage?[0]?.graphql?.hashtag?.edge_hashtag_to_top_posts?.edges;
                    break;
                case PageType.Profile:
                    nodes = data.entry_data?.ProfilePage?[0]?.graphql?.user?.edge_owner_to_timeline_media?.edges;
                    break;
            }

            if (nodes == null)
            {
                return null;
            }

            return nodes;
        }

        private static IEnumerable<IImage> GetImages(dynamic nodes)
        {
            if (nodes == null)
            {
                yield break;
            }

            foreach (var node in nodes)
            {
                dynamic edges = node?.node?.edge_media_to_caption?.edges;
                if (edges.ToString() == "[]")
                {
                    continue;
                }
                string text = edges[0]?.node?.text;
                text = text?.Replace("\\n", "\n");
                text = System.Web.HttpUtility.HtmlDecode(text);
                var hashTags = ParseHashTags(text).ToList();

                int likes = node?.node?.edge_liked_by?.count;
                if (!MeetsConditions(hashTags.Count, likes))
                {
                    continue;
                }

                var innerNode = node?.node;
                var takenDate = GetDateTime(Convert.ToDouble(innerNode?.taken_at_timestamp.ToString()));
                var image = new Image
                {
                    Likes = likes,
                    CommentCount = innerNode?.edge_media_to_comment?.count,
                    Shortcode = innerNode?.shortcode,
                    HumanoidTags = hashTags,
                    Url = innerNode?.display_url,
                    Uploaded = takenDate
                };
                yield return image;
            }
        }

        private static bool MeetsConditions(int hashTagsCount, int likes)
        {
            return hashTagsCount > MinHashTagCount && likes > MinLikes;
        }

        private static IEnumerable<string> ParseHashTags(string text)
        {
            if (text == null)
            {
                return Enumerable.Empty<string>();
            }

            return FindHashTagsRegex.Matches(text).OfType<Match>().Select(m => m?.Value.Trim(' ', '#').ToLower())
                .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
        }

        public static DateTime GetDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}

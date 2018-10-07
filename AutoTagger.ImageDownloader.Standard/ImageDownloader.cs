﻿namespace AutoTagger.ImageDownloader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using AutoTagger.Contract;
    using AutoTagger.FileHandling.Standard;

    public class ImageDownloader
    {
        private static IImageProcessorStorage storage;
        private static int downloaderRunning;
        private const int QueryImagesAtLessOrEqualImages = 30;
        private const int DbGetLimit = 300;
        private const int ParallelThreads = 100;
        private static IFileHandler fileHandler;
        private static List<string> files;
        private static int LogCount = 0;
        private readonly List<string> imagesToSetDownloadedStatus;
        private readonly List<string> imagesToSetFailedStatus;

        private enum StorageUses
        {
            None,
            Get,
            Update
        }

        private static StorageUses CurStorageUse;

        public ImageDownloader(IImageProcessorStorage db)
        {
            storage = db;
            fileHandler = new DiskFileHander();
            this.imagesToSetDownloadedStatus = new List<string>();
            this.imagesToSetFailedStatus = new List<string>();
        }

        public void Start()
        {
            files = fileHandler.GetAllUnusedImages().ToList();
            new Thread(this.GetImages).Start();
            new Thread(this.Logs).Start();
            new Thread(this.DbUpdater).Start();
        }

        private void DbUpdater()
        {
            while (true)
            {
                this.DbUpdaterLogic(this.imagesToSetDownloadedStatus, "downloaded");
                this.DbUpdaterLogic(this.imagesToSetFailedStatus, "failed");
                Thread.Sleep(500);
            }
        }

        private void DbUpdaterLogic(IList<string> input, string status)
        {
            var count = input.Count;
            if (count == 0 || !this.TrySetStatus(StorageUses.Update))
            {
                return;
            }
            storage.SetImagesStatus(input.ToList(), status);
            for (var i = count - 1; i > 0; i--)
            {
                input.RemoveAt(i);
            }
            Console.WriteLine($"Update {count} Photos to {status}");
            CurStorageUse = StorageUses.None;
        }

        private bool TrySetStatus(StorageUses newStatus)
        {
            if (CurStorageUse == newStatus)
            {
                return true;
            }
            if (CurStorageUse != StorageUses.None)
            {
                return false;
            }
            CurStorageUse = newStatus;
            if (CurStorageUse != newStatus)
            {
                return false;
            }
            return true;
        }

        public void GetImages()
        {
            var delay = 30;
            while (true)
            {
                if (downloaderRunning <= QueryImagesAtLessOrEqualImages)
                {
                    delay = 500;
                    if (this.TrySetStatus(StorageUses.Get))
                    {
                        Console.WriteLine("Start Getting Photos");
                        var images = storage.GetImagesForImageDownloader(DbGetLimit);
                        CurStorageUse = StorageUses.None;
                        var enumerable = images as IImage[] ?? images.ToArray();
                        Console.WriteLine("Get " + enumerable.Count() + " DB Entries");
                        foreach (var image in enumerable)
                        {
                            this.DownloaderHandling(image);
                            delay = 30;
                        }
                    }
                }
                Thread.Sleep(delay);
            }
        }

        private void DownloaderHandling(IImage image)
        {
            if (downloaderRunning >= ParallelThreads)
            {
                Thread.Sleep(30);
                this.DownloaderHandling(image);
                return;
            }
            if (files.Contains(image.Shortcode))
            {
                return;
            }
            files.Add(image.Shortcode);
            Interlocked.Increment(ref downloaderRunning);
            new Thread(() => this.Download(image)).Start();
        }

        public void Download(IImage image)
        {
            using (var client = new WebClient())
            {
                var url = image.LargeUrl;
                var fullPath = fileHandler.GetFullPath(image.Shortcode);
                try
                {
                    client.DownloadFile(new Uri(url), fullPath);
                    Console.WriteLine("successful downloaded: " + image.Shortcode + " (" + image.Created + ")");
                    this.imagesToSetDownloadedStatus.Add(image.Shortcode);
                    LogCount++;
                }
                catch (WebException e)
                {
                    if (e.Message.Contains("403"))
                    {
                        Console.WriteLine("Download failed with 403 at Created=" + image.Created);
                        this.imagesToSetFailedStatus.Add(image.Shortcode);
                    }
                    else if (e.Message.Contains("404"))
                    {
                        Console.WriteLine("Download failed with 404 at Created=" + image.Created);
                    }
                    else
                    {
                        Console.WriteLine("Crashed at Created=" + image.Created);
                        Console.WriteLine(e.Message);
                    }
                    fileHandler.Delete(image.Shortcode);
                }
                finally
                {
                    Interlocked.Decrement(ref downloaderRunning);
                }
            }
        }
        private void Logs()
        {
            while (true)
            {
                if (LogCount == 0)
                {
                    continue;
                }
                Console.WriteLine("Downloaded since start: " + LogCount);
                Thread.Sleep(5000);
            }
        }

    }
}

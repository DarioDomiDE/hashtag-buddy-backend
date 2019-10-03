﻿namespace Instaq.Common
{
    using Instaq.Contract.Models;
    using System.Security.Cryptography;
    using System.Text;

    public class Customer : ICustomer
    {
        public int Id { get; set; }

        public string CustomerId { get; set; }

        public int PhotosCount { get; set; }

        public int FeedbackCount { get; set; }

        public string Infos { get; set; }

        public void GenerateHash()
        {
            var secret = "Instaq";
            var sha = new SHA256CryptoServiceProvider();
            var input = this.Id + secret;
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("X2"));
            this.CustomerId = sb.ToString().ToLower();
        }
    }
}

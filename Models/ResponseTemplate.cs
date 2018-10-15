using System;
using Microsoft.Bot.Builder;

namespace Knnithyanand.Conversational.Samples.Models
{
    public class ResponseTemplate : IStoreItem
    {
        Random _rand = new Random();

        public string[] Templates { get; set; }

        public string GetMessage(string[] values)
        {
            return string.Format(Templates[_rand.Next(Templates.Length)], values);
        }

        public string ETag { get; set; } = "*";
    }
}
using System;
using System.Linq;
using System.Net.Http;
using Dwango.Nicolive.Chat.Service.Edge;
using NdgrClientSharp;
using NdgrClientSharp.NdgrApi;
using R3;

var viewApiUri = "";

using var ndgrSnapshotFetcher = new NdgrSnapshotFetcher();

// 現在の状態（運営コメントの設定やアンケートの状態）を取得する
await foreach (var chunkedMessage in ndgrSnapshotFetcher.FetchCurrentSnapshotAsync(viewApiUri))
{
    Console.WriteLine(chunkedMessage);
}




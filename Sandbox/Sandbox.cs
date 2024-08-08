using System;
using NdgrClientSharp.NdgrApi;
using R3;

var viewApiUri = "";

using var ndgrApiClient = new NdgrApiClient();

using var pastClient = new NdgrPastCommentFetcher(ndgrApiClient);

var pastComments = pastClient.FetchPastComments(viewApiUri, 100);
await pastComments.ForEachAsync(x => Console.WriteLine(x));

Console.WriteLine("---");

using var snapshotClient = new NdgrSnapshotFetcher(ndgrApiClient);

var snapshotComments = snapshotClient.FetchCurrentSnapshotAsync(viewApiUri, default);
await foreach (var snapshotComment in snapshotComments)
{
    Console.WriteLine(snapshotComment);
}

Console.WriteLine("---");

using var liveClient = new NdgrLiveCommentFetcher(ndgrApiClient);

liveClient.OnNicoliveCommentReceived.Subscribe(x => Console.WriteLine(x));
liveClient.Connect(viewApiUri);

Console.ReadLine();

liveClient.Disconnect();
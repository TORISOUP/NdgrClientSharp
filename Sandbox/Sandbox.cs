using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dwango.Nicolive.Chat.Data;
using Dwango.Nicolive.Chat.Service.Edge;
using NdgrClientSharp;
using NdgrClientSharp.NdgrApi;
using R3;

var viewApiUri = "";
using var live = new NdgrLiveCommentFetcher();


live.ConnectionStatus.Subscribe(x => Console.WriteLine(x));
live.OnMessageReceived.Subscribe(x => Console.WriteLine(x));
live.Connect(viewApiUri);
live.OnProgramEnded.Subscribe(_ => live.Disconnect());


Console.ReadLine();
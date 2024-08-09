# NdgrClientSharp

ニコニコ生放送の新コメントサーバー「NDGR（のどぐろ）」用のC#クライアントです。
次の機能を使うことができます。

* 各APIと通信できるSimpleなクライアント
    * `/api/view/v4/:view?at=now`
    * `/api/view/v4/:view?at=:at`
    * `/data/segment/v4/:segment`
    * `/data/backward/v4/:backward`
    * `/data/snapshot/v4/:snapshot`
* 通信レイヤーをラップしたEasyなクライアント
    * 生放送のリアルタイムなコメント受信
    * 生放送の過去のコメント取得
    * 生放送の現在の状態（スナップショット）の取得

## 動作環境

`.NET Standard 2.1`向けです。

また、動作には次のライブラリが必要です。

 * [Google.Protobuf](https://www.nuget.org/packages/Google.Protobuf/)
 * [R3](https://www.nuget.org/packages/R3)

 ## プロジェクト構成

 * `NdgrClientSharp` : クライアント本体
 * `NdgrClientSharp.Protocol` : 通信及びクライアントが利用するProtoBuffから生成されたデータ構造置き場
 * `NdgrClientSharp.Test` : テスト
 * `Sandbox` : 実験用のコード置き場

## ProtocolBufferの定義について

本ライブラリが使用するProtoBuffの定義は[N Airが定義するprotoファイル(v2024.801.103927)](https://github.com/n-air-app/nicolive-comment-protobuf/releases/tag/v2024.801.103927)を元にしています。


# 使用方法

## 導入

Releaseよりzipでダウンロードして、次のdllをプロジェクトに入れて下さい。

* `NdgrClientSharp.dll`
* `NdgrClientSharp.Protocol.dll`

また次のライブラリも導入している必要があります。

 * [Google.Protobuf](https://www.nuget.org/packages/Google.Protobuf/)
 * [R3](https://www.nuget.org/packages/R3)
 * [Microsoft.Bcl.TimeProvider](https://www.nuget.org/packages/Microsoft.Bcl.TimeProvider/)

加えてUnityで動作させる場合は[R3.Unity](https://github.com/Cysharp/R3?tab=readme-ov-file#unity)も導入してください。

## ViewAPI URIの取得

NDGRの通信の起点はView APIのURIです。こちらは`https://live2.nicovideo.jp/watch/{lv}/programinfo`などから取得することができます。


## 使い方

### NdgrApiClient

NDGRと通信するためのAPIクライアントです。NDGRのAPIとの通信をほぼラップせずに提供します。
結果はすべてProtoBuffから自動生成されたデータ構造をそのまま返します。

また`NdgrLiveCommentFetcher`/`NdgrPastCommentFetcher`/`NdgrSnapshotFetcher`が内部的に依存するクライアントでもあります。

自身でNDGRとの通信を細かく制御したい場合に使用してください。

```cs:使用例
// 初期化
using (var ndgrApiClient = new NdgrApiClient())
{
    // /api/view/v4/:view?at=now の取得
    var next = await ndgrApiClient.FetchViewAtNowAsync(viewApiUri);

    // /api/view/v4/:view?at=unixtime の取得
    await foreach (var chunkedEntry in ndgrApiClient.FetchViewAtAsync(viewApiUri, next.At))
    {
        switch (chunkedEntry.EntryCase)
        {
            case ChunkedEntry.EntryOneofCase.None:
                break;
            case ChunkedEntry.EntryOneofCase.Backward:
                break;
            case ChunkedEntry.EntryOneofCase.Previous:
                break;
            case ChunkedEntry.EntryOneofCase.Segment:
                break;
            case ChunkedEntry.EntryOneofCase.Next:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
```

また通信の挙動をより細かく制御したい場合はコンストラクタに`HttpClient`を渡すことができます。
ただし**コンストラクタでHttpClientを指定した場合、このHttpClientのDisposeは自身で管理してください**

```cs
// HttpClientの作成
var httpClient = new HttpClient(new HttpClientHandler());

// UAを設定したり
httpClient.DefaultRequestHeaders.Add("User-Agent", "my-user-agent");

// ndgrApiClientの作成
var ndgrApiClient = new NdgrApiClient(httpClient);

// NdgrApiClientをDisposeしただけではHttpClientはDisposeされない
// 自身でHttpClientのDisposeを行う必要あり
ndgrApiClient.Dispose();
httpClient.Dispose();
```

### NdgrLiveCommentFetcher

ニコニコ生放送の放送中のコメントをリアルタイムに取得するクライアントです。

`OnMessageReceived`から`Observable<ChunckedMessage>`としてコメントを取得できます。

```cs
// 生放送コメント取得用のクライアントを生成
var liveCommentFetcher = new NdgrLiveCommentFetcher();

// コメントの受信準備
liveCommentFetcher
    .OnMessageReceived
    .Subscribe(chukedMessage =>
    {
        switch (chukedMessage.PayloadCase)
        {
            case ChunkedMessage.PayloadOneofCase.Message:
                // コメントやギフトの情報などはMessage
                Console.WriteLine(chukedMessage.Message);
                break;
            case ChunkedMessage.PayloadOneofCase.State:
                // 番組他状態の変更などはStateから取得可能
                Console.WriteLine(chukedMessage.State);
                break;

            default:
                break;
        }
    });

// コメントの受信開始
liveCommentFetcher.Connect(viewApiUri);

// ---

// コメントの受信停止
liveCommentFetcher.Disconnect();

// リソースの解放
liveCommentFetcher.Dispose();
```

#### 補足1:メッセージの発行スレッド

`ConfigureAwait(true)`として動作します。そのため`Connect()`したときの同じスレッドでメッセージ発行される**はず**です。

ただもしスレッドを確実に指定したい場合、たとえばUnityなどでは`ObserveOnMainThread`を保険として挟んでおくのはありかもしれません。

```cs
// Unityで使う場合
// R3.UnityのObserveOnMainThreadを使うことで
// 確実にUnityメインスレッドで受信できる
liveCommentFetcher
    .OnMessageReceived
    // これ
    .ObserveOnMainThread()
    .Subscribe(chukedMessage =>
    {
        switch (chukedMessage.PayloadCase)
        {
            case ChunkedMessage.PayloadOneofCase.Message:
                // コメントやギフトの情報などはMessage
                Console.WriteLine(chukedMessage.Message);
                break;
            case ChunkedMessage.PayloadOneofCase.State:
                // 番組他状態の変更などはStateから取得可能
                Console.WriteLine(chukedMessage.State);
                break;

            default:
                break;
        }
    });
```


#### 補足2:NdgrApiClientの指定

```cs
var httpClient = new HttpClient();
var negrApiClient = new NdgrApiClient(httpClient);

// NdgrApiClientを指定することが可能
var ndgrLiveCommentFetcher = new NdgrLiveCommentFetcher(negrApiClient);

// Disposeは手動で
ndgrLiveCommentFetcher.Dispose();
negrApiClient.Dispose();
httpClient.Dispose();
```

#### 補足3:発生したエラーの検知

エラーは`Observable`の`OnErrorResume`として通知されます。

```cs
liveCommentFetcher
    .OnMessageReceived
    .Subscribe(
        onNext: chukedMessage => Console.WriteLine(chukedMessage),
        onErrorResume: ex => Console.WriteLine(ex),
        onCompleted: result => Console.WriteLine(result));
```

R3のObservableについては詳しくは[こちらの記事](https://qiita.com/toRisouP/items/e7be5a5a43058556db8f)などを参照。

#### 補足4: エラー発生時の再接続

`NdgrLiveCommentFetcher`は通信時、ステータスコード「`503 Service Unavailable`」が返ってきた場合のみ自動で再接続を試みます。
再接続時のリトライ数やバックオフタイムはプロパティから設定可能です。

```cs
var liveCommentFetcher = new NdgrLiveCommentFetcher();

liveCommentFetcher.MaxRetryCount = 5;
liveCommentFetcher.RetryInterval = TimeSpan.FromSeconds(5);
```

#### 補足5: 接続状態の確認

クライアントの接続状況は`ConnectionStatus`より`ReadOnlyReactiveProperty`として取得可能です。


```cs
// 接続状態の通知
liveCommentFetcher.ConnectionStatus
    .Subscribe(state =>
    {
        switch (state)
        {
            case ConnectionState.Disconnected:
                break;
            case ConnectionState.Connecting:
                break;
            case ConnectionState.Connected:
                break;
            default:
                break;
        }
    });

// 同期的に接続状態を取得
Console.WriteLine(liveCommentFetcher.ConnectionStatus.CurrentValue);
```

#### 補足6: 番組終了時の挙動

放送中の番組に対して`Connect()`していた場合、**番組が終了しても自動でDisconnectはしません。**
もし番組が終了を検知して`Disconnect()`したい場合は`OnProgramEnded`を購読して発行されるイベントを利用してください。

```cs
using var ndgrLiveCommentFetcher = new NdgrLiveCommentFetcher();
ndgrLiveCommentFetcher.Connect(viewApiUri);

// 番組が終了時にイベントが発行される
// それのイベントを受けてDisconnectする
ndgrLiveCommentFetcher
    .OnProgramEnded
    .Subscribe(_ => ndgrLiveCommentFetcher.Disconnect());
```


なお、すでに終了済みの番組に対して`Connect()`をした場合、`OnProgramEnded`は発行されません。
番組状態を別のAPI(`programInfo`など)で取得し、放送中であることを確認してから`Connect()`してください。

### NdgrPastCommentFetcher

放送中番組の過去のコメント(`ChunckedMessage`)を取得するクライントです。
現在時刻から遡って指定件数くらいのコメントを`Observable<ChunckedMessage>`として取得できます。


```cs
var pastCommentFetcher = new NdgrPastCommentFetcher();

// 最低100件取得する
pastCommentFetcher
    .FetchPastComments(viewApiUri, 100)
    .Subscribe(x => Console.WriteLine(x.Message));

pastCommentFetcher.Dispose();
```

`FetchPastComments()`の引数としてコメントの件数が指定できます。ただしこの引数は**最低でもこの件数を取得する**という挙動をします（コメント取得はある程度まとまった単位で実行されるため、指定した個数ピッタリ取得することができない。）

またこの引数にnullを指定した場合は過去のコメントをすべて取得します。


#### 補足:コメントの順序

件数を指定した場合、現在時刻から遡る方向で指定件数分はコメントを取得します。
ただし、`Observable`から発行されるメッセージの順序は時刻順であることを**保証しません**。

順序が重要な場合は受信後に自身でソートしてください。

```cs
using var pastCommentFetcher = new NdgrPastCommentFetcher();

// 最低100件取得する
var list = await pastCommentFetcher
    .FetchPastComments(viewApiUri, 100)
    .ToListAsync();

// 取得したコメントを並び替えて表示
foreach (var c in list
             .Where(x => x.Meta?.At != null)
             .OrderBy(x => x.Meta.At))
{
    Console.WriteLine(c);
}
```

### NdgrSnapshotFetcher

生放送の現在の状態(Snapshot)を取得するクライアントです。

```cs
using var ndgrSnapshotFetcher = new NdgrSnapshotFetcher();

// 現在の状態（運営コメントの設定やアンケートの状態）を取得する
await foreach (var chunkedMessage in ndgrSnapshotFetcher.FetchCurrentSnapshotAsync(viewApiUri))
{
    Console.WriteLine(chunkedMessage);
}
```

# ライセンスについて

MITライセンスです。

# 権利表記

### R3

MIT License

Copyright (c) 2024 Cysharp, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.


### Protocol Buffers

Copyright 2008 Google Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

    * Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above
copyright notice, this list of conditions and the following disclaimer
in the documentation and/or other materials provided with the
distribution.
    * Neither the name of Google Inc. nor the names of its
contributors may be used to endorse or promote products derived from
this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Code generated by the Protocol Buffer compiler is owned by the owner
of the input file used when generating it.  This code is not
standalone and requires a support library to be linked with it.  This
support library is itself covered by the above license.


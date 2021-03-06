# C++erの破天荒な幼馴染bot

## 概要

[C++erの破天荒な幼馴染](https://twitter.com/FriendOfCpper)です  
楽しく会話したりぶっ飛んだことをしたりC++で殴ってきたりします  
仲良くして下さい  

## コマンド

### [@FriendOfCpper](https://twitter.com/FriendOfCpper) [command] register_call_name {呼んで欲しい名前}

呼んでもらいたい名前を登録できます  
登録に成功したら登録できた旨のリプライが返って来ます  
登録した名前は挨拶などのリプライで使われます  

### [@FriendOfCpper](https://twitter.com/FriendOfCpper) [command] register_pre_developer

開発者登録の予約を行います  
開発者登録を行うと  

* 台詞の登録
* その他一部管理

などができるようになります  
とりあえず今は開発者登録予約が出来るだけです  
  
この予約を行うと、予約者に予約コード、[破天荒な幼馴染](https://twitter.com/FriendOfCpper)に予約キーがリプとして飛びます  
予約コードと予約キーを用いて[破天荒な幼馴染](https://twitter.com/FriendOfCpper)が承認するので、そのあとは開発者として[破天荒な幼馴染](https://twitter.com/FriendOfCpper)の開発に携わることが出来ます  

### [@FriendOfCpper](https://twitter.com/FriendOfCpper) [command] register_developer {予約コード} {予約キー}

開発者登録認証を行います  
[破天荒な幼馴染](https://twitter.com/FriendOfCpper)のみが実行できます（他の人がやっても無視されます）  
認証されると、認証が行われた旨のリプライが飛んでいきます  
認証が行われると、開発者として開発に携わることが出来ます  

### [@FriendOfCpper](https://twitter.com/FriendOfCpper) [command] register_regex {登録したい正規表現}

リプライの際にマッチングに使われ正規表現を登録します  
開発者登録を行った人だけが実行できます（他の人は無視されます）  
登録に成功すると、正規表現に割り振られたIDがリプライで返って来ます  
既に登録済みの正規表現の場合は、既に割り振られているIDがリプライで返って来ます  
このIDはリプライ本文の登録の際などに必要なので、しっかりと控えておいて下さい  

### [@FriendOfCpper](https://twitter.com/FriendOfCpper) [command] register_reply {正規表現ID} {登録したい正規表現}

登録した正規表現に対するリプライを登録します  
開発者登録を行った人だけが実行できます（他の人は無視されます）  
登録に成功すると、登録に成功した旨のリプライ、もしくは既に登録済である旨のリプライが返って来ます  
リプライ本文には変数が使えて、実際に登録したリプライが使われるときに状況に応じて変化します  
現在使える変数は以下のとおりです  

* #{名前} ... リプライを送ってきた人に対する呼称に置き換えられます。呼称登録を行っている場合は登録した名前、していない場合はScreenNameになります
* #{挨拶} ... 時間に応じて「おはよう」や「こんにちは」などの挨拶に置き換えられます
* #{年} ... 日本時間におけるその時の西暦に置き換えられます
* #{月} ... 日本時間におけるその時の月に置き換えられます
* #{日} ... 日本時間におけるその時の日に置き換えられます
* #{時} ... 日本時間におけるその時の時間に置き換えられます
* #{分} ... 日本時間におけるその時の分に置き換えられます
* #{秒} ... 日本時間におけるその時の秒に置き換えられます


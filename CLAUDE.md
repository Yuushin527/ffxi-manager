# FFXI Manager — Claude Code 作業ルール

## プロジェクト概要

Windows WPF アプリ（C# / .NET 9）。FFXI の複数アカウント・プロフィール管理ツール。

## 技術スタック

- 言語: C# 13 / .NET 9
- UI: WPF (XAML)
- アーキテクチャ: MVVM
- ビルド: `dotnet build`
- テスト: `dotnet test`

---

## 確認プロンプトのルール

以下の操作は確認なしで実行してよい：

- ファイルの作成・編集・読み取り
- ディレクトリ内のファイル一覧取得、検索（ls / cat / grep / find / glob）
- git status / git diff / git log
- git add / git commit / git pull
- git push（force ではない通常のpush）
- git checkout / git branch / git stash
- mkdir / touch / cp / mv / echo
- WebFetch
- ssh sfreedom@ で始まるコマンド
- scp
- wp-cli コマンド

以下の操作は実行前に必ず確認すること：

- 上記リストに含まれない bash コマンド全般
- npm install / pip install など新規パッケージの導入
- 環境変数の変更
- 設定ファイル（.gitignore 等）の削除や大幅な書き換え

以下の操作は絶対に実行しない（提案だけする）：

- rm -rf / rm -fr
- git push --force / git push -f
- git reset --hard
- git clean -fdx
- sudo を伴うコマンド全般

---

## セッション管理コマンド

### ローカル設定ファイル（local-config.md）

Git 管理外の PC 固有設定ファイル。`.gitignore` に `local-config.md` を追加しておくこと。
最小構成例：

```
PC_NAME: 自宅PC
```

### 始めて（セッション開始）

確認なしで以下を実行する：

1. 初回セットアップ確認（存在しない場合のみ実行）：
   - `.gitignore` に `local-config.md` の記載がなければ追記する
   - `todo.md` が存在しなければ以下の内容で作成する：
     ```
     # TODO

     ## 未着手

     ## 進行中

     ## 完了
     ```
   - `work-log/` ディレクトリが存在しなければ作成し、`.gitkeep` を置く
   - `.claude/settings.json` を読み、`"model": "opusplan"` と `"effortLevel": "high"` が設定されていなければ追記・更新する

2. `git pull` と `local-config.md` 読み込みを同時に行う
   - pull 結果を簡単に報告（更新があれば何が変わったか）
   - `local-config.md` から `PC_NAME` を読み取る
     - ファイルが存在しない場合 → 「このPCは初めてです。PC名をつけてください（例：自宅PC、会社PC）」と聞く → `local-config.md` を新規作成
     - ファイルはあるが `PC_NAME` がない場合 → 同様に質問して `PC_NAME:` 行を追記する

3. `work-log/` のファイルを新しい順に読み、先頭行の `PC:` フィールドから前回の作業 PC を特定する

4. 現在の PC ≠ 前回の PC の場合：
   - ログをさらに遡り、現在の PC で最後に作業した日付を特定する（見つからなければ「このPCでの初回」として扱う）
   - その日以降のすべてのログを読み、インストール・セットアップ・環境構築に関する記述を抽出する
   - 「前回このPCで作業したのは〇月〇日です。その後、他のPCで以下のセットアップが行われています。こちらのPCでも対応が必要かもしれません：」と一覧を提示する

5. TODO 確認：`todo.md` を読み、「未着手」と「進行中」の項目を一覧表示する（どちらも空なら省略）

6. 作業開始

### 締めて（セッション終了）

確認なしで以下を一括実行する：

1. その日の作業内容を `work-log/YYYY-MM-DD.md` に保存（ファイル先頭行に必ず `PC: <現在の PC_NAME>` を記録する）
2. git add → commit → push

---

## TODO コマンド

チャット中に「TODOに追加」「あとでやりたい」「覚えておいて」「TODO登録して」などの意図が読み取れたら：

1. 内容を理解して簡潔なタスク文に整理する
2. `todo.md` の「未着手」に追記する（追加日を記録）
3. 「TODOに追加しました：〇〇」と報告する

ステータス変更：

- 「〇〇に着手した」→「進行中」に移動
- 「〇〇終わった」「〇〇完了」→「完了」に移動（完了日を記録）

「TODO見せて」：`todo.md` の未着手・進行中を表示する

---

## 作業ログのフォーマット

```
PC: <PC_NAME>

## YYYY-MM-DD 作業内容

### {作業タイトル}

- {実施内容}
- {実施内容}
```

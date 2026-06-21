# 記帳行事曆 (Finance Calendar App)

> 視窗程式設計 (II) 期末專題｜元智大學資訊工程學系｜學號：1121421 許志文

---

## 專案簡介

「記帳行事曆」是一款以 C# WinForms 開發的個人財務管理工具，整合三大核心功能：

- **訂閱管理**：集中管理 Netflix、Spotify、Disney+ 等固定月費項目，自動計算每月訂閱總支出，並將扣款日同步至行事曆
- **日常記帳**：記錄日常消費，支援現金／信用卡付款方式與分類（飲食、交通、娛樂等）
- **行事曆**：月視圖行事曆，提供「行事曆模式」與「記帳模式」雙模式切換

---

## 執行環境

| 項目 | 需求 |
|------|------|
| 作業系統 | Windows 10 / 11 |
| 執行框架 | .NET 6.0 (Windows) |
| 開發工具 | Visual Studio 2022 |

---

## 安裝與執行

### 方法一：Visual Studio

1. 解壓縮專案資料夾
2. 以 Visual Studio 2022 開啟 `FinanceCalendarApp.csproj`
3. 按 `F5` 執行

### 方法二：命令列

```powershell
cd FinanceCalendarApp
dotnet run
```

---

## 功能說明

### 📊 儀表板
- 本月支出、收入、訂閱月費三張摘要卡片
- 本月分類支出統計（飲食、交通、娛樂⋯⋯）
- 目前啟用的訂閱項目總覽（橫向排列）
- 最近消費紀錄清單

### 🔄 訂閱管理
- 新增／編輯／刪除訂閱項目
- 欄位：名稱、圖示、分類、金額、幣別、扣款日、月付／年付
- 啟用／停用開關
- 訂閱扣款日自動同步至行事曆（未來 12 個月）

### 💵 日常記帳
- 新增支出／收入紀錄
- 選擇付款方式（現金 / 信用卡）與消費分類
- 月份切換，查看歷史帳目
- 顯示當月收支小計

### 📅 行事曆
提供兩種模式切換：

| 模式 | 說明 |
|------|------|
| 行事曆模式 | 顯示手動新增的事件（整天或指定時段），6 種顏色標籤 |
| 記帳模式 | 顯示每日消費金額與訂閱扣款，右側面板列出當日明細 |

---

## 資料儲存

所有資料以 **JSON 格式**儲存於本機：

```
%AppData%\FinanceCalendarApp\data.json
```

包含三張資料表：訂閱清單、消費紀錄、行事曆事件。

---

## 專案結構

```
FinanceCalendarApp/
├── Models/
│   └── Models.cs              # 資料模型（Subscription, Transaction, CalendarEvent）
├── Services/
│   └── DataService.cs         # JSON 讀寫、訂閱同步邏輯
├── Forms/
│   ├── MainForm.cs            # 主視窗（側邊欄導航）
│   ├── DashboardPage.cs       # 儀表板頁面
│   ├── SubscriptionPage.cs    # 訂閱管理頁面
│   ├── TransactionPage.cs     # 日常記帳頁面
│   └── CalendarPage.cs        # 行事曆頁面（含雙模式）
└── Program.cs                 # 程式進入點
```

---

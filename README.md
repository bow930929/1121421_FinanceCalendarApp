# 記帳行事曆 - FinanceCalendarApp

## 功能簡介
- 📊 **儀表板**：本月支出/收入總覽、訂閱費用佔比、分類統計、預算警告
- 🔄 **訂閱管理**：Netflix、Spotify 等固定月費管理，自動計入每月支出並標記在行事曆
- 💵 **日常記帳**：區分現金/信用卡，分類（飲食/交通/娛樂…），月份切換查詢
- 📅 **行事曆**：月視圖，整天或指定時段事件，訂閱扣款日自動顯示

## 執行環境
- Windows 10 / 11
- .NET 6.0 或以上（Windows Forms）
- Visual Studio 2022（建議）

## 開啟方式
1. 以 Visual Studio 開啟 `FinanceCalendarApp.csproj`
2. 按 F5 執行，或 `dotnet run`

## 資料儲存
資料以 JSON 格式儲存於：
`%AppData%\FinanceCalendarApp\data.json`

## 專案結構
```
FinanceCalendarApp/
├── Models/
│   └── Models.cs          # Subscription, Transaction, CalendarEvent
├── Services/
│   └── DataService.cs     # JSON 讀寫、訂閱同步
├── Forms/
│   ├── MainForm.cs        # 主視窗（側邊欄導航）
│   ├── DashboardPage.cs   # 儀表板
│   ├── SubscriptionPage.cs# 訂閱管理
│   ├── TransactionPage.cs # 日常記帳
│   └── CalendarPage.cs    # 行事曆
└── Program.cs
```

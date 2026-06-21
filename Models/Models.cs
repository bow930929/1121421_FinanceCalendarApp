using System;
using System.Collections.Generic;

namespace FinanceCalendarApp.Models
{
    public class Subscription
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "💳";
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TWD";
        public int BillingDay { get; set; } = 1;   // 每月幾號扣款
        public string Cycle { get; set; } = "月付"; // 月付 / 年付
        public string Category { get; set; } = "娛樂";
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public string Note { get; set; } = "";
    }

    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; } = DateTime.Today;
        public decimal Amount { get; set; }
        public string Category { get; set; } = "其他";
        public string PaymentMethod { get; set; } = "現金"; // 現金 / 信用卡
        public string Note { get; set; } = "";
        public bool IsSubscription { get; set; } = false;
        public string SubscriptionId { get; set; } = ""; // 若是訂閱自動產生
        public string Type { get; set; } = "支出"; // 支出 / 收入
    }

    public class CalendarEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Today;
        public bool IsAllDay { get; set; } = true;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Color { get; set; } = "#4A90D9";
        public string Note { get; set; } = "";
        public bool IsSubscriptionEvent { get; set; } = false;
        public string SubscriptionId { get; set; } = "";
    }

    public class AppData
    {
        public List<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
        public decimal MonthlyBudget { get; set; } = 0;
    }
}

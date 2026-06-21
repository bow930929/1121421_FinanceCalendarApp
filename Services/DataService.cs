using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceCalendarApp.Models;

namespace FinanceCalendarApp.Services
{
    public static class DataService
    {
        private static readonly string DataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FinanceCalendarApp");
        private static readonly string DataFile = Path.Combine(DataFolder, "data.json");

        private static AppData _cache = null;

        private static JsonSerializerOptions JsonOptions => new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static AppData Load()
        {
            if (_cache != null) return _cache;
            try
            {
                if (!Directory.Exists(DataFolder))
                    Directory.CreateDirectory(DataFolder);

                if (!File.Exists(DataFile))
                {
                    _cache = new AppData();
                    return _cache;
                }
                string json = File.ReadAllText(DataFile);
                _cache = JsonSerializer.Deserialize<AppData>(json, JsonOptions) ?? new AppData();
            }
            catch
            {
                _cache = new AppData();
            }
            return _cache;
        }

        public static void Save()
        {
            if (_cache == null) return;
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
            string json = JsonSerializer.Serialize(_cache, JsonOptions);
            File.WriteAllText(DataFile, json);
        }

        public static void InvalidateCache() => _cache = null;

        // ── Subscriptions ──────────────────────────────────────────────
        public static List<Subscription> GetSubscriptions(bool activeOnly = false)
        {
            var data = Load();
            return activeOnly
                ? data.Subscriptions.Where(s => s.IsActive).ToList()
                : data.Subscriptions;
        }

        public static void SaveSubscription(Subscription sub)
        {
            var data = Load();
            var existing = data.Subscriptions.FindIndex(s => s.Id == sub.Id);
            if (existing >= 0) data.Subscriptions[existing] = sub;
            else data.Subscriptions.Add(sub);
            SyncSubscriptionEvents(sub);
            Save();
        }

        public static void DeleteSubscription(string id)
        {
            var data = Load();
            data.Subscriptions.RemoveAll(s => s.Id == id);
            data.CalendarEvents.RemoveAll(e => e.SubscriptionId == id);
            Save();
        }

        // ── Transactions ───────────────────────────────────────────────
        public static List<Transaction> GetTransactions(int year, int month)
        {
            return Load().Transactions
                .Where(t => t.Date.Year == year && t.Date.Month == month)
                .OrderByDescending(t => t.Date)
                .ToList();
        }

        public static List<Transaction> GetAllTransactions()
        {
            return Load().Transactions.OrderByDescending(t => t.Date).ToList();
        }

        public static void SaveTransaction(Transaction tx)
        {
            var data = Load();
            var idx = data.Transactions.FindIndex(t => t.Id == tx.Id);
            if (idx >= 0) data.Transactions[idx] = tx;
            else data.Transactions.Add(tx);
            Save();
        }

        public static void DeleteTransaction(string id)
        {
            var data = Load();
            data.Transactions.RemoveAll(t => t.Id == id);
            Save();
        }

        // ── Calendar Events ────────────────────────────────────────────
        public static List<CalendarEvent> GetEventsForMonth(int year, int month)
        {
            return Load().CalendarEvents
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .ToList();
        }

        public static List<CalendarEvent> GetEventsForDate(DateTime date)
        {
            return Load().CalendarEvents
                .Where(e => e.Date.Date == date.Date)
                .ToList();
        }

        public static void SaveCalendarEvent(CalendarEvent ev)
        {
            var data = Load();
            var idx = data.CalendarEvents.FindIndex(e => e.Id == ev.Id);
            if (idx >= 0) data.CalendarEvents[idx] = ev;
            else data.CalendarEvents.Add(ev);
            Save();
        }

        public static void DeleteCalendarEvent(string id)
        {
            var data = Load();
            data.CalendarEvents.RemoveAll(e => e.Id == id);
            Save();
        }

        // ── Budget ─────────────────────────────────────────────────────
        public static decimal GetMonthlyBudget() => Load().MonthlyBudget;
        public static void SetMonthlyBudget(decimal budget)
        {
            Load().MonthlyBudget = budget;
            Save();
        }

        // ── Auto-sync subscription billing days to calendar ────────────
        public static void SyncSubscriptionEvents(Subscription sub)
        {
            var data = Load();
            // Remove old auto-events for this subscription (next 12 months)
            data.CalendarEvents.RemoveAll(e => e.SubscriptionId == sub.Id && e.IsSubscriptionEvent);

            if (!sub.IsActive) return;

            int months = sub.Cycle == "年付" ? 12 : 12;
            for (int i = 0; i < months; i++)
            {
                try
                {
                    var baseDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(i);
                    if (sub.Cycle == "年付" && i > 0) break;
                    int day = Math.Min(sub.BillingDay, DateTime.DaysInMonth(baseDate.Year, baseDate.Month));
                    var eventDate = new DateTime(baseDate.Year, baseDate.Month, day);

                    data.CalendarEvents.Add(new CalendarEvent
                    {
                        Title = $"💳 {sub.Name} ${sub.Amount}",
                        Date = eventDate,
                        IsAllDay = true,
                        Color = "#E74C3C",
                        IsSubscriptionEvent = true,
                        SubscriptionId = sub.Id,
                        Note = $"訂閱自動扣款：{sub.Name}"
                    });
                }
                catch { }
            }
        }

        public static void SyncAllSubscriptions()
        {
            var data = Load();
            data.CalendarEvents.RemoveAll(e => e.IsSubscriptionEvent);
            foreach (var sub in data.Subscriptions.Where(s => s.IsActive))
                SyncSubscriptionEvents(sub);
            Save();
        }

        // ── Statistics ─────────────────────────────────────────────────
        public static decimal GetMonthlySubscriptionTotal()
        {
            return Load().Subscriptions
                .Where(s => s.IsActive)
                .Sum(s => s.Cycle == "年付" ? s.Amount / 12 : s.Amount);
        }

        public static Dictionary<string, decimal> GetCategoryTotals(int year, int month)
        {
            return GetTransactions(year, month)
                .Where(t => t.Type == "支出")
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }
    }
}

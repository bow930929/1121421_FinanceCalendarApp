using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FinanceCalendarApp.Models;
using FinanceCalendarApp.Services;

namespace FinanceCalendarApp.Forms
{
    public class CalendarPage : UserControl
    {
        private int _year, _month;
        private Panel pnlCalendar;
        private Panel pnlEventDetail;
        private Label lblMonth;
        private DateTime _selectedDate;
        private bool _isLedgerMode = false; // false = 行事曆模式, true = 記帳模式

        private Button btnModeCalendar, btnModeLedger;

        private static readonly string[] WeekDays = { "日", "一", "二", "三", "四", "五", "六" };

        public CalendarPage()
        {
            _year = DateTime.Today.Year;
            _month = DateTime.Today.Month;
            _selectedDate = DateTime.Today;
            this.BackColor = Color.Transparent;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Controls.Clear();

            // ── Mode toggle ───────────────────────────────────────────
            var modePanel = new Panel { Location = new Point(0, 0), Size = new Size(400, 36), BackColor = Color.Transparent };

            btnModeCalendar = ModeBtn("📅 行事曆模式", true);
            btnModeCalendar.Location = new Point(0, 0);
            btnModeCalendar.Click += (s, e) => { _isLedgerMode = false; SetModeButtons(); RebuildCalendar(); UpdateDetailPanel(); };

            btnModeLedger = ModeBtn("💵 記帳模式", false);
            btnModeLedger.Location = new Point(155, 0);
            btnModeLedger.Click += (s, e) => { _isLedgerMode = true; SetModeButtons(); RebuildCalendar(); UpdateDetailPanel(); };

            modePanel.Controls.AddRange(new Control[] { btnModeCalendar, btnModeLedger });
            this.Controls.Add(modePanel);

            // ── Nav bar ───────────────────────────────────────────────
            var nav = new Panel { Location = new Point(0, 44), Size = new Size(1000, 40), BackColor = Color.Transparent };

            var btnPrev = NavBtn("◀");
            btnPrev.Location = new Point(0, 5);
            btnPrev.Click += (s, e) => ChangeMonth(-1);

            lblMonth = new Label
            {
                Text = FormatMonth(), Location = new Point(46, 0), Size = new Size(180, 40),
                Font = new Font("微軟正黑體", 12f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter
            };

            var btnNext = NavBtn("▶");
            btnNext.Location = new Point(228, 5);
            btnNext.Click += (s, e) => ChangeMonth(1);

            var btnToday = new Button
            {
                Text = "今天", Location = new Point(274, 7), Size = new Size(56, 26),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White, Font = new Font("微軟正黑體", 8.5f), Cursor = Cursors.Hand
            };
            btnToday.FlatAppearance.BorderSize = 0;
            btnToday.Click += (s, e) => {
                _year = DateTime.Today.Year; _month = DateTime.Today.Month;
                _selectedDate = DateTime.Today;
                lblMonth.Text = FormatMonth();
                RebuildCalendar(); UpdateDetailPanel();
            };

            // Add event / add transaction button (context-sensitive)
            var btnAdd = new Button
            {
                Text = _isLedgerMode ? "＋ 新增消費" : "＋ 新增事件",
                Location = new Point(820, 5), Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White, Font = new Font("微軟正黑體", 9.5f), Cursor = Cursors.Hand,
                Name = "btnAdd"
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => {
                if (_isLedgerMode) OpenTransactionDialog(null, _selectedDate);
                else OpenEventDialog(null, _selectedDate);
            };

            nav.Controls.AddRange(new Control[] { btnPrev, lblMonth, btnNext, btnToday, btnAdd });
            this.Controls.Add(nav);

            // ── Calendar grid ─────────────────────────────────────────
            pnlCalendar = new Panel
            {
                Location = new Point(0, 90),
                Size = new Size(710, 540),
                BackColor = Color.White
            };
            this.Controls.Add(pnlCalendar);

            // ── Detail panel ──────────────────────────────────────────
            pnlEventDetail = new Panel
            {
                Location = new Point(720, 90),
                Size = new Size(290, 540),
                BackColor = Color.White,
                AutoScroll = true
            };
            AddBorder(pnlEventDetail);
            this.Controls.Add(pnlEventDetail);

            SetModeButtons();
            RebuildCalendar();
            UpdateDetailPanel();
        }

        private void SetModeButtons()
        {
            btnModeCalendar.BackColor = !_isLedgerMode ? Color.FromArgb(52, 152, 219) : Color.FromArgb(230, 235, 245);
            btnModeCalendar.ForeColor = !_isLedgerMode ? Color.White : Color.FromArgb(60, 70, 90);
            btnModeLedger.BackColor = _isLedgerMode ? Color.FromArgb(39, 174, 96) : Color.FromArgb(230, 235, 245);
            btnModeLedger.ForeColor = _isLedgerMode ? Color.White : Color.FromArgb(60, 70, 90);

            // Update the add button label
            foreach (Control c in this.Controls)
                if (c is Panel nav && nav.Location.Y == 44)
                    foreach (Control nc in nav.Controls)
                        if (nc.Name == "btnAdd")
                            nc.Text = _isLedgerMode ? "＋ 新增消費" : "＋ 新增事件";
        }

        // ── Calendar grid builder ──────────────────────────────────────
        private void RebuildCalendar()
        {
            pnlCalendar.Controls.Clear();
            int colW = 101, rowH = 87;
            DateTime today = DateTime.Today;
            DateTime first = new DateTime(_year, _month, 1);
            int startDow = (int)first.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(_year, _month);

            // Week headers
            for (int i = 0; i < 7; i++)
            {
                pnlCalendar.Controls.Add(new Label
                {
                    Text = WeekDays[i], Location = new Point(i * colW, 0), Size = new Size(colW, 28),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("微軟正黑體", 9f, FontStyle.Bold),
                    BackColor = Color.FromArgb(240, 243, 248),
                    ForeColor = i == 0 ? Color.FromArgb(231, 76, 60) : i == 6 ? Color.FromArgb(52, 152, 219) : Color.FromArgb(40, 50, 70)
                });
            }

            // Collect data for the month
            var monthEvents = _isLedgerMode
                ? null
                : DataService.GetEventsForMonth(_year, _month);

            var monthTx = _isLedgerMode
                ? DataService.GetTransactions(_year, _month)
                : null;

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(_year, _month, d);
                int slot = d - 1 + startDow;
                int col = slot % 7, row = slot / 7;
                bool isToday = date == today, isSelected = date == _selectedDate;
                bool isSun = col == 0, isSat = col == 6;

                var cell = new Panel
                {
                    Location = new Point(col * colW, 28 + row * rowH),
                    Size = new Size(colW - 1, rowH - 1),
                    BackColor = isSelected ? Color.FromArgb(235, 244, 255) : isToday ? Color.FromArgb(250, 248, 240) : Color.White,
                    Cursor = Cursors.Hand,
                    Tag = date
                };
                cell.Paint += (s, e) => {
                    var p = (Panel)s;
                    var d2 = (DateTime)p.Tag;
                    using var pen = new Pen(Color.FromArgb(215, 220, 232), 0.5f);
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1));
                    if (d2 == _selectedDate)
                        using (var selPen = new Pen(Color.FromArgb(52, 152, 219), 2f))
                            e.Graphics.DrawRectangle(selPen, new Rectangle(1, 1, p.Width - 3, p.Height - 3));
                };

                cell.Controls.Add(new Label
                {
                    Text = isToday ? d + " ●" : d.ToString(),
                    Location = new Point(4, 2), Size = new Size(colW - 8, 20),
                    TextAlign = ContentAlignment.TopLeft,
                    Font = isToday ? new Font("微軟正黑體", 9.5f, FontStyle.Bold) : new Font("微軟正黑體", 9f),
                    ForeColor = isToday ? Color.FromArgb(52, 152, 219) : isSun ? Color.FromArgb(231, 76, 60) : isSat ? Color.FromArgb(52, 152, 219) : Color.FromArgb(40, 50, 70)
                });

                // ── Ledger mode: show spending + subscription amounts
                if (_isLedgerMode)
                {
                    var dayTx = monthTx!.Where(t => t.Date.Date == date.Date).ToList();
                    var daySubs = DataService.GetEventsForDate(date)
                        .Where(ev => ev.IsSubscriptionEvent).ToList();
                    var allSubs2 = DataService.GetSubscriptions();

                    decimal txExp = dayTx.Where(t => t.Type == "支出").Sum(t => t.Amount);
                    decimal subAmt = daySubs.Sum(ev => {
                        var sub = allSubs2.FirstOrDefault(s => s.Id == ev.SubscriptionId);
                        return sub?.Amount ?? 0m;
                    });
                    decimal totalExp = txExp + subAmt;

                    if (totalExp > 0)
                        cell.Controls.Add(new Label
                        {
                            Text = $"-NT${totalExp:N0}",
                            Location = new Point(2, 22), Size = new Size(colW - 4, 16),
                            Font = new Font("微軟正黑體", 7.5f, FontStyle.Bold),
                            ForeColor = Color.FromArgb(192, 57, 43),
                            TextAlign = ContentAlignment.TopLeft
                        });

                    int chipY = 38;
                    foreach (var subEv in daySubs.Take(2))
                    {
                        cell.Controls.Add(new Label
                        {
                            Text = "🔄 訂閱",
                            Location = new Point(2, chipY), Size = new Size(colW - 4, 15),
                            Font = new Font("微軟正黑體", 7f),
                            ForeColor = Color.FromArgb(192, 57, 43),
                            BackColor = Color.FromArgb(255, 235, 235)
                        });
                        chipY += 17;
                    }
                    var categories = dayTx.GroupBy(t => t.Category).Take(Math.Max(0, 2 - daySubs.Count)).ToList();
                    foreach (var grp in categories)
                    {
                        cell.Controls.Add(new Label
                        {
                            Text = $"{CategoryIcon(grp.Key)} {grp.Key}",
                            Location = new Point(2, chipY), Size = new Size(colW - 4, 15),
                            Font = new Font("微軟正黑體", 7f),
                            ForeColor = Color.FromArgb(80, 80, 80),
                            BackColor = Color.FromArgb(240, 240, 248)
                        });
                        chipY += 17;
                    }
                }
                else
                {
                    // ── Calendar mode: show manual events only (no subscription billing marks)
                    var dayEvents = monthEvents!.Where(ev => ev.Date.Date == date.Date && !ev.IsSubscriptionEvent).Take(3).ToList();
                    for (int ei = 0; ei < dayEvents.Count; ei++)
                    {
                        Color evColor = ParseColor(dayEvents[ei].Color);
                        cell.Controls.Add(new Label
                        {
                            Text = TruncateText(dayEvents[ei].Title, 9),
                            Location = new Point(2, 22 + ei * 18), Size = new Size(colW - 5, 16),
                            BackColor = Color.FromArgb(45, evColor.R, evColor.G, evColor.B),
                            ForeColor = evColor, Font = new Font("微軟正黑體", 7.5f),
                            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(2, 0, 0, 0)
                        });
                    }
                }

                DateTime cap = date;
                void SelectThis(object? sender, EventArgs ea) { SelectDate(cap); }
                cell.Click += SelectThis;
                foreach (Control c in cell.Controls) c.Click += SelectThis;
                pnlCalendar.Controls.Add(cell);
            }
        }

        private void SelectDate(DateTime date)
        {
            _selectedDate = date;
            RebuildCalendar();
            UpdateDetailPanel();
        }

        // ── Right panel ────────────────────────────────────────────────
        private void UpdateDetailPanel()
        {
            pnlEventDetail.Controls.Clear();

            var hdr = new Label
            {
                Text = _selectedDate.ToString("M月d日 (ddd)"),
                Location = new Point(0, 0), Size = new Size(290, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微軟正黑體", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 243, 248)
            };
            pnlEventDetail.Controls.Add(hdr);

            var btnAdd = new Button
            {
                Text = _isLedgerMode ? "＋ 新增消費" : "＋ 新增事件",
                Location = new Point(10, 44), Size = new Size(268, 28),
                FlatStyle = FlatStyle.Flat, BackColor = _isLedgerMode ? Color.FromArgb(39, 174, 96) : Color.FromArgb(52, 152, 219),
                ForeColor = Color.White, Font = new Font("微軟正黑體", 9f), Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => {
                if (_isLedgerMode) OpenTransactionDialog(null, _selectedDate);
                else OpenEventDialog(null, _selectedDate);
            };
            pnlEventDetail.Controls.Add(btnAdd);

            if (_isLedgerMode)
                BuildLedgerDetail();
            else
                BuildEventDetail();
        }

        private void BuildLedgerDetail()
        {
            // 一般消費紀錄
            var txs = DataService.GetTransactions(_year, _month)
                .Where(t => t.Date.Date == _selectedDate.Date).ToList();

            // 訂閱扣款事件（只在記帳模式顯示）
            var subEvents = DataService.GetEventsForDate(_selectedDate)
                .Where(ev => ev.IsSubscriptionEvent).ToList();

            // 取得訂閱金額用於顯示（從訂閱清單比對）
            var allSubs = DataService.GetSubscriptions();

            decimal exp = txs.Where(t => t.Type == "支出").Sum(t => t.Amount);
            decimal inc = txs.Where(t => t.Type == "收入").Sum(t => t.Amount);
            decimal subExp = subEvents.Sum(ev => {
                var sub = allSubs.FirstOrDefault(s => s.Id == ev.SubscriptionId);
                return sub?.Amount ?? 0m;
            });
            decimal totalExp = exp + subExp;

            bool hasAnything = txs.Count > 0 || subEvents.Count > 0;
            if (hasAnything)
            {
                pnlEventDetail.Controls.Add(new Label
                {
                    Text = $"支出 NT${totalExp:N0}　收入 NT${inc:N0}",
                    Location = new Point(10, 82), Size = new Size(268, 20),
                    Font = new Font("微軟正黑體", 8.5f), ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter
                });
            }

            int startY = 110;
            if (!hasAnything)
            {
                pnlEventDetail.Controls.Add(new Label { Text = "今日無消費紀錄", Location = new Point(0, startY), Size = new Size(290, 30), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray });
                return;
            }

            int cardIdx = 0;

            // ── 訂閱扣款卡片（記帳模式專屬）────────────────────────
            foreach (var ev in subEvents)
            {
                var sub = allSubs.FirstOrDefault(s => s.Id == ev.SubscriptionId);
                string amtText = sub != null ? $"-NT$ {sub.Amount:N0}" : "";
                var card = new Panel { Location = new Point(8, startY + cardIdx * 76), Size = new Size(270, 68), BackColor = Color.FromArgb(255, 248, 248) };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(4, 68), BackColor = Color.FromArgb(231, 76, 60) });
                card.Controls.Add(new Label { Text = $"🔄 {sub?.Name ?? ev.Title}", Location = new Point(10, 6), Size = new Size(250, 18), Font = new Font("微軟正黑體", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(40, 50, 70) });
                card.Controls.Add(new Label { Text = $"{sub?.Cycle ?? "訂閱"} 自動扣款", Location = new Point(10, 24), Size = new Size(250, 16), Font = new Font("微軟正黑體", 8f), ForeColor = Color.Gray });
                card.Controls.Add(new Label { Text = amtText, Location = new Point(10, 42), Size = new Size(250, 18), Font = new Font("微軟正黑體", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(192, 57, 43) });
                AddBorder(card);
                pnlEventDetail.Controls.Add(card);
                cardIdx++;
            }

            // ── 一般消費卡片 ──────────────────────────────────────────
            foreach (var t in txs)
            {
                bool isExp = t.Type == "支出";
                int cardH2 = t.IsSubscription ? 68 : 96;
                var card = new Panel { Location = new Point(6, startY + cardIdx * (cardH2 + 6)), Size = new Size(252, cardH2), BackColor = Color.White };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(4, cardH2), BackColor = isExp ? Color.FromArgb(231, 76, 60) : Color.FromArgb(39, 174, 96) });
                card.Controls.Add(new Label { Text = $"{CategoryIcon(t.Category)} {t.Category}", Location = new Point(10, 6), Size = new Size(232, 18), Font = new Font("微軟正黑體", 9f, FontStyle.Bold) });
                card.Controls.Add(new Label { Text = t.Note.Length > 0 ? t.Note : "-", Location = new Point(10, 24), Size = new Size(232, 16), Font = new Font("微軟正黑體", 8f), ForeColor = Color.Gray });
                card.Controls.Add(new Label { Text = (isExp ? "-" : "+") + $"NT$ {t.Amount:N0}", Location = new Point(10, 42), Size = new Size(232, 18), Font = new Font("微軟正黑體", 9.5f, FontStyle.Bold), ForeColor = isExp ? Color.FromArgb(192, 57, 43) : Color.FromArgb(39, 174, 96) });

                if (!t.IsSubscription)
                {
                    var btnE = SmallBtn("✏️", Color.FromArgb(243, 156, 18), 10, 66);
                    btnE.Size = new Size(44, 22);
                    btnE.Tag = t; btnE.Click += (s, e2) => { OpenTransactionDialog((Transaction)((Button)s).Tag, _selectedDate); };
                    var btnD = SmallBtn("🗑️", Color.FromArgb(231, 76, 60), 60, 66);
                    btnD.Size = new Size(44, 22);
                    btnD.Tag = t; btnD.Click += (s, e2) => {
                        if (MessageBox.Show("確定刪除？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        { DataService.DeleteTransaction(((Transaction)((Button)s).Tag).Id); UpdateDetailPanel(); RebuildCalendar(); }
                    };
                    card.Controls.AddRange(new Control[] { btnE, btnD });
                }
                AddBorder(card);
                pnlEventDetail.Controls.Add(card);
                cardIdx++;
            }
        }

        private void BuildEventDetail()
        {
            // 行事曆模式只顯示手動新增的事件，不顯示訂閱扣款
            var events = DataService.GetEventsForDate(_selectedDate)
                .Where(ev => !ev.IsSubscriptionEvent).ToList();
            int startY = 82;
            if (events.Count == 0)
            {
                pnlEventDetail.Controls.Add(new Label { Text = "今日無事件", Location = new Point(0, startY + 10), Size = new Size(290, 30), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray });
                return;
            }
            for (int i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                Color evColor = ParseColor(ev.Color);
                var card = new Panel { Location = new Point(6, startY + i * 96), Size = new Size(252, 88), BackColor = Color.White };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(4, 88), BackColor = evColor });
                card.Controls.Add(new Label { Text = ev.Title, Location = new Point(10, 6), Size = new Size(232, 20), Font = new Font("微軟正黑體", 9.5f, FontStyle.Bold) });
                string timeText = ev.IsAllDay ? "整天" : $"{ev.StartTime:hh\\:mm} - {ev.EndTime:hh\\:mm}";
                card.Controls.Add(new Label { Text = timeText, Location = new Point(10, 26), Size = new Size(232, 16), Font = new Font("微軟正黑體", 8f), ForeColor = Color.Gray });
                if (!string.IsNullOrEmpty(ev.Note))
                    card.Controls.Add(new Label { Text = ev.Note, Location = new Point(10, 44), Size = new Size(232, 16), Font = new Font("微軟正黑體", 7.5f), ForeColor = Color.Gray });
                if (!ev.IsSubscriptionEvent)
                {
                    var btnE = SmallBtn("✏️", Color.FromArgb(243, 156, 18), 10, 56);
                    btnE.Size = new Size(44, 22);
                    btnE.Tag = ev; btnE.Click += (s, e2) => OpenEventDialog((CalendarEvent)((Button)s).Tag, _selectedDate);
                    var btnD = SmallBtn("🗑️", Color.FromArgb(231, 76, 60), 60, 56);
                    btnD.Size = new Size(44, 22);
                    btnD.Tag = ev; btnD.Click += (s, e2) => {
                        if (MessageBox.Show("確定刪除？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        { DataService.DeleteCalendarEvent(((CalendarEvent)((Button)s).Tag).Id); UpdateDetailPanel(); RebuildCalendar(); }
                    };
                    card.Controls.AddRange(new Control[] { btnE, btnD });
                }
                else
                    card.Controls.Add(new Label { Text = "🔄 訂閱自動", Location = new Point(180, 55), Size = new Size(85, 18), Font = new Font("微軟正黑體", 7.5f), ForeColor = Color.FromArgb(52, 152, 219) });
                AddBorder(card);
                pnlEventDetail.Controls.Add(card);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────
        private void OpenEventDialog(CalendarEvent? ev, DateTime date)
        {
            var dlg = new EventDialog(ev, date);
            if (dlg.ShowDialog() == DialogResult.OK)
            { DataService.SaveCalendarEvent(dlg.Result); UpdateDetailPanel(); RebuildCalendar(); }
        }

        private void OpenTransactionDialog(Transaction? tx, DateTime date)
        {
            var t = tx ?? new Transaction { Date = date };
            var dlg = new TransactionDialog(t);
            if (dlg.ShowDialog() == DialogResult.OK)
            { DataService.SaveTransaction(dlg.Result); UpdateDetailPanel(); RebuildCalendar(); }
        }

        private void ChangeMonth(int delta)
        {
            _month += delta;
            if (_month > 12) { _month = 1; _year++; }
            if (_month < 1) { _month = 12; _year--; }
            lblMonth.Text = FormatMonth();
            RebuildCalendar(); UpdateDetailPanel();
        }

        private Button ModeBtn(string text, bool isCalendar)
        {
            var b = new Button
            {
                Text = text, Size = new Size(148, 32), FlatStyle = FlatStyle.Flat,
                Font = new Font("微軟正黑體", 9f), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Button NavBtn(string text)
        {
            var b = new Button { Text = text, Size = new Size(36, 28), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(40, 50, 70), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private Button SmallBtn(string text, Color color, int x, int y)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(28, 22), FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void AddBorder(Panel p)
        {
            p.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 225, 235), 0.8f), new Rectangle(0, 0, p.Width - 1, p.Height - 1));
        }

        private string FormatMonth() => $"{_year} 年 {_month:D2} 月";
        private Color ParseColor(string hex) { try { return ColorTranslator.FromHtml(hex); } catch { return Color.FromArgb(74, 144, 217); } }
        private string TruncateText(string t, int max) => t.Length > max ? t.Substring(0, max) + "…" : t;
        private string CategoryIcon(string cat) => cat switch
        {
            "娛樂" => "🎮", "飲食" => "🍜", "交通" => "🚌", "購物" => "🛍️",
            "醫療" => "💊", "教育" => "📚", "住房" => "🏠", "訂閱" => "🔄", _ => "📦"
        };
    }

    // ── Event Dialog ──────────────────────────────────────────────────────
    public class EventDialog : Form
    {
        public CalendarEvent Result { get; private set; }
        private TextBox txtTitle, txtNote;
        private DateTimePicker dtpDate;
        private CheckBox chkAllDay;
        private DateTimePicker dtpStart, dtpEnd;
        private Panel pnlTimeRow;
        private Panel pnlColorPicker;
        private string _selectedColor = "#4A90D9";

        private static readonly (string hex, string name)[] ColorOptions = {
            ("#4A90D9","藍"),("#E74C3C","紅"),("#27AE60","綠"),
            ("#F39C12","橙"),("#9B59B6","紫"),("#34495E","深")
        };

        public EventDialog(CalendarEvent? ev, DateTime defaultDate)
        {
            bool isEdit = ev != null;
            Result = ev != null ? Clone(ev) : new CalendarEvent { Date = defaultDate };
            _selectedColor = Result.Color;

            this.Text = isEdit ? "編輯事件" : "新增事件";
            this.Size = new Size(400, 410);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("微軟正黑體", 9.5f);
            this.BackColor = Color.White;
            // Enter key saves
            this.AcceptButton = null; // set after btnOk created

            int y = 20, cx = 100, cw = 260;
            void Lbl(string t, int top) => this.Controls.Add(new Label { Text = t + "：", Location = new Point(10, top + 3), Width = 87, TextAlign = ContentAlignment.TopRight });

            Lbl("標題", y); txtTitle = new TextBox { Location = new Point(cx, y), Width = cw, Text = Result.Title }; this.Controls.Add(txtTitle); y += 36;
            Lbl("日期", y); dtpDate = new DateTimePicker { Location = new Point(cx, y), Width = 140, Value = Result.Date, Format = DateTimePickerFormat.Short }; this.Controls.Add(dtpDate); y += 36;
            Lbl("整天", y); chkAllDay = new CheckBox { Location = new Point(cx, y + 2), Checked = Result.IsAllDay, AutoSize = true };
            chkAllDay.CheckedChanged += (s, e) => pnlTimeRow.Visible = !chkAllDay.Checked;
            this.Controls.Add(chkAllDay); y += 36;

            pnlTimeRow = new Panel { Location = new Point(0, y), Size = new Size(380, 36), Visible = !Result.IsAllDay };
            Lbl("時間", y);
            dtpStart = new DateTimePicker { Location = new Point(cx, 0), Width = 110, Format = DateTimePickerFormat.Time, ShowUpDown = true, Value = DateTime.Today.Add(Result.StartTime ?? TimeSpan.FromHours(9)) };
            dtpEnd   = new DateTimePicker { Location = new Point(cx + 128, 0), Width = 110, Format = DateTimePickerFormat.Time, ShowUpDown = true, Value = DateTime.Today.Add(Result.EndTime ?? TimeSpan.FromHours(10)) };
            pnlTimeRow.Controls.AddRange(new Control[] { dtpStart, new Label { Text = "~", Location = new Point(cx + 116, 3), AutoSize = true }, dtpEnd });
            this.Controls.Add(pnlTimeRow); y += 44;

            Lbl("顏色", y);
            pnlColorPicker = new Panel { Location = new Point(cx, y), Size = new Size(260, 30) };
            for (int ci = 0; ci < ColorOptions.Length; ci++)
            {
                var (hex, name) = ColorOptions[ci];
                var colorBtn = new Panel { Size = new Size(34, 24), Location = new Point(ci * 40, 2), BackColor = ColorTranslator.FromHtml(hex), Cursor = Cursors.Hand, Tag = hex };
                if (hex == _selectedColor) colorBtn.BorderStyle = BorderStyle.Fixed3D;
                colorBtn.Click += (s, e2) => {
                    _selectedColor = (string)((Panel)s).Tag;
                    foreach (Panel cp in pnlColorPicker.Controls)
                        cp.BorderStyle = (string)cp.Tag == _selectedColor ? BorderStyle.Fixed3D : BorderStyle.None;
                };
                pnlColorPicker.Controls.Add(colorBtn);
            }
            this.Controls.Add(pnlColorPicker); y += 38;

            Lbl("備註", y); txtNote = new TextBox { Location = new Point(cx, y), Width = cw, Text = Result.Note }; this.Controls.Add(txtNote); y += 40;

            var btnOk = new Button { Text = "儲存 (Enter)", DialogResult = DialogResult.OK, Location = new Point(cx, y), Width = 130, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += Save;
            var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(cx + 140, y), Width = 90, FlatStyle = FlatStyle.Flat };

            this.Controls.AddRange(new Control[] { btnOk, btnCancel });
            this.AcceptButton = btnOk; // Enter key triggers save
            this.Height = y + 80;
        }

        private void Save(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) { MessageBox.Show("請輸入事件標題"); this.DialogResult = DialogResult.None; return; }
            Result.Title = txtTitle.Text.Trim();
            Result.Date = dtpDate.Value.Date;
            Result.IsAllDay = chkAllDay.Checked;
            Result.StartTime = chkAllDay.Checked ? (TimeSpan?)null : dtpStart.Value.TimeOfDay;
            Result.EndTime   = chkAllDay.Checked ? (TimeSpan?)null : dtpEnd.Value.TimeOfDay;
            Result.Color = _selectedColor;
            Result.Note = txtNote.Text.Trim();
        }

        private CalendarEvent Clone(CalendarEvent ev) => new CalendarEvent { Id = ev.Id, Title = ev.Title, Date = ev.Date, IsAllDay = ev.IsAllDay, StartTime = ev.StartTime, EndTime = ev.EndTime, Color = ev.Color, Note = ev.Note, IsSubscriptionEvent = ev.IsSubscriptionEvent, SubscriptionId = ev.SubscriptionId };
    }
}

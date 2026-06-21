using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FinanceCalendarApp.Services;

namespace FinanceCalendarApp.Forms
{
    public class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            this.BackColor = Color.Transparent;
            this.AutoScroll = true;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Controls.Clear();
            int year = DateTime.Today.Year, month = DateTime.Today.Month;

            var txList = DataService.GetTransactions(year, month);
            decimal monthlyExpense = txList.Where(t => t.Type == "支出").Sum(t => t.Amount);
            decimal monthlyIncome  = txList.Where(t => t.Type == "收入").Sum(t => t.Amount);
            decimal subTotal       = DataService.GetMonthlySubscriptionTotal();

            // ── Summary Cards (3 cards, no budget) ──────────────────
            var cardRow = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(this.Width > 0 ? this.Width - 40 : 1000, 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            cardRow.Controls.Add(MakeSummaryCard("本月支出", $"NT$ {monthlyExpense:N0}", Color.FromArgb(231, 76, 60),  "💸"));
            cardRow.Controls.Add(MakeSummaryCard("本月收入", $"NT$ {monthlyIncome:N0}",  Color.FromArgb(39, 174, 96),  "💰"));
            cardRow.Controls.Add(MakeSummaryCard("訂閱月費", $"NT$ {subTotal:N0}",       Color.FromArgb(52, 152, 219), "🔄"));
            this.Controls.Add(cardRow);

            // ── Section: Category breakdown ──────────────────────────
            AddSectionLabel("本月分類支出", 125);
            var cats = DataService.GetCategoryTotals(year, month);
            var catPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 153),
                Size = new Size(cardRow.Width, 56),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            if (cats.Count == 0)
                catPanel.Controls.Add(new Label { Text = "本月尚無支出紀錄", ForeColor = Color.Gray, AutoSize = true });
            else
                foreach (var kv in cats.OrderByDescending(x => x.Value))
                {
                    var chip = new Panel { Size = new Size(165, 42), BackColor = Color.White, Margin = new Padding(0, 0, 8, 8) };
                    chip.Controls.Add(new Label { Text = $"{CategoryIcon(kv.Key)} {kv.Key}  NT$ {kv.Value:N0}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("微軟正黑體", 9f) });
                    SetBorder(chip);
                    catPanel.Controls.Add(chip);
                }
            this.Controls.Add(catPanel);

            // ── Section: Active subscriptions (horizontal wrap) ───────
            AddSectionLabel("目前訂閱項目", 220);
            var subs = DataService.GetSubscriptions(activeOnly: true);
            var subPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 248),
                Size = new Size(cardRow.Width, 70),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            if (subs.Count == 0)
                subPanel.Controls.Add(new Label { Text = "尚無訂閱項目", ForeColor = Color.Gray, AutoSize = true });
            else
                foreach (var s in subs)
                {
                    var chip = new Panel { Size = new Size(185, 54), BackColor = Color.FromArgb(240, 248, 255), Margin = new Padding(0, 0, 10, 6) };
                    chip.Controls.Add(new Label
                    {
                        Text = $"{s.Icon} {s.Name}\r\nNT$ {s.Amount} / {s.Cycle}",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("微軟正黑體", 8.5f)
                    });
                    SetBorder(chip);
                    subPanel.Controls.Add(chip);
                }
            this.Controls.Add(subPanel);

            // ── Section: Recent transactions ─────────────────────────
            // Calculate Y after subPanel (min 330)
            int txY = 330 + (subs.Count > 0 ? (((subs.Count - 1) / Math.Max((cardRow.Width / 195), 1)) * 62) : 0);
            txY = Math.Max(txY, 330);
            AddSectionLabel("最近消費紀錄", txY);
            BuildTransactionList(txList.Take(10).ToList(), txY + 32, cardRow.Width);
        }

        private Panel MakeSummaryCard(string title, string value, Color accent, string icon)
        {
            var card = new Panel { Size = new Size(210, 96), BackColor = Color.White, Margin = new Padding(0, 0, 14, 0) };
            SetBorder(card);
            card.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent });
            card.Controls.Add(new Label { Text = icon, Location = new Point(16, 10), Size = new Size(30, 30), Font = new Font("Segoe UI Emoji", 16f), TextAlign = ContentAlignment.MiddleCenter });
            card.Controls.Add(new Label { Text = title, Location = new Point(14, 44), Size = new Size(190, 18), Font = new Font("微軟正黑體", 8f), ForeColor = Color.Gray });
            card.Controls.Add(new Label { Text = value, Location = new Point(14, 62), Size = new Size(190, 24), Font = new Font("微軟正黑體", 11f, FontStyle.Bold), ForeColor = accent });
            return card;
        }

        private void AddSectionLabel(string text, int y)
        {
            this.Controls.Add(new Label
            {
                Text = text, Location = new Point(0, y), Size = new Size(400, 26),
                Font = new Font("微軟正黑體", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 50, 70)
            });
        }

        private void BuildTransactionList(List<FinanceCalendarApp.Models.Transaction> list, int startY, int width)
        {
            if (list.Count == 0)
            {
                this.Controls.Add(new Label { Text = "本月尚無記帳紀錄", Location = new Point(0, startY), ForeColor = Color.Gray, AutoSize = true });
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                bool isExp = t.Type == "支出";
                var row = new Panel { Location = new Point(0, startY + i * 40), Size = new Size(width, 36), BackColor = i % 2 == 0 ? Color.White : Color.FromArgb(249, 250, 252) };
                SetBorder(row);

                void L(string txt, int x, int w, Color? c = null, bool bold = false) =>
                    row.Controls.Add(new Label { Text = txt, Location = new Point(x, 0), Size = new Size(w, 36), TextAlign = ContentAlignment.MiddleLeft, Font = new Font("微軟正黑體", 9f, bold ? FontStyle.Bold : FontStyle.Regular), ForeColor = c ?? Color.FromArgb(40, 50, 70) });

                L($"{CategoryIcon(t.Category)} {t.Category}", 12, 130);
                L(t.Note.Length > 0 ? t.Note : "-", 145, 220, Color.Gray);
                L(t.PaymentMethod == "信用卡" ? "💳" : "💵", 368, 32);
                L(t.Date.ToString("MM/dd"), 403, 55, Color.Gray);
                L((isExp ? "-" : "+") + $"NT$ {t.Amount:N0}", 462, 140,
                    isExp ? Color.FromArgb(192, 57, 43) : Color.FromArgb(39, 174, 96), true);
                this.Controls.Add(row);
            }
        }

        private void SetBorder(Panel p)
        {
            p.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(220, 225, 235), 1f);
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1));
            };
        }

        private string CategoryIcon(string cat) => cat switch
        {
            "娛樂" => "🎮", "飲食" => "🍜", "交通" => "🚌", "購物" => "🛍️",
            "醫療" => "💊", "教育" => "📚", "住房" => "🏠", "訂閱" => "🔄", _ => "📦"
        };
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinanceCalendarApp.Models;
using FinanceCalendarApp.Services;

namespace FinanceCalendarApp.Forms
{
    public class SubscriptionPage : UserControl
    {
        private Panel pnlList;
        private Label lblTotal;

        public SubscriptionPage()
        {
            this.BackColor = Color.Transparent;
            this.AutoScroll = true;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Controls.Clear();

            // ── Toolbar ──────────────────────────────────────────────
            var toolbar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(900, 48),
                BackColor = Color.Transparent
            };

            lblTotal = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(400, 48),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微軟正黑體", 10f)
            };

            var btnAdd = MakeButton("＋ 新增訂閱", Color.FromArgb(52, 152, 219));
            btnAdd.Location = new Point(680, 8);
            btnAdd.Click += (s, e) => OpenDialog(null);

            toolbar.Controls.AddRange(new Control[] { lblTotal, btnAdd });
            this.Controls.Add(toolbar);

            // ── Header row ───────────────────────────────────────────
            var header = MakeHeaderRow();
            header.Location = new Point(0, 56);
            this.Controls.Add(header);

            // ── List ─────────────────────────────────────────────────
            pnlList = new Panel
            {
                Location = new Point(0, 88),
                Size = new Size(900, 600),
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(pnlList);

            RefreshList();
        }

        private void RefreshList()
        {
            pnlList.Controls.Clear();
            var subs = DataService.GetSubscriptions();
            decimal monthlyTotal = DataService.GetMonthlySubscriptionTotal();
            lblTotal.Text = $"目前訂閱：{subs.Count} 項　　每月合計：NT$ {monthlyTotal:N0}";

            for (int i = 0; i < subs.Count; i++)
            {
                var row = BuildRow(subs[i], i);
                row.Location = new Point(0, i * 52);
                pnlList.Controls.Add(row);
            }
            pnlList.Height = Math.Max(subs.Count * 52, 100);
        }

        private Panel BuildRow(Subscription sub, int index)
        {
            var row = new Panel
            {
                Size = new Size(860, 48),
                BackColor = index % 2 == 0 ? Color.White : Color.FromArgb(249, 250, 252)
            };

            // Status dot
            var dot = new Panel
            {
                Size = new Size(10, 10),
                Location = new Point(8, 19),
                BackColor = sub.IsActive ? Color.FromArgb(39, 174, 96) : Color.FromArgb(180, 180, 180)
            };

            row.Controls.Add(new Label
            {
                Text = $"{sub.Icon} {sub.Name}",
                Location = new Point(26, 0),
                Size = new Size(180, 48),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微軟正黑體", 10f, FontStyle.Bold)
            });
            row.Controls.Add(new Label
            {
                Text = sub.Category,
                Location = new Point(210, 0),
                Size = new Size(90, 48),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gray
            });
            row.Controls.Add(new Label
            {
                Text = sub.Cycle,
                Location = new Point(305, 0),
                Size = new Size(70, 48),
                TextAlign = ContentAlignment.MiddleLeft
            });
            row.Controls.Add(new Label
            {
                Text = $"每月 {sub.BillingDay} 日",
                Location = new Point(380, 0),
                Size = new Size(90, 48),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gray
            });
            row.Controls.Add(new Label
            {
                Text = $"{sub.Currency} {sub.Amount:N0}",
                Location = new Point(475, 0),
                Size = new Size(120, 48),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微軟正黑體", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            });
            row.Controls.Add(new Label
            {
                Text = sub.IsActive ? "啟用中" : "已停用",
                Location = new Point(600, 0),
                Size = new Size(70, 48),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = sub.IsActive ? Color.FromArgb(39, 174, 96) : Color.Gray
            });

            var btnEdit = new Button
            {
                Text = "✏️",
                Location = new Point(680, 11),
                Size = new Size(34, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(243, 156, 18),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = sub
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) => OpenDialog((Subscription)((Button)s).Tag);

            var btnDel = new Button
            {
                Text = "🗑️",
                Location = new Point(720, 11),
                Size = new Size(34, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = sub
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s, e) =>
            {
                var s2 = (Subscription)((Button)s).Tag;
                if (MessageBox.Show($"確定刪除「{s2.Name}」？", "確認刪除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DataService.DeleteSubscription(s2.Id);
                    RefreshList();
                }
            };

            row.Controls.AddRange(new Control[] { dot, btnEdit, btnDel });
            return row;
        }

        private Panel MakeHeaderRow()
        {
            var hdr = new Panel
            {
                Size = new Size(860, 30),
                BackColor = Color.FromArgb(240, 243, 248)
            };
            void AddHdrLabel(string text, int x, int w) =>
                hdr.Controls.Add(new Label
                {
                    Text = text, Location = new Point(x, 0), Size = new Size(w, 30),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("微軟正黑體", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 90, 110)
                });

            AddHdrLabel("名稱", 26, 180);
            AddHdrLabel("分類", 210, 90);
            AddHdrLabel("週期", 305, 70);
            AddHdrLabel("扣款日", 380, 90);
            AddHdrLabel("金額", 475, 120);
            AddHdrLabel("狀態", 600, 70);
            AddHdrLabel("操作", 680, 80);
            return hdr;
        }

        private void OpenDialog(Subscription sub)
        {
            var dlg = new SubscriptionDialog(sub);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                DataService.SaveSubscription(dlg.Result);
                RefreshList();
            }
        }

        private Button MakeButton(string text, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(150, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 9.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }

    // ── Subscription Dialog ──────────────────────────────────────────────
    public class SubscriptionDialog : Form
    {
        public Subscription Result { get; private set; }
        private TextBox txtName, txtAmount, txtNote;
        private ComboBox cboIcon, cboCycle, cboCategory, cboCurrency;
        private NumericUpDown nudDay;
        private CheckBox chkActive;
        private bool isEdit;

        private static readonly string[] Icons = { "💳", "🎵", "📺", "🎮", "📰", "☁️", "🔐", "📦", "🤖", "🎬", "📱", "💻" };
        private static readonly string[] Categories = { "娛樂", "音樂", "影視", "教育", "工具", "雲端", "新聞", "健康", "其他" };
        private static readonly string[] Cycles = { "月付", "年付" };
        private static readonly string[] Currencies = { "TWD", "USD", "JPY", "EUR" };

        public SubscriptionDialog(Subscription sub)
        {
            isEdit = sub != null;
            Result = sub != null ? CloneSubscription(sub) : new Subscription();

            this.Text = isEdit ? "編輯訂閱" : "新增訂閱";
            this.Size = new Size(420, 430);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("微軟正黑體", 9.5f);
            this.BackColor = Color.White;

            int y = 20;
            int labelW = 80, ctrlX = 110, ctrlW = 260;

            Add("名稱", y); txtName = AddTxt(ctrlX, y, ctrlW, Result.Name); y += 36;
            Add("圖示", y);
            cboIcon = AddCombo(ctrlX, y, 100, Icons, Result.Icon); y += 36;
            Add("分類", y);
            cboCategory = AddCombo(ctrlX, y, 160, Categories, Result.Category); y += 36;
            Add("金額", y); txtAmount = AddTxt(ctrlX, y, 120, Result.Amount.ToString()); y += 36;
            Add("幣別", y);
            cboCurrency = AddCombo(ctrlX, y, 90, Currencies, Result.Currency); y += 36;
            Add("週期", y);
            cboCycle = AddCombo(ctrlX, y, 100, Cycles, Result.Cycle); y += 36;
            Add("扣款日", y);
            nudDay = new NumericUpDown { Location = new Point(ctrlX, y), Width = 70, Minimum = 1, Maximum = 28, Value = Math.Min(Result.BillingDay, 28) };
            this.Controls.Add(nudDay);
            this.Controls.Add(new Label { Text = "號", Location = new Point(ctrlX + 76, y + 3), AutoSize = true });
            y += 36;
            Add("備註", y); txtNote = AddTxt(ctrlX, y, ctrlW, Result.Note); y += 36;
            chkActive = new CheckBox { Text = "啟用此訂閱", Checked = Result.IsActive, Location = new Point(ctrlX, y), AutoSize = true };
            this.Controls.Add(chkActive);
            y += 40;

            var btnOk = new Button
            {
                Text = "儲存 (Enter)", DialogResult = DialogResult.OK,
                Location = new Point(ctrlX, y), Width = 140,
                BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += Save;

            var btnCancel = new Button
            {
                Text = "取消", DialogResult = DialogResult.Cancel,
                Location = new Point(ctrlX + 150, y), Width = 90,
                FlatStyle = FlatStyle.Flat
            };

            this.AcceptButton = btnOk;
            this.Controls.AddRange(new Control[] { btnOk, btnCancel });
            this.Height = y + 80;

            void Add(string text, int top) =>
                this.Controls.Add(new Label { Text = text + "：", Location = new Point(20, top + 3), Width = labelW, TextAlign = ContentAlignment.TopRight });
        }

        private TextBox AddTxt(int x, int y, int w, string val)
        {
            var t = new TextBox { Location = new Point(x, y), Width = w, Text = val };
            this.Controls.Add(t);
            return t;
        }

        private ComboBox AddCombo(int x, int y, int w, string[] items, string selected)
        {
            var c = new ComboBox { Location = new Point(x, y), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            c.Items.AddRange(items);
            c.SelectedItem = selected;
            if (c.SelectedIndex < 0 && c.Items.Count > 0) c.SelectedIndex = 0;
            this.Controls.Add(c);
            return c;
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("請輸入訂閱名稱"); this.DialogResult = DialogResult.None; return; }
            if (!decimal.TryParse(txtAmount.Text, out decimal amt) || amt <= 0) { MessageBox.Show("請輸入正確金額"); this.DialogResult = DialogResult.None; return; }

            Result.Name = txtName.Text.Trim();
            Result.Icon = cboIcon.SelectedItem?.ToString() ?? "💳";
            Result.Category = cboCategory.SelectedItem?.ToString() ?? "其他";
            Result.Amount = amt;
            Result.Currency = cboCurrency.SelectedItem?.ToString() ?? "TWD";
            Result.Cycle = cboCycle.SelectedItem?.ToString() ?? "月付";
            Result.BillingDay = (int)nudDay.Value;
            Result.Note = txtNote.Text.Trim();
            Result.IsActive = chkActive.Checked;
        }

        private Subscription CloneSubscription(Subscription s) => new Subscription
        {
            Id = s.Id, Name = s.Name, Icon = s.Icon, Amount = s.Amount,
            Currency = s.Currency, BillingDay = s.BillingDay, Cycle = s.Cycle,
            Category = s.Category, IsActive = s.IsActive, StartDate = s.StartDate, Note = s.Note
        };
    }
}

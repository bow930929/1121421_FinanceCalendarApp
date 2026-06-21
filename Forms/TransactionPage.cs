using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FinanceCalendarApp.Models;
using FinanceCalendarApp.Services;

namespace FinanceCalendarApp.Forms
{
    public class TransactionPage : UserControl
    {
        private Panel pnlList;
        private Label lblSummary;
        private int _year, _month;
        private Button btnPrev, btnNext;
        private Label lblMonth;

        public TransactionPage()
        {
            _year = DateTime.Today.Year;
            _month = DateTime.Today.Month;
            this.BackColor = Color.Transparent;
            this.AutoScroll = true;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Controls.Clear();

            // ── Month navigator ──────────────────────────────────────
            var navRow = new Panel { Location = new Point(0, 0), Size = new Size(900, 44), BackColor = Color.Transparent };

            btnPrev = NavBtn("◀");
            btnPrev.Location = new Point(0, 6);
            btnPrev.Click += (s, e) => { if (_month == 1) { _month = 12; _year--; } else _month--; RefreshList(); UpdateMonthLabel(); };

            lblMonth = new Label
            {
                Text = $"{_year} 年 {_month:D2} 月",
                Location = new Point(50, 0),
                Size = new Size(150, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微軟正黑體", 11f, FontStyle.Bold)
            };

            btnNext = NavBtn("▶");
            btnNext.Location = new Point(202, 6);
            btnNext.Click += (s, e) => { if (_month == 12) { _month = 1; _year++; } else _month++; RefreshList(); UpdateMonthLabel(); };

            lblSummary = new Label
            {
                Location = new Point(260, 0),
                Size = new Size(380, 44),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微軟正黑體", 9.5f),
                ForeColor = Color.Gray
            };

            var btnAdd = MakeButton("＋ 新增消費", Color.FromArgb(39, 174, 96));
            btnAdd.Location = new Point(680, 6);
            btnAdd.Click += (s, e) => OpenDialog(null);

            navRow.Controls.AddRange(new Control[] { btnPrev, lblMonth, btnNext, lblSummary, btnAdd });
            this.Controls.Add(navRow);

            // ── Header ───────────────────────────────────────────────
            var header = new Panel
            {
                Location = new Point(0, 50),
                Size = new Size(860, 30),
                BackColor = Color.FromArgb(240, 243, 248)
            };
            void H(string t, int x, int w) => header.Controls.Add(new Label
            {
                Text = t, Location = new Point(x, 0), Size = new Size(w, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微軟正黑體", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 90, 110)
            });
            H("日期", 12, 70); H("類型", 85, 60); H("分類", 148, 90); H("付款", 241, 80);
            H("備註", 324, 200); H("金額", 527, 120); H("操作", 650, 60);
            this.Controls.Add(header);

            // ── List ─────────────────────────────────────────────────
            pnlList = new Panel
            {
                Location = new Point(0, 84),
                Size = new Size(860, 600),
                BackColor = Color.Transparent
            };
            this.Controls.Add(pnlList);

            RefreshList();
        }

        private void RefreshList()
        {
            pnlList.Controls.Clear();
            var txs = DataService.GetTransactions(_year, _month);
            decimal expense = txs.Where(t => t.Type == "支出").Sum(t => t.Amount);
            decimal income = txs.Where(t => t.Type == "收入").Sum(t => t.Amount);
            lblSummary.Text = $"支出：NT$ {expense:N0}　收入：NT$ {income:N0}　淨：NT$ {income - expense:N0}";

            if (txs.Count == 0)
            {
                pnlList.Controls.Add(new Label
                {
                    Text = "本月尚無記帳紀錄，點選「新增消費」開始記帳",
                    Location = new Point(0, 20), AutoSize = true, ForeColor = Color.Gray
                });
                return;
            }

            for (int i = 0; i < txs.Count; i++)
            {
                var t = txs[i];
                var row = BuildRow(t, i);
                row.Location = new Point(0, i * 42);
                pnlList.Controls.Add(row);
            }
        }

        private Panel BuildRow(Transaction t, int index)
        {
            bool isExp = t.Type == "支出";
            var row = new Panel
            {
                Size = new Size(860, 38),
                BackColor = index % 2 == 0 ? Color.White : Color.FromArgb(249, 250, 252)
            };

            void L(string txt, int x, int w, Color? c = null, bool bold = false) =>
                row.Controls.Add(new Label
                {
                    Text = txt, Location = new Point(x, 0), Size = new Size(w, 38),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("微軟正黑體", 9f, bold ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = c ?? Color.FromArgb(40, 50, 70)
                });

            L(t.Date.ToString("MM/dd"), 12, 70);
            L(t.Type, 85, 60, isExp ? Color.FromArgb(192, 57, 43) : Color.FromArgb(39, 174, 96));
            L(CategoryIcon(t.Category) + " " + t.Category, 148, 90);
            L(t.PaymentMethod == "信用卡" ? "💳 信用卡" : "💵 現金", 241, 80, Color.Gray);
            L(t.Note.Length > 0 ? t.Note : "-", 324, 200, Color.Gray);
            L((isExp ? "-" : "+") + $"NT$ {t.Amount:N0}", 527, 120,
                isExp ? Color.FromArgb(192, 57, 43) : Color.FromArgb(39, 174, 96), true);

            if (!t.IsSubscription)
            {
                var btnE = RowBtn("✏️", Color.FromArgb(243, 156, 18), 652);
                btnE.Tag = t;
                btnE.Click += (s, e) => OpenDialog((Transaction)((Button)s).Tag);

                var btnD = RowBtn("🗑️", Color.FromArgb(231, 76, 60), 690);
                btnD.Tag = t;
                btnD.Click += (s, e) =>
                {
                    var tx = (Transaction)((Button)s).Tag;
                    if (MessageBox.Show("確定刪除此紀錄？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        DataService.DeleteTransaction(tx.Id);
                        RefreshList();
                    }
                };
                row.Controls.AddRange(new Control[] { btnE, btnD });
            }
            else
            {
                row.Controls.Add(new Label
                {
                    Text = "🔄 自動",
                    Location = new Point(652, 0),
                    Size = new Size(80, 38),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.FromArgb(52, 152, 219),
                    Font = new Font("微軟正黑體", 8f)
                });
            }

            return row;
        }

        private Button RowBtn(string text, Color color, int x)
        {
            var b = new Button
            {
                Text = text, Location = new Point(x, 8), Size = new Size(32, 22),
                FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White, Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void OpenDialog(Transaction tx)
        {
            var dlg = new TransactionDialog(tx);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                DataService.SaveTransaction(dlg.Result);
                RefreshList();
            }
        }

        private void UpdateMonthLabel()
        {
            lblMonth.Text = $"{_year} 年 {_month:D2} 月";
        }

        private Button NavBtn(string text)
        {
            var b = new Button
            {
                Text = text, Size = new Size(36, 30), FlatStyle = FlatStyle.Flat,
                BackColor = Color.White, ForeColor = Color.FromArgb(40, 50, 70), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private Button MakeButton(string text, Color color)
        {
            var btn = new Button
            {
                Text = text, Size = new Size(150, 32), FlatStyle = FlatStyle.Flat,
                BackColor = color, ForeColor = Color.White,
                Font = new Font("微軟正黑體", 9.5f), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private string CategoryIcon(string cat) => cat switch
        {
            "娛樂" => "🎮", "飲食" => "🍜", "交通" => "🚌", "購物" => "🛍️",
            "醫療" => "💊", "教育" => "📚", "住房" => "🏠", "訂閱" => "🔄", _ => "📦"
        };
    }

    // ── Transaction Dialog ─────────────────────────────────────────────────
    public class TransactionDialog : Form
    {
        public Transaction Result { get; private set; }
        private DateTimePicker dtpDate;
        private ComboBox cboType, cboCategory, cboPayment;
        private TextBox txtAmount, txtNote;
        private static readonly string[] Categories = { "飲食", "交通", "娛樂", "購物", "醫療", "教育", "住房", "訂閱", "薪資", "其他" };

        public TransactionDialog(Transaction tx)
        {
            bool isEdit = tx != null;
            Result = tx != null ? Clone(tx) : new Transaction();

            this.Text = isEdit ? "編輯紀錄" : "新增消費";
            this.Size = new Size(380, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("微軟正黑體", 9.5f);
            this.BackColor = Color.White;

            int y = 20, ctrlX = 110, ctrlW = 220;

            void Lbl(string t, int top) => this.Controls.Add(new Label
            { Text = t + "：", Location = new Point(16, top + 3), Width = 90, TextAlign = ContentAlignment.TopRight });

            Lbl("日期", y);
            dtpDate = new DateTimePicker { Location = new Point(ctrlX, y), Width = ctrlW, Value = Result.Date, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpDate); y += 36;

            Lbl("類型", y);
            cboType = new ComboBox { Location = new Point(ctrlX, y), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cboType.Items.AddRange(new[] { "支出", "收入" });
            cboType.SelectedItem = Result.Type;
            this.Controls.Add(cboType); y += 36;

            Lbl("分類", y);
            cboCategory = new ComboBox { Location = new Point(ctrlX, y), Width = ctrlW, DropDownStyle = ComboBoxStyle.DropDownList };
            cboCategory.Items.AddRange(Categories);
            cboCategory.SelectedItem = Result.Category;
            if (cboCategory.SelectedIndex < 0) cboCategory.SelectedIndex = 0;
            this.Controls.Add(cboCategory); y += 36;

            Lbl("付款方式", y);
            cboPayment = new ComboBox { Location = new Point(ctrlX, y), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cboPayment.Items.AddRange(new[] { "現金", "信用卡" });
            cboPayment.SelectedItem = Result.PaymentMethod;
            if (cboPayment.SelectedIndex < 0) cboPayment.SelectedIndex = 0;
            this.Controls.Add(cboPayment); y += 36;

            Lbl("金額 (NT$)", y);
            txtAmount = new TextBox { Location = new Point(ctrlX, y), Width = 120, Text = Result.Amount > 0 ? Result.Amount.ToString() : "" };
            this.Controls.Add(txtAmount); y += 36;

            Lbl("備註", y);
            txtNote = new TextBox { Location = new Point(ctrlX, y), Width = ctrlW, Text = Result.Note };
            this.Controls.Add(txtNote); y += 40;

            var btnOk = new Button
            {
                Text = "儲存 (Enter)", DialogResult = DialogResult.OK,
                Location = new Point(ctrlX, y), Width = 130,
                BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += Save;

            var btnCancel = new Button
            {
                Text = "取消", DialogResult = DialogResult.Cancel,
                Location = new Point(ctrlX + 140, y), Width = 80, FlatStyle = FlatStyle.Flat
            };

            this.AcceptButton = btnOk;
            this.Controls.AddRange(new Control[] { btnOk, btnCancel });
            this.Height = y + 80;
        }

        private void Save(object s, EventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text, out decimal amt) || amt <= 0)
            {
                MessageBox.Show("請輸入正確金額");
                this.DialogResult = DialogResult.None;
                return;
            }
            Result.Date = dtpDate.Value.Date;
            Result.Type = cboType.SelectedItem?.ToString() ?? "支出";
            Result.Category = cboCategory.SelectedItem?.ToString() ?? "其他";
            Result.PaymentMethod = cboPayment.SelectedItem?.ToString() ?? "現金";
            Result.Amount = amt;
            Result.Note = txtNote.Text.Trim();
        }

        private Transaction Clone(Transaction t) => new Transaction
        {
            Id = t.Id, Date = t.Date, Amount = t.Amount, Category = t.Category,
            PaymentMethod = t.PaymentMethod, Note = t.Note, Type = t.Type,
            IsSubscription = t.IsSubscription, SubscriptionId = t.SubscriptionId
        };
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using FinanceCalendarApp.Services;

namespace FinanceCalendarApp.Forms
{
    public class MainForm : Form
    {
        private Panel pnlSidebar;
        private Panel pnlContent;
        private Panel pnlHeader;
        private Label lblTitle;
        private Button btnDashboard, btnSubscriptions, btnTransactions, btnCalendar;
        private Button _activeBtn;
        private UserControl _currentPage;

        public MainForm()
        {
            InitializeComponent();
            DataService.SyncAllSubscriptions();
            NavigateTo(new DashboardPage(), btnDashboard, "📊 儀表板");
        }

        private void InitializeComponent()
        {
            this.Text = "記帳行事曆";
            this.Size = new Size(1280, 720);
            this.MinimumSize = new Size(1024, 576);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("微軟正黑體", 9.5f);

            // ── Sidebar ──────────────────────────────────────────────
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(30, 39, 58)
            };

            var lblAppName = new Label
            {
                Text = "💰 記帳行事曆",
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 12f, FontStyle.Bold),
                Location = new Point(0, 20),
                Size = new Size(200, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var sep = new Panel
            {
                Location = new Point(20, 65),
                Size = new Size(160, 1),
                BackColor = Color.FromArgb(60, 70, 90)
            };

            btnDashboard = CreateNavButton("📊  儀表板", 85);
            btnSubscriptions = CreateNavButton("🔄  訂閱管理", 130);
            btnTransactions = CreateNavButton("💵  日常記帳", 175);
            btnCalendar = CreateNavButton("📅  行事曆", 220);

            btnDashboard.Click += (s, e) => NavigateTo(new DashboardPage(), btnDashboard, "📊 儀表板");
            btnSubscriptions.Click += (s, e) => NavigateTo(new SubscriptionPage(), btnSubscriptions, "🔄 訂閱管理");
            btnTransactions.Click += (s, e) => NavigateTo(new TransactionPage(), btnTransactions, "💵 日常記帳");
            btnCalendar.Click += (s, e) => NavigateTo(new CalendarPage(), btnCalendar, "📅 行事曆");

            var lblVersion = new Label
            {
                Text = "v1.0",
                ForeColor = Color.FromArgb(100, 110, 130),
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微軟正黑體", 8f)
            };

            pnlSidebar.Controls.AddRange(new Control[] { lblAppName, sep, btnDashboard, btnSubscriptions, btnTransactions, btnCalendar, lblVersion });

            // ── Header ───────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White
            };

            lblTitle = new Label
            {
                Text = "儀表板",
                Font = new Font("微軟正黑體", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 39, 58),
                Location = new Point(24, 0),
                Size = new Size(400, 52),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Bottom border
            var headerBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(230, 233, 240)
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, headerBorder });

            // ── Content ──────────────────────────────────────────────
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(pnlContent);
            mainPanel.Controls.Add(pnlHeader);

            this.Controls.Add(mainPanel);
            this.Controls.Add(pnlSidebar);
        }

        private Button CreateNavButton(string text, int top)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(0, top),
                Size = new Size(200, 42),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(180, 190, 210),
                BackColor = Color.Transparent,
                Font = new Font("微軟正黑體", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 55, 80);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(55, 65, 95);
            return btn;
        }

        public void NavigateTo(UserControl page, Button navBtn, string title)
        {
            // Reset previous active
            if (_activeBtn != null)
            {
                _activeBtn.BackColor = Color.Transparent;
                _activeBtn.ForeColor = Color.FromArgb(180, 190, 210);
            }
            // Set active
            navBtn.BackColor = Color.FromArgb(55, 65, 95);
            navBtn.ForeColor = Color.White;
            _activeBtn = navBtn;

            lblTitle.Text = title;

            pnlContent.Controls.Clear();
            page.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(page);
            _currentPage = page;
        }
    }
}

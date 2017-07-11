using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Clipboard_History.DL;

namespace Clipboard_History
{
    public partial class FrmHome : Form
    {
        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        IntPtr _nextClipboardViewer;

        /// <summary>
        /// Required designer variable.
        /// </summary>

        private readonly DataManager _dataManager;

        public FrmHome()
        {
            InitializeComponent();
            _dataManager = new DataManager();
            _nextClipboardViewer = (IntPtr)SetClipboardViewer((int)Handle);
        }

        private void frmHome_Load(object sender, EventArgs e)
        {
            UpdateClipboardContent();
        }

        private void UpdateClipboardContent()
        {

            lstHistory.Clear();

            var headersList = new List<string> { "Time", "Clipboard Content" };

            lstHistory.Sorting = SortOrder.Descending;
            lstHistory.View = View.Details;

            lstHistory.Columns.Add(new ColumnHeader());
            lstHistory.Columns[0].Text = headersList[0];
            lstHistory.Columns[0].Width = 120;

            lstHistory.Columns.Add(new ColumnHeader());
            lstHistory.Columns[1].Text = headersList[1];
            lstHistory.Columns[1].Width = lstHistory.Width - lstHistory.Columns[0].Width;

            foreach (var clip in _dataManager.GetTodayClipboardContents())
            {
                string[] row = { clip.Time.ToString("HH:mm:ss"), clip.Content };
                var listView = new ListViewItem(row);

                if (lstHistory.Items.Count % 2 == 0)
                {
                    listView.BackColor = Color.Azure;
                }

                lstHistory.Items.Add(listView);
            }


        }

        protected override void WndProc(ref Message m)
        {
            // defined in winuser.h
            const int wmDrawclipboard = 0x308;
            const int wmChangecbchain = 0x030D;

            switch (m.Msg)
            {
                case wmDrawclipboard:
                    if (txtHistory.Text != Clipboard.GetText())
                    {
                        UpdateClipboardContent();
                        _dataManager.StoreClipboardDataToFile();
                        SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    }
                    break;

                case wmChangecbchain:

                    if (m.WParam == _nextClipboardViewer)
                        _nextClipboardViewer = m.LParam;
                    else
                        SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void lstHistory_Click(object sender, EventArgs e)
        {
            txtHistory.Text = lstHistory.SelectedItems[0].SubItems[1].Text;
        }
    }
}

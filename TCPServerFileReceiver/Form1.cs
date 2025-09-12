namespace TCPServerFileReceiver;

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

public partial class Form1 : Form
{
    private Label lblStatus = null!;
    private ListBox lbConnections = null!;
    private Button btnOpenFolder = null!;
    private ArrayList alSockets = null!;
    private string receiveFolderPath = @"C:\TUGAS\Pemrograman Jaringan\week 4\Receive File";

    public Form1()
    {
        InitializeComponent();
        SetupUI();
    }

    private void SetupUI()
    {
        this.lblStatus = new Label();
        this.lbConnections = new ListBox();
        this.btnOpenFolder = new Button();

        this.Text = "TCP Server";
        this.BackColor = Color.White;
        this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        this.Size = new Size(450, 320);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        int padding = 15;
        this.lblStatus.Text = "Initializing...";
        this.lblStatus.Location = new Point(padding, padding);
        this.lblStatus.AutoSize = true;

        this.lbConnections.Location = new Point(padding, this.lblStatus.Bottom + 5);
        this.lbConnections.Size = new Size(this.ClientSize.Width - 2 * padding, 180);
        this.lbConnections.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        this.lbConnections.Font = new Font("Consolas", 8.25F);
        
        this.btnOpenFolder.Text = "Open Folder";
        this.btnOpenFolder.Size = new Size(120, 30);
        this.btnOpenFolder.Location = new Point(padding, this.lbConnections.Bottom + 10);
        this.btnOpenFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        
        // Desain Tombol Modern
        ApplyModernButtonStyle(this.btnOpenFolder);
        
        // Tambahkan Event Handler
        this.Load += new EventHandler(this.Form1_Load);
        this.btnOpenFolder.Click += new EventHandler(this.btnOpenFolder_Click);
        
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.lbConnections);
        this.Controls.Add(this.btnOpenFolder);
    }

    private void ApplyModernButtonStyle(Button btn)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = Color.FromArgb(224, 224, 224);
        btn.ForeColor = Color.Black;
        btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
        lblStatus.Text = "My IP address is: " + IPHost.AddressList[0].ToString();
        
        alSockets = new ArrayList();
        
        Directory.CreateDirectory(receiveFolderPath);
        
        Thread thdListener = new Thread(new ThreadStart(listenerThread));
        thdListener.IsBackground = true;
        thdListener.Start();
    }

    private void btnOpenFolder_Click(object? sender, EventArgs e)
    {
        try
        {
            Process.Start("explorer.exe", receiveFolderPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not open folder: " + ex.Message);
        }
    }
    
    // Method btnClearLog_Click sudah dihapus dari sini

    public void listenerThread()
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, 8080);
        tcpListener.Start();
        while (true)
        {
            try
            {
                Socket handlerSocket = tcpListener.AcceptSocket();
                LogMessage(handlerSocket.RemoteEndPoint?.ToString() + " connected.");
                
                Thread thdHandler = new Thread(() => handlerThread(handlerSocket));
                thdHandler.IsBackground = true;
                thdHandler.Start();
            }
            catch { }
        }
    }

    public void handlerThread(Socket handlerSocket)
    {
        string filePath = Path.Combine(receiveFolderPath, "Hasil_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg");
        
        try
        {
            using (NetworkStream networkStream = new NetworkStream(handlerSocket))
            using (FileStream fileStream = File.OpenWrite(filePath))
            {
                networkStream.CopyTo(fileStream);
            }
            LogMessage("File saved: " + Path.GetFileName(filePath));
        }
        catch (Exception ex)
        {
            LogMessage("Error: " + ex.Message);
        }
        finally
        {
            handlerSocket.Close();
        }
    }
    
    private void LogMessage(string message)
    {
        if (lbConnections.InvokeRequired)
        {
            lbConnections.Invoke(new Action<string>(LogMessage), message);
        }
        else
        {
            lbConnections.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") + " - " + message);
        }
    }
}
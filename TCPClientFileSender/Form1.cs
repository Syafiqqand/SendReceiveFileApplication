namespace TCPClientFileSender;

using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets; 
using System.Windows.Forms;
using System.Threading.Tasks;

public partial class Form1 : Form
{
    private Label fileLabel = null!;
    private Label serverLabel = null!;
    private TextBox tbFilename = null!;
    private TextBox tbServer = null!;
    private Button btnBrowse = null!;
    private Button btnSend = null!;
    private OpenFileDialog openFileDialog = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel statusLabel = null!;

    public Form1()
    {
        InitializeComponent();
        SetupUI();
    }

    private void SetupUI()
    {
        this.fileLabel = new Label();
        this.serverLabel = new Label();
        this.tbFilename = new TextBox();
        this.tbServer = new TextBox();
        this.btnBrowse = new Button();
        this.btnSend = new Button();
        this.openFileDialog = new OpenFileDialog();
        this.statusStrip = new StatusStrip();
        this.statusLabel = new ToolStripStatusLabel();

        this.Text = "TCP Simple Client";
        this.BackColor = Color.White;
        this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        this.Size = new Size(480, 215); 
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        int padding = 20;
        int controlHeight = 28;
        
        this.fileLabel.Text = "File:";
        this.fileLabel.Location = new Point(padding, padding);
        this.fileLabel.AutoSize = true;

        this.tbFilename.Location = new Point(padding, this.fileLabel.Bottom + 2);
        this.tbFilename.Size = new Size(320, controlHeight);
        this.tbFilename.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.tbFilename.Font = new Font("Segoe UI", 9F);

        this.btnBrowse.Text = "Browse...";
        this.btnBrowse.Location = new Point(this.tbFilename.Right + 8, this.tbFilename.Top);
        this.btnBrowse.Size = new Size(90, controlHeight);
        this.btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        this.serverLabel.Text = "Server IP:";
        this.serverLabel.Location = new Point(padding, this.tbFilename.Bottom + 12);
        this.serverLabel.AutoSize = true;

        this.tbServer.Location = new Point(padding, this.serverLabel.Bottom + 2);
        this.tbServer.Size = new Size(320, controlHeight);
        this.tbServer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.tbServer.Font = new Font("Segoe UI", 9F);

        this.btnSend.Text = "Send File";
        this.btnSend.Location = new Point(this.tbServer.Right + 8, this.tbServer.Top);
        this.btnSend.Size = new Size(90, controlHeight);
        this.btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        
        // Desain Tombol Modern
        ApplyModernButtonStyle(this.btnBrowse);
        ApplyModernButtonStyle(this.btnSend, true);

        // Konfigurasi Status Bar
        this.statusStrip.Items.Add(this.statusLabel);
        this.statusLabel.Text = "";

        // Tambahkan Event Handler
        this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
        this.btnSend.Click += new EventHandler(this.btnSend_Click);
        
        // Tambahkan semua kontrol ke Form
        this.Controls.Add(this.fileLabel);
        this.Controls.Add(this.serverLabel);
        this.Controls.Add(this.tbFilename);
        this.Controls.Add(this.tbServer);
        this.Controls.Add(this.btnBrowse);
        this.Controls.Add(this.btnSend);
        this.Controls.Add(this.statusStrip);
    }

    private void ApplyModernButtonStyle(Button btn, bool isPrimary = false)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = isPrimary ? Color.FromArgb(0, 123, 255) : Color.FromArgb(224, 224, 224);
        btn.ForeColor = isPrimary ? Color.White : Color.Black;
        btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    }

    private void btnBrowse_Click(object? sender, EventArgs e)
    {
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            tbFilename.Text = openFileDialog.FileName;
            statusLabel.Text = "File selected: " + Path.GetFileName(openFileDialog.FileName);
        }
    }

    private async void btnSend_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(tbFilename.Text) || !File.Exists(tbFilename.Text))
        {
            statusLabel.Text = "Error: Please select a valid file.";
            return;
        }
        if (string.IsNullOrEmpty(tbServer.Text))
        {
            statusLabel.Text = "Error: Please enter the server IP.";
            return;
        }

        try
        {
            statusLabel.Text = "Sending file...";
            this.btnSend.Enabled = false;
            this.btnBrowse.Enabled = false;
            
            byte[] fileBuffer;
            using (Stream fileStream = File.OpenRead(tbFilename.Text))
            {
                fileBuffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileBuffer, 0, (int)fileStream.Length);
            }
            
            using (TcpClient clientSocket = new TcpClient())
            {
                await clientSocket.ConnectAsync(tbServer.Text, 8080);
                using (NetworkStream networkStream = clientSocket.GetStream())
                {
                    await networkStream.WriteAsync(fileBuffer, 0, fileBuffer.Length);
                }
            }
            statusLabel.Text = "File sent successfully!";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Error: " + ex.Message;
        }
        finally
        {
            this.btnSend.Enabled = true;
            this.btnBrowse.Enabled = true;
        }
    }
}
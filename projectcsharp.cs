using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class MainForm : Form
{
    private MenuStrip menuStrip;
    private ToolStripMenuItem aboutMenuItem;
    private Button selectFolderButton;
    private TextBox folderPathTextBox;
    private ListBox foldersListBox;
    private DataGridView filesDataGridView;
    private Button processFilesButton;
    private FolderBrowserDialog folderBrowserDialog;

    public MainForm()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Основное окно
        this.Text = "File Manager";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Size = new System.Drawing.Size((int)(Screen.PrimaryScreen.WorkingArea.Width * 0.75), (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.75));

        // Меню
        menuStrip = new MenuStrip();
        aboutMenuItem = new ToolStripMenuItem("About");
        aboutMenuItem.Click += AboutMenuItem_Click;
        menuStrip.Items.Add(aboutMenuItem);
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);

        // Кнопка выбора папки
        selectFolderButton = new Button { Text = "Select Folder", Top = 30, Left = 10, Width = 100 };
        selectFolderButton.Click += SelectFolderButton_Click;
        this.Controls.Add(selectFolderButton);

        // TextBox для отображения пути к папке
        folderPathTextBox = new TextBox { Top = 30, Left = 120, Width = 400, ReadOnly = true };
        this.Controls.Add(folderPathTextBox);

        // ListBox для отображения папок
        foldersListBox = new ListBox { Top = 70, Left = 10, Width = 200, Height = 300 };
        foldersListBox.DoubleClick += FoldersListBox_DoubleClick;
        this.Controls.Add(foldersListBox);

        // DataGridView для отображения файлов
        filesDataGridView = new DataGridView { Top = 70, Left = 220, Width = 400, Height = 300 };
        filesDataGridView.Columns.Add("FileName", "File Name");
        filesDataGridView.Columns.Add("LastModified", "Last Modified");
        filesDataGridView.Columns.Add("FileSize", "File Size (Bytes)");
        filesDataGridView.Columns.Add("RandomDelay", "Random Delay");
        filesDataGridView.DoubleClick += FilesDataGridView_DoubleClick;
        this.Controls.Add(filesDataGridView);

        // Кнопка для обработки файлов
        processFilesButton = new Button { Text = "Process Files", Top = 380, Left = 10, Width = 100, Visible = false };
        processFilesButton.Click += ProcessFilesButton_Click;
        this.Controls.Add(processFilesButton);

        // Диалог выбора папки
        folderBrowserDialog = new FolderBrowserDialog();
    }

    private void AboutMenuItem_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Developer: Komarova Ruslana\nVersion: 1.0", "About");
    }

    private void SelectFolderButton_Click(object sender, EventArgs e)
    {
        if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        {
            folderPathTextBox.Text = folderBrowserDialog.SelectedPath;
            LoadFolderContents(folderBrowserDialog.SelectedPath);
            processFilesButton.Visible = true;
        }
    }

    private void LoadFolderContents(string folderPath)
    {
        foldersListBox.Items.Clear();
        filesDataGridView.Rows.Clear();

        var directories = Directory.GetDirectories(folderPath);
        var files = Directory.GetFiles(folderPath);

        foreach (var dir in directories)
        {
            foldersListBox.Items.Add(dir);
        }

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            filesDataGridView.Rows.Add(fileInfo.Name, fileInfo.LastWriteTime, fileInfo.Length);
        }
    }

    private void FoldersListBox_DoubleClick(object sender, EventArgs e)
    {
        if (foldersListBox.SelectedItem != null)
        {
            var selectedFolder = foldersListBox.SelectedItem.ToString();
            var folderInfo = new DirectoryInfo(selectedFolder);

            var folderDetailsForm = new Form
            {
                Text = "Folder Details",
                Size = new System.Drawing.Size(400, 200),
                StartPosition = FormStartPosition.CenterParent
            };

            var label = new Label
            {
                Text = $"Name: {folderInfo.Name}\nLast Modified: {folderInfo.LastWriteTime}",
                Dock = DockStyle.Fill
            };

            folderDetailsForm.Controls.Add(label);
            folderDetailsForm.ShowDialog();
        }
    }

    private void FilesDataGridView_DoubleClick(object sender, EventArgs e)
    {
        if (filesDataGridView.CurrentRow != null)
        {
            var fileName = filesDataGridView.CurrentRow.Cells[0].Value.ToString();
            var filePath = Path.Combine(folderPathTextBox.Text, fileName);

            var result = MessageBox.Show($"Do you want to duplicate the file '{fileName}'?", "Duplicate File", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                var newFilePath = GetNewFilePath(filePath);
                File.Copy(filePath, newFilePath);
                LoadFolderContents(folderPathTextBox.Text);
            }
        }
    }

    private string GetNewFilePath(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        var counter = 1;
        var newFilePath = Path.Combine(directory, $"{fileName} - Copy{extension}");

        while (File.Exists(newFilePath))
        {
            newFilePath = Path.Combine(directory, $"{fileName} - Copy ({counter}){extension}");
            counter++;
        }

        return newFilePath;
    }

    public async void ProcessFilesButton_Click(object sender, EventArgs e)
    {
        var random = new Random();
        var tasks = new List<Task>();

        foreach (DataGridViewRow row in filesDataGridView.Rows)
        {
            if (row.Cells[0].Value != null)
            {
                var delay = random.Next(1, filesDataGridView.Rows.Count);

                var task = Task.Delay(delay * 1000).ContinueWith(_ =>
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        row.Cells["RandomDelay"].Value = delay;
                    });
                });

                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);
    }


    [STAThread]
    public static void Main()
    {
        Application.Run(new MainForm());
    }
}


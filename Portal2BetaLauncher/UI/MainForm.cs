using System.Diagnostics;
using Portal2BetaLauncher.Models;
using Portal2BetaLauncher.Services;

namespace Portal2BetaLauncher.UI;

public sealed class MainForm : Form
{
    private readonly BuildScanner scanner = new();
    private readonly LaunchService launcher = new();
    private readonly FixService fixes = new();
    private readonly SteamShortcutService steam = new();
    private readonly PortForwardingService forwarding = new();
    private readonly WindowsDebugger debugger = new();
    private readonly ScanRootsStore scanRoots = new();
    private readonly ProcessInspector processInspector = new();
    private readonly List<DetectedBuild> detected = new();
    private readonly ListView builds = new();
    private readonly ListBox folders = new();
    private readonly TextBox map = new();
    private readonly NumericUpDown width = new();
    private readonly NumericUpDown height = new();
    private readonly CheckBox startSteam = new();
    private readonly RichTextBox debugLog = new();
    private readonly NumericUpDown forwardListen = new();
    private readonly NumericUpDown forwardTarget = new();
    private readonly TextBox forwardHost = new();
    private readonly ToolStripStatusLabel status = new();

    // Debugger / process explorer controls.
    private readonly ListView processList = new();
    private readonly ImageList processIcons = new();
    private readonly TextBox processFilter = new();
    private readonly Label processCount = new();
    private readonly Label betaPerfTitle = new();
    private readonly Label betaPerfInfo = new();
    private readonly Label betaPerfPath = new();
    private readonly Label betaPerfArgs = new();
    private readonly Label betaPerfCpu = new();
    private readonly Label betaPerfMemory = new();
    private readonly Label betaPerfWorkingSet = new();
    private readonly Label betaPerfThreads = new();
    private readonly Label betaPerfHandles = new();
    private readonly Label betaPerfUptime = new();
    private readonly Label betaPerfPriority = new();
    private readonly Label betaPerfArch = new();
    private readonly System.Windows.Forms.Timer processRefreshTimer = new();
    private readonly System.Windows.Forms.Timer betaPerfTimer = new();
    private readonly Dictionary<int, (TimeSpan Cpu, DateTime Stamp)> betaCpuSamples = new();
    private int processRefreshRunning;

    public MainForm()
    {
        Text = "Portal 2 Beta Launcher";
        Width = 1380;
        Height = 860;
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;

        processIcons.ImageSize = new Size(20, 20);
        processIcons.ColorDepth = ColorDepth.Depth32Bit;
        processList.SmallImageList = processIcons;

        BuildLayout();
        LoadFolders();

        scanner.Status += s => SafeUi(() => status.Text = s);
        debugger.Output += (_, e) => SafeUi(() =>
        {
            debugLog.SelectionStart = debugLog.TextLength;
            debugLog.SelectionColor = e.IsError ? Color.Firebrick : debugLog.ForeColor;
            debugLog.AppendText(e.Message + Environment.NewLine);
            debugLog.SelectionColor = debugLog.ForeColor;
        });

        processRefreshTimer.Interval = 4000;
        processRefreshTimer.Tick += (_, _) => _ = RefreshProcessListAsync();
        processRefreshTimer.Start();

        betaPerfTimer.Interval = 1000;
        betaPerfTimer.Tick += (_, _) => UpdateBetaPerformance();
        betaPerfTimer.Start();

        FormClosed += (_, _) =>
        {
            processRefreshTimer.Stop();
            betaPerfTimer.Stop();
            debugger.Dispose();
        };

        _ = RefreshProcessListAsync();
    }

    private void BuildLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildsTab());
        tabs.TabPages.Add(DebugTab());
        tabs.TabPages.Add(NetworkTab());
        tabs.TabPages.Add(FixesTab());
        Controls.Add(tabs);

        var bar = new StatusStrip();
        status.Text = "Ready";
        bar.Items.Add(status);
        Controls.Add(bar);
        bar.BringToFront();
    }

    private TabPage BuildsTab()
    {
        var p = new TabPage("Builds");

        builds.Dock = DockStyle.Fill;
        builds.View = View.Details;
        builds.FullRowSelect = true;
        builds.MultiSelect = false;
        builds.HideSelection = false;
        builds.Columns.Add("Build", 90);
        builds.Columns.Add("Date", 160);
        builds.Columns.Add("Path", 660);
        builds.Columns.Add("Executable", 220);

        var top = new Panel { Dock = DockStyle.Top, Height = 178 };
        AddLabel(top, "Map:", 10, 12);
        map.SetBounds(50, 8, 220, 24);
        top.Controls.Add(map);

        AddLabel(top, "W:", 290, 12);
        width.SetBounds(318, 8, 90, 24);
        width.Minimum = 0;
        width.Maximum = 10000;
        width.Value = 1280;
        top.Controls.Add(width);

        AddLabel(top, "H:", 420, 12);
        height.SetBounds(448, 8, 90, 24);
        height.Minimum = 0;
        height.Maximum = 10000;
        height.Value = 720;
        top.Controls.Add(height);

        startSteam.Text = "Start Steam first";
        startSteam.SetBounds(555, 8, 150, 24);
        top.Controls.Add(startSteam);

        top.Controls.Add(Button("Scan All Drives", 10, 48, 120, async (_, _) => await ScanAll()));
        top.Controls.Add(Button("Scan Added", 140, 48, 110, async (_, _) => await ScanAdded()));
        top.Controls.Add(Button("Launch", 260, 48, 100, (_, _) => LaunchSelected()));
        top.Controls.Add(Button("Add to Steam", 370, 48, 120, (_, _) => AddSteam()));
        top.Controls.Add(Button("Debug", 500, 48, 100, (_, _) => DebugSelected()));
        top.Controls.Add(Button("Copy Command", 610, 48, 120, (_, _) => CopyCommand()));

        AddLabel(top, "Extra folders to scan:", 10, 88);
        folders.SetBounds(10, 112, 720, 55);
        folders.HorizontalScrollbar = true;
        top.Controls.Add(folders);

        top.Controls.Add(Button("Add Folder...", 745, 112, 110, (_, _) => AddFolder()));
        top.Controls.Add(Button("Remove", 865, 112, 90, (_, _) => RemoveFolder()));
        top.Controls.Add(Button("Select Folder...", 965, 112, 110, (_, _) => SelectFolderOnly()));
        top.Controls.Add(Button("Open", 1085, 112, 80, (_, _) => OpenSelectedFolder()));

        p.Controls.Add(builds);
        p.Controls.Add(top);
        return p;
    }

    private TabPage DebugTab()
    {
        var p = new TabPage("Debugger");
        var subtabs = new TabControl { Dock = DockStyle.Fill };
        subtabs.TabPages.Add(ProcessExplorerTab());
        subtabs.TabPages.Add(BetaPerformanceTab());
        subtabs.TabPages.Add(DebugEventsTab());
        p.Controls.Add(subtabs);
        return p;
    }

    private TabPage ProcessExplorerTab()
    {
        var p = new TabPage("Processes");

        var toolbar = new Panel { Dock = DockStyle.Top, Height = 48 };
        toolbar.Controls.Add(Button("Refresh", 8, 9, 80, async (_, _) => await RefreshProcessListAsync()));
        toolbar.Controls.Add(Button("End Task", 94, 9, 90, (_, _) => EndSelectedProcess()));
        toolbar.Controls.Add(Button("Start Task...", 190, 9, 105, (_, _) => StartAnyTask()));
        toolbar.Controls.Add(Button("Properties", 301, 9, 90, (_, _) => ShowProcessProperties()));
        toolbar.Controls.Add(Button("Open Folder", 397, 9, 95, (_, _) => OpenProcessFolder()));
        toolbar.Controls.Add(Button("Attach Debugger", 498, 9, 125, (_, _) => AttachSelectedProcess()));
        toolbar.Controls.Add(Button("Start + Debug...", 629, 9, 115, (_, _) => StartAndDebugTask()));

        AddLabel(toolbar, "Filter:", 755, 13);
        processFilter.SetBounds(800, 9, 230, 24);
        processFilter.TextChanged += (_, _) => _ = RefreshProcessListAsync();
        toolbar.Controls.Add(processFilter);

        processCount.SetBounds(1045, 13, 260, 24);
        processCount.AutoSize = true;
        processCount.Text = "0 processes";
        toolbar.Controls.Add(processCount);

        processList.Dock = DockStyle.Fill;
        processList.View = View.Details;
        processList.FullRowSelect = true;
        processList.HideSelection = false;
        processList.MultiSelect = false;
        processList.DoubleClick += (_, _) => ShowProcessProperties();
        processList.Columns.Add("Program", 185);
        processList.Columns.Add("PID", 75);
        processList.Columns.Add("Status", 100);
        processList.Columns.Add("CPU", 75);
        processList.Columns.Add("Private Memory", 115);
        processList.Columns.Add("Working Set", 115);
        processList.Columns.Add("Threads", 75);
        processList.Columns.Add("Handles", 75);
        processList.Columns.Add("Priority", 90);
        processList.Columns.Add("Start Time", 150);
        processList.Columns.Add("Architecture", 90);
        processList.Columns.Add("Session", 70);
        processList.Columns.Add("Window", 260);
        processList.Columns.Add("Path", 550);

        p.Controls.Add(processList);
        p.Controls.Add(toolbar);
        return p;
    }

    private TabPage BetaPerformanceTab()
    {
        var p = new TabPage("Portal 2 Beta Performance");
        var top = new Panel { Dock = DockStyle.Top, Height = 88 };
        betaPerfTitle.SetBounds(14, 12, 1000, 28);
        betaPerfTitle.Font = new Font(Font, FontStyle.Bold);
        betaPerfTitle.Text = "No Portal 2 Beta selected";
        top.Controls.Add(betaPerfTitle);
        betaPerfInfo.SetBounds(14, 44, 1000, 24);
        betaPerfInfo.Text = "Select an hl2.exe or portal2.exe beta process in Processes.";
        top.Controls.Add(betaPerfInfo);
        p.Controls.Add(top);

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(16),
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddPerfRow(table, "CPU:", betaPerfCpu);
        AddPerfRow(table, "Private Memory:", betaPerfMemory);
        AddPerfRow(table, "Working Set:", betaPerfWorkingSet);
        AddPerfRow(table, "Threads:", betaPerfThreads);
        AddPerfRow(table, "Handles:", betaPerfHandles);
        AddPerfRow(table, "Uptime:", betaPerfUptime);
        AddPerfRow(table, "Priority:", betaPerfPriority);
        AddPerfRow(table, "Architecture:", betaPerfArch);
        AddPerfRow(table, "Executable Path:", betaPerfPath);
        AddPerfRow(table, "Command Line:", betaPerfArgs);

        p.Controls.Add(table);
        return p;
    }

    private static void AddPerfRow(TableLayoutPanel table, string label, Label value)
    {
        var l = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Padding = new Padding(0, 5, 0, 5) };
        value.AutoSize = true;
        value.MaximumSize = new Size(1000, 0);
        value.Padding = new Padding(0, 5, 0, 5);
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(l, 0, table.RowCount);
        table.Controls.Add(value, 1, table.RowCount);
    }

    private TabPage DebugEventsTab()
    {
        var p = new TabPage("Debug Events");
        debugLog.Dock = DockStyle.Fill;
        debugLog.Multiline = true;
        debugLog.ReadOnly = true;
        debugLog.ScrollBars = RichTextBoxScrollBars.Both;
        debugLog.WordWrap = false;
        debugLog.Font = new Font(FontFamily.GenericMonospace, 9);

        var bar = new Panel { Dock = DockStyle.Top, Height = 46 };
        bar.Controls.Add(Button("Stop Debugger", 10, 8, 115, (_, _) => debugger.Stop()));
        bar.Controls.Add(Button("Clear", 130, 8, 80, (_, _) => debugLog.Clear()));
        p.Controls.Add(debugLog);
        p.Controls.Add(bar);
        return p;
    }

    private TabPage NetworkTab()
    {
        var p = new TabPage("Multiplayer workaround");
        var info = new Label
        {
            Text = "TCP port forwarding helper (uses Windows netsh). Run the launcher elevated when Windows requests administrator rights. This does not fix the beta's p2p2 implementation itself.",
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(10)
        };
        p.Controls.Add(info);

        AddLabel(p, "Listen:", 10, 58);
        forwardListen.SetBounds(65, 54, 90, 24);
        forwardListen.Minimum = 1;
        forwardListen.Maximum = 65535;
        forwardListen.Value = 27015;
        p.Controls.Add(forwardListen);

        AddLabel(p, "Target:", 175, 58);
        forwardHost.SetBounds(225, 54, 180, 24);
        forwardHost.Text = "127.0.0.1";
        p.Controls.Add(forwardHost);

        AddLabel(p, "Target Port:", 420, 58);
        forwardTarget.SetBounds(500, 54, 90, 24);
        forwardTarget.Minimum = 1;
        forwardTarget.Maximum = 65535;
        forwardTarget.Value = 27015;
        p.Controls.Add(forwardTarget);

        p.Controls.Add(Button("Add TCP Proxy", 610, 52, 120, async (_, _) => await Forward(true)));
        p.Controls.Add(Button("Remove", 740, 52, 90, async (_, _) => await Forward(false)));
        p.Controls.Add(Button("List Rules", 840, 52, 90, async (_, _) => MessageBox.Show(await forwarding.ListWindowsTcpPortProxiesAsync(), "Port Proxy")));
        return p;
    }

    private TabPage FixesTab()
    {
        var p = new TabPage("No-Steam fixes");
        var list = new ListBox { Left = 10, Top = 10, Width = 330, Height = 570 };
        list.Items.AddRange(fixes.GetFixNames().Cast<object>().ToArray());
        p.Controls.Add(list);

        var target = new TextBox { Left = 360, Top = 40, Width = 600 };
        p.Controls.Add(target);
        AddLabel(p, "Beta root folder:", 360, 16);
        p.Controls.Add(Button("Browse...", 970, 38, 90, (_, _) =>
        {
            using var d = new FolderBrowserDialog();
            if (d.ShowDialog() == DialogResult.OK)
                target.Text = d.SelectedPath;
        }));

        p.Controls.Add(Button("Install selected fix", 360, 82, 150, (_, _) =>
        {
            if (list.SelectedItem is not string n || string.IsNullOrWhiteSpace(target.Text))
            {
                MessageBox.Show("Choose a fix and beta root folder first.");
                return;
            }

            try
            {
                fixes.Install(n, target.Text);
                MessageBox.Show("Fix installed. Existing files were backed up in .p2beta-backups.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fix installer");
            }
        }));
        return p;
    }

    private async Task RefreshProcessListAsync()
    {
        if (Interlocked.Exchange(ref processRefreshRunning, 1) != 0)
            return;

        try
        {
            var filter = processFilter.Text.Trim();
            var selectedPid = SelectedProcessPid();
            var snapshots = await Task.Run(() => processInspector.GetSnapshots());
            if (IsDisposed) return;
            var visible = snapshots.Where(s =>
                string.IsNullOrWhiteSpace(filter) ||
                s.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                s.Path.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                s.Pid.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)).ToArray();

            processList.BeginUpdate();
            processList.Items.Clear();
            foreach (var s in visible)
            {
                var item = new ListViewItem(s.Name, GetProcessIconIndex(s.Path));
                item.Tag = s.Pid;
                item.SubItems.Add(s.Pid.ToString());
                item.SubItems.Add(s.Status);
                item.SubItems.Add(s.Cpu);
                item.SubItems.Add(s.Memory);
                item.SubItems.Add(s.WorkingSet);
                item.SubItems.Add(s.Threads);
                item.SubItems.Add(s.Handles);
                item.SubItems.Add(s.Priority);
                item.SubItems.Add(s.StartTime);
                item.SubItems.Add(s.Architecture);
                item.SubItems.Add(s.Session);
                item.SubItems.Add(s.WindowTitle);
                item.SubItems.Add(s.Path);
                processList.Items.Add(item);
                if (selectedPid == s.Pid)
                    item.Selected = true;
            }
            processList.EndUpdate();
            processCount.Text = $"{visible.Length} shown / {snapshots.Count} total processes";
            UpdateBetaPerformance();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                status.Text = "Process refresh failed: " + ex.Message;
        }
        finally
        {
            Interlocked.Exchange(ref processRefreshRunning, 0);
        }
    }

    private int GetProcessIconIndex(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return -1;

        var key = path;
        if (processIcons.Images.ContainsKey(key))
            return processIcons.Images.IndexOfKey(key);

        try
        {
            using var icon = Icon.ExtractAssociatedIcon(path);
            if (icon is null) return -1;
            processIcons.Images.Add(key, icon.ToBitmap());
            return processIcons.Images.IndexOfKey(key);
        }
        catch { return -1; }
    }

    private int? SelectedProcessPid()
    {
        if (processList.SelectedItems.Count == 0 || processList.SelectedItems[0].Tag is not int pid)
            return null;
        return pid;
    }

    private Process? TryGetSelectedProcess()
    {
        var pid = SelectedProcessPid();
        if (pid is null) return null;
        try { return Process.GetProcessById(pid.Value); }
        catch { return null; }
    }

    private void EndSelectedProcess()
    {
        using var process = TryGetSelectedProcess();
        if (process is null)
        {
            MessageBox.Show("Select a running process first.");
            return;
        }

        var name = process.ProcessName;
        try
        {
            if (process.Id == Environment.ProcessId)
            {
                MessageBox.Show("The launcher cannot end itself.");
                return;
            }

            if (!process.CloseMainWindow())
                process.Kill(entireProcessTree: true);
            else if (!process.WaitForExit(1200))
                process.Kill(entireProcessTree: true);

            status.Text = $"Ended task {name} (PID {process.Id}).";
            _ = RefreshProcessListAsync();
        }
        catch (Exception ex) { MessageBox.Show($"Could not end {name}: {ex.Message}", "End Task"); }
    }

    private void StartAnyTask()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Start Task",
            Filter = "Programs (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true,
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            var p = Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
            status.Text = p is null ? "Program did not start." : $"Started {Path.GetFileName(dialog.FileName)} (PID {p.Id}).";
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Start Task"); }
    }

    private void StartAndDebugTask()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Start Task and Debug",
            Filter = "Programs (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true,
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            var p = Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
            if (p is null) throw new InvalidOperationException("Windows did not return a process for the program.");
            Thread.Sleep(150);
            debugger.DebugProcess(p);
            debugLog.Clear();
            debugLog.AppendText($"Starting debugger for {dialog.FileName} (PID {p.Id})...\r\n");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Start + Debug"); }
    }

    private void AttachSelectedProcess()
    {
        var pid = SelectedProcessPid();
        if (pid is null)
        {
            MessageBox.Show("Select a process first.");
            return;
        }

        debugLog.Clear();
        debugLog.AppendText($"Attaching to PID {pid.Value}...\r\n");
        debugger.Attach(pid.Value);
    }

    private void ShowProcessProperties()
    {
        using var process = TryGetSelectedProcess();
        if (process is null)
        {
            MessageBox.Show("Select a process first.");
            return;
        }

        var path = "Access denied";
        try { path = process.MainModule?.FileName ?? "Unknown"; } catch { }
        var start = "Unknown";
        try { start = process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"); } catch { }
        var priority = "Unknown";
        try { priority = process.PriorityClass.ToString(); } catch { }

        var version = "Unknown";
        var description = "Unknown";
        var product = "Unknown";
        try
        {
            var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
            version = info.FileVersion ?? "Unknown";
            description = info.FileDescription ?? "Unknown";
            product = info.ProductName ?? "Unknown";
        }
        catch { }

        var details = $"Process: {process.ProcessName}\r\nPID: {process.Id}\r\n" +
                      $"Window: {process.MainWindowTitle}\r\nPath: {path}\r\n" +
                      $"Product: {product}\r\nDescription: {description}\r\nFile Version: {version}\r\n" +
                      $"Start Time: {start}\r\nCPU Time: {process.TotalProcessorTime}\r\n" +
                      $"Private Memory: {ProcessInspector.FormatBytes(process.PrivateMemorySize64)}\r\n" +
                      $"Working Set: {ProcessInspector.FormatBytes(process.WorkingSet64)}\r\n" +
                      $"Paged Memory: {ProcessInspector.FormatBytes(process.PagedMemorySize64)}\r\n" +
                      $"Threads: {process.Threads.Count}\r\nHandles: {TryGetHandleCount(process)}\r\n" +
                      $"Priority: {priority}\r\nSession: {TryGetSessionId(process)}\r\n" +
                      $"Architecture: {GetArchitecture(process)}";

        using var form = new Form
        {
            Text = $"Properties — {process.ProcessName} (PID {process.Id})",
            Width = 760,
            Height = 520,
            StartPosition = FormStartPosition.CenterParent,
        };
        var text = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            WordWrap = false,
            Font = new Font(FontFamily.GenericMonospace, 10),
            Text = details,
            ScrollBars = RichTextBoxScrollBars.Both,
        };
        form.Controls.Add(text);
        form.ShowDialog(this);
    }

    private void OpenProcessFolder()
    {
        using var process = TryGetSelectedProcess();
        if (process is null) { MessageBox.Show("Select a process first."); return; }
        try
        {
            var path = process.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(path)) throw new InvalidOperationException("Executable path is unavailable.");
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Open Folder"); }
    }

    private void UpdateBetaPerformance()
    {
        var pid = SelectedProcessPid();
        if (pid is null)
        {
            ResetBetaPerformance();
            return;
        }

        try
        {
            using var process = Process.GetProcessById(pid.Value);
            var snapshot = new ProcessSnapshot(pid.Value, process.ProcessName, "", ProcessInspector.FormatBytes(process.PrivateMemorySize64), "", ProcessInspector.FormatBytes(process.WorkingSet64), process.Threads.Count.ToString(), TryGetHandleCount(process), TryGetPriority(process), "", process.MainWindowTitle, TryGetPath(process), GetArchitecture(process), TryGetSessionId(process));
            if (!ProcessInspector.IsLikelyPortal2(snapshot))
            {
                ResetBetaPerformance("Selected process is not recognized as a Portal 2 Beta executable.");
                return;
            }

            var now = DateTime.UtcNow;
            var cpu = process.TotalProcessorTime;
            var cpuText = "0.0%";
            if (betaCpuSamples.TryGetValue(pid.Value, out var previous))
            {
                var elapsed = (now - previous.Stamp).TotalMilliseconds;
                if (elapsed > 0)
                    cpuText = $"{Math.Max(0, (cpu - previous.Cpu).TotalMilliseconds / elapsed / Environment.ProcessorCount * 100d):0.0}%";
            }
            betaCpuSamples[pid.Value] = (cpu, now);

            var uptime = DateTime.Now - process.StartTime;
            betaPerfTitle.Text = $"{process.ProcessName}.exe — PID {process.Id}";
            betaPerfInfo.Text = "Per-process performance only. No whole-system performance counters are shown here.";
            betaPerfCpu.Text = cpuText;
            betaPerfMemory.Text = ProcessInspector.FormatBytes(process.PrivateMemorySize64);
            betaPerfWorkingSet.Text = ProcessInspector.FormatBytes(process.WorkingSet64);
            betaPerfThreads.Text = process.Threads.Count.ToString();
            betaPerfHandles.Text = TryGetHandleCount(process);
            betaPerfUptime.Text = uptime.ToString(@"dd\.hh\:mm\:ss");
            betaPerfPriority.Text = TryGetPriority(process);
            betaPerfArch.Text = GetArchitecture(process);
            betaPerfPath.Text = TryGetPath(process);
            betaPerfArgs.Text = GetCommandLine(process);
        }
        catch
        {
            ResetBetaPerformance("The selected process exited or its details are no longer accessible.");
        }
    }

    private void ResetBetaPerformance(string message = "Select an hl2.exe or portal2.exe beta process in Processes.")
    {
        betaPerfTitle.Text = "No Portal 2 Beta selected";
        betaPerfInfo.Text = message;
        betaPerfCpu.Text = betaPerfMemory.Text = betaPerfWorkingSet.Text = betaPerfThreads.Text = betaPerfHandles.Text = betaPerfUptime.Text = betaPerfPriority.Text = betaPerfArch.Text = betaPerfPath.Text = betaPerfArgs.Text = "—";
    }

    private void LoadFolders()
    {
        folders.Items.Clear();
        foreach (var folder in scanRoots.Load()) folders.Items.Add(folder);
    }

    private void AddFolder()
    {
        using var d = new FolderBrowserDialog
        {
            Description = "Choose a folder containing one or more Portal 2 beta builds. The launcher will recursively scan it."
        };
        if (d.ShowDialog() != DialogResult.OK) return;
        AddRoot(d.SelectedPath);
    }

    private void SelectFolderOnly()
    {
        using var d = new FolderBrowserDialog
        {
            Description = "Choose a folder to scan once without saving it to the permanent scan list."
        };
        if (d.ShowDialog() != DialogResult.OK) return;
        _ = ScanRoots(new[] { d.SelectedPath }, false, $"Scanning selected folder: {d.SelectedPath}");
    }

    private void AddRoot(string path)
    {
        if (!Directory.Exists(path)) { MessageBox.Show("That folder no longer exists."); return; }
        for (var i = 0; i < folders.Items.Count; i++)
        {
            if (string.Equals((string)folders.Items[i], path, StringComparison.OrdinalIgnoreCase))
            {
                folders.SelectedIndex = i;
                return;
            }
        }
        folders.Items.Add(path);
        scanRoots.Save(folders.Items.Cast<object>().OfType<string>());
        folders.SelectedIndex = folders.Items.Count - 1;
        status.Text = "Scan folder added.";
    }

    private void RemoveFolder()
    {
        if (folders.SelectedIndex < 0) return;
        folders.Items.RemoveAt(folders.SelectedIndex);
        scanRoots.Save(folders.Items.Cast<object>().OfType<string>());
        status.Text = "Scan folder removed.";
    }

    private void OpenSelectedFolder()
    {
        if (folders.SelectedItem is not string folder || !Directory.Exists(folder))
        {
            MessageBox.Show("Select an existing added folder first.");
            return;
        }
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true });
    }

    private Task ScanAll() => ScanRoots(GetAddedFolders(), true, "Scanning every ready drive plus added folders...");
    private Task ScanAdded() => ScanRoots(GetAddedFolders(), false, "Scanning added folders...");
    private IReadOnlyList<string> GetAddedFolders() => folders.Items.Cast<object>().OfType<string>().Where(Directory.Exists).ToArray();

    private async Task ScanRoots(IEnumerable<string> roots, bool includeAllReadyDrives, string message)
    {
        builds.Items.Clear();
        detected.Clear();
        status.Text = message;
        try
        {
            IReadOnlyList<DetectedBuild> result = await scanner.ScanAsync(roots, includeAllReadyDrives);
            detected.AddRange(result);
            PopulateBuildList(result);
            status.Text = $"Found {result.Count} executable match(es).";
        }
        catch (OperationCanceledException) { status.Text = "Scan cancelled."; }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Scan"); status.Text = "Scan failed."; }
    }

    private void PopulateBuildList(IReadOnlyList<DetectedBuild> result)
    {
        foreach (var d in result)
        {
            var i = builds.Items.Add(d.Build.BuildNumber);
            i.SubItems.Add(d.Build.Date);
            i.SubItems.Add(d.StartDirectory);
            i.SubItems.Add(Path.GetFileName(d.ExecutablePath));
        }
    }

    private DetectedBuild? Selected() => builds.SelectedIndices.Count == 0 ? null : detected[builds.SelectedIndices[0]];

    private LaunchOptions Options() => new()
    {
        Map = map.Text.Trim(),
        Height = (int)height.Value,
        Width = (int)width.Value,
        StartSteam = startSteam.Checked
    };

    private void LaunchSelected()
    {
        var d = Selected();
        if (d is null) { MessageBox.Show("Select a build first."); return; }
        try { launcher.Start(d, Options()); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Launch"); }
    }

    private void DebugSelected()
    {
        var d = Selected();
        if (d is null) { MessageBox.Show("Select a build first."); return; }
        try
        {
            var proc = launcher.Start(d, Options());
            debugLog.Clear();
            debugLog.AppendText($"Started {d.Build.DisplayName} as PID {proc.Id}. Attaching debugger...\r\n");
            debugger.DebugProcess(proc);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Debugger"); }
    }

    private void AddSteam()
    {
        var d = Selected();
        if (d is null) { MessageBox.Show("Select a build first."); return; }
        try
        {
            steam.AddShortcut(d, Options());
            MessageBox.Show("Steam shortcut added. Steam may need to be restarted to refresh the library.");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Steam shortcut"); }
    }

    private void CopyCommand()
    {
        var d = Selected();
        if (d is null) { MessageBox.Show("Select a build first."); return; }
        Clipboard.SetText(LaunchService.BuildCommandLine(d, Options()));
    }

    private async Task Forward(bool add)
    {
        try
        {
            var r = new ForwardRule("Portal2Beta", "TCP", (int)forwardListen.Value, forwardHost.Text.Trim(), (int)forwardTarget.Value);
            var s = add ? await forwarding.AddWindowsTcpPortProxyAsync(r) : await forwarding.RemoveWindowsTcpPortProxyAsync(r);
            MessageBox.Show(string.IsNullOrWhiteSpace(s) ? "Done." : s, "Port forwarding");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Port forwarding"); }
    }

    private void SafeUi(Action action)
    {
        if (IsDisposed) return;
        try { if (InvokeRequired) BeginInvoke(action); else action(); }
        catch (InvalidOperationException) { }
    }

    private static Button Button(string text, int x, int y, int w, EventHandler click)
    {
        var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 28 };
        b.Click += click;
        return b;
    }

    private static void AddLabel(Control p, string text, int x, int y) => p.Controls.Add(new Label { Text = text, Left = x, Top = y + 4, AutoSize = true });

    private static string TryGetPath(Process process)
    {
        try { return process.MainModule?.FileName ?? "Unknown"; } catch { return "Access denied"; }
    }

    private static string TryGetHandleCount(Process process)
    {
        try { return process.HandleCount.ToString(); } catch { return "—"; }
    }

    private static string TryGetPriority(Process process)
    {
        try { return process.PriorityClass.ToString(); } catch { return "Unknown"; }
    }

    private static string TryGetSessionId(Process process)
    {
        try { return process.SessionId.ToString(); } catch { return "Unknown"; }
    }

    private static string GetArchitecture(Process process)
    {
        if (!Environment.Is64BitOperatingSystem) return "32-bit";
        try
        {
            if (!IsWow64Process(process.Handle, out var wow64)) return "Unknown";
            return wow64 ? "32-bit" : "64-bit";
        }
        catch { return "Unknown"; }
    }

    private static string GetCommandLine(Process process)
    {
        return "Command line is unavailable through the safe process API on this target. See executable path above.";
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool IsWow64Process(IntPtr process, out bool wow64Process);
}

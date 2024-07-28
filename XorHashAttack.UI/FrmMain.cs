namespace XorHashAttack.UI
{
    public partial class FrmMain : Form
    {
        private bool silentCancel = false;
        private CancellationTokenSource cts = new();

        public FrmMain()
        {
            InitializeComponent();
#if DEBUG
            var lines = new List<string>();
            var hash = new byte[256 / 8];
            Random.Shared.NextBytes(hash);
            TbHash.Text = Utils.ToHex(hash);
            //Add 1.5 times as many hashes to the list as "hash" has bits
            for (var i = 0; i < hash.Length * 12; i++)
            {
                Random.Shared.NextBytes(hash);
                lines.Add(Utils.ToHex(hash));
            }
            TbHashList.Lines = [.. lines];
#endif
            cts.Cancel();
        }

        private void ThHashList_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            if (e.Data.GetDataPresent("FileDrop", true))
            {
                e.Effect = DragDropEffects.Copy;
                e.Message = "Read hashes from %1";
                e.MessageReplacementToken = "dropped files";
            }
            else
            {
                TbHashList.Text = string.Join(Environment.NewLine, e.Data.GetFormats());
            }
        }

        private async void TbHashList_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            try
            {
                if (e.Data.GetData("FileDrop") is string[] dropItem)
                {
                    if (dropItem.Length == 0)
                    {
                        return;
                    }

                    var files = dropItem.Where(File.Exists).ToArray();
                    if (files.Length == 0)
                    {
                        if (dropItem.Length > 0)
                        {
                            throw new InvalidDataException("No files were read. Directories cannot be used in this context.");
                        }
                    }
                    var lines = new List<string>();
                    foreach (var file in files)
                    {
                        lines.AddRange(await Utils.ReadLines(file).ConfigureAwait(true));
                    }
                    TbHashList.Lines = lines.Distinct().ToArray();
                    MessageBox.Show($"Added {TbHashList.Lines.Length} hashes", "Import hashes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Err(ex.Message, "Operation failed");
            }
        }

        private async void BtnBreakXor_Click(object sender, EventArgs e)
        {
            if (!Utils.IsHexData(TbHash.Text))
            {
                Err("Value is not a valid hexadecimal hash. Must be an even number of hexadecimal digits", "Invalid hash");
                TbHash.Select();
                TbHash.SelectAll();
                return;
            }
            var lines = TbHashList.Lines
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim())
                .ToArray();
            foreach (var line in lines)
            {
                if (!Utils.IsHexData(line))
                {
                    Err($"Hash list contains invalid hexadecimal hash '{line}'. Hashes must be an even number of hexadecimal digits", "Invalid hash");
                    TbHashList.Select();
                    TbHashList.SelectAll();
                    return;
                }
            }
            var hash = TbHash.Text;
            var requested = Utils.FromHex(hash);
            var hashes = lines.Select(Utils.FromHex).ToArray();
            var opt = CbOptimize.Checked ? Lib.OptimizeLevel.ToBaseHashes : Lib.OptimizeLevel.None;
            cts.Cancel();
            cts.Dispose();
            cts = new();
            SetEnabledState(false);
            try
            {
                var result = await Task.Run(() => Lib.XorAttackGenerator.BreakXor(requested, hashes, opt, cts.Token));
                var line = hash + " = " + string.Join(" ^ ", result.Select(Utils.ToHex).Select(Utils.TrimHash));
                if (line.Length > short.MaxValue)
                {
                    line = line[..short.MaxValue] + "...";
                }
                MessageBox.Show(line, $"Attack succeeded using {result.Length} hashes");
                if (MessageBox.Show("Copy result hash list to clipboard?", "Save result", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    Clipboard.Clear();
                    Clipboard.SetText(string.Join(Environment.NewLine, result.Select(Utils.ToHex)));
                }
            }
            catch (OperationCanceledException)
            {
                if (!silentCancel)
                {
                    Err("The current computation has been cancelled", "Operation cancelled");
                }
            }
            catch (Exception ex)
            {
                Err(ex, "Operation failed");
            }
            finally
            {
                cts.Cancel();
                SetEnabledState(true);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            cts.Cancel();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            silentCancel = true;
            cts.Cancel();
        }

        private void SetEnabledState(bool state)
        {
            foreach (var c in Controls)
            {
                if (c is Control control)
                {
                    control.Enabled = state;
                }
            }
            BtnCancel.Enabled = !state;
        }

        private static void Err(string text, string title)
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void Err(Exception text, string title) => Err(text.Message, title);
    }
}

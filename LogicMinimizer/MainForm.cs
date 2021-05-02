using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogicMinimizer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeGrid();
        }

        private int VariablesNumber
        {
            get { return Convert.ToInt32(toolStripVariables.SelectedItem); }
        }

        private int FunctionsNumber
        {
            get { return Convert.ToInt32(toolStripFunctions.SelectedItem); }
        }

        private int TermsNumber
        {
            get
            {
                return (int)Math.Pow(2, VariablesNumber);
            }
        }

        private void InitializeGrid()
        {
            var num = VariablesNumber;
            var numRows = TermsNumber;

            dataGridView1.EndEdit();
            dataGridView1.SuspendLayout();
            dataGridView1.Columns.Clear();

            for (int i = 0; i < num; i++)
            {
                var col = new DataGridViewTextBoxColumn();
                col.HeaderText = LogicFunction.LiteralVars[i].ToString();
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns.Add(col);
            }

            for (int i = 0; i < FunctionsNumber; i++)
            {
                var col = new DataGridViewTextBoxColumn();
                col.HeaderText = "Y" + (i + 1).ToString();
                col.ReadOnly = false;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns.Add(col);
            }

            for (int i = 0; i < numRows; i++)
            {
                var r = dataGridView1.Rows.Add();
                var row = dataGridView1.Rows[r];
                row.HeaderCell.Value = i.ToString();
                var s = i.ToBinaryString(num);
                for (int j = 0; j < num; j++)
                {
                    row.Cells[j].Value = s[j];
                }
            }

            dataGridView1.ResumeLayout();
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            var c = dataGridView1.CurrentCell.ColumnIndex;
            var r = dataGridView1.CurrentCell.RowIndex;

            if (!dataGridView1.Columns[c].ReadOnly)
            {
                if (e.Control && e.KeyCode == Keys.V)
                {
                    var v = Clipboard.GetText().Split(new[] { "\r\n" }, StringSplitOptions.None);
                    for (int i = 0; (i < v.Length - 1) && (i + r < dataGridView1.Rows.Count); i++)
                    {
                        var w = v[i].Split('\t');
                        for (int j = 0; (j < v.Length) && (j + c < dataGridView1.Columns.Count); j++)
                        {
                            dataGridView1.Rows[i + r].Cells[j + c].Value = ("0".Equals(w[j])) ? "0" : ("1".Equals(w[j])) ? "1" : "";
                        }
                    }
                }
                else if (e.KeyCode == Keys.Cancel)
                {
                    dataGridView1.Rows[r].Cells[c].Value = "";
                }
            }
        }

        private void toolStripFunctions_TextChanged(object sender, EventArgs e)
        {
            InitializeGrid();
        }

        private void toolStripVariables_TextChanged(object sender, EventArgs e)
        {
            InitializeGrid();
        }

        private void toolStripClear_Click(object sender, EventArgs e)
        {
            InitializeGrid();
        }

        private void toolStripRun_Click(object sender, EventArgs e)
        {
            dataGridView1.EndEdit();
            var functions = new List<string>();
            for (int c = VariablesNumber; c < dataGridView1.Columns.Count; c++)
            {
                int?[] m = new int?[TermsNumber];
                int?[] d = new int?[TermsNumber];
                int k = 0, j = 0;

                for (int r = 0; r < TermsNumber; r++)
                {
                    var value = dataGridView1.Rows[r].Cells[c].Value;
                    if ("1".Equals(value))
                    {
                        m[k++] = r;
                    }
                    else if (!("0".Equals(value)))
                    {
                        d[j++] = r;
                    }
                }

                var terms = m.Where(t => t.HasValue).Select(t => t.Value).ToArray();
                var dontCare = d.Where(t => t.HasValue).Select(t => t.Value).ToArray();

                var solver = new Solver(VariablesNumber, terms, dontCare);
                var function = solver.Solve();               

                // Test the result function
                var result = LogicFunction.Execute(function, VariablesNumber).ToArray();

                var test = Enumerable.Range(0, TermsNumber)
                    .Select(i =>
                    {
                        var value = dataGridView1.Rows[i].Cells[c].Value;
                        var bit = "1".Equals(value) ? true : "0".Equals(value) ? false : (bool?)null;
                        return new { Idx = i, Bit = bit };
                    })
                    .Where(t => t.Bit.HasValue)
                    .All(t => result[t.Idx] == t.Bit);
                
                functions.Add(dataGridView1.Columns[c].HeaderText + " = " + function + (test ? "": " >>> Test FAIL! <<<") );
            }

            textBox1.Text = string.Join("\r\n", functions.ToArray());
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}

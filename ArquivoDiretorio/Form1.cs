using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ArquivoDiretorio.Classes;

namespace ArquivoDiretorio
{
    public partial class Form1 : Form
    {
        private static SqlConnection sqlCon;


        public Form1()
        {
            InitializeComponent();

            this.Text = Application.ProductName.ToString() + " ".PadLeft(40) + Application.ProductVersion;


            SqlConnection sqlconnection = new SqlConnection();
            sqlCon = sqlconnection;
            sqlCon.ConnectionString = Classes.clsVariaveis.Conexao;
            sqlCon.Open();

        }

        private static async Task<string> AlimentaWaveAsync(string _folder)
        {
            // criando datatable
            DataTable dtCam = new DataTable();
            dtCam.Columns.Add("WAVE", typeof(string));
            dtCam.Columns.Add("PATH", typeof(string));


            // pegando somente o nome dos arquivos da pasta
            DirectoryInfo dir_files = new DirectoryInfo(_folder);
            FileInfo[] files2 = dir_files.GetFiles("*.WAV", SearchOption.AllDirectories);

            foreach (var fil in files2)
            {
                // incluindo na datatable
                DataRow dr = dtCam.NewRow();
                dr["WAVE"] = fil.Name;
                dr["PATH"] = fil.FullName;
                dtCam.Rows.Add(dr);
            }

            // incluindo dados da datatable na tabela : WAVE (sql) via slqBulkCopy
            using (SqlBulkCopy s = new SqlBulkCopy(Classes.clsVariaveis.Conexao))
            {
                s.BatchSize = 2500;
                s.NotifyAfter = 5000;

                s.DestinationTableName = "WAVE";
                s.ColumnMappings.Add("WAVE", "wave");
                s.ColumnMappings.Add("PATH", "path");

                await s.WriteToServerAsync(dtCam);
            }

            return "OK";
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = @"\\172.17.14.3\gravacoes vonix\";
            if (folderBrowserDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            // mostra a pasta selecionada
            label1.Text = folderBrowserDialog1.SelectedPath;
            this.Refresh();

            clsVariaveis.GstrSQL = "select * from Wave_Dir_Lido where [path] = '" + label1.Text + "' ";
            DataTable dt1 = new DataTable();
            dt1 = await Classes.clsBanco.ConsultaAsync(clsVariaveis.GstrSQL);
            if (dt1.Rows.Count == 0)
            {
                button1.Enabled = false;
                this.Cursor = Cursors.WaitCursor;


                string resp = await AlimentaWaveAsync(folderBrowserDialog1.SelectedPath);

                if (resp == "OK")
                {
                    clsVariaveis.GstrSQL = "insert into Wave_Dir_Lido ( [path] ) values ('" + label1.Text + "')";
                    bool booAg = await clsBanco.ExecuteQueryAsync(clsVariaveis.GstrSQL);

                    MessageBox.Show("Importado com sucesso  ", "WAVE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("ocorreu um erro na importacao", "WAVE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("esta pasta já foi processada", "WAVE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                label1.Text = "";
                this.Refresh();
            }


            this.Cursor = Cursors.Default;
            button1.Enabled = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            }
        }
    }
}

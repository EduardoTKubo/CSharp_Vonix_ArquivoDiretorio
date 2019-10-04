

namespace ArquivoDiretorio.Classes
{
    class clsVariaveis
    {
        private static readonly string conexao = @"Data Source=10.0.32.59;Initial Catalog=Vonix; User ID=sa; Password=SRV@admin2012;";

        public static string Conexao
        {
            get { return conexao; }
        }


        private static string gstrSQL = string.Empty;
        public static string GstrSQL
        {
            get { return gstrSQL; }
            set { gstrSQL = value; }
        }
    }
}

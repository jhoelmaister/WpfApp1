 using System.Data;
 using Microsoft.Data.SqlClient;


public static class ConexionDB
{
    private static string cadenaConexion = "Server=MAISTER;Database=edberBase7;User Id=sa;Password=papa1122;TrustServerCertificate=True;";

    public static SqlConnection AbrirConexion()
    {
        SqlConnection conexion = new SqlConnection(cadenaConexion);
        if (conexion.State == ConnectionState.Closed) conexion.Open();
        return conexion;
    }

    public static void CerrarConexion(SqlConnection? conexion)
    {
        if (conexion != null && conexion.State == ConnectionState.Open)
        {
            conexion.Close();
            conexion.Dispose();
        }
    }
}
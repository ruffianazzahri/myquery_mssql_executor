namespace Synergy_Test.Function
{

    //Accessing database from this function
    public class DatabaseAccessLayer
    {
        public string ConnectionString = "Data Source=LAPTOP-MU13V76B;Initial Catalog=test;Trusted_Connection=True;";

        public string DbConnection()
        {
            return ConnectionString;
        }
    }
}

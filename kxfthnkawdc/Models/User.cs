namespace kxfthnkawdc.Models;

public sealed class User
{
    public required int Id { get; init; }
    private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

    public string Name
    {
        get
        {
            using var messagesData = _dbContext.DataSource.CreateCommand("select name from users where id=(@id)");
            messagesData.Parameters.AddWithValue("id", Id);
            using var reader = messagesData.ExecuteReader();
            if(reader.Read())
                return (string)reader["Name"];
            throw new Exception("User not found");
        }
        init => _ = value;
    }
}

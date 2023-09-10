namespace kxfthnkawdc.Models;

public sealed class User
{
    private readonly ApplicationDbContext _dbContext;

    public required int Id { get; init; }
    public string Name
    {
        get
        {
            using var messagesData = _dbContext.DataSource.CreateCommand("select name from users where id=(@id)");
            messagesData.Parameters.AddWithValue("id", Id);
            using var reader = messagesData.ExecuteReader();
            if(reader.Read())
                return (string)reader["name"];
            throw new Exception("User not found");
        }
        init => _ = value;
    }

    public string Password { get; private set; }

    public User(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}

namespace Api.Dtos;

public record UserDto(int Id, string UserName, bool IsAdmin, DateTime CreatedAt);

public record CreateUserRequest(string UserName, string Password, bool IsAdmin);

public record UpdateUserRequest(string UserName, string? Password, bool IsAdmin);


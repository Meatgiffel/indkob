namespace Api.Dtos;

public record LoginRequest(string UserName, string Password);

public record AuthUserDto(int Id, string UserName, bool IsAdmin);


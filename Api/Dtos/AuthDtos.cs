namespace Api.Dtos;

public record LoginRequest(string UserName, string Password, bool RememberMe = false);

public record AuthUserDto(int Id, string UserName, bool IsAdmin);

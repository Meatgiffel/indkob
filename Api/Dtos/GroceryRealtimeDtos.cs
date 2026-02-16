namespace Api.Dtos;

public record GroceryChangedEventDto(string Type, int? EntryId, DateTime AtUtc);

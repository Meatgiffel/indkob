namespace Api.Dtos;

public record MealPlanDayDto(DateOnly Date, string? Dinner);

public record UpdateMealPlanDayRequest(string? Dinner);


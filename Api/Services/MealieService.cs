using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Dtos;

namespace Api.Services;

/// <summary>
/// Thin server-to-server client for a self-hosted Mealie instance.
/// Base URL + bearer token come from configuration (Mealie:BaseUrl / Mealie:ApiToken);
/// the token never reaches the browser.
/// </summary>
public class MealieService(HttpClient http, IConfiguration config)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly CultureInfo Da = CultureInfo.GetCultureInfo("da-DK");

    public bool IsConfigured =>
        http.BaseAddress is not null
        && !string.IsNullOrWhiteSpace(config["Mealie:ApiToken"]);

    public async Task<IReadOnlyList<RecipeSummaryDto>> SearchRecipesAsync(string? query, CancellationToken ct)
    {
        var url = "api/recipes?perPage=50&orderBy=name&orderDirection=asc";
        if (!string.IsNullOrWhiteSpace(query))
        {
            url += "&search=" + Uri.EscapeDataString(query.Trim());
        }

        var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var payload = await JsonSerializer.DeserializeAsync<MealieListResponse>(stream, JsonOptions, ct);

        var items = payload?.Items ?? new List<MealieRecipeItem>();
        return items
            .Where(i => !string.IsNullOrWhiteSpace(i.Slug) && !string.IsNullOrWhiteSpace(i.Name))
            .Select(i => new RecipeSummaryDto(
                i.Id ?? string.Empty,
                i.Slug!,
                i.Name!,
                string.IsNullOrWhiteSpace(i.Description) ? null : i.Description!.Trim()))
            .ToList();
    }

    /// <summary>
    /// Fetches a recipe and returns its name plus normalized ingredients
    /// (clean food name + a human amount). Returns null if the recipe is missing.
    /// </summary>
    public async Task<(string Slug, string Name, IReadOnlyList<NormalizedIngredient> Ingredients)?> GetRecipeAsync(
        string slug, CancellationToken ct)
    {
        var response = await http.GetAsync($"api/recipes/{Uri.EscapeDataString(slug)}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var recipe = await JsonSerializer.DeserializeAsync<MealieRecipeDetail>(stream, JsonOptions, ct);
        if (recipe is null)
        {
            return null;
        }

        var ingredients = (recipe.RecipeIngredient ?? new List<MealieIngredient>())
            .Select(NormalizeIngredient)
            .Where(i => !string.IsNullOrWhiteSpace(i.Name))
            .ToList();

        return (recipe.Slug ?? slug, recipe.Name ?? slug, ingredients);
    }

    private static NormalizedIngredient NormalizeIngredient(MealieIngredient ing)
    {
        var display = (ing.Display ?? ing.OriginalText ?? string.Empty).Trim();

        // Prefer the parsed food name; fall back to the rendered display text for
        // unparsed ingredients (food == null).
        var name = ing.Food?.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = display;
        }

        var amount = FormatAmount(ing.Quantity, ing.Unit?.Name);

        if (string.IsNullOrWhiteSpace(display))
        {
            display = string.Join(" ", new[] { amount, name }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        return new NormalizedIngredient(name ?? string.Empty, amount, display);
    }

    private static string? FormatAmount(double? quantity, string? unit)
    {
        string qty = string.Empty;
        if (quantity is > 0)
        {
            qty = quantity.Value % 1 == 0
                ? ((long)quantity.Value).ToString(CultureInfo.InvariantCulture)
                : quantity.Value.ToString("0.##", Da);
        }

        unit = unit?.Trim();
        var parts = new[] { qty, unit }.Where(s => !string.IsNullOrWhiteSpace(s));
        var result = string.Join(" ", parts).Trim();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    public readonly record struct NormalizedIngredient(string Name, string? Amount, string Display);

    private sealed class MealieListResponse
    {
        public List<MealieRecipeItem>? Items { get; set; }
    }

    private sealed class MealieRecipeItem
    {
        public string? Id { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private sealed class MealieRecipeDetail
    {
        public string? Slug { get; set; }
        public string? Name { get; set; }

        [JsonPropertyName("recipeIngredient")]
        public List<MealieIngredient>? RecipeIngredient { get; set; }
    }

    private sealed class MealieIngredient
    {
        public double? Quantity { get; set; }
        public MealieNamed? Unit { get; set; }
        public MealieNamed? Food { get; set; }
        public string? Note { get; set; }
        public string? Display { get; set; }
        public string? OriginalText { get; set; }
    }

    private sealed class MealieNamed
    {
        public string? Name { get; set; }
    }
}
